using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using Microsoft.Win32;
using rskibbe.Ini.Contracts;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class DupcHashGenViewModel : ScreenViewModelBase
{

    IIniFile _iniFile;

    [ObservableProperty]
    ObservableCollection<string> _hashAlgorithmItems;

    [ObservableProperty]
    string? _selectedHashAlgorithmItem;

    [ObservableProperty]
    string _text;

    [ObservableProperty]
    string _hash;

    [ObservableProperty]
    string _selectedWordlistFile;

    Process? _removeDuplicatesProcess;

    CancellationTokenSource? _removeDuplicatesProcessCts;

    [ObservableProperty]
    bool _isRemovingDuplicates;

    public DupcHashGenViewModel(
        IIniFile iniFile
    )
    {
        _iniFile = iniFile;

        _hashAlgorithmItems = new()
        {
            "MD5",
            "SHA1",
            "SHA256",
            "SHA512",
        };
        _selectedHashAlgorithmItem = _hashAlgorithmItems.First();
        _text = "";
        _hash = "";

        _selectedWordlistFile = "";
    }

    partial void OnTextChanged(string value)
    {
        GenerateHashCommand.Execute(null);
    }

    partial void OnSelectedHashAlgorithmItemChanged(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return;
        GenerateHashCommand.Execute(null);
    }

    [RelayCommand]
    public void SelectWordlistFile()
    {
        var ofd = new OpenFileDialog();
        ofd.InitialDirectory = GetWordlistsDirectory();
        var result = ofd.ShowDialog();
        if (result != true)
            return;
        SelectedWordlistFile = ofd.FileName;
    }

    [RelayCommand]
    public void ClearDupFile()
    {
        SelectedWordlistFile = "";
    }

    [RelayCommand]
    public async Task StartRemovingDuplicatesAsync()
    {
        if (string.IsNullOrEmpty(SelectedWordlistFile))
        {
            MessageBox.Show("Please select a wordlist file", "GovTools", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (!File.Exists(SelectedWordlistFile))
        {
            MessageBox.Show("The selected wordlist file doesn't exist", "GovTools", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        IsRemovingDuplicates = true;
        var scriptFilePath = Path.Combine(AppContext.BaseDirectory, @"Packages\DupC\DupCleaner.py");
        var targetFile = Path.Combine(GetWordlistsDirectory(), $"Wordlist_DUP_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

        // TODO: external class?
        _removeDuplicatesProcess = new Process()
        {
            StartInfo = new()
            {
                FileName = "cmd",
                Arguments = $"/c PP3.exe \"{scriptFilePath}\" --inTxt \"{SelectedWordlistFile}\" --out \"{targetFile}\"",
                WorkingDirectory = Path.Combine(AppContext.BaseDirectory, @"Packages\Py3\App\Python"),
                CreateNoWindow = true,
                UseShellExecute = false,
            }
        };

        _removeDuplicatesProcessCts = new CancellationTokenSource();
        try
        {
            _removeDuplicatesProcess.Start();
        }
        catch (Exception ex)
        {
            IsRemovingDuplicates = false;
            var msg = $"Couldn't run the duplication removal process:{Environment.NewLine}{Environment.NewLine}{ex.Message}";
            MessageBox.Show(msg, "GovTools", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await _removeDuplicatesProcess.WaitForExitAsync(_removeDuplicatesProcessCts.Token);
            await Task.Delay(2000, _removeDuplicatesProcessCts.Token);
        }
        catch (OperationCanceledException)
        {
            // NO-OP
        }
        finally
        {
            IsRemovingDuplicates = false;
        }
    }

    [RelayCommand]
    public void StopRemovingDuplicates()
    {
        if (_removeDuplicatesProcessCts != null)
            _removeDuplicatesProcessCts.Cancel();

        // TODO: separate class? -> <xy>.TryKillAllProcesses?
        try
        {
            foreach (var proc in Process.GetProcessesByName("PP3"))
                proc.Kill();
        }
        catch (Exception)
        {
            // NO-OP
        }
    }

    [RelayCommand]
    public async Task WriteHashToFileAsync()
    {
        GenerateHashCommand.Execute(null);
        if (string.IsNullOrEmpty(Hash))
            return;
        var outputFilePath = Path.Combine(GetHashoutDirectory(), $"Hash_{SelectedHashAlgorithmItem}_{DateTime.Now:yyyyMMdd_HHmmssfff}.txt");
        await File.WriteAllTextAsync(outputFilePath, Hash, new UTF8Encoding(false));
    }

    [RelayCommand]
    public void GenerateHash()
    {
        if (string.IsNullOrEmpty(SelectedHashAlgorithmItem))
        {
            MessageBox.Show("Please select a hash algorithm first", "GovTools", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(Text);
        using HashAlgorithm? algo = SelectedHashAlgorithmItem switch
        {
            "MD5" => MD5.Create(),
            "SHA1" => SHA1.Create(),
            "SHA256" => SHA256.Create(),
            "SHA512" => SHA512.Create(),
            _ => null,
        };
        if (algo == null)
            return;
        var hashData = algo.ComputeHash(bytes);
        Hash = Convert.ToHexString(hashData);
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

    // TODO: robert, refactor
    public string GetHashoutDirectory()
    {
        var govCrackerPath = _iniFile.GetString("General", "GovCrackerPath");
        if (string.IsNullOrEmpty(govCrackerPath))
            return Path.Combine(AppContext.BaseDirectory, "_Hashout");
        else
            return Path.Combine(govCrackerPath, "_Hashout");
    }

}
