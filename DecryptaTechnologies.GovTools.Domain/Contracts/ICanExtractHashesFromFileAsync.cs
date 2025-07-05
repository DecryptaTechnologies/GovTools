namespace DecryptaTechnologies.GovTools.Domain.Contracts;

/// <summary>
/// Denotes something that is able to extract hashes from a single target file
/// </summary>
public interface ICanExtractHashesFromFileAsync
{

    string SelectedFile { get; set; }

    Task<bool?> ExtractHashesFromFileAsync(string filePath);

    bool SupportsFileName(string fileName);

}
