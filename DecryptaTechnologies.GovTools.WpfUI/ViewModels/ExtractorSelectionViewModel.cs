using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.WpfUI.Extractors;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using DecryptaTechnologies.GovTools.WpfUI.ValueObjects;
using rskibbe.I18n.Contracts;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class ExtractorSelectionViewModel : ScreenViewModelBase
{

    protected ITranslator _translator;

    protected IExtractorManager _extractorManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredExtractorGroupItems))]
    ObservableCollection<ExtractorItemGroupsViewModel> _extractorGroupItems;

    public ObservableCollection<ExtractorItemGroupsViewModel> FilteredExtractorGroupItems
    {
        get
        {
            var lowerSearchText = SearchText.ToLower();
            var filteredItems = ExtractorGroupItems.Where(x =>
            {
                var groupHasMatches = x.Filter(lowerSearchText);
                return groupHasMatches;
            });
            return new(filteredItems);
        }
    }

    [ObservableProperty]
    string _searchText;

    Subject<string> _searchTextSubject;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedExtractor))]
    SelectableItemViewModel<IExtractor>? _selectedExtractorItem;

    public IExtractor? SelectedExtractor
    {
        get
        {
            if (SelectedExtractorItem != null)
                return SelectedExtractorItem.Item;
            return null;
        }
    }

    public ExtractorSelectionViewModel(
        ITranslator translator,
        IExtractorManager extractorManager
    )
    {
        _translator = translator;
        _extractorManager = extractorManager;

        _searchText = "";
        _searchTextSubject = new();
        _searchTextSubject
            .Throttle(TimeSpan.FromMilliseconds(300))
            .Subscribe(OnDebouncedSearchTextChanged);
        _extractorGroupItems = new();
    }

    public override async Task ActivateAsync()
    {
        await base.ActivateAsync();

        // TODO: use some sort of data store.. time...
        ExtractorGroupItems = new()
        {
             new ExtractorItemGroupsViewModel()
             {
                 Name = "Windows",
                 ExtractorItems = new()
                 {
                     new(App.Container.Resolve<BitlockerExtractor>()),
                     new(App.Container.Resolve<WindowsLoginPasswordExtractor>()),
                     new(App.Container.Resolve<WinHelloExtractor>()),
                 },
             },
             new ExtractorItemGroupsViewModel()
             {
                 Name = "Apple",
                 ExtractorItems = new()
                 {
                     new(App.Container.Resolve<ApfsAppleMacbooksExtractor>()),
                     new(App.Container.Resolve<ItunesBackupIphoneExtractor>()),
                 },
             },
             new ExtractorItemGroupsViewModel()
             {
                 Name = "Linux",
                 ExtractorItems = new()
                 {
                     new(App.Container.Resolve<ECryptFsExtractor>()),                    
                     new(App.Container.Resolve<LinuxUnifiedKeySystemLUKSExtractor>()),
                     new(App.Container.Resolve<LinuxLoginPasswordExtractor>()),
                 },
             },
             new ExtractorItemGroupsViewModel()
             {
                 Name = "Cryptocurrency Wallets",
                 ExtractorItems = new()
                 {
                     new(App.Container.Resolve<BitcoinWalletExtractor>()),
                     new(App.Container.Resolve<DogeWalletExtractor>()),
                     new(App.Container.Resolve<LitecoinWalletExtractor>()),
                     new(App.Container.Resolve<ElectrumWalletExtractor>()),
                     new(App.Container.Resolve<EthereumWalletExtractor>()),
                     new(App.Container.Resolve<ExodusWalletExtractor>()),
                     //new(App.Container.Resolve<>()),
                     new(App.Container.Resolve<MultiBitWalletExtractor>()),
                     new(App.Container.Resolve<MetaMaskWalletExtractor>()),
                     new(App.Container.Resolve<MetaMaskSeedExtractor>()),
                 },
             },
             new ExtractorItemGroupsViewModel()
             {
                 Name = "Documents",
                 ExtractorItems = new()
                 {           
                     new(App.Container.Resolve<PDFExtractor>()),
                     new(App.Container.Resolve<OfficeWordExcelExtractor>()),                 
                     new(App.Container.Resolve<LibreOfficeOpenOfficeExtractor>()),
                 },
             },
             new ExtractorItemGroupsViewModel()
             {
                 Name = "Other Encryption",
                 ExtractorItems = new()
                 {
                     new(App.Container.Resolve<VeraCryptFileExtractor>()),
                     new(App.Container.Resolve<VeraCryptPartitionExtractor>()),
                     new(App.Container.Resolve<VeraCryptBootPartitionExtractor>()),
                     new(App.Container.Resolve<TrueCryptFileExtractor>()),
                     new(App.Container.Resolve<TrueCryptPartitionExtractor>()),
                     new(App.Container.Resolve<TrueCryptBootPartitionExtractor>()),
                     new(App.Container.Resolve<KeepassExtractor>()),
                     new(App.Container.Resolve<SevenZipExtractor>()),
                     new(App.Container.Resolve<ZipExtractor>()),
                     new(App.Container.Resolve<RarExtractor>()),
                     new(App.Container.Resolve<MozillaFirefoxMasterPasswordExtractor>()),
                 },
             },
         };
    }

    partial void OnSearchTextChanged(string value)
    {
        _searchTextSubject.OnNext(value);
    }

    private void OnDebouncedSearchTextChanged(string searchText)
    {
        OnPropertyChanged(nameof(FilteredExtractorGroupItems));
    }

    [RelayCommand]
    private void ClearSearchText()
    {
        SearchText = "";
    }

    [RelayCommand]
    public async Task SelectExtractorAsync(SelectableItemViewModel<IExtractor> selectableItemViewModel)
    {
        if (selectableItemViewModel.Item is ICanReactToSelectionAsync selectionReactor)
            await selectionReactor.ReactToSelectionAsync(this);

        OnExtractorSelected(new ExtractorEventArgs(selectableItemViewModel.Item));
    }

    protected virtual void OnExtractorSelected(ExtractorEventArgs e)
    {
        ExtractorSelected?.Invoke(this, e);
    }

    public override Task DeactivateAsync()
    {
        base.DeactivateAsync();

        SelectedExtractorItem = null;
        foreach (var extractorGroupItem in ExtractorGroupItems)
            extractorGroupItem.ExtractorItems.Clear();
        ExtractorGroupItems.Clear();

        return Task.CompletedTask;
    }

    public event EventHandler<ExtractorEventArgs>? ExtractorSelected;

}
