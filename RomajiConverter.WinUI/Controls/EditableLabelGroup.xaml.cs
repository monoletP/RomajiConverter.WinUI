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

    public static readonly DependencyProperty RomajiVisibilityProperty = DependencyProperty.Register(
        nameof(RomajiVisibility),
        typeof(Visibility), typeof(EditableLabelGroup), new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty HiraganaVisibilityProperty = DependencyProperty.Register(
        nameof(HiraganaVisibility),
        typeof(Visibility), typeof(EditableLabelGroup), new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty IsHyphenProperty = DependencyProperty.Register(
        nameof(IsHyphen),
        typeof(bool), typeof(EditableLabelGroup), new PropertyMetadata(false, OnIsHyphenChanged));

    public static readonly DependencyProperty MyFontSizeProperty = DependencyProperty.Register(nameof(MyFontSize),
        typeof(double), typeof(EditableLabelGroup), new PropertyMetadata(14d));

    public static readonly DependencyProperty BorderVisibilitySettingProperty =
        DependencyProperty.Register(nameof(BorderVisibilitySetting), typeof(BorderVisibilitySetting),
            typeof(EditableLabelGroup),
            new PropertyMetadata(BorderVisibilitySetting.Hidden));

    private ReplaceString _selectedHiraganaPron;
    private ReplaceString _selectedHiraganaKana;
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
        UpdateRomajiVisibility();
        UpdateHiraganaVisibility();
    }

    public ConvertedUnit Unit
    {
        get => (ConvertedUnit)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public Visibility RomajiVisibility
    {
        get => (Visibility)GetValue(RomajiVisibilityProperty);
        set
        {
            SetValue(RomajiVisibilityProperty, value);
            UpdateRomajiVisibility();
        }
    }

    public Visibility HiraganaVisibility
    {
        get => (Visibility)GetValue(HiraganaVisibilityProperty);
        set
        {
            SetValue(HiraganaVisibilityProperty, value);
            UpdateHiraganaVisibility();
        }
    }

    public bool IsHyphen
    {
        get => (bool)GetValue(IsHyphenProperty);
        set => SetValue(IsHyphenProperty, value);
    }

    private static void OnIsHyphenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EditableLabelGroup group)
        {
            group.UpdateRomajiVisibility();
            group.UpdateHiraganaVisibility();

            // IsHyphen 변경 시 현재 선택된 항목들을 동기화
            group.SyncSelectionsOnHyphenChange();
        }
    }

    /// <summary>
    /// IsHyphen 변경 시 선택된 항목들을 동기화
    /// </summary>
    private void SyncSelectionsOnHyphenChange()
    {
        // 현재 표시되고 있는 항목의 SelectId를 기준으로 다른 항목들을 동기화
        var currentSelectId = Unit.SelectId;

        // 모든 항목들을 현재 SelectId에 맞춰 동기화
        var romajiPron = Unit.ReplaceRomajiPron.FirstOrDefault(p => p.Id == currentSelectId);
        var romajiKana = Unit.ReplaceRomajiKana.FirstOrDefault(p => p.Id == currentSelectId);
        var hiraganaPron = Unit.ReplaceHiraganaPron.FirstOrDefault(p => p.Id == currentSelectId);
        var hiraganaKana = Unit.ReplaceHiraganaKana.FirstOrDefault(p => p.Id == currentSelectId);

        if (romajiPron != null)
        {
            _selectedRomajiPron = romajiPron;
            Unit.RomajiPron = romajiPron.Value;
        }

        if (romajiKana != null)
        {
            _selectedRomajiKana = romajiKana;
            Unit.RomajiKana = romajiKana.Value;
        }

        if (hiraganaPron != null)
        {
            _selectedHiraganaPron = hiraganaPron;
            Unit.HiraganaPron = hiraganaPron.Value;
        }

        if (hiraganaKana != null)
        {
            _selectedHiraganaKana = hiraganaKana;
            Unit.HiraganaKana = hiraganaKana.Value;
        }

        // PropertyChanged 이벤트 발생
        OnPropertyChanged(nameof(SelectedRomajiPron));
        OnPropertyChanged(nameof(SelectedRomajiKana));
        OnPropertyChanged(nameof(SelectedHiraganaPron));
        OnPropertyChanged(nameof(SelectedHiraganaKana));
    }

    private void UpdateRomajiVisibility()
    {
        if (RomajiVisibility == Visibility.Visible)
        {
            if (IsHyphen)
            {
                // RomajiPron 표시
                RomajiPronLabel.IsEnabled = true;
                RomajiPronLabel.Opacity = 1;
                RomajiPronLabel.Visibility = Visibility.Visible;

                // RomajiKana 숨김
                RomajiKanaLabel.IsEnabled = false;
                RomajiKanaLabel.Opacity = 0;
                RomajiKanaLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                // RomajiKana 표시
                RomajiKanaLabel.IsEnabled = true;
                RomajiKanaLabel.Opacity = 1;
                RomajiKanaLabel.Visibility = Visibility.Visible;

                // RomajiPron 숨김
                RomajiPronLabel.IsEnabled = false;
                RomajiPronLabel.Opacity = 0;
                RomajiPronLabel.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            // 둘 다 숨김
            RomajiPronLabel.IsEnabled = false;
            RomajiPronLabel.Opacity = 0;
            RomajiPronLabel.Visibility = Visibility.Collapsed;

            RomajiKanaLabel.IsEnabled = false;
            RomajiKanaLabel.Opacity = 0;
            RomajiKanaLabel.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateHiraganaVisibility()
    {
        if (HiraganaVisibility == Visibility.Visible)
        {
            if (IsHyphen)
            {
                // HiraganaPron 표시
                HiraganaPronLabel.IsEnabled = true;
                HiraganaPronLabel.Opacity = 1;
                HiraganaPronLabel.Visibility = Visibility.Visible;

                // HiraganaKana 숨김
                HiraganaKanaLabel.IsEnabled = false;
                HiraganaKanaLabel.Opacity = 0;
                HiraganaKanaLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                // HiraganaKana 표시
                HiraganaKanaLabel.IsEnabled = true;
                HiraganaKanaLabel.Opacity = 1;
                HiraganaKanaLabel.Visibility = Visibility.Visible;

                // HiraganaPron 숨김
                HiraganaPronLabel.IsEnabled = false;
                HiraganaPronLabel.Opacity = 0;
                HiraganaPronLabel.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            // 둘 다 숨김
            HiraganaPronLabel.IsEnabled = false;
            HiraganaPronLabel.Opacity = 0;
            HiraganaPronLabel.Visibility = Visibility.Collapsed;

            HiraganaKanaLabel.IsEnabled = false;
            HiraganaKanaLabel.Opacity = 0;
            HiraganaKanaLabel.Visibility = Visibility.Collapsed;
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

            // RomajiPron 선택 시 같은 ID의 다른 타입들도 동시에 업데이트
            if ((_selectedRomajiPron?.IsSystem ?? true))
            {
                var id = _selectedRomajiPron?.Id ?? 1;

                var correspondingRomajiKana = Unit.ReplaceRomajiKana.FirstOrDefault(p => p.Id == id);
                if (correspondingRomajiKana != null)
                {
                    _selectedRomajiKana = correspondingRomajiKana;
                    Unit.RomajiKana = correspondingRomajiKana.Value;
                }

                var correspondingHiraganaPron = Unit.ReplaceHiraganaPron.FirstOrDefault(p => p.Id == id);
                if (correspondingHiraganaPron != null)
                {
                    _selectedHiraganaPron = correspondingHiraganaPron;
                    Unit.HiraganaPron = correspondingHiraganaPron.Value;
                }

                var correspondingHiraganaKana = Unit.ReplaceHiraganaKana.FirstOrDefault(p => p.Id == id);
                if (correspondingHiraganaKana != null)
                {
                    _selectedHiraganaKana = correspondingHiraganaKana;
                    Unit.HiraganaKana = correspondingHiraganaKana.Value;
                }

                Unit.SelectId = id;
            }

            Unit.RomajiPron = _selectedRomajiPron?.Value ?? string.Empty;
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

            // RomajiKana 선택 시 같은 ID의 다른 타입들도 동시에 업데이트
            if ((_selectedRomajiKana?.IsSystem ?? true))
            {
                var id = _selectedRomajiKana?.Id ?? 1;

                var correspondingRomajiPron = Unit.ReplaceRomajiPron.FirstOrDefault(p => p.Id == id);
                if (correspondingRomajiPron != null)
                {
                    _selectedRomajiPron = correspondingRomajiPron;
                    Unit.RomajiPron = correspondingRomajiPron.Value;
                }

                var correspondingHiraganaPron = Unit.ReplaceHiraganaPron.FirstOrDefault(p => p.Id == id);
                if (correspondingHiraganaPron != null)
                {
                    _selectedHiraganaPron = correspondingHiraganaPron;
                    Unit.HiraganaPron = correspondingHiraganaPron.Value;
                }

                var correspondingHiraganaKana = Unit.ReplaceHiraganaKana.FirstOrDefault(p => p.Id == id);
                if (correspondingHiraganaKana != null)
                {
                    _selectedHiraganaKana = correspondingHiraganaKana;
                    Unit.HiraganaKana = correspondingHiraganaKana.Value;
                }

                Unit.SelectId = id;
            }

            Unit.RomajiKana = _selectedRomajiKana?.Value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    public ReplaceString SelectedHiraganaPron
    {
        get => _selectedHiraganaPron;
        set
        {
            if (Equals(value, _selectedHiraganaPron)) return;
            _selectedHiraganaPron = value;

            // HiraganaPron 선택 시 같은 ID의 다른 타입들도 동시에 업데이트
            if ((_selectedHiraganaPron?.IsSystem ?? true))
            {
                var id = _selectedHiraganaPron?.Id ?? 1;

                var correspondingHiraganaKana = Unit.ReplaceHiraganaKana.FirstOrDefault(p => p.Id == id);
                if (correspondingHiraganaKana != null)
                {
                    _selectedHiraganaKana = correspondingHiraganaKana;
                    Unit.HiraganaKana = correspondingHiraganaKana.Value;
                }

                var correspondingRomajiPron = Unit.ReplaceRomajiPron.FirstOrDefault(p => p.Id == id);
                if (correspondingRomajiPron != null)
                {
                    _selectedRomajiPron = correspondingRomajiPron;
                    Unit.RomajiPron = correspondingRomajiPron.Value;
                }

                var correspondingRomajiKana = Unit.ReplaceRomajiKana.FirstOrDefault(p => p.Id == id);
                if (correspondingRomajiKana != null)
                {
                    _selectedRomajiKana = correspondingRomajiKana;
                    Unit.RomajiKana = correspondingRomajiKana.Value;
                }

                Unit.SelectId = id;
            }

            Unit.HiraganaPron = _selectedHiraganaPron?.Value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    public ReplaceString SelectedHiraganaKana
    {
        get => _selectedHiraganaKana;
        set
        {
            if (Equals(value, _selectedHiraganaKana)) return;
            _selectedHiraganaKana = value;

            // HiraganaKana 선택 시 같은 ID의 다른 타입들도 동시에 업데이트
            if ((_selectedHiraganaKana?.IsSystem ?? true))
            {
                var id = _selectedHiraganaKana?.Id ?? 1;

                var correspondingHiraganaPron = Unit.ReplaceHiraganaPron.FirstOrDefault(p => p.Id == id);
                if (correspondingHiraganaPron != null)
                {
                    _selectedHiraganaPron = correspondingHiraganaPron;
                    Unit.HiraganaPron = correspondingHiraganaPron.Value;
                }

                var correspondingRomajiPron = Unit.ReplaceRomajiPron.FirstOrDefault(p => p.Id == id);
                if (correspondingRomajiPron != null)
                {
                    _selectedRomajiPron = correspondingRomajiPron;
                    Unit.RomajiPron = correspondingRomajiPron.Value;
                }

                var correspondingRomajiKana = Unit.ReplaceRomajiKana.FirstOrDefault(p => p.Id == id);
                if (correspondingRomajiKana != null)
                {
                    _selectedRomajiKana = correspondingRomajiKana;
                    Unit.RomajiKana = correspondingRomajiKana.Value;
                }

                Unit.SelectId = id;
            }

            Unit.HiraganaKana = _selectedHiraganaKana?.Value ?? string.Empty;
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
        HiraganaPronLabel.Destroy();
        HiraganaKanaLabel.Destroy();
        ClearValue(UnitProperty);
        ClearValue(RomajiVisibilityProperty);
        ClearValue(HiraganaVisibilityProperty);
        ClearValue(IsHyphenProperty);
        ClearValue(MyFontSizeProperty);
    }
}
