namespace DecryptaTechnologies.GovTools.Domain.Contracts;

/// <summary>
/// Denotes something that is able to check an output file
/// </summary>
public interface ICanCheckHashOutputFileAsync
{

    /// <summary>
    /// Checks an output file
    /// </summary>
    /// <param name="filePath">The complete path to the file</param>
    /// <returns>Returns <see langword="true"/>, if the check was successful, <see langword="false"/>, if not and <see langword="null"/> if like cancelled</returns>
    Task<bool?> CheckHashOutputFileAsync(string filePath);

}
