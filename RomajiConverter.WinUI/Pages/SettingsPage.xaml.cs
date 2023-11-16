using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System.Drawing;
using System.Drawing.Text;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using RomajiConverter.WinUI.Extensions;
using RomajiConverter.WinUI.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomajiConverter.WinUI.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        InitFontFamily();
    }

    private void BackButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        Frame.Navigate(typeof(MainPage), null, new SlideNavigationTransitionInfo
        {
            Effect = SlideNavigationTransitionEffect.FromLeft
        });
    }

    private void InitFontFamily()
    {
        foreach (var font in new InstalledFontCollection().Families)
        {
            FontFamilyComboBox.Items.Add(font.Name);
        }
    }

    private async void ResetButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        var contentDialog = new ContentDialog
        {
            Title = "�Ƿ�ָ�Ĭ�����ã�",
            Content = "���е����ý����ᱻ���档",
            CloseButtonText = "ȡ��",
            PrimaryButtonText = "ȷ��",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.Content.XamlRoot
        };

        var result = await contentDialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            App.Config.ResetSetting();
        }
    }

    #region ��ɫѡȡ

    private void FontColorTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            FontColorPicker.Color = FontColorTextBox.Text.ToDrawingColor().ToWindowsUIColor();
        }
        catch
        {
        }
    }

    private void FontColorTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            FontColorPicker.Color = FontColorTextBox.Text.ToDrawingColor().ToWindowsUIColor();
        }
        finally
        {
            FontColorTextBox.Text = App.Config.FontColor;
        }
    }

    private void BackgroundColorTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            BackgroundColorPicker.Color = BackgroundColorTextBox.Text.ToDrawingColor().ToWindowsUIColor();
        }
        catch
        {
        }
    }

    private void BackgroundColorTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            BackgroundColorPicker.Color = BackgroundColorTextBox.Text.ToDrawingColor().ToWindowsUIColor();
        }
        finally
        {
            BackgroundColorTextBox.Text = App.Config.BackgroundColor;
        }
    }

    #endregion
}