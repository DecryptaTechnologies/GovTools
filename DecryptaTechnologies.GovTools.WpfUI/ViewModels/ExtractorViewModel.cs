using Autofac;
using Autofac.Core;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using DecryptaTechnologies.GovTools.WpfUI.ValueObjects;
using rskibbe.I18n.Contracts;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class ExtractorViewModel : HostScreenViewModelBase
{

    protected ITranslator _translator;

    protected IExtractorManager _extractorManager;

    public ExtractorViewModel(
        ITranslator translator,
        IExtractorManager extractorManager
    )
    {
        _translator = translator;
        _extractorManager = extractorManager;
    }

    public override async Task ActivateAsync()
    {
        await base.ActivateAsync();
        await ShowExtractorSelectionAsync();        
    }

    private async Task ShowExtractorSelectionAsync()
    {
        var extractorSelectionViewModel = App.Container.Resolve<ExtractorSelectionViewModel>();
        async void ExtractorSelected(object? sender, ExtractorEventArgs e)
        {
            extractorSelectionViewModel.ExtractorSelected -= ExtractorSelected;
            e.Extractor.Reset();
            await ShowExtractorDetailsAsync(e.Extractor);
        }
        extractorSelectionViewModel.ExtractorSelected += ExtractorSelected;
        await SwitchScreenAsync(extractorSelectionViewModel);
    }

    private async Task ShowExtractorDetailsAsync(IExtractor extractor)
    {
        var parms = new List<Parameter>()
        {
            new TypedParameter(typeof(IExtractor), extractor),
        };
        var extractorDetailViewModel = App.Container.Resolve<ExtractorDetailViewModel>(parms);
        async void Backing(object? sender, EventArgs e)
        {
            extractorDetailViewModel.Backing -= Backing;
            await ShowExtractorSelectionAsync();
        }
        extractorDetailViewModel.Backing += Backing;
        await SwitchScreenAsync(extractorDetailViewModel);
    }

}
