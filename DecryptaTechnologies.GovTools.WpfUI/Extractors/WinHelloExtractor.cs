using Autofac;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.WpfUI.ViewModels;
using rskibbe.I18n.Contracts;
using rskibbe.Ini.Contracts;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace DecryptaTechnologies.GovTools.WpfUI.Extractors;

public class WinHelloExtractor : ExtractorBase,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait
{

    Process? _process;

    public override string Name => "Windows Hello PIN";

    public override string? ImageUrl => "/Resources/extractor-windows-login-password.png";

    string _folderPath;

    public WinHelloExtractor(
        IIniFile iniFile,
        ITranslator translator
    ) : base(iniFile, translator)
    {
        _folderPath = "";
    }

    public override List<IExtractorStepViewModel> GetRequiredScreens()
    {
        var winHelloExtractorViewModel = App.Container.Resolve<WinHelloExtractorViewModel>();
        winHelloExtractorViewModel.Completed += WinHelloExtractorViewModel_Completed;
        return [
            winHelloExtractorViewModel,
        ];
    }

    private void WinHelloExtractorViewModel_Completed(object? sender, EventArgs e)
    {
        var winHelloExtractorViewModel = sender as WinHelloExtractorViewModel;
        if (winHelloExtractorViewModel == null)
            return;
        winHelloExtractorViewModel.Completed -= WinHelloExtractorViewModel_Completed;
        _folderPath = winHelloExtractorViewModel.FolderPath;
    }

    public override async Task<bool?> RunAsync()
    {
        var py3FilePath = Path.Combine(AppContext.BaseDirectory, "Packages\\Py3\\App\\Python\\PP3.exe");
        var winhelloScriptFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\GTP\WinHello\WINHELLO2hashcat.py");
        var tempFolderPath = Path.Combine(AppContext.BaseDirectory, @"Packages\Temp");
        var windowsPath = @"C:\Windows";
        var relativePath = $@"_Hashout\WindowsHelloPIN_Extraction_{DateTime.Now:ddMMyy_HHmmssfff}.txt";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);
        var cryptoPath = $"--cryptokeys \"{windowsPath}\\ServiceProfiles\\LocalService\\AppData\\Roaming\\Microsoft\\Crypto\\Keys\" --masterkey \"{windowsPath}\\System32\\Microsoft\\Protect\\S-1-5-18\\User\" --system \"{tempFolderPath}\\SYSTEM\" --security \"{tempFolderPath}\\SECURITY\" --ngc \"{windowsPath}\\ServiceProfiles\\LocalService\\AppData\\Local\\Microsoft\\Ngc\" > \"{outputFilePath}\"";
        var batchFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\GTP\WinHello\WinHelloPIN_Extraction.bat");
        var tempSysPath = Path.Combine(AppContext.BaseDirectory, @$"Packages\Temp\SYSTEM");
        var tempSecPath = Path.Combine(AppContext.BaseDirectory, @$"Packages\Temp\SECURITY");

        if (string.IsNullOrEmpty(_folderPath))
        {
            // LIVE
            var sb = new StringBuilder();
            sb.AppendLine("chcp 65001");
            sb.AppendLine(@"TAKEOWN /f %windir%\ServiceProfiles\LocalService\AppData\Local\Microsoft\Ngc /r /a < nul");
            sb.AppendLine(@"ICACLS %windir%\ServiceProfiles\LocalService\AppData\Local\Microsoft\Ngc /grant ""%username%"":(F) /t");
            sb.AppendLine($"call reg.exe save hklm\\system \"{tempSysPath}\" /y & reg.exe save hklm\\security \"{tempSecPath}\" /y");
            sb.AppendLine($"call \"{py3FilePath}\" \"{winhelloScriptFilePath}\" {cryptoPath}");

            await File.WriteAllTextAsync(batchFilePath, sb.ToString(), new UTF8Encoding(false))
                .ConfigureAwait(false);

            await Task.Delay(3000)
                .ConfigureAwait(false);

            _process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {

                    FileName = "cmd.exe",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    Verb = "runas",
                    WorkingDirectory = Path.Combine(AppContext.BaseDirectory, @"Packages\GTP\WinHello"),
                    Arguments = "/c " + $"\"{batchFilePath}\""
                }
            };
            try
            {
                _process.Start();
            }
            catch (Exception)
            {
                MessageBox.Show($"{_translator.Translate("MainForm.AdminRights")}", "GovTools", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            await _process.WaitForExitAsync();

            if (await ((ICanCheckHashOutputFileAsync)this)
                .CheckHashOutputFileAsync(outputFilePath)
                .ConfigureAwait(false) == false)
                return false;

            return true;
        }
        else
        {
            // OFFLINE
            var cryptoPathOffline = $"--cryptokeys \"{_folderPath}\\ServiceProfiles\\LocalService\\AppData\\Roaming\\Microsoft\\Crypto\\Keys\" --masterkey \"{_folderPath}\\System32\\Microsoft\\Protect\\S-1-5-18\\User\" --system \"{_folderPath}\\System32\\config\\SYSTEM\" --security \"{_folderPath}\\System32\\config\\SECURITY\" --ngc \"{_folderPath}\\ServiceProfiles\\LocalService\\AppData\\Local\\Microsoft\\Ngc\" > \"{outputFilePath}\"";
            var sb = new StringBuilder();
            
            sb.AppendLine("chcp 65001");
            sb.AppendLine($"call \"{py3FilePath}\" \"{winhelloScriptFilePath}\" {cryptoPathOffline}");

            await File.WriteAllTextAsync(batchFilePath, sb.ToString(), new UTF8Encoding(false))
                .ConfigureAwait(false);

            await Task.Delay(3000)
                .ConfigureAwait(false);

            _process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    Verb = "runas",
                    WorkingDirectory = Path.Combine(AppContext.BaseDirectory, @"Packages\GTP\WinHello"),
                    Arguments = "/c " + $"\"{batchFilePath}\""
                }
            };
            try
            {
                _process.Start();
            }
            catch (Exception)
            {
                MessageBox.Show($"{_translator.Translate("MainForm.AdminRights")}", "GovTools", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            await _process.WaitForExitAsync();

            if (await ((ICanCheckHashOutputFileAsync)this)
                .CheckHashOutputFileAsync(outputFilePath)
                .ConfigureAwait(false) == false)
                return false;

            return true;

        }
    }

    public override void Stop()
    {
        if (_process == null)
            return;

        _stopped = true;
        _process.Kill();

        KillAllPP3Processes();
    }

    private void KillAllPP3Processes()
    {
        var exeProcess = Process.GetProcessesByName("PP3");
        if (exeProcess.Length == 0)
            return;

        foreach (var process in exeProcess)
            process.Kill();
    }

}




