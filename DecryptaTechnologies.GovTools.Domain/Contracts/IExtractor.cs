namespace DecryptaTechnologies.GovTools.Domain.Contracts;

// TODO: rename to IHashExtractor?

/// <summary>
/// An interface denoting something which is able to be run, to extract hashes
/// </summary>
public interface IExtractor
{

    string Name { get; }

    string? ImageUrl { get; }

    string Hint { get; }

    List<IExtractorStepViewModel> GetRequiredScreens();

    void Reset();

    /// <summary>
    /// Executes the extraction process of the individual <see cref="IExtractor"/>
    /// </summary>
    /// <returns>Returns <see langword="true"/> if the extraction process was successful, <see langword="false"/> if it wasn't successful and <see langword="null"/> if the process has been cancelled</returns>
    Task<bool?> RunAsync();

    /// <summary>
    /// Stops the ongoing / running extraction process
    /// </summary>
    void Stop();

}
