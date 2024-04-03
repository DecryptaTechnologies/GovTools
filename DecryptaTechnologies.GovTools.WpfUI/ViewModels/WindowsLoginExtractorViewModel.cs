using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using Microsoft.Win32;
using rskibbe.I18n.Contracts;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class WindowsLoginExtractorViewModel : ScreenViewModelBase, IExtractorStepViewModel
{

    protected ITranslator _translator;

    [ObservableProperty]
    string _folderPath;

    public WindowsLoginExtractorViewModel(
        ITranslator translator
    )
    {
        _translator = translator;
        _folderPath = "";
    }

    [RelayCommand]
    public async Task LiveMode()
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
                WindowStyle = ProcessWindowStyle.Minimized,
                Verb = "runas",
                WorkingDirectory = AppContext.BaseDirectory,
                Arguments = $"/c reg.exe save hklm\\sam \"{samPath}\" /y & reg.exe save hklm\\system \"{tempSysPath}\" /y & reg.exe save hklm\\security \"{tempSecPath}\" /y"
            }
        };
        try
        {
            process.Start();
        }
        catch (Exception)
        {
            MessageBox.Show($"{_translator.Translate("MainForm.AdminRights")}", "GovTools", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        await process.WaitForExitAsync();
        await Task.Delay(2000);
        FolderPath = Path.Combine(AppContext.BaseDirectory, "Packages\\Temp");
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
