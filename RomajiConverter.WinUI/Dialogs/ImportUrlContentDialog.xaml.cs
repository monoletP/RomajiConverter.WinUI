using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Web;
using Windows.ApplicationModel.Resources;
using Microsoft.UI.Xaml.Controls;
using RomajiConverter.WinUI.Helpers.LyricsHelpers;
using RomajiConverter.WinUI.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomajiConverter.WinUI.Dialogs;

public sealed partial class ImportUrlContentDialog : ContentDialog, INotifyPropertyChanged
{
    private string _errorText;

    private string _url;

    public ImportUrlContentDialog()
    {
        InitializeComponent();
        Url = string.Empty;
        ErrorText = string.Empty;

        PrimaryButtonClick += OnPrimaryButtonClick;
        Closing += OnClosing;
    }

    public string Url
    {
        get => _url;
        set
        {
            if (value == _url) return;
            _url = value;
            OnPropertyChanged();
        }
    }

    public string ErrorText
    {
        get => _errorText;
        set
        {
            if (value == _errorText) return;
            _errorText = value;
            OnPropertyChanged();
        }
    }

    public List<MultilingualLrc> LrcResult { get; set; } = new();

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void TextBox_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
    {
        ErrorText = string.Empty;
    }

    private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        args.Cancel = args.Result == ContentDialogResult.Primary;
    }

    /*
     * GetLrc������ʱ,���¹رմ���ʱLrcResult��Ϊ��
     * �������:����PrimaryButton��Close�߼�,�ֶ���OnPrimaryButtonClick�����йرմ���
     *
     * ����Hide�޷�ָ��ContentDialogResult,�����������ContentDialogResult����ΪNone
     * ���MainPage��Ҫ�ж�LrcResult�Ƿ�Ϊ��,����������Ⱦ���
     */
    private async void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var url = Url;

        try
        {
            if (url.Contains("music.163.com"))
            {
                var songId = HttpUtility.ParseQueryString(new Uri(url).Query)["id"];

                LrcResult = await CloudMusicLyricsHelper.GetLrc(songId);
            }
            else if (url.Contains("kugou.com"))
            {
                LrcResult = await KuGouMusicLyricsHelper.GetLrc(url);
            }
            else if (url.Contains("y.qq.com"))
            {
                LrcResult = await QQMusicLyricsHelper.GetLrc(url);
            }
            else
            {
                throw new Exception(ResourceLoader.GetForViewIndependentUse().GetString("InvalidUrl"));
            }
        }
        catch (Exception e)
        {
            ErrorText = e.Message;
            return;
        }

        Hide();
    }
}