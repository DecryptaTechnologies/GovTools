using DecryptaTechnologies.GovTools.Domain.Contracts;
using System.IO;

namespace DecryptaTechnologies.GovTools.Domain.Models;

public class FileSizeBiggerZeroHashOutputFileChecker : ICanCheckHashOutputFileAsync
{

    public Task<bool?> CheckHashOutputFileAsync(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        bool? isntFileEmpty;
        try
        {
            isntFileEmpty = fileInfo.Length != 0;
        }
        catch (FileNotFoundException)
        {
            isntFileEmpty = false;
        }
        return Task.FromResult(isntFileEmpty);
    }

}
