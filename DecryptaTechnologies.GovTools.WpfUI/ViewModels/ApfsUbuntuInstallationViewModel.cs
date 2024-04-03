using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using rskibbe.I18n.Contracts;
using System.Windows;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class ApfsUbuntuInstallationViewModel : ExtractorStepBaseViewModel
{

    protected IWindowsService _windowsService;
    
    protected ITranslator _translator;

    public ApfsUbuntuInstallationViewModel(
        IWindowsService windowsService,
        ITranslator translator
    )
    {
        _windowsService = windowsService;
        _translator = translator;
    }

    public override async Task ActivateAsync()
    {
        await base.ActivateAsync();
    }

    [RelayCommand]
    public void ActivateLinuxSubsystem()
    {
        try
        {
            _windowsService.ActivateLinuxSubSystem();
        }
        catch (Exception ex)
        { 
            MessageBox.Show($"{_translator.Translate($"ActivateLinuxDialog.ActivationError")}:{Environment.NewLine}{Environment.NewLine}{ex.Message}");
            return;
        }
    }

    [RelayCommand]
    public void Continue()
    {
        OnCompleted(EventArgs.Empty);
    }

}
