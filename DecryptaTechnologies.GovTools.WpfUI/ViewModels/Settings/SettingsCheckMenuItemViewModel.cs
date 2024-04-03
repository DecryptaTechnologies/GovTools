using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels.Settings;

public partial class SettingsCheckMenuItemViewModel : SettingsMenuItemViewModel
{

    private bool _initialValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BeenChanged))]
    private bool _isChecked;

    public bool BeenChanged => IsChecked != _initialValue;

    public SettingsCheckMenuItemViewModel()
    {

    }

    public SettingsCheckMenuItemViewModel(bool isChecked) : this()
    {
        _initialValue = isChecked;
        _isChecked = isChecked;
    }

    partial void OnIsCheckedChanged(bool oldValue, bool newValue)
    {
        OnCheckedChanged(EventArgs.Empty);
        Debug.WriteLine(nameof(OnIsCheckedChanged));
    }

    protected virtual void OnCheckedChanged(EventArgs e)
    {
        IsCheckedChanged?.Invoke(this, e);
    }

    public event EventHandler? IsCheckedChanged;

}