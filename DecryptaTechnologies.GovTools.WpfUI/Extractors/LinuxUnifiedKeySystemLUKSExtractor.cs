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

[ExtractorType(ExtractorTypes.LinuxUnifiedKeySystemLUKS)]
public class LinuxUnifiedKeySystemLUKSExtractor : ExtractorBase,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait
{

    protected Process? _process;

    public override string Name => "LUKS Encryption";

    public override string? ImageUrl => "/Resources/extractor-luks.png";

    public override string Hint => $"{_translator.Translate("Extractors.LUKS")}";


    string _selectedFile;

    public LinuxUnifiedKeySystemLUKSExtractor(
        IIniFile iniFile,
        ITranslator translator
    ) : base(iniFile, translator)
    {
        _selectedFile = "";
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
        _selectedFile = e.Value;
    }

    public bool SupportsFileName(string fileName)
        => true;

    public override Task<bool?> RunAsync()
        => ExtractHashesFromFileAsync(_selectedFile);

    public async Task<bool?> ExtractHashesFromFileAsync(string filePath)
    {
        var py3FilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Py3\App\Python\PP3.exe");
        var scriptFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\GTP\LUKS\luks2hashcat.py");

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\LUKS_Extraction_{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}.txt";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);
        var batFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Temp\Extraction.bat");
        var sb = new StringBuilder();

        // sb.AppendLine($"call \"{dd2FilePath}\" if=\"{filePath}\" of=\"{outputFilePath}\" bs=512 count=4097");
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
