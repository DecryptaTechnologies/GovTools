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

[ExtractorType(ExtractorTypes.ItunesBackupIphone)]
public class ItunesBackupIphoneExtractor : ExtractorBase,
    ICanExtractHashesFromFileAsync,
    ICanCheckHashOutputFileAsync
{

    protected Process? _process;

    public override string Name => "iTunes Backups (iPhone)";

    public override string? ImageUrl => "/Resources/extractor-iphone.png";

    public override string Hint => $"{_translator.Translate("Extractors.iTunes")}";

    string _selectedFile;

    public ItunesBackupIphoneExtractor(
        IIniFile iniFile,
        ITranslator translator
    ) : base(iniFile, translator)
    {
        _selectedFile = "";
    }

    public override List<IExtractorStepViewModel> GetRequiredScreens()
    {
        var folderOrFileExtractorStepViewModel = App.Container.Resolve<FolderOrFileExtractorStepViewModel>();
        folderOrFileExtractorStepViewModel.Filter = "Plist Files (*.plist)| *.plist";
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
        => fileName.ToLower().EndsWith(".plist");

    public override Task<bool?> RunAsync()
        => ExtractHashesFromFileAsync(_selectedFile);

    public async Task<bool?> ExtractHashesFromFileAsync(string filePath)
    {
        //var perlFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Perl\Perl\bin\perl_govtools.exe");
        //var scriptFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\JtR\run\itunes_backup2john.pl");
        var scriptFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\GTP\iTB\iTunesBackup2hashcat.exe");

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = $@"_Hashout\iTunes_Extraction_{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}.txt";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);
        var batFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\Temp\Extraction.bat");
        var sb = new StringBuilder();

        sb.AppendLine("chcp 65001");
        sb.AppendLine($"call \"{scriptFilePath}\" \"{filePath}\" > \"{outputFilePath}\"");

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

}
