using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using DecryptaTechnologies.GovTools.WpfUI.ValueObjects;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class MainLayoutViewModel : HostScreenViewModelBase,
    IRecipient<NavigationMessage>
{

    [ObservableProperty]
    SidenavViewModel? _sidenavViewModel;

    public async override Task ActivateAsync()
    {
        await base.ActivateAsync();
        WeakReferenceMessenger.Default.RegisterAll(this);
        await PrepareSidenavAsync();
    }

    private async Task PrepareSidenavAsync()
    {
        SidenavViewModel = App.Container.Resolve<SidenavViewModel>();
        SidenavViewModel.IsVisible = true;
        await SidenavViewModel.MenuItems[0].Click();
    }

    public async void Receive(NavigationMessage e)
    {
        await SwitchScreenAsync(e.Screen);
    }

}
