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

[ExtractorType(ExtractorTypes.LibreOfficeOpenOffice)]
public class LibreOfficeOpenOfficeExtractor : ExtractorBase,
    ICanExtractHashesFromFileAsync,
    ICanExtractHashesFromFolderAsync,
    ICanCheckHashOutputFileAsync
{

    protected Process? _process;

    public override string Name => "OpenOffice / LibreOffice";

    public override string? ImageUrl => "/Resources/extractor-openoffice.png";

    public override string Hint => $"{_translator.Translate("Extractors.OpenOffice")}";


    string _selectedFileOrFolder;

    bool _isFileMode;

    public LibreOfficeOpenOfficeExtractor(
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
        folderOrFileExtractorStepViewModel.Filter = "Open/Libre Office Files (*.od*;*.ot*)|*.odt;*.ott;*.ods;*.ots";
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
    {
        var supportedExtensions = new List<string>() { ".odt", ".ott", ".ods", ".ots", };
        var fileExtension = Path.GetExtension(fileName);
        return supportedExtensions.Contains(fileExtension);
    }

    public override Task<bool?> RunAsync()
    {
        if (_isFileMode)
            return ExtractHashesFromFileAsync(_selectedFileOrFolder);
        return ExtractHashesFromFolderAsync(_selectedFileOrFolder);
    }

    public async Task<bool?> ExtractHashesFromFileAsync(string filePath)
    {
        var py3FilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Py3\App\Python\PP3.exe");
        var scriptFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\GTP\LibreOffice\libreoffice2john.py");

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\OpenOffice_Extraction_{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}.txt";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);
        var batFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Temp\Extraction.bat");
        var sb = new StringBuilder();

        sb.AppendLine($"call \"{py3FilePath}\" \"{scriptFilePath}\" \"{filePath}\" > \"{outputFilePath}\"");

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

        return await CheckHashOutputFileAsync(outputFilePath)
            .ConfigureAwait(false);
    }

    public Task<bool?> CheckHashOutputFileAsync(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        bool? isntFileEmpty = fileInfo.Length != 0;
        return Task.FromResult(isntFileEmpty);
    }

    public async Task<bool?> ExtractHashesFromFolderAsync(string folderPath)
    {
        var results = new List<Tuple<string, bool?>>();
        var files = Directory.GetFiles(folderPath, "*.*");
        foreach (var file in files)
        {
            if (!IsLibreOfficeFile(file))
                continue;
            var wasSuccessful = await ExtractHashesFromFileAsync(file)
                .ConfigureAwait(false);
            if (wasSuccessful == null)
                return null;
            results.Add(new(file, wasSuccessful));
        }
        return results.All(x => x.Item2 == true);
    }

    public bool IsLibreOfficeFile(string filePath)
    {
        var libreOfficeExtensions = new HashSet<string>() { ".odt", ".ott", ".ods", ".ots", };
        var extension = Path.GetExtension(filePath);
        return libreOfficeExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

}
