using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using RomajiConverter.WinUI.Extensions;
using RomajiConverter.Core.Models;
using System.Threading.Tasks;
using System.Runtime.ConstrainedExecution;

namespace RomajiConverter.WinUI.Pages;

public sealed partial class OutputPage : Page
{
    public OutputPage()
    {
        InitializeComponent();

        SpaceCheckBox.Toggled += ThirdCheckBox_OnToggled;
        NewLineCheckBox.Toggled += ThirdCheckBox_OnToggled;
        RomajiCheckBox.Toggled += ThirdCheckBox_OnToggled;
        HiraganaCheckBox.Toggled += ThirdCheckBox_OnToggled;
        IsHyphenCheckBox.Toggled += ThirdCheckBox_OnToggled;
        JPCheckBox.Toggled += ThirdCheckBox_OnToggled;
        KanjiHiraganaCheckBox.Toggled += ThirdCheckBox_OnToggled;
        CHCheckBox.Toggled += ThirdCheckBox_OnToggled;
    }

    /// <summary>
    /// 显示文本
    /// </summary>
    public void RenderText()
    {
        OutputTextBox.Text = GetResultText();
    }

    /// <summary>
    /// 获取结果文本
    /// </summary>
    /// <returns></returns>
    private string GetResultText()
    {
        string GetStringWithParticles(IEnumerable<ConvertedUnit> units, bool isPron, bool isRomaji = true)
        {
            if (isRomaji)
            {
                             // 공백 처리
                var result = new StringBuilder();
                ConvertedUnit previous = null;

                foreach (var unit in units)
                {
                    string unitRomaji = (isPron) ? unit.RomajiPron : unit.RomajiKana;
                    if (previous == null)
                    { }
                    else if (!SpaceCheckBox.IsOn ||
                        //공백 처리가 비활성화되어 있으면 공백 없이 바로 추가
                        unit.Pos1 == "助詞" || unit.Pos1 == "助動詞" || unit.Pos1 == "接尾辞"  ||
                        // 조사 또는 조동사 또는 접미사인 경우 공백 없이 바로 추가
                        unit.Pos2 == "非自立" || 
                        //비자립일 경우 공백 없이 바로 추가
                        (previous.Pos1 != "助詞" && unit.Pos2 == "非自立可能") ||
                        //앞 품사가 조사가 아니고 비자립가능일 경우 공백 없이 바로 추가
                        previous.Pos1 == "接頭辞" || 
                        // 앞 형태소가 접두사인 경우 공백 없이 바로 추가
                        (previous.Pos2 == "数詞" && (unit.Pos2 == "数詞" || unit.Pos3 == "助数詞可能"))
                        // 앞 형태소가 수사이고 현재 형태소가 수사 또는 조수사 가능일 경우 공백 없이 바로 추가
                        )
                    {
                        //바로 앞 글자의 응, 촉음 발음 세분화 
                        if(result.Length > 1 && unitRomaji.Length > 0)
                        {
                            if (nnCheckBox.IsOn)
                                result[result.Length - 1] = Clarifynn(result[result.Length - 1], unitRomaji[0]);
                            if (xtsCheckBox.IsOn)
                                result[result.Length - 1] = Clarifyxts(result[result.Length - 1], unitRomaji[0]);
                        }

                    }
                    else
                    {
                        //바로 앞 글자의 응 발음 ㅇ으로
                        if (previous != null && result.Length > 0 && nnCheckBox.IsOn)
                            result[result.Length - 1] = Clarifynn(result[result.Length - 1], '아');
                        // 그 이외 경우 공백 삽입
                        result.Append(" ");
                    }


                    //응 발음 세분화
                    if (unitRomaji.Length > 1 && unit.Pos2 != "固有名詞")
                    {
                        string unitRomajiFinal = "";
                        for(int i = 0; i < unitRomaji.Length - 1; i++)
                        {
                            char curRomaji = unitRomaji[i];
                            if (nnCheckBox.IsOn)
                                curRomaji = Clarifynn(curRomaji, unitRomaji[i + 1]);
                            if (xtsCheckBox.IsOn)
                                curRomaji = Clarifyxts(curRomaji, unitRomaji[i + 1]);
                            unitRomajiFinal += curRomaji;
                        }
                        //마지막 글자 ㄴ 받침일 경우 ㅇ으로
                        if (nnCheckBox.IsOn)
                            unitRomajiFinal += Clarifynn(unitRomaji[unitRomaji.Length - 1], '아');
                        else unitRomajiFinal += unitRomaji[unitRomaji.Length - 1];
                        result.Append(unitRomajiFinal);
                    }
                    //한글자인 경우
                    else if(unitRomaji.Length == 1 && unit.Pos2 != "固有名詞")
                    {
                        if (nnCheckBox.IsOn)
                            result.Append(Clarifynn(unitRomaji[0], '아'));
                    }
                    else result.Append(unitRomaji);

                    previous = unit;
                }

                var resultString = result.ToString();

                // 라인의 첫 번째 문자가 공백이면 제거
                if (resultString.StartsWith(" "))
                {
                    resultString = resultString.TrimStart();
                }

                // 한글 종성 처리 (로마지에만 적용)
                if (resultString.Length > 1)
                {
                    StringBuilder finalResult = new StringBuilder(resultString.Length);
                    char[] singleJamos = new[] { 'ㅅ', 'ㄴ' };

                    for (int i = 0; i < resultString.Length; i++)
                    {
                        char current = resultString[i];

                        // ㅅ 또는 ㄴ이 단독으로 존재하고, 앞 글자가 있는 경우
                        if (i > 0 && singleJamos.Contains(current))
                        {
                            char previous_char = resultString[i - 1];

                            // 앞 글자가 공백인 경우 제거 후 처리
                            if (previous_char == ' ' && i > 1)
                            {
                                previous_char = resultString[i - 2];
                                finalResult.Length--;
                            }

                            //장음 예외 처리
                            bool isHyphen = previous_char == '-';
                            if (isHyphen && i > 1)
                                previous_char = resultString[i - 2];

                            // 앞 글자가 한글 완성형인지 확인 (가-힣 범위: U+AC00-U+D7A3)
                            if (previous_char >= 0xAC00 && previous_char <= 0xD7A3)
                            {
                                // 한글 조합을 위한 계산
                                int unicodeValue = previous_char;
                                int baseCode = 0xAC00;
                                int choseongIndex = isHyphen ? 11 // 장음이면 초성 ㅇ으로
                                    : (unicodeValue - baseCode) / (21 * 28);
                                int jungseongIndex = ((unicodeValue - baseCode) % (21 * 28)) / 28;
                                int jongseongIndex = 0;

                                // ㅅ이면 19번 종성, ㄴ이면 4번 종성으로 설정
                                if (current == 'ㅅ')
                                    jongseongIndex = 19; // ㅅ 종성 인덱스
                                else if (current == 'ㄴ')
                                    jongseongIndex = 4;  // ㄴ 종성 인덱스

                                // 새 문자 생성
                                char newChar = (char)(baseCode + (choseongIndex * 21 * 28) + (jungseongIndex * 28) + jongseongIndex);
                                
                                if (i != resultString.Length - 1)
                                {
                                    //다음 글자와 비교해 응, 촉음 발음 세분화
                                    if (nnCheckBox.IsOn)
                                        newChar = Clarifynn(newChar, resultString[i + 1]);
                                    if (xtsCheckBox.IsOn)
                                        newChar = Clarifyxts(newChar, resultString[i + 1]);
                                }

                                // 결과에 추가 (이전 글자는 제거, 새 글자 추가)
                                finalResult.Length--; // 마지막 글자 제거
                                finalResult.Append(newChar);

                                continue; // 현재 문자는 처리 완료했으므로 건너뛰기
                            }
                        }

                        // 일반 문자는 그대로 추가
                        finalResult.Append(current);
                    }

                    resultString = finalResult.ToString();
                }

                return resultString;
            }
            else
            {
                // 히라가나인 경우 공백 처리 없이 단순 연결
                return string.Join("", units.Select(p => (isPron) ? p.HiraganaPron : p.HiraganaKana));
            }
        }

        var output = new StringBuilder();
        for (var i = 0; i < App.ConvertedLineList.Count; i++)
        {
            var item = App.ConvertedLineList[i];
            if (JPCheckBox.IsOn)
            {
                if (KanjiHiraganaCheckBox.IsOn)
                {
                    var japanese = item.Japanese;
                    var leftParenthesis = App.Config.LeftParenthesis;
                    var rightParenthesis = App.Config.RightParenthesis;

                    var kanjiUnitList = item.Units.Where(p => p.IsKanji);
                    var replacedIndex = 0;
                    foreach (var kanjiUnit in kanjiUnitList)
                    {
                        var kanjiIndex = japanese.IndexOf(kanjiUnit.Japanese, replacedIndex);
                        var hiraganaIndex = kanjiIndex + kanjiUnit.Japanese.Length;
                        // IsHyphenCheckBox 상태에 따라 Pron 또는 Kana 사용
                        var hiraganaText = IsHyphenCheckBox.IsOn ? kanjiUnit.HiraganaPron : kanjiUnit.HiraganaKana;
                        japanese = japanese.Insert(hiraganaIndex,
                            $"{leftParenthesis}{hiraganaText}{rightParenthesis}");
                        replacedIndex = hiraganaIndex;
                    }

                    output.AppendLine(japanese);
                }
                else
                {
                    output.AppendLine(item.Japanese);
                }
            }

            if (HiraganaCheckBox.IsOn)
            {
                // IsHyphenCheckBox 상태에 따라 HiraganaPron 또는 HiraganaKana 사용
                bool useHiraganaPron = IsHyphenCheckBox.IsOn;
                output.AppendLine(GetStringWithParticles(item.Units, useHiraganaPron, false));
            }

            if (RomajiCheckBox.IsOn)
            {
                // IsHyphenCheckBox 상태에 따라 RomajiPron 또는 RomajiKana 사용
                bool useRomajiPron = IsHyphenCheckBox.IsOn;
                output.AppendLine(GetStringWithParticles(item.Units, useRomajiPron, true));
            }

            if (CHCheckBox.IsOn && !string.IsNullOrWhiteSpace(item.Chinese))
                output.AppendLine(item.Chinese);
            if (NewLineCheckBox.IsOn && i < App.ConvertedLineList.Count - 1)
                output.AppendLine();
        }

        if (App.ConvertedLineList.Any())
            output.Remove(output.Length - Environment.NewLine.Length, Environment.NewLine.Length);
        return output.ToString();
    }

    private char Clarifynn(char previous, char cur)
    {
        if (previous < 0xAC00 || previous > 0xD7A3)
            return previous;
        int unicodeValue = previous;
        int baseCode = 0xAC00;
        int choseongIndex = (unicodeValue - baseCode) / (21 * 28);
        int jungseongIndex = ((unicodeValue - baseCode) % (21 * 28)) / 28;
        int jongseongIndex = (unicodeValue - baseCode) % 28;

        if (jongseongIndex == 4 || jongseongIndex == 21 || jongseongIndex == 16) //종성이 ㄴ, ㅇ, ㅁ
        {
            if (cur == ' ')
            {
                //cur 글자가 공백일 경우
                //종성을 ㅇ으로 변환
                jongseongIndex = 21;
            }
            else if (cur >= 0xAC00 && cur <= 0xD7A3)
            {
                int nextChoseongIndex = (cur - baseCode) / (21 * 28);
                if (nextChoseongIndex == 0 || nextChoseongIndex == 15 || nextChoseongIndex == 11)
                {
                    //cur 글자의 초성이 ㄱ, ㅋ, ㅇ인 경우
                    //종성을 ㅇ으로 변환
                    jongseongIndex = 21;
                }
                else if (nextChoseongIndex == 6 || nextChoseongIndex == 7 || nextChoseongIndex == 17)
                {
                    //cur 글자의 초성이 ㅁ, ㅂ, ㅍ인 경우
                    //종성을 ㅁ으로 변환
                    jongseongIndex = 16;
                }
                else
                {
                    //그 이외는 ㄴ으로
                    jongseongIndex = 4;
                }
            }
        }

        char ret = (char)(baseCode + (choseongIndex * 21 * 28) + (jungseongIndex * 28) + jongseongIndex);
        return ret;
    }

    private char Clarifyxts(char previous, char cur)
    {
        if (previous < 0xAC00 || previous > 0xD7A3)
            return previous;
        int unicodeValue = previous;
        int baseCode = 0xAC00;
        int choseongIndex = (unicodeValue - baseCode) / (21 * 28);
        int jungseongIndex = ((unicodeValue - baseCode) % (21 * 28)) / 28;
        int jongseongIndex = (unicodeValue - baseCode) % 28;

        if (jongseongIndex == 19 || jongseongIndex == 1 || jongseongIndex == 7 || jongseongIndex == 17 || jongseongIndex == 8) //종성이 ㅅ, ㄱ, ㄷ, ㅂ, ㄹ
        {
            if (cur == ' ')
            {
                //cur 글자가 공백일 경우 무시
            }
            else if (cur >= 0xAC00 && cur <= 0xD7A3)
            {
                int nextChoseongIndex = (cur - baseCode) / (21 * 28);
                if (nextChoseongIndex == 15)
                {
                    //cur 글자의 초성이 ㅋ인 경우
                    //종성을 ㄱ으로 변환
                    jongseongIndex = 1;
                }
                else if (nextChoseongIndex == 16)
                {
                    //cur 글자의 초성이 ㅌ인 경우
                    //종성을 ㄷ으로 변환
                    jongseongIndex = 7;
                }
                else if (nextChoseongIndex == 17)
                {
                    //cur 글자의 초성이 ㅍ인 경우
                    //종성을 ㅂ로 변환
                    jongseongIndex = 17;
                }
                else if (nextChoseongIndex == 5)
                {
                    //cur 글자의 초성이 ㄹ인 경우
                    //종성을 ㄹ로 변환
                    jongseongIndex = 8;
                }
                else
                {
                    //그 외는 ㅅ으로
                    jongseongIndex = 19;
                }
            }
        }

        char ret = (char)(baseCode + (choseongIndex * 21 * 28) + (jungseongIndex * 28) + jongseongIndex);
        return ret;
    }

    /// <summary>
    /// 生成文本区的ToggleSwitch通用事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ThirdCheckBox_OnToggled(object sender, RoutedEventArgs e)
    {
        OutputTextBox.Text = GetResultText();
    }

    /// <summary>
    /// 复制按钮事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CopyButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        try
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(OutputTextBox.Text);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"클립보드 복사 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 滚动事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OutputTextBox_OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var pointer = e.GetCurrentPoint((UIElement)sender);
        if (pointer.PointerDeviceType != PointerDeviceType.Mouse || KeyboardExtension.IsKeyDown(VirtualKey.Control))
        {
            if (pointer.Properties.MouseWheelDelta < 0 && App.Config.OutputTextBoxFontSize > 3.047)
                App.Config.OutputTextBoxFontSize /= 1.1;
            else if (pointer.Properties.MouseWheelDelta > 0 && App.Config.OutputTextBoxFontSize < 53.1)
                App.Config.OutputTextBoxFontSize *= 1.1;
            e.Handled = true;
        }
    }
}

