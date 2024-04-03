using System.Diagnostics;
using System.Windows;

namespace DecryptaTechnologies.GovTools.WpfUI.Utils;

public class ApplicationUtils
{

    public static void Restart()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = Process.GetCurrentProcess()!.MainModule!.FileName,
            UseShellExecute = true,
            // Verb = "runas" // Run the process as administrator, if required
        });
        Application.Current.Shutdown();
    }

}
