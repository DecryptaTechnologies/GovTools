using Autofac;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using rskibbe.I18n.Contracts;
using rskibbe.Ini.Contracts;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class GeneralSettingsSubCategoryViewModel : SettingsSubCategoryViewModel
{

    protected ITranslator _translator;

    protected IIniFile _iniFile;

    public GeneralSettingsSubCategoryViewModel(
        ITranslator translator,
        IIniFile iniFile
    )
    {
        _translator = translator;
        _iniFile = iniFile;
        Name = translator.Translate("Settings.General").Value;

        AddLanguageMenuItem();
        AddGovCrackerFolderItem();
    }

    private void AddLanguageMenuItem()
    {
        var langIso = _iniFile.GetString("General", "LanguageIso", "en-US");
        var configuredLanguage = _translator.Languages.SingleOrDefault(x => x.Iso == langIso);
        var parms = new List<Autofac.Core.Parameter>();
        if (configuredLanguage != null)
            parms.Add(new NamedParameter("selectedItem", configuredLanguage));
        var vm = App.Container.Resolve<SettingsSelectionMenuItemViewModel>(parms);
        vm.Name = _translator.Translate("Settings.Language.Name").Value;
        vm.Description = _translator.Translate("Settings.Language.Description").Value;
        vm.Items = new ObservableCollection<object>(_translator.Languages);
        vm.ItemSelected += Language_ItemSelected;
        _allMenuItems.Add(vm);
    }

    private void Language_ItemSelected(object? sender, EventArgs e)
    {
        var vm = sender as SettingsSelectionMenuItemViewModel;
        if (vm == null)
            return;
        var language = vm.SelectedItem as ILanguage;
        if (language == null)
            return;
        _iniFile.SetString("General", "LanguageIso", language.Iso);

        ApplicationUtils.Restart();
    }

    private void AddGovCrackerFolderItem()
    {
        var govCrackerPath = _iniFile.GetString("General", "GovCrackerPath");
        var parms = new List<Autofac.Core.Parameter>()
        {
            new NamedParameter("text", govCrackerPath),
            new NamedParameter("isDeleteButtonVisible", true),
        };
        var vm = App.Container.Resolve<SettingsFolderTextMenuItemViewModel>(parms);
        vm.ValidateInputFn = selectedFolderPath =>
        {
            var govCrackerFilePath = Path.Combine(selectedFolderPath, "GovCracker.exe");
            return File.Exists(govCrackerFilePath);
        };
        vm.HandleFailedInputValidationFn = selectedFolderPath =>
        {
            var msgText = $"{_translator.Translate("Settings.GovCracker.GovCrackerExeNotFound")}";
            MessageBox.Show($"{msgText}: '{selectedFolderPath}'", "", MessageBoxButton.OK, MessageBoxImage.Information);
        };
        vm.Name = $"{_translator.Translate("Settings.GovCracker.Name")}";
        vm.Description = $"{_translator.Translate("Settings.GovCracker.Description")}";
        vm.TextChanged += GovCrackerFolder_Changed;
        _allMenuItems.Add(vm);
    }

    private void GovCrackerFolder_Changed(object? sender, EventArgs e)
    {
        var vm = sender as SettingsFolderTextMenuItemViewModel;
        if (vm == null)
            return;
        _iniFile.SetString("General", "GovCrackerPath", $"{vm.Text}");
    }

}
