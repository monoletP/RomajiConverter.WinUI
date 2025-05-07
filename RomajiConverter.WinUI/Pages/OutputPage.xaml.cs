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
        /*
        string GetString(IEnumerable<string> array)
        {
            return string.Join(SpaceCheckBox.IsOn ? " " : "", array);
        }*/
        string GetStringWithParticles(IEnumerable<ConvertedUnit> units)
        {
            if (!SpaceCheckBox.IsOn)
                return string.Join("", units.Select(p => p.Romaji));

            // 공백 처리가 활성화된 경우 조사를 앞 단어와 붙임
            var result = new StringBuilder();
            ConvertedUnit previous = null;

            foreach (var unit in units)
            {
                if (previous == null)
                {
                    result.Append(unit.Romaji);
                }
                else if (unit.Pos1 == "助詞" || unit.Pos1 == "助動詞" || unit.Pos1 == "接尾辞")
                {
                    // 조사 또는 조동사 또는 접미사인 경우 공백 없이 바로 추가
                    result.Append(unit.Romaji);
                }
                else if (unit.Pos2 == "非自立")
                {
                    //비자립일 경우 공백 없이 바로 추가
                    result.Append(unit.Romaji);
                }
                else if (previous.Pos1 != "助詞" && unit.Pos2 == "非自立可能")
                {
                    //앞 품사가 조사가 아니고 비자립가능일 경우 공백 없이 바로 추가
                    result.Append(unit.Romaji);
                }
                else if (previous.Pos1 == "接頭辞")
                {
                    // 앞 형태소가 접두사인 경우 공백 없이 바로 추가
                    result.Append(unit.Romaji);
                }
                else if (previous.Pos2 == "数詞" && (unit.Pos2 == "数詞" || unit.Pos3 == "助数詞可能"))
                {
                    // 앞 형태소가 수사이고 현재 형태소가 수사 또는 조수사 가능일 경우 공백 없이 바로 추가
                    result.Append(unit.Romaji);
                }
                else
                {
                    // 그 이외 경우 공백 삽입
                    result.Append(" ").Append(unit.Romaji);
                }

                previous = unit;
            }

            var resultString = result.ToString();

            // 라인의 첫 번째 문자가 공백이면 제거
            if (resultString.StartsWith(" "))
            {
                resultString = resultString.TrimStart();
            }

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
                        japanese = japanese.Insert(hiraganaIndex,
                            $"{leftParenthesis}{kanjiUnit.Hiragana}{rightParenthesis}");
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
                //output.AppendLine(GetString(item.Units.Select(p => p.Hiragana)));
                output.AppendLine(GetStringWithParticles(item.Units.Select(p =>
                new ConvertedUnit(p.Japanese, p.Hiragana, p.Hiragana, p.IsKanji, p.Pos1, p.Pos2, p.Pos3))));
            }

            if (RomajiCheckBox.IsOn)
            {
                //output.AppendLine(GetString(item.Units.Select(p => p.Romaji)));
                output.AppendLine(GetStringWithParticles(item.Units));
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