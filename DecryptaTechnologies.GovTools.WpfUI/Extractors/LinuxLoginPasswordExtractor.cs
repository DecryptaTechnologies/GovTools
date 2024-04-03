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

namespace DecryptaTechnologies.GovTools.WpfUI.Extractors;

[ExtractorType(ExtractorTypes.LinuxLoginPassword)]
public class LinuxLoginPasswordExtractor : ExtractorBase
{

    protected Process? _process;

    public override string Name => "Linux Login Password";

    public override string? ImageUrl => "/Resources/extractor-linux.png";

    public override string Hint => $"{_translator.Translate("Extractors.LinuxLogin")}";

    string _selectedFilePath;


    public LinuxLoginPasswordExtractor(
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

    public override async Task<bool?> RunAsync()
    {
        var fileLines = await File
            .ReadAllLinesAsync(_selectedFilePath)
            .ConfigureAwait(false);

        var userHashes = new List<string>();
        foreach (var line in fileLines)
        {
            if (!line.Contains(":$6$"))
                continue;
            var lineParts = line.Split(":");
            if (lineParts.Length >= 2)
                userHashes.Add(lineParts[1]);
        }

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var editOutputFilePath = Path.Combine(GetHashoutDirectory(), @$"_Hashout\LinuxLogin_Extraction_HashFile_{timestamp}.txt");
        await File.WriteAllLinesAsync(editOutputFilePath, userHashes)
            .ConfigureAwait(false);

        return true;
    }

}
