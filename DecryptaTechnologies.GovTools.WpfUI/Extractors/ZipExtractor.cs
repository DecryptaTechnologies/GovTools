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

[ExtractorType(ExtractorTypes.Zip)]
public class ZipExtractor : ExtractorBase,
    ICanExtractHashesFromFileAsync,
    ICanExtractHashesFromFolderAsync,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait
{

    Process? _process;

    public override string Name => "ZIP";

    public override string? ImageUrl => "/Resources/extractor-zip.png";

    public override string Hint => $"{_translator.Translate("Extractors.ZIP")}";


    string _selectedFileOrFolder;

    bool _isFileMode;

    public ZipExtractor(
        IIniFile iniFile,
        ITranslator translator
    ) : base(iniFile, translator)
    {
        _selectedFileOrFolder = "";
        _isFileMode = true;
    }

    public override List<IExtractorStepViewModel> GetRequiredScreens()
    {
        var folderOrFileExtractorStepViewModel = App.Container.Resolve<FolderOrFileExtractorStepViewModel>();
        folderOrFileExtractorStepViewModel.Filter = "ZIP Files (*.zip)| *.zip";
        folderOrFileExtractorStepViewModel.IsFileSelectionEnabled = true;
        folderOrFileExtractorStepViewModel.IsFolderSelectionEnabled = true;
        folderOrFileExtractorStepViewModel.FileSelected += This_FileSelected;
        folderOrFileExtractorStepViewModel.FolderSelected += This_FolderSelected;
        return [
            folderOrFileExtractorStepViewModel,
        ];
    }

    private void This_FileSelected(object? sender, StringEventArgs e)
    {
        _selectedFileOrFolder = e.Value;
        _isFileMode = true;
    }

    private void This_FolderSelected(object? sender, StringEventArgs e)
    {
        _selectedFileOrFolder = e.Value;
        _isFileMode = false;
    }

    public bool SupportsFileName(string fileName)
        => fileName.ToLower().EndsWith(".zip");

    public override Task<bool?> RunAsync()
    {
        if (_isFileMode)
            return ExtractHashesFromFileAsync(_selectedFileOrFolder);
        return ExtractHashesFromFolderAsync(_selectedFileOrFolder);
    }

    public async Task<bool?> ExtractHashesFromFileAsync(string filePath)
    {
        var exeFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\JtR\run\zip2john.exe");

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\ZIP_Extraction_{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}.txt";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);

        var batFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Temp\Extraction.bat");
        var sb = new StringBuilder();

        sb.AppendLine($"call \"{exeFilePath}\" \"{filePath}\" > \"{outputFilePath}\"");

        await File.WriteAllTextAsync(batFilePath, sb.ToString())
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

        await PostProcessHashOutputFileAsync(outputFilePath)
            .ConfigureAwait(false);

        return true;
    }

    public async Task PostProcessHashOutputFileAsync(string filePath)
    {
        var results = await File.ReadAllTextAsync(filePath)
            .ConfigureAwait(false);
        results = results
            .Substring(results.IndexOf("$")).Split(":").FirstOrDefault();

        await Task.Delay(500)
            .ConfigureAwait(false);

        await File.WriteAllTextAsync(filePath, results)
            .ConfigureAwait(false);
    }

    public override void Stop()
    {
        if (_process == null)
            return;

        _stopped = true;
        _process.Kill();

        KillAllZIP2JohnProcesses();
    }

    private void KillAllZIP2JohnProcesses()
    {
        var exeProcess = Process.GetProcessesByName("zip2john");
        if (exeProcess.Length == 0)
            return;

        foreach (var process in exeProcess)
            process.Kill();
    }

    public async Task<bool?> ExtractHashesFromFolderAsync(string folderPath)
    {
        var results = new List<Tuple<string, bool?>>();
        var files = Directory.GetFiles(folderPath, "*.zip");
        foreach (var file in files)
        {
            var wasSuccessful = await ExtractHashesFromFileAsync(file)
                .ConfigureAwait(false);
            if (wasSuccessful == null)
                return null;
            results.Add(new(file, wasSuccessful));
        }
        return results.All(x => x.Item2 == true);
    }

}
