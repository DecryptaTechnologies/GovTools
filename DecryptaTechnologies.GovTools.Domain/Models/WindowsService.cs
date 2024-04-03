using DecryptaTechnologies.GovTools.Domain.Contracts;
using System.Diagnostics;

namespace DecryptaTechnologies.GovTools.Domain.Models;

public class WindowsService : IWindowsService
{

    public void ActivateLinuxSubSystem()
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Verb = "runas",
                UseShellExecute = true,
                Arguments = "Enable-WindowsOptionalFeature -O -F  Microsoft-Windows-Subsystem-Linux; Write-Host \"\";Write-Host \"Press Enter to continue ... \";$null = Read-Host"
            }
        };
        process.Start();
    }

    /// <inheritdoc/>
    public bool IsExecutableInPath(string executableName)
    {
        var paths = Environment
            .GetEnvironmentVariable("PATH")?
            .Split(';');
        
        if (paths == null)
            return false;

        foreach (string path in paths)
        {
            string fullPath = System.IO.Path.Combine(path, executableName);
            if (System.IO.File.Exists(fullPath))
                return true;
        }
        return false;
    }

}
