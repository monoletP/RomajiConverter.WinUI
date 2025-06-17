using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RomajiConverter.Core.Models;

public class ConvertedUnit : INotifyPropertyChanged
{
    private string _hiraganaPron;
    private string _hiraganaKana;
    private bool _isKanji;
    private string _pos1;
    private string _pos2;
    private string _pos3;
    private string _japanese;
    private ObservableCollection<ReplaceString> _replaceHiraganaPron;
    private ObservableCollection<ReplaceString> _replaceHiraganaKana;
    private ObservableCollection<ReplaceString> _replaceRomajiPron;
    private ObservableCollection<ReplaceString> _replaceRomajiKana;
    private string _romajiPron;
    private string _romajiKana;
    private ushort _selectId;

    public ConvertedUnit(string japanese, string hiraganaPron, string hiraganaKana, string romajiPron, string romajiKana, bool isKanji, string pos1, string pos2, string pos3)
    {
        Japanese = japanese;
        RomajiPron = romajiPron;
        HiraganaPron = hiraganaPron;
        RomajiKana = romajiKana;
        HiraganaKana = hiraganaKana;
        IsKanji = isKanji;
        Pos1 = pos1;
        Pos2 = pos2;
        Pos3 = pos3;
        SelectId = 1;
        ReplaceHiraganaPron = new ObservableCollection<ReplaceString> { new(1, hiraganaPron, true) };
        ReplaceRomajiPron = new ObservableCollection<ReplaceString> { new(1, romajiPron, true) };
        ReplaceHiraganaKana = new ObservableCollection<ReplaceString> { new(1, hiraganaKana, true) };
        ReplaceRomajiKana = new ObservableCollection<ReplaceString> { new(1, romajiKana, true) };
        _pos3 = pos3;
    }

    public string Japanese
    {
        get => _japanese;
        set
        {
            if (value == _japanese) return;
            _japanese = value;
            OnPropertyChanged();
        }
    }

    public string RomajiPron
    {
        get => _romajiPron;
        set
        {
            if (value == _romajiPron) return;
            _romajiPron = value;
            OnPropertyChanged();
        }
    }

    public string RomajiKana
    {
        get => _romajiKana;
        set
        {
            if (value == _romajiKana) return;
            _romajiKana = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ReplaceString> ReplaceRomajiPron
    {
        get => _replaceRomajiPron;
        set
        {
            if (Equals(value, _replaceRomajiPron)) return;
            _replaceRomajiPron = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ReplaceString> ReplaceRomajiKana
    {
        get => _replaceRomajiKana;
        set
        {
            if (Equals(value, _replaceRomajiKana)) return;
            _replaceRomajiKana = value;
            OnPropertyChanged();
        }
    }

    public string HiraganaPron
    {
        get => _hiraganaPron;
        set
        {
            if (value == _hiraganaPron) return;
            _hiraganaPron = value;
            OnPropertyChanged();
        }
    }

    public string HiraganaKana
    {
        get => _hiraganaKana;
        set
        {
            if (value == _hiraganaKana) return;
            _hiraganaKana = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ReplaceString> ReplaceHiraganaPron
    {
        get => _replaceHiraganaPron;
        set
        {
            if (Equals(value, _replaceHiraganaPron)) return;
            _replaceHiraganaPron = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ReplaceString> ReplaceHiraganaKana
    {
        get => _replaceHiraganaKana;
        set
        {
            if (Equals(value, _replaceHiraganaKana)) return;
            _replaceHiraganaKana = value;
            OnPropertyChanged();
        }
    }

    public bool IsKanji
    {
        get => _isKanji;
        set
        {
            if (value == _isKanji) return;
            _isKanji = value;
            OnPropertyChanged();
        }
    }
    public string Pos1
    {
        get => _pos1;
        set
        {
            if (value == _pos1) return;
            _pos1 = value;
            OnPropertyChanged();
        }
    }
    public string Pos2
    {
        get => _pos2;
        set
        {
            if (value == _pos2) return;
            _pos2 = value;
            OnPropertyChanged();
        }
    }
    public string Pos3
    {
        get => _pos3;
        set
        {
            if (value == _pos3) return;
            _pos3 = value;
            OnPropertyChanged();
        }
    }

    public ushort SelectId
    {
        get => _selectId;
        set
        {
            if (value == _selectId) return;
            _selectId = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}