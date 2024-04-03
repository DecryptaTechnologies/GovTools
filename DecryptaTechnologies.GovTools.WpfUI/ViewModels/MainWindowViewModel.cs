using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using DecryptaTechnologies.GovTools.WpfUI.ValueObjects;
using DecryptaTechnologies.GovTools.WpfUI.Views;
using DecryptaTechnologies.GovTools.WpfUI.Views.Settings;
using rskibbe.I18n.Contracts;
using rskibbe.Ini.Contracts;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class MainWindowViewModel : HostScreenViewModelBase,
    IRecipient<OpenSettingsMessage>,
    IRecipient<LoadingCompletedMessage>,
    IRecipient<WindowClosingMessage>
{

    protected IGovTools _govTools;

    protected IGovCracker _govCracker;

    protected IIniFile _iniFile;

    protected ITranslator _translator;

    [ObservableProperty]
    bool _isToolbarVisible;

    public string Title => $"GovTools {VersionText}";

    public string VersionText
    {
        get
        {
            // TODO: fix, without clickonce
            var clickOnceVersion = Environment.GetEnvironmentVariable("ClickOnce_CurrentVersion");
            if (!string.IsNullOrEmpty(clickOnceVersion))
                return $"v{clickOnceVersion}";
            else
                return "v1.0.0";
        }
    }

    public MainWindowViewModel(
        IGovTools? govTools = null,
        IGovCracker? govCracker = null,
        IIniFile? iniFile = null,
        ITranslator? translator = null
    )
    {
        _govTools = govTools ?? App.Container.Resolve<IGovTools>();
        _govCracker = govCracker ?? App.Container.Resolve<IGovCracker>();
        _iniFile = iniFile ?? App.Container.Resolve<IIniFile>();
        _translator = translator ?? App.Container.Resolve<ITranslator>();
    }

    [RelayCommand]
    public void OpenGovCracker()
    {
        try
        {
            _govCracker.OpenProgram();
        }
        catch (Exception ex)
        {
            var nl = Environment.NewLine;
            MessageBox.Show($"Couldn't open GovCracker executable:{nl}{nl}'{ex.Message}'");
        }
    }

    [RelayCommand]
    public void OpenLocalPath(string relativePath)
    {
        var folderPath = Path.Combine(AppContext.BaseDirectory, relativePath);
        Process.Start("explorer.exe", $"{folderPath}");
    }

    [RelayCommand]
    public void OpenHashoutFolder()
    {
        Process.Start("explorer.exe", _govTools.GetHashoutDirectory());
    }

    [RelayCommand]
    public void OpenWordlistFolder()
    {
        Process.Start("explorer.exe", _govTools.GetWordlistDirectory());
    }

    public async override Task ActivateAsync()
    {
        WeakReferenceMessenger.Default.RegisterAll(this);
        await _iniFile.LoadAsync();
        await SwitchScreenAsync<StartViewModel>();
    }

    public override Task DeactivateAsync()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        return base.DeactivateAsync();
    }

    public void Receive(OpenSettingsMessage message)
    {
        // TODO: meh, but time...
        var dialogVm = App.Container.Resolve<SettingsDialogWindowViewModel>();
        var dialogV = new SettingsDialogWindowView() { DataContext = dialogVm, };
        dialogV.ShowDialog();
    }

    public async void Receive(LoadingCompletedMessage message)
    {
        IsToolbarVisible = true;
        await SwitchScreenAsync<MainLayoutViewModel>();
        _govTools.ShowLegalMessageIfNotDebugging();
    }

    public async void Receive(WindowClosingMessage message)
    {
        await _iniFile.SaveAsync();
    }

    [RelayCommand]
    public void OpenAboutDialog()
    {
        // TODO: meh, mvvm...
        var dialogVm = App.Container.Resolve<AboutDialogViewModel>();
        var dialogV = new AboutDialogView() { DataContext = dialogVm, };
        dialogV.ShowDialog();
    }

}
