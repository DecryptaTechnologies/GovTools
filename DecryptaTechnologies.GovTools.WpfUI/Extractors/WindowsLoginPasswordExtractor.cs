using Autofac;
using DecryptaTechnologies.GovTools.Domain.Attributes;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.Domain.ValueObjects;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using DecryptaTechnologies.GovTools.WpfUI.ViewModels;
using MvvmDialogs;
using rskibbe.I18n.Contracts;
using rskibbe.Ini.Contracts;
using System.Diagnostics;
using System.IO;

namespace DecryptaTechnologies.GovTools.WpfUI.Extractors;

[ExtractorType(ExtractorTypes.WindowsLoginPassword)]
public class WindowsLoginPasswordExtractor : ExtractorBase,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait,
    ICanPostProcessHashOutputFileAsync
{

    protected IDialogService _dialogService;

    string _folderPath;

    public override string Name => "Windows Login Password";

    public override string? ImageUrl => "/Resources/extractor-windows-login-password.png";

    public WindowsLoginPasswordExtractor(
        IDialogService dialogService,
        IIniFile iniFile,
        ITranslator translator
    ) : base(iniFile, translator)
    {
        _dialogService = dialogService;
        _folderPath = "";
    }

    public override List<IExtractorStepViewModel> GetRequiredScreens()
    {
        var windowsLoginExtractorViewModel = App.Container.Resolve<WindowsLoginExtractorViewModel>();
        windowsLoginExtractorViewModel.Completed += WindowsLoginExtractorViewModel_Completed;
        return [
            windowsLoginExtractorViewModel,
        ];
    }

    private void WindowsLoginExtractorViewModel_Completed(object? sender, EventArgs e)
    {
        var windowsLoginExtractorViewModel = sender as WindowsLoginExtractorViewModel;
        if (windowsLoginExtractorViewModel == null)
            return;
        windowsLoginExtractorViewModel.Completed -= WindowsLoginExtractorViewModel_Completed;
        _folderPath = windowsLoginExtractorViewModel.FolderPath;
    }

    public async override Task<bool?> RunAsync()
    {
        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), @$"_Hashout\WinLogin_Extraction_RawFile_{timestamp}.txt");
        var batFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Temp\Extraction.bat");

        var samFolderPath = Path.Combine(_folderPath, "SAM");
        var sysFolderPath = Path.Combine(_folderPath, "SYSTEM");
        var secFolderPath = Path.Combine(_folderPath, "SECURITY");

        var secretsDumpFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\SD\secretsdump.exe");
        var batFileContents = $"call \"{secretsDumpFilePath}\" -sam \"{samFolderPath}\" -system \"{sysFolderPath}\" -security \"{secFolderPath}\" local > \"{outputFilePath}\"";
        await File.WriteAllTextAsync(batFilePath, batFileContents)
            .ConfigureAwait(false);

        await Task.Delay(1000)
            .ConfigureAwait(false);

        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                // Verb = "runas"
                WorkingDirectory = AppContext.BaseDirectory,
                Arguments = $"/c \"{batFilePath}\""
            }
        };
        process.Start();
        await process.WaitForExitAsync()
            .ConfigureAwait(false);

        if (await ((ICanCheckHashOutputFileAsync)this)
            .CheckHashOutputFileAsync(outputFilePath)
            .ConfigureAwait(false) == false)
            return false;

        await Task.Delay(500)
            .ConfigureAwait(false);

        await PostProcessHashOutputFileAsync(outputFilePath)
            .ConfigureAwait(false);

        return true;
    }

    public async Task<bool> PostProcessHashOutputFileAsync(string filePath)
    {
        var lines = await File
            .ReadAllLinesAsync(filePath)
            .ConfigureAwait(false);

        var list = new List<string>();
        foreach (var line in lines)
        {
            if (!line.Contains(":::"))
                continue;
            var item = line
                    .Replace(":", "")
                    .Right(32)
                    .Replace("31d6cfe0d16ae931b73c59d7e0c089c0", "");
            list.Add(item);
        }

        await Task.Delay(500)
            .ConfigureAwait(false);

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var editOutputFilePath = Path.Combine(AppContext.BaseDirectory, @$"_Hashout\WinLogin_Extraction_HashFile_{timestamp}.txt");
        await File.WriteAllLinesAsync(editOutputFilePath, list)
            .ConfigureAwait(false);

        return true;
    }

}
