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

[ExtractorType(ExtractorTypes.SevenZip)]
public class SevenZipExtractor : ExtractorBase,
    ICanExtractHashesFromFileAsync,
    ICanExtractHashesFromFolderAsync,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait
{

    Process? _process;

    public override string Name => "7zip";

    public override string? ImageUrl => "/Resources/extractor-sevenzip.png";

    public override string Hint => $"{_translator.Translate("Extractors.SevenZip")}";


    string _selectedFileOrFolder;

    bool _isFileMode;

    public SevenZipExtractor(
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
        folderOrFileExtractorStepViewModel.Filter = "7z Files (*.7z*)|*.7z";
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
        => fileName.ToLower().EndsWith(".7z");

    public override Task<bool?> RunAsync()
    {
        if (_isFileMode)
            return ExtractHashesFromFileAsync(_selectedFileOrFolder);
        return ExtractHashesFromFolderAsync(_selectedFileOrFolder);
    }

    public async Task<bool?> ExtractHashesFromFileAsync(string filePath)
    {
        var scriptFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\GTP\7z2hashcat\7z2hashcat6414.exe");
        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\7zip_Extraction_{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}.txt";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);
        var batFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Temp\Extraction.bat");
        var sb = new StringBuilder();

        sb.AppendLine("chcp 65001");
        sb.AppendLine($"call \"{scriptFilePath}\" \"{filePath}\" > \"{outputFilePath}\"");

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

        try
        {
            KillAll7zip2JohnProcesses();
        }
        catch (Exception)
        {
            // NO-OP
        }

        return true;
    }

    public override void Stop()
    {
        if (_process == null)
            return;

        _stopped = true;
        _process.Kill();

        KillAll7zip2JohnProcesses();
    }

    private void KillAll7zip2JohnProcesses()
    {
        var exeProcess = Process.GetProcessesByName("7z2hashcat6414");
        if (exeProcess.Length == 0)
            return;

        foreach (var process in exeProcess)
            process.Kill();
    }

    public async Task<bool?> ExtractHashesFromFolderAsync(string folderPath)
    {
        var pdfResults = new List<Tuple<string, bool?>>();
        var pdfFiles = Directory.GetFiles(folderPath, "*.7z");
        foreach (var pdfFile in pdfFiles)
        {
            var wasSuccessful = await ExtractHashesFromFileAsync(pdfFile)
                .ConfigureAwait(false);
            if (wasSuccessful == null)
                return null;
            pdfResults.Add(new(pdfFile, wasSuccessful));
        }
        return pdfResults.All(x => x.Item2 == true);
    }

}
