using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using Microsoft.Win32;
using MvvmDialogs;
using rskibbe.I18n.Contracts;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class WindowsLoginExtractorInputDialogViewModel : ViewModelBase, IModalDialogViewModel
{

    protected ITranslator _translator;

    [ObservableProperty]
    string _folderPath;

    bool? _dialogResult;

    public bool? DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }

    public WindowsLoginExtractorInputDialogViewModel(
        ITranslator translator
    )
    {
        _translator = translator;
        _folderPath = "";
    }

    [RelayCommand]
    public void LiveMode()
    {
        var samPath = Path.Combine(AppContext.BaseDirectory, @$"Packages\Temp\SAM");
        var tempSysPath = Path.Combine(AppContext.BaseDirectory, @$"Packages\Temp\SYSTEM");
        var tempSecPath = Path.Combine(AppContext.BaseDirectory, @$"Packages\Temp\SECURITY");
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = "cmd.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                WorkingDirectory = AppContext.BaseDirectory,
                Arguments = $"/c reg.exe save hklm\\sam \"{samPath}\" /y & reg.exe save hklm\\system \"{tempSysPath}\" /y & reg.exe save hklm\\security \"{tempSecPath}\" /y"
            }
        };
        process.Start();
        process.WaitForExit();
        FolderPath = Path.Combine(AppContext.BaseDirectory, "Packages\\Temp");
        DialogResult = true;
    }

    [RelayCommand]
    public void SelectFolderPath()
    {
        var dlg = new OpenFolderDialog();
        if (dlg.ShowDialog() != true)
            return;
        FolderPath = dlg.FolderName;
    }

    [RelayCommand]
    public void Start()
    {
        if (string.IsNullOrEmpty(FolderPath))
        {
            var translation = _translator.Translate($"{"WindowsLoginPassword.PathEmpty"}");
            MessageBox.Show($"{translation}");
            return;
        }
        DialogResult = true;
    }

}
