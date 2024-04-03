using CommunityToolkit.Mvvm.Input;
using rskibbe.I18n.Contracts;
using rskibbe.Ini.Contracts;
using System.IO;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class SetupWelcomeViewModel : SetupScreenViewModelBase
{

    protected ITranslator _translator;

    protected IIniFile _iniFile;

    public SetupWelcomeViewModel(
        ITranslator translator,
        IIniFile iniFile
    )
    {
        _translator = translator;
        _iniFile = iniFile;
    }

    public override Task ActivateAsync()
    {
        base.ActivateAsync();
        List<string> requiredDirs = [
            Path.Combine(AppContext.BaseDirectory, "logs"),
        ];
        requiredDirs.ForEach(x => Directory.CreateDirectory(x));
        return Task.CompletedTask;
    }

    [RelayCommand]
    public async Task ChangeLanguageAsync(string iso)
    {
        _iniFile.SetString("General", "LanguageIso", iso);
        await _translator.ChangeLanguageAsync(iso);
    }

    protected override bool CanContinueAsync()
        => true;

}
