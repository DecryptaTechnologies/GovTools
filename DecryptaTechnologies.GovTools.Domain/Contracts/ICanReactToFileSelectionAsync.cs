namespace DecryptaTechnologies.GovTools.Domain.Contracts;

/// <summary>
/// Denotes something which can react to a file selection in an async way
/// </summary>
public interface ICanReactToFileSelectionAsync
{

    /// <summary>
    /// Defines behaviour on how the thing will react to a file selection
    /// </summary>
    Task ReactToFileSelectionAsync();

}
