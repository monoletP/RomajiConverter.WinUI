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

            // IsHyphen ���� �� ���� ���õ� �׸���� ����ȭ
            group.SyncSelectionsOnHyphenChange();
        }
    }

    /// <summary>
    /// IsHyphen ���� �� ���õ� �׸���� ����ȭ
    /// </summary>
    private void SyncSelectionsOnHyphenChange()
    {
        // ���� ǥ�õǰ� �ִ� �׸��� SelectId�� �������� �ٸ� �׸���� ����ȭ
        var currentSelectId = Unit.SelectId;

        // ��� �׸���� ���� SelectId�� ���� ����ȭ
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

        // PropertyChanged �̺�Ʈ �߻�
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
                // RomajiPron ǥ��
                RomajiPronLabel.IsEnabled = true;
                RomajiPronLabel.Opacity = 1;
                RomajiPronLabel.Visibility = Visibility.Visible;

                // RomajiKana ����
                RomajiKanaLabel.IsEnabled = false;
                RomajiKanaLabel.Opacity = 0;
                RomajiKanaLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                // RomajiKana ǥ��
                RomajiKanaLabel.IsEnabled = true;
                RomajiKanaLabel.Opacity = 1;
                RomajiKanaLabel.Visibility = Visibility.Visible;

                // RomajiPron ����
                RomajiPronLabel.IsEnabled = false;
                RomajiPronLabel.Opacity = 0;
                RomajiPronLabel.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            // �� �� ����
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
                // HiraganaPron ǥ��
                HiraganaPronLabel.IsEnabled = true;
                HiraganaPronLabel.Opacity = 1;
                HiraganaPronLabel.Visibility = Visibility.Visible;

                // HiraganaKana ����
                HiraganaKanaLabel.IsEnabled = false;
                HiraganaKanaLabel.Opacity = 0;
                HiraganaKanaLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                // HiraganaKana ǥ��
                HiraganaKanaLabel.IsEnabled = true;
                HiraganaKanaLabel.Opacity = 1;
                HiraganaKanaLabel.Visibility = Visibility.Visible;

                // HiraganaPron ����
                HiraganaPronLabel.IsEnabled = false;
                HiraganaPronLabel.Opacity = 0;
                HiraganaPronLabel.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            // �� �� ����
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

            // RomajiPron ���� �� ���� ID�� �ٸ� Ÿ�Ե鵵 ���ÿ� ������Ʈ
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

            // RomajiKana ���� �� ���� ID�� �ٸ� Ÿ�Ե鵵 ���ÿ� ������Ʈ
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

            // HiraganaPron ���� �� ���� ID�� �ٸ� Ÿ�Ե鵵 ���ÿ� ������Ʈ
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

            // HiraganaKana ���� �� ���� ID�� �ٸ� Ÿ�Ե鵵 ���ÿ� ������Ʈ
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
