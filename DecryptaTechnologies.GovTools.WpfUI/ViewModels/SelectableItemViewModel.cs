using CommunityToolkit.Mvvm.ComponentModel;
using DecryptaTechnologies.GovTools.WpfUI.Utils;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class SelectableItemViewModel<T> : ViewModelBase
{

    [ObservableProperty]
    T _item;

    [ObservableProperty]
    bool _isSelected;

    public SelectableItemViewModel(T item)
    {
        Item = item;
    }

    public override string ToString()
        => $"{Item}";

}
