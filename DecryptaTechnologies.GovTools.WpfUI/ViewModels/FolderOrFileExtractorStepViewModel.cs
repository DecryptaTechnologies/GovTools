using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.WpfUI.ValueObjects;
using Microsoft.Win32;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class FolderOrFileExtractorStepViewModel : ExtractorStepBaseViewModel
{

    [ObservableProperty]
    string _selectedFile;

    [ObservableProperty]
    string _selectedFolder;

    [ObservableProperty]
    bool _isFileSelectionEnabled;

    [ObservableProperty]
    bool _isFolderSelectionEnabled;

    [ObservableProperty]
    string _filter;

    public FolderOrFileExtractorStepViewModel(
        bool isFileSelectionEnabled = true,
        bool isFolderSelectionEnabled = false,
        string filter = "All Files (*.*)| *.*"
    )
    {
        _selectedFile = "";
        _selectedFolder = "";
        _isFileSelectionEnabled = isFileSelectionEnabled;
        _isFolderSelectionEnabled = isFolderSelectionEnabled;
        _filter = filter;
    }

    [RelayCommand]
    public void SelectFile()
    {
        var ofd = new OpenFileDialog();
        ofd.Filter = Filter;
        ofd.InitialDirectory = AppContext.BaseDirectory;
        var result = ofd.ShowDialog();
        if (result != true)
            return;
        SelectedFile = ofd.FileName;
        OnFileSelected(new StringEventArgs(ofd.FileName));
        OnCompleted(EventArgs.Empty);
    }

    protected virtual void OnFileSelected(StringEventArgs e)
    {
        FileSelected?.Invoke(this, e);
    }

    [RelayCommand]
    public void SelectFolder()
    {
        var ofd = new OpenFolderDialog();
        var result = ofd.ShowDialog();
        if (result != true)
            return;
        SelectedFolder = ofd.FolderName;
        OnFolderSelected(new StringEventArgs(ofd.FolderName));
        OnCompleted(EventArgs.Empty);
    }

    protected virtual void OnFolderSelected(StringEventArgs e)
    {
        FolderSelected?.Invoke(this, e);
    }

    public event EventHandler<StringEventArgs>? FileSelected;

    public event EventHandler<StringEventArgs>? FolderSelected;

}
