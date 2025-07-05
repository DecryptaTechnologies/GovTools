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
using System.Windows;

namespace DecryptaTechnologies.GovTools.WpfUI.Extractors;

[ExtractorType(ExtractorTypes.APFSAppleMacbooks)]
public class ApfsAppleMacbooksExtractor : ExtractorBase,
    ICanExtractHashesFromFileAsync,
    IFileSizeBiggerZeroHashOutputFileCheckerTrait
{

    protected IWindowsService _windowsService;

    protected Process? _process;

    public string SelectedFile { get; set; }

    public override string? ImageUrl => "/Resources/extractor-apfs.png";

    public override string Hint => $"{_translator.Translate("Extractors.APFS")}";

    public override string Name => "APFS (MacBooks / iMacs)";

    public ApfsAppleMacbooksExtractor(
        IWindowsService windowsService,
        IIniFile iniFile,
        ITranslator translator
    ) : base(iniFile, translator)
    {
        _windowsService = windowsService;
        SelectedFile = "";
    }

    public override List<IExtractorStepViewModel> GetRequiredScreens()
    {
        var apfsUbuntuInstallationViewModel = App.Container.Resolve<ApfsUbuntuInstallationViewModel>();

        var folderOrFileExtractorStepViewModel = App.Container.Resolve<FolderOrFileExtractorStepViewModel>();
        folderOrFileExtractorStepViewModel.FileSelected += This_FileSelected;
        return [
            apfsUbuntuInstallationViewModel,
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
        var apfsFolder = Path.Combine(AppContext.BaseDirectory, @"Packages\APFS");
        var apfsShellFilePath = Path.Combine(apfsFolder, "apfs.sh");

        var nl = Environment.NewLine;
        var ubuntuSafePath = $"{SelectedFile[..2].ToLower()}{SelectedFile[2..]}"
            .Replace("\\", "/")
            .Replace(":", "");

        var fileName = $"Extraction_APFS_Output_{Path.GetFileNameWithoutExtension(filePath)}.txt";
        var buildOutputDirectory = Path.Combine(AppContext.BaseDirectory, @"Packages\APFS\apfs2hashcat\build");
        var outputFilePath = Path.Combine(buildOutputDirectory, fileName);

        var cmd = $"#!/bin/bash{nl}sudo -s apt update && sudo apt install fuse libfuse3-dev bzip2 libbz2-dev cmake g++ git libattr1-dev zlib1g-dev && cd apfs2hashcat && cd build && ./apfs-dump-quick /mnt/\"{ubuntuSafePath}\" {fileName}{nl}read -rsn 1 -p 'Press any key to continue . . . ';echo";
        await File.WriteAllTextAsync(apfsShellFilePath, cmd)
            .ConfigureAwait(false);

        var ubuntuFileNameToRun = "ubuntu2004.exe";
        if (!_windowsService.IsExecutableInPath(ubuntuFileNameToRun))
        {
            if (!_windowsService.IsExecutableInPath("ubuntu2204.exe"))
            {
                MessageBox.Show($"{_translator.Translate("Extractors.Ubuntu")}", "GovTools", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else
            {
                ubuntuFileNameToRun = "ubuntu2204.exe";
            }
        }

        _process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                WorkingDirectory = apfsFolder,
                Arguments = $"/c {ubuntuFileNameToRun} run sudo -s bash apfs.sh"
            }
        };
        _process.Start();
        await _process.WaitForExitAsync()
            .ConfigureAwait(false);

        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(outputFilePath);
        var targetFile = new DirectoryInfo(buildOutputDirectory)
            .GetFiles($"{fileNameWithoutExt}.*")
            .SingleOrDefault();

        if (targetFile == null)
            return false;

        if (await ((ICanCheckHashOutputFileAsync)this)
            .CheckHashOutputFileAsync(targetFile.FullName)
            .ConfigureAwait(false) == false)
            return false;

        await Task.Delay(500)
            .ConfigureAwait(false);

        var postProcessResult = await PostProcessHashOutputFileAsync(targetFile.FullName)
            .ConfigureAwait(false);

        return postProcessResult;
    }

    public async Task<bool> PostProcessHashOutputFileAsync(string filePath)
    {
        var targetFileLines = await File
            .ReadAllLinesAsync(filePath)
            .ConfigureAwait(false);

        ApfExtractionDataset? lastDataset = null;
        var datasets = new List<ApfExtractionDataset>();
        foreach (var line in targetFileLines)
        {
            if (line.StartsWith("UUID"))
            {
                var uuid = line.Split(":").LastOrDefault()?.Trim();
                if (lastDataset != null)
                {
                    datasets.Add(lastDataset);
                    lastDataset = new ApfExtractionDataset()
                    {
                        UUID = $"{uuid}"
                    };
                }
                else
                {
                    lastDataset = new ApfExtractionDataset()
                    {
                        UUID = $"{uuid}"
                    };
                }
            }
            else if (line.StartsWith("KEK Wrpd"))
            {
                var kekWrpd = line.Split(":").LastOrDefault()?.Trim();
                lastDataset!.KEKWrpd = $"{kekWrpd}";
            }
            else if (line.StartsWith("Iterat's"))
            {
                var iterations = line.Split(":").LastOrDefault()?.Trim();
                lastDataset!.Iterations = $"{iterations}";
            }
            else if (line.StartsWith("Salt"))
            {
                var salt = line.Split(":").LastOrDefault()?.Trim();
                lastDataset!.Salt = $"{salt}";
            }
        }
        if (lastDataset != null && !datasets.Contains(lastDataset))
            datasets.Add(lastDataset);

        if (datasets.Count == 3)
        {
            // move middle to top
            datasets = [datasets[1], datasets[2], datasets[0]];
        }

        if (datasets.Count == 0)
            return false;

        var hashoutFileLines = new List<string>();
        var beautfiedLogFileLines = new List<string>();
        foreach (var dataset in datasets)
        {
            var hash = $"$fvde$2$16${dataset.Salt}${dataset.Iterations}${dataset.KEKWrpd}";
            hashoutFileLines.Add(hash);

            beautfiedLogFileLines.Add($"UUID:       {dataset.UUID}");
            beautfiedLogFileLines.Add($"Salt:       {dataset.Salt}");
            beautfiedLogFileLines.Add($"Iterations: {dataset.Iterations}");
            beautfiedLogFileLines.Add($"KEK Wrpd:   {dataset.KEKWrpd}");
            beautfiedLogFileLines.Add($"Hash:       {hash}");
            beautfiedLogFileLines.Add("");
            beautfiedLogFileLines.Add("");
        }

        var timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        var relativePath = @$"_Hashout\APFS_Extraction_Hash_{Path.GetFileNameWithoutExtension(SelectedFile)}_{timestamp}.txt";
        var outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);

        await File
            .WriteAllLinesAsync(outputFilePath, hashoutFileLines, Encoding.UTF8)
            .ConfigureAwait(false);

        timestamp = $"{DateTime.Now:ddMMyy_HHmmssfff}";
        relativePath = @$"_Hashout\APFS_Rawfile_UUID_{Path.GetFileNameWithoutExtension(SelectedFile)}_{timestamp}.txt";
        outputFilePath = Path.Combine(GetHashoutDirectory(), relativePath);

        await File
            .WriteAllLinesAsync(outputFilePath, beautfiedLogFileLines, Encoding.UTF8)
            .ConfigureAwait(false);

        return true;
    }

    public class ApfExtractionDataset
    {

        public string UUID { get; set; }

        public string KEKWrpd { get; set; }

        public string Iterations { get; set; }

        public string Salt { get; set; }

        public ApfExtractionDataset()
        {
            UUID = "";
            KEKWrpd = "";
            Iterations = "";
            Salt = "";
        }

    }

}
