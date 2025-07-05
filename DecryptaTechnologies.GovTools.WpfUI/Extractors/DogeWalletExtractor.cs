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

[ExtractorType(ExtractorTypes.DogecoinWallet)]
public class DogeWalletExtractor : ExtractorBase,
    ICanExtractHashesFromFileAsync,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait
{

    protected Process? _process;

    public override string Name => "Dogecoin";

    public override string? ImageUrl => "/Resources/extractor-dogecoin.png";

    public override string Hint => $"{_translator.Translate("Extractors.Dogecoin")}";

    public string SelectedFile { get; set; }

    public DogeWalletExtractor(
        IIniFile iniFile,
        ITranslator translator
    ) : base(iniFile, translator)
    {
        SelectedFile = "";
    }

    public override List<IExtractorStepViewModel> GetRequiredScreens()
    {
        var folderOrFileExtractorStepViewModel = App.Container.Resolve<FolderOrFileExtractorStepViewModel>();
        folderOrFileExtractorStepViewModel.Filter = "Wallet.dat Files (*.dat)| *.dat";
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
        => fileName.ToLower().EndsWith("wallet.dat");

    public override Task<bool?> RunAsync()
        => ExtractHashesFromFileAsync(SelectedFile);

    public async Task<bool?> ExtractHashesFromFileAsync(string filePath)
    {
        var py2FilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Py2\App\Python\PP2.exe");
        var scriptFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\JtR\run\bitcoin2john.py");

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\Dogecoin_Extraction_{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}.txt";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);
        var batFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Temp\Extraction.bat");
        var sb = new StringBuilder();

        sb.AppendLine($"call \"{py2FilePath}\" \"{scriptFilePath}\" \"{filePath}\" > \"{outputFilePath}\"");

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
