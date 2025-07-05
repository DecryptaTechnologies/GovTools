using Autofac;
using DecryptaTechnologies.GovTools.Domain.Attributes;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.Domain.ValueObjects;
using DecryptaTechnologies.GovTools.WpfUI.ValueObjects;
using DecryptaTechnologies.GovTools.WpfUI.ViewModels;
using rskibbe.I18n.Contracts;
using rskibbe.Ini.Contracts;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DecryptaTechnologies.GovTools.WpfUI.Extractors;

[ExtractorType(ExtractorTypes.Bitlocker)]
public class BitlockerExtractor : ExtractorBase,
    ICanExtractHashesFromFileAsync,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait,
    ICanPostProcessHashOutputFileAsync
{

    Process? _process;

    public override string Name => "Bitlocker";

    public override string? ImageUrl => "/Resources/extractor-bitlocker.png";

    public override string Hint => $"{_translator.Translate("Extractors.Bitlocker")}";

    public string SelectedFile { get; set; }

    public BitlockerExtractor(
        IIniFile iniFile,
        ITranslator translator
    ) : base(iniFile, translator)
    {
        SelectedFile = "";
    }

    public override List<IExtractorStepViewModel> GetRequiredScreens()
    {
        var folderOrFileExtractorStepViewModel = App.Container.Resolve<FolderOrFileExtractorStepViewModel>();
        folderOrFileExtractorStepViewModel.FileSelected += This_FileSelected;
        return [
            folderOrFileExtractorStepViewModel,
        ];
    }

    private void This_FileSelected(object? sender, StringEventArgs e)
    {
        SelectedFile = e.Value;
    }

    public bool SupportsFileName(string fileName)
    {
        var supportedExtensions = new List<string>() { ".001", ".dd", ".img" };
        var fileExtension = Path.GetExtension(fileName);
        return supportedExtensions.Contains(fileExtension);
    }

    public override Task<bool?> RunAsync()
        => ExtractHashesFromFileAsync(SelectedFile);

    public async Task<bool?> ExtractHashesFromFileAsync(string filePath)
    {
        var bitlockerExePath = Path.Combine(AppContext.BaseDirectory, @"Packages\JtR\run\bitlocker2john.exe");

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\Bitlocker_Extraction_{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}.txt";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);

        var batFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Temp\Extraction.bat");
        var sb = new StringBuilder();
        sb.AppendLine("chcp 65001");
        sb.AppendLine("echo off");
        sb.AppendLine($"Set LOGFILE=\"{outputFilePath}\"");
        sb.AppendLine("Call :LOG > %LOGFILE%");
        sb.AppendLine();
        sb.AppendLine("Exit /b");
        sb.AppendLine();
        sb.AppendLine(":LOG");
        sb.AppendLine($"Call \"{bitlockerExePath}\" -i \"{filePath}\"");
        await File.WriteAllTextAsync(batFilePath, sb.ToString(), new UTF8Encoding(false))
            .ConfigureAwait(false);

        await Task.Delay(1000)
            .ConfigureAwait(false);

        _process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                WorkingDirectory = AppContext.BaseDirectory,
                Arguments = $"/c \"{batFilePath}\"",
            }
        };
        _process.Start();
        await _process.WaitForExitAsync()
            .ConfigureAwait(false);

        if (_stopped)
            return null;

        if (await ((ICanCheckHashOutputFileAsync)this)
            .CheckHashOutputFileAsync(outputFilePath)
            .ConfigureAwait(false) == false)
            return false;

        await Task.Delay(500)
            .ConfigureAwait(false);

        var postProcessResult = await PostProcessHashOutputFileAsync(outputFilePath)
            .ConfigureAwait(false);

        return postProcessResult;
    }

    public async Task<bool> PostProcessHashOutputFileAsync(string filePath)
    {
        var bitlockerResults = await File
            .ReadAllTextAsync(filePath)
            .ConfigureAwait(false);

        var indexOfTargetPortion = bitlockerResults.IndexOf("$1$");
        if (indexOfTargetPortion == -1)
            return false;

        bitlockerResults = bitlockerResults
            .Substring(indexOfTargetPortion)
            .Substring(1, 198);

        await Task
            .Delay(500)
            .ConfigureAwait(false);

        await File
            .WriteAllTextAsync(filePath, $"$bitlocker${bitlockerResults}")
            .ConfigureAwait(false);

        return true;
    }

    public override void Stop()
    {
        if (_process == null)
            return;

        _stopped = true;
        _process.Kill();

        KillAllBitlocker2JohnProcesses();
    }

    private void KillAllBitlocker2JohnProcesses()
    {
        var exeProcess = Process.GetProcessesByName("bitlocker2john");
        if (exeProcess.Length == 0)
            return;

        foreach (var process in exeProcess)
            process.Kill();
    }

}
