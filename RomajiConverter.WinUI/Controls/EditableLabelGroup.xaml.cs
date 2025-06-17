using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RomajiConverter.Core.Models;
using RomajiConverter.WinUI.Enums;

namespace RomajiConverter.WinUI.Controls;

public sealed partial class EditableLabelGroup : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(nameof(Unit),
        typeof(ConvertedUnit),
        typeof(EditableLabelGroup), new PropertyMetadata(null));

    public static readonly DependencyProperty RomajiPronVisibilityProperty = DependencyProperty.Register(
        nameof(RomajiPronVisibility),
        typeof(Visibility), typeof(EditableLabelGroup), new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty RomajiKanaVisibilityProperty = DependencyProperty.Register(
        nameof(RomajiKanaVisibility),
        typeof(Visibility), typeof(EditableLabelGroup), new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty HiraganaVisibilityProperty =
        DependencyProperty.Register(nameof(HiraganaVisibility), typeof(HiraganaVisibility), typeof(EditableLabelGroup),
            new PropertyMetadata(HiraganaVisibility.Collapsed));

    public static readonly DependencyProperty MyFontSizeProperty = DependencyProperty.Register(nameof(MyFontSize),
        typeof(double), typeof(EditableLabelGroup), new PropertyMetadata(14d));

    public static readonly DependencyProperty BorderVisibilitySettingProperty =
        DependencyProperty.Register(nameof(BorderVisibilitySetting), typeof(BorderVisibilitySetting),
            typeof(EditableLabelGroup),
            new PropertyMetadata(BorderVisibilitySetting.Hidden));

    private ReplaceString _selectedHiragana;

    private ReplaceString _selectedRomajiPron;
    private ReplaceString _selectedRomajiKana;

    public EditableLabelGroup(ConvertedUnit unit)
    {
        InitializeComponent();
        Unit = unit;
        MyFontSize = 14;
        SelectedRomajiPron = Unit.ReplaceRomajiPron.FirstOrDefault(p => p.Id == unit.SelectId) ?? Unit.ReplaceRomajiPron[0];
        SelectedRomajiKana = Unit.ReplaceRomajiKana.FirstOrDefault(p => p.Id == unit.SelectId) ?? Unit.ReplaceRomajiKana[0];
        SelectedHiraganaPron = Unit.ReplaceHiraganaPron.FirstOrDefault(p => p.Id == unit.SelectId) ?? Unit.ReplaceHiraganaPron[0];
        SelectedHiraganaKana = Unit.ReplaceHiraganaKana.FirstOrDefault(p => p.Id == unit.SelectId) ?? Unit.ReplaceHiraganaKana[0];
        BorderVisibilitySetting = BorderVisibilitySetting.Highlight;
    }

    public ConvertedUnit Unit
    {
        get => (ConvertedUnit)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public Visibility RomajiPronVisibility
    {
        get => (Visibility)GetValue(RomajiPronVisibilityProperty);
        set
        {
            switch (value)
            {
                case Visibility.Visible:
                    RomajiPronLabel.IsEnabled = true;
                    RomajiPronLabel.Opacity = 1;
                    RomajiPronLabel.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:
                    RomajiPronLabel.IsEnabled = false;
                    RomajiPronLabel.Opacity = 0;
                    RomajiPronLabel.Visibility = Visibility.Collapsed;
                    break;
            }

            SetValue(RomajiPronVisibilityProperty, value);
        }
    }

    public Visibility RomajiKanaVisibility
    {
        get => (Visibility)GetValue(RomajiKanaVisibilityProperty);
        set
        {
            switch (value)
            {
                case Visibility.Visible:
                    RomajiKanaLabel.IsEnabled = true;
                    RomajiKanaLabel.Opacity = 1;
                    RomajiKanaLabel.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:
                    RomajiKanaLabel.IsEnabled = false;
                    RomajiKanaLabel.Opacity = 0;
                    RomajiKanaLabel.Visibility = Visibility.Collapsed;
                    break;
            }
            SetValue(RomajiKanaVisibilityProperty, value);
        }
    }

    public HiraganaVisibility HiraganaVisibility
    {
        get => (HiraganaVisibility)GetValue(HiraganaVisibilityProperty);
        set
        {
            switch (value)
            {
                case HiraganaVisibility.Visible:
                    HiraganaLabel.IsEnabled = true;
                    HiraganaLabel.Opacity = 1;
                    HiraganaLabel.Visibility = Visibility.Visible;
                    break;
                case HiraganaVisibility.Collapsed:
                    HiraganaLabel.IsEnabled = false;
                    HiraganaLabel.Opacity = 0;
                    HiraganaLabel.Visibility = Visibility.Collapsed;
                    break;
                case HiraganaVisibility.Hidden:
                    HiraganaLabel.IsEnabled = false;
                    HiraganaLabel.Opacity = 0;
                    HiraganaLabel.Visibility = Visibility.Visible;
                    break;
            }

            SetValue(HiraganaVisibilityProperty, value);
        }
    }

    public double MyFontSize
    {
        get => (double)GetValue(MyFontSizeProperty);
        set => SetValue(MyFontSizeProperty, value);
    }

    public BorderVisibilitySetting BorderVisibilitySetting
    {
        get => (BorderVisibilitySetting)GetValue(BorderVisibilitySettingProperty);
        set => SetValue(BorderVisibilitySettingProperty, value);
    }

    public ReplaceString SelectedRomajiPron
    {
        get => _selectedRomajiPron;
        set
        {
            if (Equals(value, _selectedRomajiPron)) return;
            _selectedRomajiPron = value;
            if ((_selectedRomajiPron?.IsSystem ?? true) && (SelectedHiraganaPron?.IsSystem ?? true))
                SelectedHiraganaPron = Unit.ReplaceHiraganaPron.FirstOrDefault(p => p.Id == _selectedRomajiPron?.Id);
            Unit.RomajiPron = _selectedRomajiPron?.Value ?? string.Empty;
            Unit.SelectId = _selectedRomajiPron?.Id ?? 1;
            OnPropertyChanged();
        }
    }

    public ReplaceString SelectedRomajiKana
    {
        get => _selectedRomajiKana;
        set
        {
            if (Equals(value, _selectedRomajiKana)) return;
            _selectedRomajiKana = value;
            if ((_selectedRomajiKana?.IsSystem ?? true) && (SelectedHiraganaKana?.IsSystem ?? true))
                SelectedHiraganaKana = Unit.ReplaceHiraganaKana.FirstOrDefault(p => p.Id == _selectedRomajiKana?.Id);
            Unit.RomajiKana = _selectedRomajiKana?.Value ?? string.Empty;
            Unit.SelectId = _selectedRomajiKana?.Id ?? 1;
            OnPropertyChanged();
        }
    }

    public ReplaceString SelectedHiraganaPron
    {
        get => _selectedHiragana;
        set
        {
            if (Equals(value, _selectedHiragana)) return;
            _selectedHiragana = value;
            if ((_selectedHiragana?.IsSystem ?? true) && (SelectedRomajiPron?.IsSystem ?? true))
                SelectedRomajiPron = Unit.ReplaceRomajiPron.FirstOrDefault(p => p.Id == _selectedHiragana?.Id);
            Unit.HiraganaPron = _selectedHiragana?.Value ?? string.Empty;
            Unit.SelectId = _selectedHiragana?.Id ?? 1;
            OnPropertyChanged();
        }
    }
    
    public ReplaceString SelectedHiraganaKana
    {
        get => _selectedHiragana;
        set
        {
            if (Equals(value, _selectedHiragana)) return;
            _selectedHiragana = value;
            if ((_selectedHiragana?.IsSystem ?? true) && (SelectedRomajiKana?.IsSystem ?? true))
                SelectedRomajiKana = Unit.ReplaceRomajiKana.FirstOrDefault(p => p.Id == _selectedHiragana?.Id);
            Unit.HiraganaKana = _selectedHiragana?.Value ?? string.Empty;
            Unit.SelectId = _selectedHiragana?.Id ?? 1;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Destroy()
    {
        RomajiPronLabel.Destroy();
        RomajiKanaLabel.Destroy();
        HiraganaLabel.Destroy();
        Bindings.StopTracking();
        ClearValue(UnitProperty);
        ClearValue(RomajiPronVisibilityProperty);
        ClearValue(RomajiKanaVisibilityProperty);
        ClearValue(HiraganaVisibilityProperty);
        ClearValue(MyFontSizeProperty);
    }
}