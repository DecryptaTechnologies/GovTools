using Autofac;
using Autofac.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.WpfUI.ValueObjects;
using rskibbe.I18n.Contracts;
using System.Collections.ObjectModel;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class SidenavViewModel : ObservableObject
{

    protected ITranslator _translator;

    [ObservableProperty]
    ObservableCollection<MenuItemViewModel> _menuItems;

    [ObservableProperty]
    bool _isVisible;

    public SidenavViewModel(
        ITranslator translator
    )
    {
        _translator = translator;
        _menuItems = MenuItemsBuilder
            .New
            .Add<ExtractorViewModel>("Sidebar.Extractor")
            .Add<CombiLenViewModel>("Sidebar.CombiLen")
            .Add<DupcHashGenViewModel>("Sidebar.DupcHashGen")
            .Add<BulkExtractorViewModel>("Sidebar.BulkExtractor")
            .Add(() => WeakReferenceMessenger.Default.Send<OpenSettingsMessage>(), "Sidebar.Settings")
            .Build();
    }

}

// TODO: refactor, organize
public class MenuItemsBuilder
{

    NavigationItemViewModel? _activeNavigationItemVm;

    List<MenuItemViewModel> _menuItems;

    public static MenuItemsBuilder New => new();

    public MenuItemsBuilder()
    {
        _menuItems = new();
    }

    public MenuItemsBuilder Add<T>(
        string translationKey,
        Func<bool>? canExecute = null
    ) where T : IScreen
    {
        var parms = new List<Parameter>()
        {
            new TypedParameter(typeof(Func<NavigationItemViewModel, Task>), (NavigationItemViewModel x) =>
            {
                if (_activeNavigationItemVm != null)
                {
                    if (x.Text == _activeNavigationItemVm.Text)
                        return Task.CompletedTask;
                    _activeNavigationItemVm.IsActive = false;
                }
                _activeNavigationItemVm = x;
                x.IsActive = true;
                var screenVm = App.Container.Resolve<T>();
                WeakReferenceMessenger.Default.Send(new NavigationMessage(screenVm));
                return Task.CompletedTask;
            }),
            new TypedParameter(typeof(string), translationKey),
            new TypedParameter(typeof(Func<bool>), canExecute),
        };
        var navItemVm = App.Container.Resolve<NavigationItemViewModel>(parms);
        _menuItems.Add(navItemVm);
        return this;
    }

    public MenuItemsBuilder Add(
        Action action,
        string translationKey,
        Func<bool>? canExecute = null
    )
    {
        var parms = new List<Parameter>()
        {
            new TypedParameter(typeof(Func<Task>), () =>
            {
                action.Invoke();
                return Task.CompletedTask;
            }),
            new TypedParameter(typeof(string), translationKey),
            new TypedParameter(typeof(Func<bool>), canExecute),
        };
        var navItemVm = App.Container.Resolve<FunctionItemViewModel>(parms);
        _menuItems.Add(navItemVm);
        return this;
    }

    public ObservableCollection<MenuItemViewModel> Build()
    {
        return new ObservableCollection<MenuItemViewModel>(_menuItems);
    }

}
