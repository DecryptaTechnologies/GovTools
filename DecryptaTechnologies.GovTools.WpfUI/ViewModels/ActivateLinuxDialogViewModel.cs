using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using MvvmDialogs;
using rskibbe.I18n.Contracts;
using System.Windows;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class ActivateLinuxDialogViewModel : ViewModelBase, IModalDialogViewModel
{

    protected ITranslator _translator;

    protected IWindowsService _windowsService;

    bool? _dialogResult;

    public bool? DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }

    public ActivateLinuxDialogViewModel(
        ITranslator translator,
        IWindowsService windowsService
    )
    {
        _translator = translator;
        _windowsService = windowsService;
    }

    [RelayCommand]
    public void ActivateLinux()
    {
        try
        {
            _windowsService.ActivateLinuxSubSystem();
        }
        catch (Exception ex)
        {
            var nl = Environment.NewLine;
            var msg = _translator.Translate($"ActivateLinuxDialog.ActivationError");
            MessageBox.Show($"{msg}:{nl}{nl}{ex.Message}", "", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        DialogResult = true;
    }

}
