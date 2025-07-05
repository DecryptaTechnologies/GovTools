namespace DecryptaTechnologies.GovTools.Domain.Contracts;

/// <summary>
/// Denotes something that is able to extract hashes based on a target folder
/// </summary>
public interface ICanExtractHashesFromFolderAsync
{

    string SelectedFolder { get; set; }

    Task<bool?> ExtractHashesFromFolderAsync(string folderPath);

    bool SupportsFileName(string fileName);

}
