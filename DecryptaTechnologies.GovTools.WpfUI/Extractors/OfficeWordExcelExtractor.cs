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

[ExtractorType(ExtractorTypes.OfficeWordExcel)]
public class OfficeWordExcelExtractor : ExtractorBase,
    ICanExtractHashesFromFileAsync,
    ICanExtractHashesFromFolderAsync,
    ICanCheckHashOutputFileAsync
{

    protected Process? _process;

    public override string Name => "Office (Word, Excel, etc.)";

    public override string? ImageUrl => "/Resources/extractor-office.png";

    public override string Hint => $"{_translator.Translate("Extractors.Office")}";


    string _selectedFileOrFolder;

    bool _isFileMode;

    public OfficeWordExcelExtractor(
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
        folderOrFileExtractorStepViewModel.Filter = "Office Files (*.do*;*.xl*)|*.xls;*.xlsx;*.xlsm;*.xltm;*.doc;*.docs;*.dot;*.dotm;*.docm";
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
        var supportedExtensions = new List<string>() { ".xls", ".xlsx", ".xlsm", ".xltm", ".doc", ".docx", ".dot", ".dotm", ".docm", };
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
        var py2FilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Py2\App\Python\PP2.exe");
        var scriptFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\GTP\Office\office2hashcat.py");

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\Office_Extraction_{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}.txt";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);
        var batFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Temp\Extraction.bat");
        var sb = new StringBuilder();

        sb.AppendLine("chcp 65001");
        sb.AppendLine($"call \"{py2FilePath}\" \"{scriptFilePath}\" \"{filePath}\" > \"{outputFilePath}\"");

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
            if (!IsOfficeFile(file))
                continue;
            var wasSuccessful = await ExtractHashesFromFileAsync(file)
                .ConfigureAwait(false);
            if (wasSuccessful == null)
                return null;
            results.Add(new(file, wasSuccessful));
        }
        return results.All(x => x.Item2 == true);
    }

    public bool IsOfficeFile(string filePath)
    {
        var officeExtensions = new HashSet<string>() { ".xls", ".xlsx", ".xlsm", ".xltm", ".doc", ".docx", ".dot", ".dotm", ".docm" };
        var extension = Path.GetExtension(filePath);
        return officeExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

}
