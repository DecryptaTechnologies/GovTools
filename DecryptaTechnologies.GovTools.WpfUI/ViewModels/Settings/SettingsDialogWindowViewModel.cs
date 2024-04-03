using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.WpfUI.Utils;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class SettingsDialogWindowViewModel : HostScreenViewModelBase
{

    public SettingsDialogWindowViewModel()
    {

    }

    public async override Task ActivateAsync()
    {
        await base.ActivateAsync();
        await SwitchScreenAsync<SettingsViewModel>();
    }

    [RelayCommand]
    public void Close()
    {

    }

}
