namespace DecryptaTechnologies.GovTools.Domain.Contracts;

public interface IWindowsService
{

    void ActivateLinuxSubSystem();

    /// <summary>
    /// Checks if a given exe file like ubuntu2004.exe exists in the PATH environment variable
    /// </summary>
    bool IsExecutableInPath(string executableName);

}
