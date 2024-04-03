using Autofac;
using DecryptaTechnologies.GovTools.WpfUI.Utils;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class StartViewModel : HostScreenViewModelBase
{

    public StartViewModel()
    {
    }

    public async override Task ActivateAsync()
    {
        var loadingViewModel = App.Container.Resolve<LoadingViewModel>();
        await SwitchScreenAsync(loadingViewModel);
        return;
    }

}
