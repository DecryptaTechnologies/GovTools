using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;
public partial class SettingsSelectionMenuItemViewModel : SettingsMenuItemViewModel
{

    private object? _initialValue;

    [ObservableProperty]
    private ObservableCollection<object> _items;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BeenChanged))]
    private object? _selectedItem;

    public bool BeenChanged => SelectedItem != _initialValue;

    public bool IsVisible => string.IsNullOrEmpty(Description);

    public SettingsSelectionMenuItemViewModel()
    {
        Items = new ObservableCollection<object>();
    }

    public SettingsSelectionMenuItemViewModel(object? selectedItem) : this()
    {
        _initialValue = selectedItem;
        _selectedItem = selectedItem;
    }

    partial void OnSelectedItemChanged(object? oldValue, object? newValue)
    {
        OnItemSelected(EventArgs.Empty);
        Debug.WriteLine(nameof(OnSelectedItemChanged));
    }

    protected virtual void OnItemSelected(EventArgs e)
    {
        ItemSelected?.Invoke(this, e);
    }

    public event EventHandler? ItemSelected;

}
