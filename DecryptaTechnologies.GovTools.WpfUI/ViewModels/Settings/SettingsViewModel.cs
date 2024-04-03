using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class SettingsViewModel : ScreenViewModelBase
{

    [ObservableProperty]
    string _searchText;

    Subject<string> _searchTextSubject;

    List<SettingsCategoryViewModel> _allCategories;

    List<SettingsCategoryViewModel> _filteredCategories;

    [ObservableProperty]
    ObservableCollection<SettingsCategoryViewModel> _categories;

    public SettingsViewModel()
    {
        _searchText = "";
        _searchTextSubject = new Subject<string>();
        _searchTextSubject
            .Throttle(TimeSpan.FromMilliseconds(400))
            .Subscribe(SearchTextChangedDebounced);
        _allCategories = new List<SettingsCategoryViewModel>()
        {
            App.Container.Resolve<ApplicationSettingsCategoryViewModel>(),
        };
        _filteredCategories = new List<SettingsCategoryViewModel>();
        _categories = new ObservableCollection<SettingsCategoryViewModel>(_allCategories);
    }

    public async override Task ActivateAsync()
    {
        await base.ActivateAsync();
        SearchTextChangedDebounced("");
    }

    partial void OnSearchTextChanged(string? oldValue, string newValue)
    {
        _searchTextSubject.OnNext(newValue);
    }

    private void SearchTextChangedDebounced(string searchText)
    {
        SearchText = searchText;
        if (string.IsNullOrEmpty(SearchText))
        {
            _allCategories.ForEach(x => x.Filter(SearchText));
            Categories = new ObservableCollection<SettingsCategoryViewModel>(_allCategories);
            return;
        }
        _filteredCategories = _allCategories.Where(x => x.Filter(SearchText)).ToList();
        Categories = new ObservableCollection<SettingsCategoryViewModel>(_filteredCategories);
    }

}

public partial class SettingsCategoryViewModel : ObservableObject
{

    [ObservableProperty]
    string _searchText;

    [ObservableProperty]
    string _name;

    protected List<SettingsSubCategoryViewModel> _allCategories;

    List<SettingsSubCategoryViewModel> _filteredCategories;

    [ObservableProperty]
    ObservableCollection<SettingsSubCategoryViewModel> _categories;

    public SettingsCategoryViewModel()
    {
        SearchText = "";
        Name = "";
        _allCategories = new List<SettingsSubCategoryViewModel>();
        _filteredCategories = new List<SettingsSubCategoryViewModel>();
        _categories = new ObservableCollection<SettingsSubCategoryViewModel>(_allCategories);
    }

    public bool Filter(string searchText)
    {
        SearchText = searchText;
        if (string.IsNullOrEmpty(searchText))
        {
            _allCategories.ForEach(x => x.Filter(searchText));
            Categories = new ObservableCollection<SettingsSubCategoryViewModel>(_allCategories);
            return true;
        }
        _filteredCategories = _allCategories.Where(x => x.Filter(SearchText)).ToList();
        Categories = new ObservableCollection<SettingsSubCategoryViewModel>(_filteredCategories);
        return Categories.Count > 0 || Name.ToLower().Contains(SearchText);
    }

}

public partial class SettingsSubCategoryViewModel : ObservableObject
{

    [ObservableProperty]
    string _searchText;

    [ObservableProperty]
    string _name;

    protected List<SettingsMenuItemViewModel> _allMenuItems;

    List<SettingsMenuItemViewModel> _filteredMenuItems;

    [ObservableProperty]
    ObservableCollection<SettingsMenuItemViewModel> _menuItems;

    public SettingsSubCategoryViewModel()
    {
        SearchText = "";
        Name = "";
        _allMenuItems = new List<SettingsMenuItemViewModel>();
        _filteredMenuItems = new List<SettingsMenuItemViewModel>();
        _menuItems = new ObservableCollection<SettingsMenuItemViewModel>(_allMenuItems);
    }

    public bool Filter(string searchText)
    {
        SearchText = searchText;
        if (string.IsNullOrEmpty(SearchText))
        {
            _allMenuItems.ForEach(x => x.Filter(searchText));
            MenuItems = new ObservableCollection<SettingsMenuItemViewModel>(_allMenuItems);
            return true;
        }
        _filteredMenuItems = _allMenuItems.Where(x => x.Filter(SearchText)).ToList();
        MenuItems = new ObservableCollection<SettingsMenuItemViewModel>(_filteredMenuItems);
        return MenuItems.Count > 0 || Name.ToLower().Contains(searchText.ToLower());
    }

}

public partial class SettingsMenuItemViewModel : ObservableObject
{

    [ObservableProperty]
    string _searchText;

    [ObservableProperty]
    string _name;

    [ObservableProperty]
    string _description;

    public SettingsMenuItemViewModel()
    {
        SearchText = "";
        Name = "";
        Description = "";
    }

    public bool Filter(string searchText)
    {
        SearchText = searchText;
        if (string.IsNullOrEmpty(SearchText))
            return true;
        var lowerSearchText = searchText.ToLower();
        return Name.ToLower().Contains(lowerSearchText) || Description.ToLower().Contains(lowerSearchText);
    }

}
