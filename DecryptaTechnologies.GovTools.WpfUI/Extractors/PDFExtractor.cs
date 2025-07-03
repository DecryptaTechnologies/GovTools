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

[ExtractorType(ExtractorTypes.PDF)]
public class PDFExtractor : ExtractorBase,
    ICanExtractHashesFromFileAsync,
    ICanExtractHashesFromFolderAsync,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait
{

    protected Process? _process;

    public override string Name => "PDF";

    public override string? ImageUrl => "/Resources/extractor-pdf.png";

    public override string Hint => $"{_translator.Translate("Extractors.PDF")}";

    string _selectedFileOrFolder;

    bool _isFileMode;

    public PDFExtractor(
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
        folderOrFileExtractorStepViewModel.Filter = "PDF Files (*.pdf)| *.pdf";
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
        => fileName.ToLower().EndsWith(".pdf");

    public override Task<bool?> RunAsync()
    {
        if (_isFileMode)
            return ExtractHashesFromFileAsync(_selectedFileOrFolder);
        return ExtractHashesFromFolderAsync(_selectedFileOrFolder);
    }

    public async Task<bool?> ExtractHashesFromFileAsync(string filePath)
    {
        var py2FilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Py2\App\Python\PP2.exe");
        var scriptFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\GTP\PDF\pdf2hashcat.py");

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\PDF_Extraction_{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}.txt";
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

        return await ((ICanCheckHashOutputFileAsync)this)
            .CheckHashOutputFileAsync(outputFilePath)
            .ConfigureAwait(false);
    }

    public async Task<bool?> ExtractHashesFromFolderAsync(string folderPath)
    {
        var pdfResults = new List<Tuple<string, bool?>>();
        var pdfFiles = Directory.GetFiles(folderPath, "*.pdf");
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
