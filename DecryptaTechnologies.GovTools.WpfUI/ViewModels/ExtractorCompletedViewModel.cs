using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using rskibbe.Ini.Contracts;
using System.Diagnostics;
using System.IO;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class ExtractorCompletedViewModel : ScreenViewModelBase
{

    IGovTools _govTools;

    public ExtractorCompletedViewModel(
        IGovTools govTools
    )
    {
        _govTools = govTools;
    }

    [RelayCommand]
    public void OpenHashoutFolder()
    {
        Process.Start("explorer.exe", _govTools.GetHashoutDirectory());
    }

    [RelayCommand]
    public void GoBackToFirstExtractorScreen()
    {
        OnBackToFirstExtractorScreenRequequested(EventArgs.Empty);
    }

    protected virtual void OnBackToFirstExtractorScreenRequequested(EventArgs e)
    {
        BackToFirstExtractorScreenRequequested?.Invoke(this, e);
    }

    public event EventHandler? BackToFirstExtractorScreenRequequested;


}
