using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using Microsoft.Win32;
using rskibbe.Ini.Contracts;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class CombiLenViewModel : ScreenViewModelBase
{

    IIniFile _iniFile;

    [ObservableProperty]
    bool _isCombinating;

    [ObservableProperty]
    string _selectedWordlistFileOne;

    [ObservableProperty]
    string _selectedWordlistFileTwo;

    [ObservableProperty]
    string _selectedWordlistFileThree;

    Process? _combinatorProcess;

    CancellationTokenSource? _combinatorProcessCts;

    [ObservableProperty]
    bool _isLenning;

    [ObservableProperty]
    string _selectedLenWordlistFile;

    [ObservableProperty]
    string _wordLengthFromText;

    [ObservableProperty]
    string _wordLengthToText;

    Process? _lenProcess;

    CancellationTokenSource? _lenProcessCts;

    public CombiLenViewModel(
        IIniFile iniFile
    )
    {
        _iniFile = iniFile;

        _selectedWordlistFileOne = "";
        _selectedWordlistFileTwo = "";
        _selectedWordlistFileThree = "";

        _selectedLenWordlistFile = "";
        _wordLengthFromText = "";
        _wordLengthToText = "";
    }

    [RelayCommand]
    public void SelectWordlistFile(string which)
    {
        var ofd = new OpenFileDialog();
        ofd.Filter = "All Files (*.*)| *.*";
        ofd.InitialDirectory = GetWordlistsDirectory();
        var result = ofd.ShowDialog();
        if (result != true)
            return;
        switch (which)
        {
            case "one":
                SelectedWordlistFileOne = ofd.FileName;
                break;
            case "two":
                SelectedWordlistFileTwo = ofd.FileName;
                break;
            case "three":
                SelectedWordlistFileThree = ofd.FileName;
                break;
        }
    }

    [RelayCommand]
    public async Task StartCombinatorAsync()
    {
        var wordlistFilePaths = new List<string>()
        {
            SelectedWordlistFileOne,
            SelectedWordlistFileTwo,
            SelectedWordlistFileThree,
        };

        var nonEmptyWordlistFilePaths = wordlistFilePaths
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        switch (nonEmptyWordlistFilePaths.Count)
        {
            case 0:
                var msgSelectTwo = "Please select 2 wordlists to be combined";
                MessageBox.Show(msgSelectTwo, "GovTools", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            case 1:
                var msgOneMore = "Please select 1 more wordlist to be combined with the first one";
                MessageBox.Show(msgOneMore, "GovTools", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
        }

        IsCombinating = true;
        var targetFile = Path.Combine(AppContext.BaseDirectory, $@"_Wordlists\Combinator_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

        var escapedNonEmptyWordlistFilePaths = nonEmptyWordlistFilePaths
            .Select(x => $"\"{x}\"");

        var combinatorFileName = nonEmptyWordlistFilePaths.Count == 2 ? "combinator.exe" : "combinator3.exe";

        // TODO: external class?
        _combinatorProcess = new Process()
        {
            StartInfo = new()
            {
                FileName = "cmd",
                Arguments = $"/c {combinatorFileName} {string.Join(" ", escapedNonEmptyWordlistFilePaths)} > \"{targetFile}\"",
                WorkingDirectory = Path.Combine(AppContext.BaseDirectory, @"Packages\GTP\HCUtils"),
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };

        _combinatorProcessCts = new CancellationTokenSource();
        try
        {
            _combinatorProcess.Start();
        }
        catch (Exception ex)
        {
            IsCombinating = false;
            var msg = $"Couldn't run the combinator process:{Environment.NewLine}{Environment.NewLine}{ex.Message}";
            MessageBox.Show(msg, "GovTools", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await _combinatorProcess.WaitForExitAsync(_combinatorProcessCts.Token);
            await Task.Delay(2000, _combinatorProcessCts.Token);
        }
        catch (OperationCanceledException)
        {
            // NO-OP
        }
        finally
        {
            IsCombinating = false;
        }
    }

    [RelayCommand]
    public void StopCombinator()
    {
        if (_combinatorProcessCts != null)
            _combinatorProcessCts.Cancel();

        // TODO: separate class? -> Combinator.TryKillAllProcesses?
        try
        {
            foreach (var proc in Process.GetProcessesByName("combinator"))
                proc.Kill();
            foreach (var proc in Process.GetProcessesByName("combinator3"))
                proc.Kill();
        }
        catch (Exception)
        {
            // NO-OP
        }
    }

    [RelayCommand]
    public void SelectLenWordlistFile()
    {
        var ofd = new OpenFileDialog();
        ofd.Filter = "All Files (*.*)| *.*";
        ofd.InitialDirectory = GetWordlistsDirectory();
        var result = ofd.ShowDialog();
        if (result != true)
            return;
        SelectedLenWordlistFile = ofd.FileName;
    }

    [RelayCommand]
    public async Task StartLenAsync()
    {
        if (string.IsNullOrEmpty(SelectedLenWordlistFile))
        {
            MessageBox.Show("Please select a wordlist file to use for the len process", "GovTools", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!int.TryParse(WordLengthFromText, out var wordLengthFrom) || wordLengthFrom <= 0)
        {
            MessageBox.Show("Please select a starting length to len", "GovTools", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (!int.TryParse(WordLengthToText, out var wordLengthTo) || wordLengthTo < wordLengthFrom)
        {
            MessageBox.Show("Please select a matching 'until' length to len", "GovTools", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        IsLenning = true;

        var targetFile = Path.Combine(GetWordlistsDirectory(), $"Len_Wordlist_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

        // TODO: external class?
        _lenProcess = new Process()
        {
            StartInfo = new()
            {
                FileName = "cmd",
                Arguments = $"/c len.exe {wordLengthFrom} {wordLengthTo} < \"{SelectedLenWordlistFile}\" > \"{targetFile}\"",
                WorkingDirectory = Path.Combine(AppContext.BaseDirectory, @"Packages\GTP\HCUtils"),
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };

        _lenProcessCts = new CancellationTokenSource();
        try
        {
            _lenProcess.Start();
        }
        catch (Exception ex)
        {
            IsLenning = false;
            var msg = $"Couldn't run the len process:{Environment.NewLine}{Environment.NewLine}{ex.Message}";
            MessageBox.Show(msg, "GovTools", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await _lenProcess.WaitForExitAsync(_lenProcessCts.Token);
            await Task.Delay(2000, _lenProcessCts.Token);
        }
        catch (OperationCanceledException)
        {
            // NO-OP
        }
        finally
        {
            IsLenning = false;
        }
    }

    [RelayCommand]
    public void StopLen()
    {
        if (_lenProcessCts != null)
            _lenProcessCts.Cancel();

        // TODO: separate class? -> Len.TryKillAllProcesses?
        try
        {
            foreach (var proc in Process.GetProcessesByName("len"))
                proc.Kill();
        }
        catch (Exception)
        {
            // NO-OP
        }
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

    [RelayCommand]
    public void ClearWordlistFile(string wordlist)
    {
        switch (wordlist)
        {
            case "one":
                SelectedWordlistFileOne = "";
                break;
            case "two":
                SelectedWordlistFileTwo = "";
                break;
            case "three":
                SelectedWordlistFileThree = "";
                break;
            case "len":
                SelectedLenWordlistFile = "";
                break;
        }
    }

}
