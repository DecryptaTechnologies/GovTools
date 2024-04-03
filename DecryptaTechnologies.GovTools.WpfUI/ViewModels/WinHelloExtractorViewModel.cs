using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using Microsoft.Win32;
using rskibbe.I18n.Contracts;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class WinHelloExtractorViewModel : ScreenViewModelBase, IExtractorStepViewModel
{

    protected ITranslator _translator;

    [ObservableProperty]
    string _folderPath;

    public WinHelloExtractorViewModel(
        ITranslator translator
    )
    {
        _translator = translator;
        _folderPath = "";
    }

    [RelayCommand]
    public void LiveMode()
    {
        OnCompleted(EventArgs.Empty);
    }

    [RelayCommand]
    public void SelectFolderPath()
    {
        var dlg = new OpenFolderDialog();
        if (dlg.ShowDialog() != true)
            return;
        FolderPath = dlg.FolderName;
        OnCompleted(EventArgs.Empty);
    }

    protected virtual void OnCompleted(EventArgs e)
    {
        Completed?.Invoke(this, e);
    }

    public event EventHandler? Completed;

}
