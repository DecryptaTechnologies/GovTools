using Autofac;
using rskibbe.I18n.Contracts;
using rskibbe.Ini.Contracts;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class ApplicationSettingsCategoryViewModel : SettingsCategoryViewModel
{

    protected ITranslator _translator;

    protected IIniFile _iniFile;

    public ApplicationSettingsCategoryViewModel(
        ITranslator translator,
        IIniFile iniFile
    )
    {
        _translator = translator;
        _iniFile = iniFile;
        Name = translator.Translate("Settings.Application").Value;
        _allCategories.Add(App.Container.Resolve<GeneralSettingsSubCategoryViewModel>());
    }

}
