using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using rskibbe.I18n.Contracts;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class KeepassKeyFileSelectionViewModel : ExtractorStepBaseViewModel
{

    [ObservableProperty]
    string _selectedKeyFilePath;

    protected ITranslator _translator;

    public KeepassKeyFileSelectionViewModel(
        ITranslator translator
    )
    {
        _translator = translator;
        _selectedKeyFilePath = "";
    }

    public override async Task ActivateAsync()
    {
        await base.ActivateAsync();
    }

    [RelayCommand]
    public void SelectKeyFile()
    {
        var ofd = new OpenFileDialog();
        ofd.InitialDirectory = AppContext.BaseDirectory;
        var result = ofd.ShowDialog();
        if (result != true)
            return;
        SelectedKeyFilePath = ofd.FileName;
        OnCompleted(EventArgs.Empty);
    }

    [RelayCommand]
    public void Continue()
    {
        OnCompleted(EventArgs.Empty);
    }

}
