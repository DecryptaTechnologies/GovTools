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
    string _initialDirectory;

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
        _initialDirectory = "";
    }

    [RelayCommand]
    private void SelectFile()
    {
        var ofd = new OpenFileDialog();
        ofd.Filter = Filter;
        ofd.InitialDirectory = string.IsNullOrEmpty(InitialDirectory) ? AppContext.BaseDirectory : InitialDirectory;
        var result = ofd.ShowDialog();
        if (result != true)
            return;
        SelectedFolder = "";
        SelectedFile = ofd.FileName;
        OnFileSelected(new StringEventArgs(ofd.FileName));
        OnCompleted(EventArgs.Empty);
    }

    protected virtual void OnFileSelected(StringEventArgs e)
    {
        FileSelected?.Invoke(this, e);
    }

    [RelayCommand]
    private void SelectFolder()
    {
        var ofd = new OpenFolderDialog();
        ofd.InitialDirectory = string.IsNullOrEmpty(InitialDirectory) ? AppContext.BaseDirectory : InitialDirectory;
        var result = ofd.ShowDialog();
        if (result != true)
            return;
        SelectedFile = "";
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
