using Autofac;
using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.WpfUI.Utils;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class ExtractorDetailViewModel : HostScreenViewModelBase
{

    public IExtractor SelectedExtractor { get; set; }

    public List<IExtractorStepViewModel> RequiredExtractorScreens { get; set; }

    int _selectedScreenIndex;

    public ExtractorDetailViewModel(
        IExtractor selectedExtractor
    )
    {
        SelectedExtractor = selectedExtractor;
        RequiredExtractorScreens = [];
        _selectedScreenIndex = -1;
    }

    public override async Task ActivateAsync()
    {
        await base.ActivateAsync();
        RequiredExtractorScreens.AddRange(SelectedExtractor.GetRequiredScreens());
        await ShowFirstRequiredScreenAsync();
    }

    private async Task ShowFirstRequiredScreenAsync()
    {
        _selectedScreenIndex = 0;
        await SwitchScreenAsync(RequiredExtractorScreens[0]);
        RequiredExtractorScreens[0].Completed += RequiredScreen_Completed;
    }

    private async void RequiredScreen_Completed(object? sender, EventArgs e)
    {
        RequiredExtractorScreens[_selectedScreenIndex].Completed -= RequiredScreen_Completed;

        var wasLastRequiredScreen = _selectedScreenIndex >= RequiredExtractorScreens.Count - 1;
        if (!wasLastRequiredScreen)
        {
            await ShowNextScreenAsync();
            return;
        }

        await ShowExtractorRunningScreenAsync();

        var extractorResult = await SelectedExtractor.RunAsync();

        var extractionCancelled = extractorResult == null;
        if (extractionCancelled)
        {
            await SwitchScreenAsync<ExtractorCancelledViewModel>();
            return;
        }

        var extractionFailed = extractorResult == false;
        if (extractionFailed)
        {
            await SwitchScreenAsync<ExtractorFailedViewModel>();
            return;
        }

        var extractorCompletedViewModel = App.Container.Resolve<ExtractorCompletedViewModel>();
        async void BackToFirstExtractorScreenRequequested(object? sender, EventArgs e)
        {
            extractorCompletedViewModel.BackToFirstExtractorScreenRequequested -= BackToFirstExtractorScreenRequequested;
            SelectedExtractor.Reset();
            await ShowFirstRequiredScreenAsync();
        }
        extractorCompletedViewModel.BackToFirstExtractorScreenRequequested += BackToFirstExtractorScreenRequequested;
        await SwitchScreenAsync(extractorCompletedViewModel);
    }

    private async Task ShowNextScreenAsync()
    {
        _selectedScreenIndex++;
        RequiredExtractorScreens[_selectedScreenIndex].Completed += RequiredScreen_Completed;
        await SwitchScreenAsync(RequiredExtractorScreens[_selectedScreenIndex]);
    }

    private async Task ShowExtractorRunningScreenAsync()
    {
        var extractorRunningViewModel = App.Container.Resolve<ExtractorRunningViewModel>();
        void ExtractorRunningViewModel_Cancelled(object? sender, EventArgs e)
        {
            extractorRunningViewModel.Cancelled -= ExtractorRunningViewModel_Cancelled;
            SelectedExtractor.Stop();
        }
        extractorRunningViewModel.Cancelled += ExtractorRunningViewModel_Cancelled;
        await SwitchScreenAsync(extractorRunningViewModel);
    }

    public override async Task DeactivateAsync()
    {
        await base.DeactivateAsync();
        SelectedExtractor.Stop();
        RequiredExtractorScreens.Clear();
    }

    [RelayCommand]
    public void GoBackToSelection()
    {
        OnBacking(EventArgs.Empty);
    }

    protected virtual void OnBacking(EventArgs e)
    {
        Backing?.Invoke(this, e);
    }

    public event EventHandler? Backing;

}
