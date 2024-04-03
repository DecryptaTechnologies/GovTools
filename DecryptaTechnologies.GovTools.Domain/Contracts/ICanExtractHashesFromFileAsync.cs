namespace DecryptaTechnologies.GovTools.Domain.Contracts;

public interface ICanExtractHashesFromFileAsync
{

    Task<bool?> ExtractHashesFromFileAsync(string filePath);

    bool SupportsFileName(string fileName);

}
