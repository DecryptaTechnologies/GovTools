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

[ExtractorType(ExtractorTypes.MozillaFirefoxMasterPassword)]
public class MozillaFirefoxMasterPasswordExtractor : ExtractorBase,
    ICanExtractHashesFromFileAsync,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait
{

    protected Process? _process;

    public override string Name => "Mozilla-Firefox (Master Password)";

    public override string? ImageUrl => "/Resources/extractor-firefox.png";

    public override string Hint => $"{_translator.Translate("Extractors.Firefox")}";

    public string SelectedFile { get; set; }

    public MozillaFirefoxMasterPasswordExtractor(
        IIniFile iniFile,
        ITranslator translator
    ) : base(iniFile, translator)
    {
        SelectedFile = "";
    }

    public override List<IExtractorStepViewModel> GetRequiredScreens()
    {
        var folderOrFileExtractorStepViewModel = App.Container.Resolve<FolderOrFileExtractorStepViewModel>();
        folderOrFileExtractorStepViewModel.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        folderOrFileExtractorStepViewModel.Filter = "DB Files (*.db)| *.db";
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
        => fileName.ToLower().EndsWith(".db");

    public override Task<bool?> RunAsync()
        => ExtractHashesFromFileAsync(SelectedFile);

    public async Task<bool?> ExtractHashesFromFileAsync(string filePath)
    {
        var py3FilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Py3\App\Python\PP3.exe");
        var scriptFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\GTP\Moz\mozilla2hashcat.py");

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\Firefox_Extraction_{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}.txt";
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

        return await ((ICanCheckHashOutputFileAsync)this)
            .CheckHashOutputFileAsync(outputFilePath)
            .ConfigureAwait(false);
    }

}
