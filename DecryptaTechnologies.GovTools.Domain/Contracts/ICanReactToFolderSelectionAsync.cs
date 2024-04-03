namespace DecryptaTechnologies.GovTools.Domain.Contracts;

/// <summary>
/// Denotes something which can react to a folder selection in an async way
/// </summary>
public interface ICanReactToFolderSelectionAsync
{

    /// <summary>
    /// Defines behaviour on how the thing will react to a folder selection
    /// </summary>
    Task ReactToFolderSelectionAsync();

}
