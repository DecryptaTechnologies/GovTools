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

[ExtractorType(ExtractorTypes.VeraCryptBootPartition)]
public class VeraCryptBootPartitionExtractor : ExtractorBase,
    ICanExtractHashesFromFileAsync,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait
{

    protected Process? _process;

    public override string Name => "VeraCrypt (Boot-Partition)";

    public override string? ImageUrl => "/Resources/extractor-veracrypt.png";

    public override string Hint => $"{_translator.Translate("Extractors.VeraCryptBoot")}";


    string _selectedFilePath;

    public VeraCryptBootPartitionExtractor(
        IIniFile iniFile,
        ITranslator translator
    ) : base(iniFile, translator)
    {
        _selectedFilePath = "";
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
        _selectedFilePath = e.Value;
    }

    public bool SupportsFileName(string fileName)
        => true;

    public override Task<bool?> RunAsync()
        => ExtractHashesFromFileAsync(_selectedFilePath);

    public async Task<bool?> ExtractHashesFromFileAsync(string filePath)
    {
        //var dd2FilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\DD\dd2.exe");
        //sb.AppendLine($"call \"{dd2FilePath}\" if=\"{filePath}\" of=\"{outputFilePath}\" skip=128 bs=512 count=1");
        //sb.AppendLine($"call \"{dd2FilePath}\" if=\"{filePath}\" of=\"{outputFilePath}\" skip=256 bs=512 count=1");

        var py3FilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Py3\App\Python\PP3.exe");
        var scriptFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\VT\veracrypt2hashcat.py");
        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\VeraCrypt_BootPartition_Extraction_{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}.txt";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);
        var batFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Temp\Extraction.bat");
        var sb = new StringBuilder();

        sb.AppendLine($"call \"{py3FilePath}\" \"{scriptFilePath}\" --offset bootable \"{filePath}\" >> \"{outputFilePath}\"");
        sb.AppendLine($"call \"{py3FilePath}\" \"{scriptFilePath}\" --offset hidden \"{filePath}\" >> \"{outputFilePath}\"");
        sb.AppendLine($"call \"{py3FilePath}\" \"{scriptFilePath}\" --offset bootable+hidden \"{filePath}\" >> \"{outputFilePath}\"");

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
