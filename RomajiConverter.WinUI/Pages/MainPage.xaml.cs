using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Newtonsoft.Json;
using RomajiConverter.Core.Models;
using RomajiConverter.WinUI.Dialogs;
using RomajiConverter.WinUI.Helpers;
using RomajiConverter.WinUI.Helpers.LyricsHelpers;
using RomajiConverter.WinUI.Models;
using WinRT.Interop;

namespace RomajiConverter.WinUI.Pages;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
    {
        //提供跨页面操作对象
        MainInputPage.MainEditPage = MainEditPage;
        MainInputPage.MainOutputPage = MainOutputPage;

        MainEditPage.MainOutputPage = MainOutputPage;
    }

    #region 菜单栏

    /// <summary>
    /// 导入网易云歌词
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ImportCloudMusicButton_OnClick(object sender, RoutedEventArgs e)
    {
        ShowLrc(await CloudMusicLyricsHelper.GetLrc(CloudMusicLyricsHelper.GetLastSongId()));
    }

    /// <summary>
    /// 通过链接导入歌词
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ImportUrlButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ImportUrlContentDialog
        {
            XamlRoot = App.MainWindow.Content.XamlRoot
        };
        var dialogResult = await dialog.ShowAsync();

        if (dialog.LrcResult.Count != 0) ShowLrc(dialog.LrcResult);
    }

    /// <summary>
    /// 显示歌词
    /// </summary>
    /// <param name="lrc"></param>
    private void ShowLrc(List<MultilingualLrc> lrc)
    {
        var stringBuilder = new StringBuilder();

        if (lrc.Select(p => p.CLrc).All(p => p.Length == 0))
            // 没有翻译
            foreach (var item in lrc)
                stringBuilder.AppendLine(item.JLrc);
        else
            // 有翻译
            foreach (var item in lrc)
            {
                stringBuilder.AppendLine(item.JLrc);
                stringBuilder.AppendLine(item.CLrc);
            }

        MainInputPage.SetTextBoxText(stringBuilder.ToString());
    }

    /// <summary>
    /// 打开按钮事件
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
                App.ConvertedLineList =
                    JsonConvert.DeserializeObject<List<ConvertedLine>>(await File.ReadAllTextAsync(file.Path));
                MainEditPage.RenderEditPanel();
            }
            catch (JsonSerializationException exception)
            {
                var resourceLoader = ResourceLoader.GetForViewIndependentUse();
                throw new Exception(resourceLoader.GetString("NotValidLyricsFile"), exception);
            }
    }

    /// <summary>
    /// 保存按钮事件
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
            await FileIO.WriteTextAsync(file,
                JsonConvert.SerializeObject(App.ConvertedLineList, Formatting.Indented));
    }

    /// <summary>
    /// 导出图片按钮事件
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
            foreach (var line in App.ConvertedLineList)
            {
                var renderLine = new List<string[]>();
                foreach (var unit in line.Units)
                {
                    var renderUnit = new List<string>();

                    // 로마지 표시 (IsHyphen 상태에 따라 Pron 또는 Kana 선택)
                    if (MainEditPage.ToggleSwitchState.Romaji)
                    {
                        if (MainEditPage.ToggleSwitchState.IsHyphen)
                            renderUnit.Add(unit.RomajiPron);
                        else
                            renderUnit.Add(unit.RomajiKana);
                    }

                    // 히라가나 표시 (IsHyphen 상태에 따라 Pron 또는 Kana 선택)
                    if (MainEditPage.ToggleSwitchState.Hiragana)
                    {
                        if (MainEditPage.ToggleSwitchState.IsOnlyShowKanji)
                        {
                            if (unit.IsKanji)
                            {
                                var hiraganaText = MainEditPage.ToggleSwitchState.IsHyphen ? unit.HiraganaPron : unit.HiraganaKana;
                                renderUnit.Add(hiraganaText);
                            }
                            else
                            {
                                renderUnit.Add(" ");
                            }
                        }
                        else
                        {
                            var hiraganaText = MainEditPage.ToggleSwitchState.IsHyphen ? unit.HiraganaPron : unit.HiraganaKana;
                            renderUnit.Add(hiraganaText);
                        }
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

    /// <summary>
    /// 设置按钮事件
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
        GC.Collect();
    }

    #endregion
}
