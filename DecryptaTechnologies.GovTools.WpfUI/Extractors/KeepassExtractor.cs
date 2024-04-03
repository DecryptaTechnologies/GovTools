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

[ExtractorType(ExtractorTypes.Keepass)]
public class KeepassExtractor : ExtractorBase,
    ICanExtractHashesFromFileAsync,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait,
    ICanPostProcessHashOutputFileAsync
{

    protected Process? _process;

    string _optionalKeyFilePath;

    string _databaseFilePath;

    public override string? ImageUrl => "/Resources/extractor-keepass.png";

    public override string Hint => $"{_translator.Translate("KeepassDialog.Title")}";

    public override string Name => "KeePass Password Safe";

    public KeepassExtractor(
        IIniFile iniFile,
        ITranslator translator
    ) : base(iniFile, translator)
    {
        _optionalKeyFilePath = "";
        _databaseFilePath = "";
    }

    public override List<IExtractorStepViewModel> GetRequiredScreens()
    {
        var keepassKeyFileSelectionViewModel = App.Container.Resolve<KeepassKeyFileSelectionViewModel>();
        keepassKeyFileSelectionViewModel.Completed += KeepassKeyFileSelectionViewModel_Completed;

        var folderOrFileExtractorStepViewModel = App.Container.Resolve<FolderOrFileExtractorStepViewModel>();
        folderOrFileExtractorStepViewModel.Filter = "KeePass Database (*.kdbx)| *.kdbx";
        folderOrFileExtractorStepViewModel.FileSelected += This_FileSelected;
        return [
            keepassKeyFileSelectionViewModel,
            folderOrFileExtractorStepViewModel,
        ];
    }

    private void KeepassKeyFileSelectionViewModel_Completed(object? sender, EventArgs e)
    {
        var keepassKeyFileSelectionViewModel = sender as KeepassKeyFileSelectionViewModel;
        if (keepassKeyFileSelectionViewModel == null)
            return;
        _optionalKeyFilePath = keepassKeyFileSelectionViewModel.SelectedKeyFilePath;
    }

    private void This_FileSelected(object? sender, StringEventArgs e)
    {
        _databaseFilePath = e.Value;
    }

    public bool SupportsFileName(string fileName)
        => true;

    public override void Reset()
    {
        base.Reset();
        _optionalKeyFilePath = "";
        _databaseFilePath = "";
    }

    public override Task<bool?> RunAsync()
        => ExtractHashesFromFileAsync(_databaseFilePath);

    public async Task<bool?> ExtractHashesFromFileAsync(string filePath)
    {
        var keepassExtractorFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\JtR\run\keepass2john.exe");
  
        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\Keepass_File_Extraction_{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}.txt";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);
        var batFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Temp\Extraction.bat");
        var sb = new StringBuilder();

        var dynamicPart = "";
        var keyfileProvided = !string.IsNullOrEmpty(_optionalKeyFilePath);
        if (keyfileProvided)
            dynamicPart = $"-k \"{_optionalKeyFilePath}\" ";
        sb.AppendLine($"call \"{keepassExtractorFilePath}\" {dynamicPart}\"{_databaseFilePath}\" > \"{outputFilePath}\"");

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

        if (await ((ICanCheckHashOutputFileAsync)this)
            .CheckHashOutputFileAsync(outputFilePath)
            .ConfigureAwait(false) == false)
            return false;

        await Task.Delay(500)
            .ConfigureAwait(false);

        var postProcessResult = await PostProcessHashOutputFileAsync(outputFilePath)
            .ConfigureAwait(false);

        return postProcessResult;
    }

    public async Task<bool> PostProcessHashOutputFileAsync(string filePath)
    {
        var results = await File
            .ReadAllTextAsync(filePath)
            .ConfigureAwait(false);

        var indexOfTargetPortion = results.IndexOf("$keepass$");
        if (indexOfTargetPortion == -1)
            return false;

        results = results
            .Substring(indexOfTargetPortion + "$keepass$".Length);

        await Task
            .Delay(500)
            .ConfigureAwait(false);

        await File
            .WriteAllTextAsync(filePath, $"$keepass${results}")
            .ConfigureAwait(false);

        return true;
    }

}
