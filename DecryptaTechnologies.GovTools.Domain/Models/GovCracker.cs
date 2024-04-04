using DecryptaTechnologies.GovTools.Domain.Contracts;
using rskibbe.I18n.Contracts;
using rskibbe.Ini.Contracts;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace DecryptaTechnologies.GovTools.Domain.Models;

public class GovCracker : IGovCracker
{

    protected IIniFile _iniFile;

    protected ITranslator _translator;

    public GovCracker(
        IIniFile iniFile,
        ITranslator translator
    )
    {
        _iniFile = iniFile;
        _translator = translator;
    }

    public void OpenProgram()
    {
        var govCrackerPath = _iniFile.GetString("General", "GovCrackerPath");
        if (string.IsNullOrEmpty(govCrackerPath))
        {
            // TODO: ad for GovCracker
            MessageBox.Show($"{_translator.Translate("Settings.GovCracker.GovCrackerFolderPath")}", "GovTools", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var govCrackerFilePath = Path.Combine(govCrackerPath, "GovCracker.exe");
        if (!File.Exists(govCrackerFilePath))
        {
            _iniFile.SetString("General", "GovCrackerPath", "");
            MessageBox.Show($"{_translator.Translate("Settings.GovCracker.GovCrackerFolderPathFailed")}", "", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Process.Start(new ProcessStartInfo()
        {
            FileName = govCrackerFilePath,
            WorkingDirectory = govCrackerPath,
        });
    }

}
