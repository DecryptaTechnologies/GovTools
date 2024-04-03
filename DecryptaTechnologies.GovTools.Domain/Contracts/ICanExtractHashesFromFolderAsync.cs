namespace DecryptaTechnologies.GovTools.Domain.Contracts;

public interface ICanExtractHashesFromFolderAsync
{

    Task<bool?> ExtractHashesFromFolderAsync(string folderPath);

    bool SupportsFileName(string fileName);

}
