using Autofac;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using rskibbe.I18n.Contracts;
using rskibbe.Ini.Contracts;
using System.Collections.ObjectModel;
using System.Globalization;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class SetupViewModel : HostScreenViewModelBase
{

    protected ITranslator _translator;

    protected IIniFile _iniFile;

    int _currentScreenIndex;

    ObservableCollection<SetupScreenViewModelBase> _setupScreens;

    public SetupViewModel(
        ITranslator translator,
        IIniFile iniFile
    )
    {
        _translator = translator;
        _iniFile = iniFile;
        _setupScreens = new ObservableCollection<SetupScreenViewModelBase>()
        {
            App.Container.Resolve<SetupWelcomeViewModel>(),
            App.Container.Resolve<SetupEulaViewModel>(),
            //App.Container.Resolve<SetupLanguagesViewModel>(),
            App.Container.Resolve<SetupCompletedViewModel>(),
        };
        _currentScreenIndex = -1;
    }

    public async override Task ActivateAsync()
    {
        if(_translator.Languages.Count == 0)
            await _translator.LoadLanguagesAsync();
        if (_translator.Languages.Count > 0)
        {
            var pcLanguageIso = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
            var pcLanguage = _translator.Languages.SingleOrDefault(x => x.Iso.StartsWith(pcLanguageIso));
            if (pcLanguage != null)
            {
                await _translator.ChangeLanguageAsync(pcLanguage.Iso);
                _iniFile.SetString("General", "LanguageIso", pcLanguage.Iso);
            }
        }
        await ShowNextScreenAsync();
    }

    public async Task ShowNextScreenAsync()
    {
        if (_currentScreenIndex > -1)
        {
            var currentScreen = _setupScreens[_currentScreenIndex];
            currentScreen.Backed -= Screen_Backed;
            currentScreen.Continued -= Screen_Continued;
        }
        _currentScreenIndex++;
        var nextScreen = _setupScreens[_currentScreenIndex];
        nextScreen.Backed += Screen_Backed;
        nextScreen.Continued += Screen_Continued;
        await SwitchScreenAsync(nextScreen);
    }

    public async Task ShowPreviousScreenAsync()
    {
        if (_currentScreenIndex > -1)
        {
            var currentScreen = _setupScreens[_currentScreenIndex];
            currentScreen.Backed -= Screen_Backed;
            currentScreen.Continued -= Screen_Continued;
        }
        _currentScreenIndex--;
        var previousScreen = _setupScreens[_currentScreenIndex];
        previousScreen.Backed += Screen_Backed;
        previousScreen.Continued += Screen_Continued;
        await SwitchScreenAsync(previousScreen);
    }

    private async void Screen_Continued(object? sender, EventArgs e)
    {
        var hasMoreScreens = _currentScreenIndex < _setupScreens.Count - 1;
        if (hasMoreScreens)
        {
            await ShowNextScreenAsync();
            return;
        }

        var currentScreen = _setupScreens[_currentScreenIndex];
        currentScreen.Backed -= Screen_Backed;
        currentScreen.Continued -= Screen_Continued;
        SetupCompleted?.Invoke(this, EventArgs.Empty);
    }

    private async void Screen_Backed(object? sender, EventArgs e)
    {
        var hasPreviousScreens = _currentScreenIndex > 0;
        if (hasPreviousScreens)
        {
            await ShowPreviousScreenAsync();
            return;
        }
    }

    public event EventHandler? SetupCompleted;

}
