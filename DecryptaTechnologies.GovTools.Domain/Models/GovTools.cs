using DecryptaTechnologies.GovTools.Domain.Contracts;
using rskibbe.I18n.Contracts;
using rskibbe.Ini.Contracts;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace DecryptaTechnologies.GovTools.Domain.Models;

public class GovTools : IGovTools
{

    protected IIniFile _iniFile;

    protected ITranslator _translator;

    public GovTools(
        IIniFile iniFile,
        ITranslator translator
    )
    {
        _iniFile = iniFile;
        _translator = translator;
    }

    public string GetHashoutDirectory()
        => GetGovCrackerConfigBasedSubPath("_Hashout");

    public string GetWordlistDirectory()
        => GetGovCrackerConfigBasedSubPath("_Wordlists");

    public string GetGovCrackerConfigBasedSubPath(string subDirectory)
    {
        var govCrackerPath = _iniFile.GetString("General", "GovCrackerPath");
        if (!string.IsNullOrEmpty(govCrackerPath))
            return Path.Combine(govCrackerPath, subDirectory);
        return Path.Combine(AppContext.BaseDirectory, subDirectory);
    }

    public void ShowLegalMessageIfNotDebugging()
    {
        if (Debugger.IsAttached)
            return;
        var msg = $"{_translator.Translate("MainForm.LoadMessage")}";
        var denied = MessageBox.Show(msg, "GovTools", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No;
        if (denied)
        {
            Application.Current.Shutdown();
            return;
        }
    }

}
