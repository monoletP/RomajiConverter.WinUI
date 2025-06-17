using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using MeCab;
using MeCab.Extension.UniDic;
using RomajiConverter.Core.Extensions;
using RomajiConverter.Core.Models;

namespace RomajiConverter.Core.Helpers;

public static class RomajiHelper
{
    /// <summary>
    /// 分词器
    /// </summary>
    private static MeCabTagger _tagger;

    /// <summary>
    /// 自定义词典<原文, 假名>
    /// </summary>
    private static Dictionary<string, string> _customizeDict;

    public static void Init()
    {
        //词典路径
        var dicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "unidic");
        var parameter = new MeCabParam
        {
            DicDir = dicPath,
            LatticeLevel = MeCabLatticeLevel.Zero
        };
        _tagger = MeCabTagger.Create(parameter);

        var str = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customizeDict.txt"));
        var list = str.Split(Environment.NewLine);
        _customizeDict = new Dictionary<string, string>();
        foreach (var item in list)
        {
            if (string.IsNullOrWhiteSpace(item)) continue;
            var array = item.Split(" ");
            if (array.Length < 2) continue;
            if (!_customizeDict.ContainsKey(array[0]))
                _customizeDict.Add(array[0], array[1]);
        }
    }

    #region 主逻辑

    /// <summary>
    /// 生成转换结果列表(此处主要实现区分中文,识别变体)
    /// </summary>
    /// <param name="text"></param>
    /// <param name="isAutoVariant"></param>
    /// <param name="chineseRate"></param>
    /// <returns></returns>
    public static List<ConvertedLine> ToRomaji(string text, bool isAutoVariant = false, float chineseRate = 1f)
    {
        var lineTextList = text.RemoveEmptyLine().Split(Environment.NewLine);

        var convertedText = new List<ConvertedLine>();

        for (var index = 0; index < lineTextList.Length; index++)
        {
            var line = lineTextList[index];

            var convertedLine = new ConvertedLine();

            if (IsChinese(line, chineseRate)) continue;

            convertedLine.Japanese = line.Replace("\0", ""); //텍스트에 \0이 포함되어 있으면 첫 번째 \0에만 붙여넣기 되므로 아무것도 없는 것으로 대체해야 합니다. 다음에도 동일하게 적용됩니다.

            var sentences = line.LineToUnits(); //줄을 절로 나눔
            var multiUnits = new List<ConvertedUnit[]>();
            foreach (var sentence in sentences)
            {
                if (IsEnglish(sentence))
                {
                    multiUnits.Add(new[] { new ConvertedUnit(sentence, sentence, sentence, sentence, sentence, false, "", "", "") });
                    continue;
                }

                var units = SentenceToRomaji(sentence);

                //변형 처리
                if (isAutoVariant)
                {
                    var regex = new Regex("[^a-zA-Z0-9\uAC00-\uD7A3 ]", RegexOptions.Compiled);

                    var romajiProns = string.Join("", units.Select(p => p.RomajiPron)); //문장 전체 로마자 표기(장음 -로 표현)
                    var romajiKanas = string.Join("", units.Select(p => p.RomajiKana)); //문장 전체 로마자 표기(장음 유지)

                    var hanMatches = regex.Matches(romajiKanas);
                    if (hanMatches.Any(p => p.Success)) //이 문장을 로마자로 번역한 후 영어가 아닌 문자가 있는지 확인하세요. 만약 있다면, 변형된 부분을 수정한 후 번역해 보세요.
                    {
                        var tempSentence = sentence; //원 문장
                        var tempRomajiPron = romajiProns; //원문의 로마자 표기
                        var tempRomajiKana = romajiKanas;
                        foreach (Match match in hanMatches)
                        {
                            if (match.Success == false) continue; //영어가 아닌 문자 반복

                            tempSentence =
                                tempSentence.Replace(match.Value[0],
                                    VariantHelper.GetVariant(match.Value[0])); //문장을 바꿈
                            convertedLine.Japanese =
                                convertedLine.Japanese.Replace(match.Value[0],
                                    VariantHelper.GetVariant(match.Value[0])); //그런데 이 필드를 업데이트해 주세요

                            tempRomajiPron =
                                string.Join("", SentenceToRomaji(tempSentence).Select(p => p.RomajiPron)); //문장의 로마자 표기를 바꿔보세요
                            tempRomajiKana =
                                string.Join("", SentenceToRomaji(tempSentence).Select(p => p.RomajiKana));
                            var tempHanMatches = regex.Matches(tempRomajiKana);
                            if (tempHanMatches.Any(p => p.Success) ==
                                false) //로마자 표기가 이제 완전히 영어 및 한글로 되어 있다면, 시도한 대체 이후의 문장은 괜찮고 중단할 수 있다는 것을 의미합니다. 완전히 영어로 되어 있지 않다면 다음 문자를 계속해서 대체하십시오.
                                break;
                        }

                        units = SentenceToRomaji(tempSentence);
                    }
                }

                multiUnits.Add(units);
            }

            convertedLine.Units = multiUnits.SelectMany(p => p).ToArray();

            if (index + 1 < lineTextList.Length &&
                IsChinese(lineTextList[index + 1], chineseRate))
                convertedLine.Chinese = lineTextList[index + 1];

            convertedLine.Index = (ushort)convertedText.Count;
            convertedText.Add(convertedLine);
        }

        return convertedText;
    }

    /// <summary>
    /// 分句转为罗马音
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static ConvertedUnit[] SentenceToRomaji(string str)
    {
        var list = _tagger.ParseToNodes(str).ToArray();

        var result = new List<ConvertedUnit>();

        foreach (var item in list)
        {
            ConvertedUnit unit = null;
            string pos1 = item.GetPos1();
            string pos2 = item.GetPos2();
            string pos3 = item.GetPos3();
            // 디버깅용 출력
            //Console.WriteLine($"Word: {item.Surface}, POS1: {item.GetPos1()}, POS2: {item.GetPos2()}, POS3: {item.GetPos3()}, POS4: {item.GetPos4()}, ");

            if (item.CharType > 0)
            {
                var features = CustomSplit(item.Feature);
                Console.WriteLine($"features: {string.Join(", ", features)}, ");
                //Console.WriteLine($"GetOrth: {item.GetOrth()}, GetOrthBase: {item.GetOrthBase()}, GetPron: {item.GetPron()}, GetPronBase: {item.GetPronBase()}");
                if (TryCustomConvert(item.Surface, out var customResult))
                {
                    //사용자 정의 사전 확인
                    unit = new ConvertedUnit(item.Surface,
                        customResult,
                        customResult,
                        KanaHelper.KatakanaToRomaji(customResult),
                        KanaHelper.KatakanaToRomaji(customResult),
                        true,
                        pos1,
                        pos2,
                        pos3);
                }
                else if (features.Length > 0 && pos1 != "助詞" && IsJapanese(item.Surface))
                {
                    //순수 가나확인
                    unit = new ConvertedUnit(item.Surface,
                        KanaHelper.ToHiragana(item.Surface),
                        KanaHelper.ToHiragana(item.Surface),
                        KanaHelper.ConvertLongSound2Hyphen(KanaHelper.KatakanaToRomaji(item.Surface)),
                        KanaHelper.ConvertHyphen2LongSound(KanaHelper.KatakanaToRomaji(item.Surface)),
                        false,
                        pos1,
                        pos2,
                        pos3);
                }
                else if (new[] { "補助記号" }.Contains(pos1))
                {
                    //구두점이나 인식 불가 문자 처리
                    unit = new ConvertedUnit(item.Surface,
                        item.Surface,
                        item.Surface,
                        "",
                        "",
                        false,
                        "助詞",//조사처럼 앞에 띄어쓰기 없도록
                        pos2,
                        pos3);
                }
                else if (IsEnglish(item.Surface) && pos2 != "数詞")
                {
                    //영어
                    unit = new ConvertedUnit(item.Surface,
                        item.Surface,
                        item.Surface,
                        item.Surface,
                        item.Surface,
                        false,
                        pos1,
                        pos2,
                        pos3);
                }
                else if (features.Length <= 6)
                {
                    //구두점이나 인식 불가 문자 처리 2
                    unit = new ConvertedUnit(item.Surface,
                        item.Surface,
                        item.Surface,
                        "",
                        "",
                        false,
                        "助詞",//조사처럼 앞에 띄어쓰기 없도록
                        pos2,
                        pos3);
                }
                else
                {
                    //한자나 조사 처리
                    //var kana = GetKana(item);
                    var pron = GetPron(item);
                    var kana = GetKana(item);

                    unit = new ConvertedUnit(item.Surface,
                        KanaHelper.ToHiragana(pron),
                        KanaHelper.ToHiragana(kana),
                        KanaHelper.KatakanaToRomaji(pron),
                        KanaHelper.KatakanaToRomaji(kana),
                        !IsJapanese(item.Surface),
                        pos1,
                        pos2,
                        pos3);
                    var (replaceHiraganaPron, replaceRomajiPron, replaceHiraganaKana, replaceRomajiKana) = GetReplaceData(item);
                    unit.ReplaceHiraganaPron = replaceHiraganaPron;
                    unit.ReplaceRomajiPron = replaceRomajiPron;
                    unit.ReplaceHiraganaKana = replaceHiraganaKana;
                    unit.ReplaceRomajiKana = replaceRomajiKana;
                }
            }
            else if (item.Stat != MeCabNodeStat.Bos && item.Stat != MeCabNodeStat.Eos)
            {
                unit = new ConvertedUnit(item.Surface,
                    item.Surface,
                    item.Surface,
                    item.Surface,
                    item.Surface,
                    false,
                    pos1,
                    pos2,
                    pos3);
            }

            if (unit != null)
                result.Add(unit);
        }

        return result.ToArray();
    }

    #endregion

    #region 帮助方法

    /// <summary>
    /// 自定义分隔方法(Feature可能存在如 a,b,c,"d,e",f 格式的数据,此处不能把双引号中的内容也分隔开)
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string[] CustomSplit(string str)
    {
        var list = new List<string>();
        var item = new List<char>();
        var haveMark = false;
        foreach (var c in str)
            if (c == ',' && !haveMark)
            {
                list.Add(new string(item.ToArray()));
                item.Clear();
            }
            else if (c == '"')
            {
                item.Add(c);
                haveMark = !haveMark;
            }
            else
            {
                item.Add(c);
            }

        return list.ToArray();
    }

    /// <summary>
    /// 모든 발음 가져오기
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private static (ObservableCollection<ReplaceString> replaceHiraganaPron, ObservableCollection<ReplaceString>
        replaceRomajiPron, ObservableCollection<ReplaceString> replaceHiraganaKana, ObservableCollection<ReplaceString>
        reoakceRomajiKana) GetReplaceData(MeCabNode node)
    {
        var length = node.Length;
        var replaceNodeList = new List<MeCabNode>();

        GetAllReplaceNode(replaceNodeList, node);

        void GetAllReplaceNode(List<MeCabNode> list, MeCabNode node)
        {
            if (node != null && !list.Contains(node) && node.Length == length)
            {
                list.Add(node);
                GetAllReplaceNode(list, node.BNext);
                GetAllReplaceNode(list, node.ENext);
            }
        }

        var replaceHiraganaPron = new ObservableCollection<ReplaceString>();
        var replaceRomajiPron = new ObservableCollection<ReplaceString>();
        var replaceHiraganaKana = new ObservableCollection<ReplaceString>();
        var replaceRomajiKana = new ObservableCollection<ReplaceString>();

        ushort i = 1;
        foreach (var meCabNode in replaceNodeList.DistinctBy(GetPron))
        {
            var pron = GetPron(meCabNode);
            var kana = GetKana(meCabNode);
            if (kana != null)
            {
                replaceHiraganaPron.Add(new ReplaceString(i, KanaHelper.ToHiragana(pron), true));
                replaceRomajiPron.Add(new ReplaceString(i, KanaHelper.KatakanaToRomaji(pron), true));
                replaceHiraganaKana.Add(new ReplaceString(i, KanaHelper.ToHiragana(kana), true));
                replaceRomajiKana.Add(new ReplaceString(i, KanaHelper.KatakanaToRomaji(kana), true));
                i++;
            }
        }

        return (replaceHiraganaPron, replaceRomajiPron, replaceHiraganaKana, replaceRomajiKana);
    }

    private static string GetKana(MeCabNode node)
    {
        return node.GetPos1() == "助詞" ? node.GetPron() : node.GetKana();
    }

    private static string GetPron(MeCabNode node)
    {
        return node.GetPron();
    }

    /// <summary>
    /// 自定义转换规则
    /// </summary>
    /// <param name="str"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private static bool TryCustomConvert(string str, out string result)
    {
        if (_customizeDict.ContainsKey(str))
        {
            result = _customizeDict[str];
            return true;
        }

        result = "";
        return false;
    }

    /// <summary>
    /// 判断字符串(句子)是否简体中文
    /// </summary>
    /// <param name="str"></param>
    /// <param name="rate">容错率(0-1)</param>
    /// <returns></returns>
    private static bool IsChinese(string str, float rate)
    {
        if (str.Length < 2)
            return false;

        var wordArray = str.ToCharArray();
        var total = wordArray.Length;
        var chCount = 0f;
        var enCount = 0f;

        foreach (var word in wordArray)
        {
            if (word != 'ー' && IsJapanese(word.ToString()))
                //含有日文直接返回否
                return false;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var gbBytes = Encoding.Unicode.GetBytes(word.ToString());

            if (gbBytes.Length == 2) // double bytes char.  
            {
                if (gbBytes[1] >= 0x4E && gbBytes[1] <= 0x9F) //中文
                    chCount++;
                else
                    total--;
            }
            else if (gbBytes.Length == 1)
            {
                var byteAscii = int.Parse(gbBytes[0].ToString());
                if ((byteAscii >= 65 && byteAscii <= 90) || (byteAscii >= 97 && byteAscii <= 122)) //英文字母
                    enCount++;
                else
                    total--;
            }
        }

        if (chCount == 0) return false; //一个简体中文都没有

        return (chCount + enCount) / total >= rate;
    }

    /// <summary>
    /// 判断字符串是否全为单字节
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static bool IsEnglish(string str)
    {
        return new Regex("^[\x20-\x7E]+$", RegexOptions.Compiled).IsMatch(str);
    }

    /// <summary>
    /// 判断字符串是否全为假名
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static bool IsJapanese(string str)
    {
        return Regex.IsMatch(str, @"^[\u3040-\u30ff]+$", RegexOptions.Compiled);
    }


    #endregion
}