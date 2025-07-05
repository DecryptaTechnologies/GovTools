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

[ExtractorType(ExtractorTypes.MetaMaskWallet)]
public class MetaMaskWalletExtractor : ExtractorBase,
    ICanExtractHashesFromFolderAsync,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait
{

    protected Process? _process;

    public override string Name => "MetaMask (Password)";

    public override string? ImageUrl => "/Resources/extractor-metamask.png";

    public override string Hint => $"{_translator.Translate("Extractors.MetaMask")}";

    public string SelectedFolder { get; set; }

    public MetaMaskWalletExtractor(
        IIniFile iniFile,
        ITranslator translator
    ) : base(iniFile, translator)
    {
        SelectedFolder = "";
    }

    public override List<IExtractorStepViewModel> GetRequiredScreens()
    {
        var folderOrFileExtractorStepViewModel = App.Container.Resolve<FolderOrFileExtractorStepViewModel>();
        folderOrFileExtractorStepViewModel.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        folderOrFileExtractorStepViewModel.Filter = "LDB Files (*.ldb)| *.ldb";
        folderOrFileExtractorStepViewModel.FileSelected += This_FileSelected;
        return [
            folderOrFileExtractorStepViewModel,
        ];
    }

    private void This_FileSelected(object? sender, StringEventArgs e)
    {
        SelectedFolder = e.Value;
    }

    public bool SupportsFileName(string fileName)
        => true;

    public override Task<bool?> RunAsync()
    {
        var selectedFolderPath = Path.GetDirectoryName(SelectedFolder);
        if (string.IsNullOrEmpty(selectedFolderPath))
            return Task.FromResult((bool?)false);
        return ExtractHashesFromFolderAsync(selectedFolderPath);
    }

    public async Task<bool?> ExtractHashesFromFolderAsync(string folderPath)
    {
        var exeFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\GTP\MM\GovTools_MetaMask_Extractor.exe");

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\MetaMask_Extraction_Hash_{timestamp}.txt";
        var jsonVaultOutputPath = $@"_Hashout\MetaMask_Extraction_VaultJSON_{timestamp}.txt";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);
        var jsonOutputFilePath = Path.Combine(GetHashoutDirectory(), jsonVaultOutputPath);
        var batFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Temp\Extraction.bat");
        var sb = new StringBuilder();

        sb.AppendLine($"call \"{exeFilePath}\" \"{folderPath}\" \"{outputFilePath}\" \"{jsonOutputFilePath}\"");

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
