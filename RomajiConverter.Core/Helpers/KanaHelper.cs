using System.Globalization;
using System.Text;

namespace RomajiConverter.Core.Helpers;

/// <summary>
/// 此类用于片假、平假互转
/// </summary>
public static class KanaHelper
{
    /// <summary>
    /// 转为片假名
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToKatakana(string str)
    {
        var stringBuilder = new StringBuilder();
        foreach (var c in str)
        {
            var bytes = Encoding.Unicode.GetBytes(c.ToString());
            if (bytes.Length == 2 && bytes[1] == 0x30 && bytes[0] >= 0x40 && bytes[0] <= 0x9F)
                stringBuilder.Append(Encoding.Unicode.GetString(new[] { (byte)(bytes[0] + 0x60), bytes[1] }));
            else
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 转为平假名
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToHiragana(string str)
    {
        var stringBuilder = new StringBuilder();
        foreach (var c in str)
        {
            var bytes = Encoding.Unicode.GetBytes(c.ToString());
            if (bytes.Length == 2 && bytes[1] == 0x30 && bytes[0] >= 0xA0 && bytes[0] <= 0xFA)
                stringBuilder.Append(Encoding.Unicode.GetString(new[] { (byte)(bytes[0] - 0x60), bytes[1] }));
            else
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 假名转罗马音
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string KatakanaToRomaji(string str)
    {
        var result = new StringBuilder();

        for (var i = 0; i < str.Length;)
        {
            if (i < str.Length - 1)
            {
                var extendedWord = str.Substring(i, 2);
                if (ExtendedKanaDictionary.ContainsKey(extendedWord))
                {
                    result.Append(ExtendedKanaDictionary[extendedWord]);
                    i += 2;
                    continue;
                }
            }

            var word = str[i].ToString();
            if (KanaDictionary.ContainsKey(word))
            {
                //장음 처리
                if(i > 0 && (KanaDictionary[word] == "아" || KanaDictionary[word] == "이" || KanaDictionary[word] == "우" ||
                    KanaDictionary[word] == "에" || KanaDictionary[word] == "오") 
                    && result.Length > 0 && IsLongsound(result[result.Length - 1], KanaDictionary[word][0]))
                {
                    result.Append("-");
                }
                //일반적인 변환
                else result.Append(KanaDictionary[word]);
            }
            else if (word == "ー")
            {
                //장음 - 처리
                result.Append("-");
            }
            else
            {
                //식별 불가. 그대로 유지
                result.Append(word);
            }

            i++;
        }

        //촉음, n 처리
        for (var i = 0; i < result.Length; i++)
        {
            //촉음
            if (result[i] == 'っ' || result[i] == 'ッ')
            {
                // 현재 위치가 0이거나 이전 문자를 참조할 수 없는 경우
                if (i == 0 || result.Length <= 1)
                {
                    result[i] = 'ㅅ'; // 기본값으로 처리
                    continue;
                }

                int unicodeIndex = result[i - 1] - 0xAC00;
                int jungseong = (unicodeIndex / 28) % 21;
                int choseong = unicodeIndex / (21 * 28);
                int newChar = 0xAC00 + (choseong * 21 * 28) + (jungseong * 28) + 19;
                result[i - 1] = (char)newChar;

                result.Remove(i, 1); // 현재 'っ' 제거
                i--; // 인덱스 조정
            }
            //n
            if (result[i] == 'ん' || result[i] == 'ン')
            {
                // 현재 위치가 0이거나 이전 문자를 참조할 수 없는 경우
                if (i == 0 || result.Length <= 1)
                {
                    result[i] = 'ㄴ'; // 기본값으로 처리
                    continue;
                }

                int unicodeIndex = result[i - 1] - 0xAC00;
                int jungseong = (unicodeIndex / 28) % 21;
                int choseong = unicodeIndex / (21 * 28);
                int newChar = 0xAC00 + (choseong * 21 * 28) + (jungseong * 28) + 4;
                result[i - 1] = (char)newChar;

                result.Remove(i, 1); // 현재 'ん' 제거
                i--; // 인덱스 조정
            }
        }

        return result.ToString();
    }

    //한글이 초성 중성만 있는지 확인하는 메서드
    public static bool IsHangulWithoutBatchim(char ch)
    {
        // 한글 완성형 범위: U+AC00 ~ U+D7A3
        if (ch < 0xAC00 || ch > 0xD7A3)
            return false;

        int unicodeIndex = ch - 0xAC00;
        int jongseongIndex = unicodeIndex % 28;

        // 종성 인덱스가 0이면 받침 없음
        return jongseongIndex == 0;
    }

    //이전 한글 문자와 현재 한글 문자가 같은 중성을 가지면서 현재 한글 문자가 초성이 'ㅇ'인지 확인하는 메서드
    public static bool IsLongsound(char prev, char current)
    {
        // 한글 완성형인지 확인
        if (prev < 0xAC00 || prev > 0xD7A3 || current < 0xAC00 || current > 0xD7A3)
            return false;

        int prevIndex = prev - 0xAC00;
        int currIndex = current - 0xAC00;

        int prevJungseong = (prevIndex % (21 * 28)) / 28;
        int currJungseong = (currIndex % (21 * 28)) / 28;
        int currChoseong = currIndex / (21 * 28);

        // 초성 'ㅇ' 인덱스는 11
        bool isIeung = (currChoseong == 11);

        // 예외 중성 쌍
        bool isSpecialJungseongPair =
            (prevJungseong == 8 && currJungseong == 13) || // ㅗ → ㅜ
            (prevJungseong == 6 && currJungseong == 0) || // ㅑ → ㅏ
            (prevJungseong == 7 && currJungseong == 4) || // ㅕ → ㅓ
            (prevJungseong == 12 && currJungseong == 8) || // ㅛ → ㅗ
            (prevJungseong == 12 && currJungseong == 13) || // ㅛ → ㅜ
            (prevJungseong == 17 && currJungseong == 13);   // ㅠ → ㅜ

        return isIeung && (prevJungseong == currJungseong || isSpecialJungseongPair);
    }

    public static Dictionary<string, string> KanaDictionary = new Dictionary<string, string>
    {
        //히라가나
        { "あ", "아" }, { "い", "이" }, { "う", "우" }, { "え", "에" }, { "お", "오" },
        { "か", "카" }, { "き", "키" }, { "く", "쿠" }, { "け", "케" }, { "こ", "코" },
        { "さ", "사" }, { "し", "시" }, { "す", "스" }, { "せ", "세" }, { "そ", "소" },
        { "た", "타" }, { "ち", "치" }, { "つ", "츠" }, { "て", "테" }, { "と", "토" },
        { "な", "나" }, { "に", "니" }, { "ぬ", "누" }, { "ね", "네" }, { "の", "노" },
        { "は", "하" }, { "ひ", "히" }, { "ふ", "후" }, { "へ", "헤" }, { "ほ", "호" },
        { "ま", "마" }, { "み", "미" }, { "む", "무" }, { "め", "메" }, { "も", "모" },
        { "や", "야" }, { "ゆ", "유" }, { "よ", "요" },
        { "ら", "라" }, { "り", "리" }, { "る", "루" }, { "れ", "레" }, { "ろ", "로" },
        { "わ", "와" }, { "を", "오" },
        { "が", "가" }, { "ぎ", "기" }, { "ぐ", "구" }, { "げ", "게" }, { "ご", "고" },
        { "ざ", "자" }, { "じ", "지" }, { "ず", "즈" }, { "ぜ", "제" }, { "ぞ", "조" },
        { "だ", "다" }, { "ぢ", "지" }, { "づ", "즈" }, { "で", "데" }, { "ど", "도" },
        { "ば", "바" }, { "び", "비" }, { "ぶ", "부" }, { "べ", "베" }, { "ぼ", "보" },
        { "ぱ", "파" }, { "ぴ", "피" }, { "ぷ", "푸" }, { "ぺ", "페" }, { "ぽ", "포" },

        //가타가나
        { "ア", "아" }, { "イ", "이" }, { "ウ", "우" }, { "エ", "에" }, { "オ", "오" },
        { "カ", "카" }, { "キ", "키" }, { "ク", "쿠" }, { "ケ", "케" }, { "コ", "코" },
        { "サ", "사" }, { "シ", "시" }, { "ス", "스" }, { "セ", "세" }, { "ソ", "소" },
        { "タ", "타" }, { "チ", "치" }, { "ツ", "츠" }, { "テ", "테" }, { "ト", "토" },
        { "ナ", "나" }, { "ニ", "니" }, { "ヌ", "누" }, { "ネ", "네" }, { "ノ", "노" },
        { "ハ", "하" }, { "ヒ", "히" }, { "フ", "후" }, { "ヘ", "헤" }, { "ホ", "호" },
        { "マ", "마" }, { "ミ", "미" }, { "ム", "무" }, { "メ", "메" }, { "モ", "모" },
        { "ヤ", "야" }, { "ユ", "유" }, { "ヨ", "요" },
        { "ラ", "라" }, { "リ", "리" }, { "ル", "루" }, { "レ", "레" }, { "ロ", "로" },
        { "ワ", "와" }, { "ヲ", "오" },
        { "ガ", "가" }, { "ギ", "기" }, { "グ", "구" }, { "ゲ", "게" }, { "ゴ", "고" },
        { "ザ", "자" }, { "ジ", "지" }, { "ズ", "즈" }, { "ゼ", "제" }, { "ゾ", "조" },
        { "ダ", "다" }, { "ヂ", "지" }, { "ヅ", "즈" }, { "デ", "데" }, { "ド", "도" },
        { "バ", "바" }, { "ビ", "비" }, { "ブ", "부" }, { "ベ", "베" }, { "ボ", "보" },
        { "パ", "파" }, { "ピ", "피" }, { "プ", "푸" }, { "ペ", "페" }, { "ポ", "포" },

        //소가나
        { "ぁ", "아" }, { "ぃ", "이" }, { "ぅ", "우" }, { "ぇ", "에" }, { "ぉ", "오" },
        { "ゃ", "야" }, { "ゅ", "유" }, { "ょ", "요" }, { "ゎ", "와" },
        { "ァ", "아" }, { "ィ", "이" }, { "ゥ", "우" }, { "ェ", "에" }, { "ォ", "오" },
        { "ャ", "야" }, { "ュ", "유" }, { "ョ", "요" }, { "ヮ", "와" },
    };

    public static Dictionary<string, string> ExtendedKanaDictionary = new Dictionary<string, string>
    {
        //히라가나 요음
        { "きゃ", "캬" }, { "きゅ", "큐" }, { "きょ", "쿄" },
        { "しゃ", "샤" }, { "しゅ", "슈" }, { "しょ", "쇼" },
        { "ちゃ", "챠" }, { "ちゅ", "츄" }, { "ちょ", "쵸" },
        { "にゃ", "냐" }, { "にゅ", "뉴" }, { "にょ", "뇨" },
        { "ひゃ", "햐" }, { "ひゅ", "휴" }, { "ひょ", "효" },
        { "みゃ", "먀" }, { "みゅ", "뮤" }, { "みょ", "묘" },
        { "りゃ", "랴" }, { "りゅ", "류" }, { "りょ", "료" },
        { "ぎゃ", "갸" }, { "ぎゅ", "규" }, { "ぎょ", "교" },
        { "じゃ", "자" }, { "じゅ", "주" }, { "じょ", "조" },
        { "ぢゃ", "자" }, { "ぢゅ", "주" }, { "ぢょ", "조" },
        { "びゃ", "뱌" }, { "びゅ", "뷰" }, { "びょ", "뵤" },
        { "ぴゃ", "퍄" }, { "ぴゅ", "퓨" }, { "ぴょ", "표" },

        //가타가나 요음
        { "キャ", "캬" }, { "キュ", "큐" }, { "キョ", "쿄" },
        { "シャ", "샤" }, { "シュ", "슈" }, { "ショ", "쇼" },
        { "チャ", "챠" }, { "チュ", "츄" }, { "チョ", "쵸" },
        { "ニャ", "냐" }, { "ニュ", "뉴" }, { "ニョ", "뇨" },
        { "ヒャ", "햐" }, { "ヒュ", "휴" }, { "ヒョ", "효" },
        { "ミャ", "먀" }, { "ミュ", "뮤" }, { "ミョ", "묘" },
        { "リャ", "랴" }, { "リュ", "류" }, { "リョ", "료" },
        { "ギャ", "갸" }, { "ギュ", "규" }, { "ギョ", "교" },
        { "ジャ", "자" }, { "ジュ", "주" }, { "ジョ", "조" },
        { "ヂャ", "자" }, { "ヂュ", "주" }, { "ヂョ", "조" },
        { "ビャ", "뱌" }, { "ビュ", "뷰" }, { "ビョ", "뵤" },
        { "ピャ", "퍄" }, { "ピュ", "퓨" }, { "ピョ", "표" },

        //그 외 언어들
        { "イェ", "예" },
        { "ウィ", "위" }, { "ウェ", "웨" }, { "ウォ", "워" },
        { "ヴァ", "바" }, { "ヴィ", "비" }, { "ヴ", "부" }, { "ヴェ", "베" }, { "ヴォ", "보" },
        { "ヴュ", "뷰" },
        { "クァ", "콰" }, { "クィ", "퀴" }, { "クェ", "퀘" }, { "クォ", "쿼" },
        { "グァ", "과" },
        { "シェ", "셰" },
        { "ジェ", "제" },
        { "チェ", "체" },
        { "ツァ", "차" }, { "ツィ", "치" }, { "ツェ", "체" }, { "ツォ", "초" },
        { "ティ", "티" }, { "トゥ", "투" },
        { "テュ", "튜" },
        { "ディ", "디" }, { "ドゥ", "두" },
        { "デュ", "듀" },
        { "ファ", "파" }, { "フィ", "피" }, { "フェ", "페" }, { "フォ", "포" },
        { "フュ", "퓨" },
    };
}