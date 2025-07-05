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

[ExtractorType(ExtractorTypes.TrueCryptPartition)]
public class TrueCryptPartitionExtractor : ExtractorBase,
    ICanExtractHashesFromFileAsync,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait
{

    protected Process? _process;

    public override string Name => "TrueCrypt (Partition)";

    public override string? ImageUrl => "/Resources/extractor-truecrypt.png";

    public override string Hint => $"{_translator.Translate("Extractors.TrueCryptPartition")}";

    public string SelectedFile { get; set; }

    public TrueCryptPartitionExtractor(
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
        => true;

    public override Task<bool?> RunAsync()
        => ExtractHashesFromFileAsync(SelectedFile);

    public async Task<bool?> ExtractHashesFromFileAsync(string filePath)
    {
        //var dd2FilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\DD\dd2.exe");
        // sb.AppendLine($"call \"{dd2FilePath}\" if=\"{filePath}\" of=\"{outputFilePath}\" skip=128 bs=512 count=1"); 
        var py3FilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Py3\App\Python\PP3.exe");
        var scriptFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\VT\truecrypt2hashcat.py");

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\TrueCrypt_Partition_Extraction_{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}.txt";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);
        var batFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Temp\Extraction.bat");
        var sb = new StringBuilder();

        sb.AppendLine("chcp 65001");
        sb.AppendLine($"call \"{py3FilePath}\" \"{scriptFilePath}\" \"{filePath}\" >> \"{outputFilePath}\"");
        sb.AppendLine($"call \"{py3FilePath}\" \"{scriptFilePath}\" --offset hidden \"{filePath}\" >> \"{outputFilePath}\"");

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
}
