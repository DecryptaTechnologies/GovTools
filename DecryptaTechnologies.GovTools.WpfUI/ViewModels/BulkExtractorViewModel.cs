using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using Microsoft.Win32;
using rskibbe.Ini.Contracts;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class BulkExtractorViewModel : ScreenViewModelBase
{

    protected IIniFile _iniFile;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOnlyWordlistChecked), nameof(IsFullExtractionChecked))]
    BulkExtractorModes _selectedBulkExtractorMode;

    public bool IsOnlyWordlistChecked => SelectedBulkExtractorMode == BulkExtractorModes.OnlyWordlist;

    public bool IsFullExtractionChecked => SelectedBulkExtractorMode == BulkExtractorModes.FullExtraction;

    [ObservableProperty]
    string _selectedImageFilePath;

    [ObservableProperty]
    string _wordLengthFromText;

    [ObservableProperty]
    string _wordLengthToText;

    [ObservableProperty]
    bool _isExtracting;

    Process? _extractorProcess;

    CancellationTokenSource? _extractorProcessCts;

    public BulkExtractorViewModel(
        IIniFile iniFile    
    )
    {
        _iniFile = iniFile;

        _selectedBulkExtractorMode = BulkExtractorModes.OnlyWordlist;
        _selectedImageFilePath = "";
        _wordLengthFromText = "";
        _wordLengthToText = "";
    }

    [RelayCommand]
    public void UseOnlyWordlist()
    {
        SelectedBulkExtractorMode = BulkExtractorModes.OnlyWordlist;
    }

    [RelayCommand]
    public void UseFullExtraction()
    {
        SelectedBulkExtractorMode = BulkExtractorModes.FullExtraction;
    }

    [RelayCommand]
    public void SelectImageFilePath()
    {
        var ofd = new OpenFileDialog();
        ofd.Filter = "All Files (*.*)| *.*";
        ofd.InitialDirectory = AppContext.BaseDirectory;
        var result = ofd.ShowDialog();
        if (result != true)
            return;
        SelectedImageFilePath = ofd.FileName;
    }

    [RelayCommand]
    public void ClearBulkFile()
    {
        SelectedImageFilePath = "";
    }

    [RelayCommand]
    public void Stop()
    {
        if (_extractorProcessCts != null)
            _extractorProcessCts.Cancel();

        // TODO: separate class? -> <xy>.TryKillAllProcesses?
        try
        {
            foreach (var proc in Process.GetProcessesByName("bulk_extractor64"))
                proc.Kill();
        }
        catch (Exception)
        {
            // NO-OP
        }
    }

    [RelayCommand]
    public async Task ExtractAsync()
    {
        if (string.IsNullOrEmpty(SelectedImageFilePath))
        {
            MessageBox.Show("Please select an image file to bulk extract from", "GovTools", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        IsExtracting = true;

        var targetFile = Path.Combine(GetWordlistsDirectory(), $"Wordlist_BulkExtractor_{DateTime.Now:yyyyMMdd_HHmmss}");

        var minW = WordLengthFromText != "" ? $" -S word_min={WordLengthFromText} " : "";
        var maxW = WordLengthToText != "" ? $" -S word_max={WordLengthToText} " : "";

        string arguments;
        if (SelectedBulkExtractorMode == BulkExtractorModes.OnlyWordlist)
        {
            arguments = $"/c bulk_extractor64.exe {minW}{maxW}-E wordlist -o \"{targetFile}\" \"{SelectedImageFilePath}\"";
        }
        else
        {
            arguments = $"/c bulk_extractor64.exe {minW}{maxW}-e base16 -e facebook -e outlook -e hiberfile -e wordlist -e xor -o \"{targetFile}\" \"{SelectedImageFilePath}\"";
        }

        // TODO: external class?
        _extractorProcess = new Process()
        {
            StartInfo = new()
            {
                FileName = "cmd",
                Arguments = arguments,
                WorkingDirectory = Path.Combine(AppContext.BaseDirectory, @"Packages\BuEx"),
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };

        _extractorProcessCts = new CancellationTokenSource();
        try
        {
            _extractorProcess.Start();
        }
        catch (Exception ex)
        {
            IsExtracting = false;
            var msg = $"Couldn't run the bulk extraction process:{Environment.NewLine}{Environment.NewLine}{ex.Message}";
            MessageBox.Show(msg, "GovTools", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await _extractorProcess.WaitForExitAsync(_extractorProcessCts.Token);
        }
        catch (OperationCanceledException)
        {
            // NO-OP
        }
        finally
        {
            IsExtracting = false;
        }
    }

    public enum BulkExtractorModes
    {
        OnlyWordlist,
        FullExtraction,
    }

    // TODO: robert, refactor
    public string GetWordlistsDirectory()
    {
        var govCrackerPath = _iniFile.GetString("General", "GovCrackerPath");
        if (string.IsNullOrEmpty(govCrackerPath))
            return Path.Combine(AppContext.BaseDirectory, "_Wordlists");
        else
            return Path.Combine(govCrackerPath, "_Wordlists");
    }

}
