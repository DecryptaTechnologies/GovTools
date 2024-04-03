using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using System.Diagnostics;

namespace DecryptaTechnologies.GovTools.WpfUI.Utils;

public partial class HostScreenViewModelBase : ViewModelBase, IHostScreen
{

    [ObservableProperty]
    IScreen? _currentScreen;

    public virtual Task BeforeActivateAsync()
    {
        Debug.WriteLine($"{GetType().Name}.{nameof(BeforeActivateAsync)}");
        return Task.CompletedTask;
    }

    public async virtual Task ActivateAsync()
    {
        Debug.WriteLine($"{GetType().Name}.{nameof(ActivateAsync)}");
        if (CurrentScreen != null)
            await CurrentScreen.ActivateAsync();
    }

    public async virtual Task DeactivateAsync()
    {
        if (CurrentScreen != null)
            await CurrentScreen.DeactivateAsync();
        Debug.WriteLine($"{GetType().Name}.{nameof(DeactivateAsync)}");
    }

    public async Task SwitchScreenAsync<T>() where T : IScreen
    {
        if (CurrentScreen != null)
            await CurrentScreen.DeactivateAsync();
        var screen = App.Container.Resolve<T>();
        CurrentScreen = screen;
        await screen.ActivateAsync();
    }

    public async Task SwitchScreenAsync(IScreen screen)
    {
        if (CurrentScreen != null)
        {
            await BeforeDeactivateChildAsync(screen);
            await CurrentScreen.DeactivateAsync();
        }
        CurrentScreen = screen;
        await BeforeActivateChildAsync(screen);
        await screen.BeforeActivateAsync();
        await screen.ActivateAsync();
    }

    public virtual Task BeforeDeactivateChildAsync(IScreen screen)
    {
        Debug.WriteLine($"{GetType().Name}.{nameof(BeforeDeactivateChildAsync)}");
        return Task.CompletedTask;
    }  
    
    public virtual Task BeforeActivateChildAsync(IScreen screen)
    {
        Debug.WriteLine($"{GetType().Name}.{nameof(BeforeActivateChildAsync)}");
        return Task.CompletedTask;
    }

}
