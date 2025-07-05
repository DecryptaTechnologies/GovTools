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

[ExtractorType(ExtractorTypes.EthereumWallet)]
public class EthereumWalletExtractor : ExtractorBase,
    ICanExtractHashesFromFileAsync,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait,
    ICanPostProcessHashOutputFileAsync
{

    protected Process? _process;

    public override string Name => "MyEtherWallet (Keystore)";

    public override string? ImageUrl => "/Resources/extractor-myether.png";

    public override string Hint => $"{_translator.Translate("Extractors.ETH")}";


    public string SelectedFile { get; set; }

    public EthereumWalletExtractor(
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
        var py2FilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Py2\App\Python\PP2.exe");
        var scriptFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\JtR\run\ethereum2john.py");

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\Ethereum_Extraction_{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}.txt";
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

    public async Task<bool> PostProcessHashOutputFileAsync(string filePath)
    {
        var fileContents = await File
            .ReadAllTextAsync(filePath)
            .ConfigureAwait(false);

        var indexOfTargetPortion = fileContents.IndexOf("$");
        if (indexOfTargetPortion == -1)
            return false;

        fileContents = fileContents
            .Substring(indexOfTargetPortion);

        await Task
            .Delay(500)
            .ConfigureAwait(false);

        await File
            .WriteAllTextAsync(filePath, fileContents)
            .ConfigureAwait(false);

        return true;
    }

}
