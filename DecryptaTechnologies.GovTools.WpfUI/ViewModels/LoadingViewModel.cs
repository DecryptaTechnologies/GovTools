using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using DecryptaTechnologies.GovTools.WpfUI.ValueObjects;
using rskibbe.I18n.Contracts;
using rskibbe.Ini.Contracts;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class LoadingViewModel : ScreenViewModelBase
{

    protected IIniFile _iniFile;

    protected ITranslator _translator;

    public string VersionText
    {
        get
        {
            var clickOnceVersion = Environment.GetEnvironmentVariable("ClickOnce_CurrentVersion");
            if (!string.IsNullOrEmpty(clickOnceVersion))
                return $"v {clickOnceVersion}";
            else
                return "v 1.2.3.4";
        }
    }

    [ObservableProperty]
    private int _progress;

    public LoadingViewModel(
        IIniFile iniFile,
        ITranslator translator
    )
    {
        _iniFile = iniFile;
        _translator = translator;
    }

    public async override Task ActivateAsync()
    {
        await base.ActivateAsync();
        Progress = 10;

        var langIso = _iniFile.GetString("General", "LanguageIso", "en-US");
        if (_translator.Languages.Count == 0)
            await _translator.LoadLanguagesAsync();

        await _translator.ChangeLanguageAsync(langIso);
        CultureInfo.CurrentCulture = new CultureInfo(langIso);
        CultureInfo.CurrentUICulture = new CultureInfo(langIso);
        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
            XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        Progress = 20;

        if (!Debugger.IsAttached)
            await Task.Delay(200);
        Progress = 30;

        if (!Debugger.IsAttached)
            await Task.Delay(300);
        Progress = 50;

        // await _attackAlgorithmService.LoadAllAttackAlgorithmsAsync();
        // await _encodingService.LoadAllEncodingsAsync();

        if (!Debugger.IsAttached)
            await Task.Delay(200);
        Progress = 70;

        if (!Debugger.IsAttached)
            await Task.Delay(400);
        Progress = 90;

        if (!Debugger.IsAttached)
            await Task.Delay(500);

        Progress = 100;

        LoadingCompleted?.Invoke(this, EventArgs.Empty);
        WeakReferenceMessenger.Default.Send<LoadingCompletedMessage>();
    }

    public event EventHandler? LoadingCompleted;

}
