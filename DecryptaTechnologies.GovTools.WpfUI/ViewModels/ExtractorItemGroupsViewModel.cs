using CommunityToolkit.Mvvm.ComponentModel;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using System.Collections.ObjectModel;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class ExtractorItemGroupsViewModel : ViewModelBase
{

    [ObservableProperty]
    string _name;

    string? _filterText;

    [ObservableProperty]
    ObservableCollection<SelectableItemViewModel<IExtractor>> _extractorItems;

    [ObservableProperty]
    ObservableCollection<SelectableItemViewModel<IExtractor>> _filteredExtractorItems;

    public ExtractorItemGroupsViewModel()
    {
        _name = "";
        _filterText = "";
        _extractorItems = new();
        _filteredExtractorItems = new();
    }

    public bool Filter(string? filterText = null)
    {
        _filterText = filterText;

        if (string.IsNullOrWhiteSpace(_filterText))
        {
            FilteredExtractorItems = new(ExtractorItems);
            return true;
        }

        FilteredExtractorItems = new(ExtractorItems.Where(x => x.Item.Name.ToLower().Contains(_filterText)));
        return FilteredExtractorItems.Count > 0;
    }

}
