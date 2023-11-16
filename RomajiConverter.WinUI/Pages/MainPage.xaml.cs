using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Newtonsoft.Json;
using RomajiConverter.WinUI.Controls;
using RomajiConverter.WinUI.Enums;
using RomajiConverter.WinUI.Helpers;
using RomajiConverter.WinUI.Models;
using WinRT.Interop;
using Microsoft.UI.Xaml.Media.Animation;
using RomajiConverter.WinUI.Extensions;

namespace RomajiConverter.WinUI.Pages;

public sealed partial class MainPage : Page
{
    /// <summary>
    /// ��ǰ��ת���������
    /// </summary>
    private List<ConvertedLine> _convertedLineList = new();

    public MainPage()
    {
        InitializeComponent();

        EditRomajiCheckBox.Toggled += EditToggleSwitch_OnToggled;
        EditHiraganaCheckBox.Toggled += EditToggleSwitch_OnToggled;
        IsOnlyShowKanjiCheckBox.Toggled += EditToggleSwitch_OnToggled;

        SpaceCheckBox.Toggled += ThirdCheckBox_OnToggled;
        NewLineCheckBox.Toggled += ThirdCheckBox_OnToggled;
        RomajiCheckBox.Toggled += ThirdCheckBox_OnToggled;
        HiraganaCheckBox.Toggled += ThirdCheckBox_OnToggled;
        JPCheckBox.Toggled += ThirdCheckBox_OnToggled;
        KanjiHiraganaCheckBox.Toggled += ThirdCheckBox_OnToggled;
        CHCheckBox.Toggled += ThirdCheckBox_OnToggled;
    }

    #region ������

    /// <summary>
    /// ���������Ƹ��
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ImportCloudMusicButton_OnClick(object sender, RoutedEventArgs e)
    {
        ShowLrc(await CloudMusicHelper.GetLrc(CloudMusicHelper.GetLastSongId()));
    }

    /// <summary>
    /// ��ʾ���
    /// </summary>
    /// <param name="lrc"></param>
    private void ShowLrc(List<ReturnLrc> lrc)
    {
        var stringBuilder = new StringBuilder();
        foreach (var item in lrc)
        {
            stringBuilder.AppendLine(item.JLrc);
            stringBuilder.AppendLine(item.CLrc);
        }

        InputTextBox.Text = stringBuilder.ToString();
    }

    /// <summary>
    /// ת����ť�¼�
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ConvertButton_OnClick(object sender, RoutedEventArgs e)
    {
        _convertedLineList =
            RomajiHelper.ToRomaji(InputTextBox.Text, SpaceCheckBox.IsOn, AutoVariantCheckBox.IsChecked.Value);

        if (App.Config.IsDetailMode)
            RenderEditPanel();
        else
            OutputTextBox.Text = GetResultText();
    }

    #endregion

    #region �༭��

    /// <summary>
    /// ��Ⱦ�༭���
    /// </summary>
    private void RenderEditPanel()
    {
        EditPanel.Children.Clear();
        for (var i = 0; i < _convertedLineList.Count; i++)
        {
            var item = _convertedLineList[i];

            var line = new WrapPanel();
            foreach (var unit in item.Units)
            {
                var group = new EditableLabelGroup(unit);
                group.RomajiVisibility = EditRomajiCheckBox.IsOn ? Visibility.Visible : Visibility.Collapsed;
                if (EditHiraganaCheckBox.IsOn)
                {
                    if (IsOnlyShowKanjiCheckBox.IsOn && group.Unit.IsKanji == false)
                        group.HiraganaVisibility = HiraganaVisibility.Collapsed;
                    else
                        group.HiraganaVisibility = HiraganaVisibility.Visible;
                }
                else
                {
                    group.HiraganaVisibility = HiraganaVisibility.Collapsed;
                }

                line.Children.Add(group);
            }

            EditPanel.Children.Add(line);
            if (item.Units.Any() && i < _convertedLineList.Count - 1)
                EditPanel.Children.Add(new Grid
                {
                    Height = 1,
                    Background = new SolidColorBrush(Color.FromArgb(170, 170, 170, 170)),
                    Margin = new Thickness(4, 4, 4, 4)
                });
        }
    }

    /// <summary>
    /// �༭������¼�(���ڵ����հ��������ı���ʧ��)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EditBorder_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        Focus(FocusState.Programmatic);
    }

    /// <summary>
    /// �༭����ToggleSwitchͨ���¼�
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EditToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        var senderName = ((ToggleSwitch)sender).Name;
        foreach (object children in EditPanel.Children)
        {
            WrapPanel wrapPanel;
            if (children.GetType() == typeof(WrapPanel))
                wrapPanel = (WrapPanel)children;
            else
                continue;

            var isLineContainsKanji = wrapPanel.Children.Any(p => ((EditableLabelGroup)p).Unit.IsKanji);

            foreach (EditableLabelGroup editableLabelGroup in wrapPanel.Children)
                switch (senderName)
                {
                    case "EditRomajiCheckBox":
                        editableLabelGroup.RomajiVisibility =
                            EditRomajiCheckBox.IsOn ? Visibility.Visible : Visibility.Collapsed;
                        break;
                    case "EditHiraganaCheckBox":
                        if (EditHiraganaCheckBox.IsOn)
                            if (IsOnlyShowKanjiCheckBox.IsOn && !editableLabelGroup.Unit.IsKanji)
                                if (isLineContainsKanji)
                                    editableLabelGroup.HiraganaVisibility = HiraganaVisibility.Hidden;
                                else
                                    editableLabelGroup.HiraganaVisibility = HiraganaVisibility.Collapsed;
                            else
                                editableLabelGroup.HiraganaVisibility = HiraganaVisibility.Visible;
                        else
                            editableLabelGroup.HiraganaVisibility = HiraganaVisibility.Collapsed;
                        break;
                    case "IsOnlyShowKanjiCheckBox":
                        if (EditHiraganaCheckBox.IsOn && editableLabelGroup.Unit.IsKanji == false)
                            if (IsOnlyShowKanjiCheckBox.IsOn)
                                if (isLineContainsKanji)
                                    editableLabelGroup.HiraganaVisibility = HiraganaVisibility.Hidden;
                                else
                                    editableLabelGroup.HiraganaVisibility = HiraganaVisibility.Collapsed;
                            else
                                editableLabelGroup.HiraganaVisibility = HiraganaVisibility.Visible;
                        break;
                }
        }
    }

    /// <summary>
    /// �����ı���ť�¼�
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ConvertTextButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        OutputTextBox.Text = GetResultText();
    }

    /// <summary>
    /// �򿪰�ť�¼�
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private async void ReadButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var fileOpenPicker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        fileOpenPicker.FileTypeFilter.Add(".json");

        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(fileOpenPicker, hwnd);

        var file = await fileOpenPicker.PickSingleFileAsync();
        if (file != null)
            try
            {
                _convertedLineList =
                    JsonConvert.DeserializeObject<List<ConvertedLine>>(await File.ReadAllTextAsync(file.Path));
                RenderEditPanel();
            }
            catch (JsonSerializationException exception)
            {
                var resourceLoader = ResourceLoader.GetForViewIndependentUse();
                throw new Exception(resourceLoader.GetString("NotValidLyricsFile"), exception);
            }
    }

    /// <summary>
    /// ���水ť�¼�
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var fileSavePicker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        fileSavePicker.FileTypeChoices.Add("json", new List<string> { ".json" });

        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(fileSavePicker, hwnd);

        var file = await fileSavePicker.PickSaveFileAsync();
        if (file != null)
        {
            CachedFileManager.DeferUpdates(file);
            await FileIO.WriteTextAsync(file,
                JsonConvert.SerializeObject(_convertedLineList, Formatting.Indented));
            await CachedFileManager.CompleteUpdatesAsync(file);
        }
    }

    /// <summary>
    /// ����ͼƬ��ť�¼�
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ConvertPictureButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var fileSavePicker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };
        fileSavePicker.FileTypeChoices.Add("png", new List<string> { ".png" });

        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(fileSavePicker, hwnd);

        var file = await fileSavePicker.PickSaveFileAsync();
        if (file != null)
        {
            var renderData = new List<string[][]>();
            foreach (var line in _convertedLineList)
            {
                var renderLine = new List<string[]>();
                foreach (var unit in line.Units)
                {
                    var renderUnit = new List<string>();
                    if (EditRomajiCheckBox.IsOn)
                        renderUnit.Add(unit.Romaji);
                    if (EditHiraganaCheckBox.IsOn)
                    {
                        if (IsOnlyShowKanjiCheckBox.IsOn)
                            renderUnit.Add(unit.IsKanji ? unit.Hiragana : " ");
                        else
                            renderUnit.Add(unit.Hiragana);
                    }

                    renderUnit.Add(unit.Japanese);
                    renderLine.Add(renderUnit.ToArray());
                }

                renderData.Add(renderLine.ToArray());
            }

            using var image = renderData.ToImage(new GenerateImageHelper.ImageSetting(App.Config));
            image.Save(file.Path, ImageFormat.Png);
            if (App.Config.IsOpenExplorerAfterSaveImage)
                Process.Start("explorer.exe", $"/select,\"{file.Path}\"");
        }
    }

    #endregion

    #region �����ı���

    /// <summary>
    /// �����ı�
    /// </summary>
    /// <returns></returns>
    private string GetResultText()
    {
        string GetString(IEnumerable<string> array)
        {
            return string.Join(SpaceCheckBox.IsOn ? " " : "", array);
        }

        var output = new StringBuilder();
        for (var i = 0; i < _convertedLineList.Count; i++)
        {
            var item = _convertedLineList[i];
            if (RomajiCheckBox.IsOn)
                output.AppendLine(GetString(item.Units.Select(p => p.Romaji)));
            if (HiraganaCheckBox.IsOn)
                output.AppendLine(GetString(item.Units.Select(p => p.Hiragana)));
            if (JPCheckBox.IsOn)
            {
                if (KanjiHiraganaCheckBox.IsOn)
                {
                    var japanese = item.Japanese;
                    var leftParenthesis = App.Config.LeftParenthesis;
                    var rightParenthesis = App.Config.RightParenthesis;

                    var kanjiUnitList = item.Units.Where(p => p.IsKanji);
                    foreach (var kanjiUnit in kanjiUnitList)
                    {
                        var kanjiIndex = japanese.IndexOf(kanjiUnit.Japanese);
                        var hiraganaIndex = kanjiIndex + kanjiUnit.Japanese.Length;
                        japanese = japanese.Insert(hiraganaIndex,
                            $"{leftParenthesis}{kanjiUnit.Hiragana}{rightParenthesis}");
                    }

                    output.AppendLine(japanese);
                }
                else
                {
                    output.AppendLine(item.Japanese);
                }
            }

            if (CHCheckBox.IsOn && !string.IsNullOrWhiteSpace(item.Chinese))
                output.AppendLine(item.Chinese);
            if (NewLineCheckBox.IsOn && i < _convertedLineList.Count - 1)
                output.AppendLine();
        }

        if (_convertedLineList.Any())
            output.Remove(output.Length - Environment.NewLine.Length, Environment.NewLine.Length);
        return output.ToString();
    }

    /// <summary>
    /// �����ı�����ToggleSwitchͨ���¼�
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ThirdCheckBox_OnToggled(object sender, RoutedEventArgs e)
    {
        OutputTextBox.Text = GetResultText();
    }

    /// <summary>
    /// ���ư�ť�¼�
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CopyButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(OutputTextBox.Text);
        Clipboard.SetContent(dataPackage);
    }

    #endregion

    #region �л�ҳ��

    /// <summary>
    /// ���ð�ť�¼�
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void SettingButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        Frame.Navigate(typeof(SettingsPage), null, new SlideNavigationTransitionInfo
        {
            Effect = SlideNavigationTransitionEffect.FromRight
        });
    }

    #endregion

    private void InputTextBox_OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var pointer = e.GetCurrentPoint((UIElement)sender);
        if (pointer.PointerDeviceType != PointerDeviceType.Mouse || KeyboardExtension.IsKeyDown(VirtualKey.Control))
        {
            if (pointer.Properties.MouseWheelDelta < 0 && InputTextBox.FontSize > 14 / Math.Pow(1.1, 16))
            {
                InputTextBox.FontSize /= 1.1;
            }
            else if (pointer.Properties.MouseWheelDelta > 0 && InputTextBox.FontSize < Math.Floor(14 * Math.Pow(1.1, 14)))
            {
                InputTextBox.FontSize *= 1.1;
            }
            e.Handled = true;
        }
    }
}