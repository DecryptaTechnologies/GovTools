using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class SettingsFolderTextMenuItemViewModel : SettingsMenuItemViewModel
{

    public Action<OpenFolderDialog>? ApplyFolderDialogParamsFn { get; set; }

    public Func<string, bool>? ValidateInputFn { get; set; }

    public Action<string>? HandleFailedInputValidationFn { get; set; }

    private string? _initialValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BeenChanged))]
    private string? _text;

    public bool BeenChanged => Text != _initialValue;

    public bool IsVisible => string.IsNullOrEmpty(Description);

    bool _isDeleteButtonVisible;

    public bool IsDeleteButtonVisible => _isDeleteButtonVisible;

    public SettingsFolderTextMenuItemViewModel()
    {

    }

    public SettingsFolderTextMenuItemViewModel(
        string? text,
        bool isDeleteButtonVisible = false
    ) : this()
    {
        _initialValue = text;
        _text = text;
        _isDeleteButtonVisible = isDeleteButtonVisible;
    }

    [RelayCommand]
    public void DisplayFolderDialog()
    {
        var ofd = new OpenFolderDialog();
        ApplyFolderDialogParamsFn?.Invoke(ofd);
        if (ofd.ShowDialog() == true)
        {
            var valid = ValidateInputFn?.Invoke(ofd.FolderName);
            if (valid == false)
            {
                HandleFailedInputValidationFn?.Invoke(ofd.FolderName);
                return;
            }
        }
        Text = ofd.FolderName;
    }

    partial void OnTextChanged(string? oldValue, string? newValue)
    {
        OnTheTextChanged(EventArgs.Empty);
    }

    protected virtual void OnTheTextChanged(EventArgs e)
    {
        TextChanged?.Invoke(this, e);
    }

    [RelayCommand]
    public void ClearFolderPath()
    {
        Text = "";
    }

    public event EventHandler? TextChanged;

}
