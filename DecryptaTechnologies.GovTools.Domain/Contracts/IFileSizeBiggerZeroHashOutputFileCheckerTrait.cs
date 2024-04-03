using DecryptaTechnologies.GovTools.Domain.Models;

namespace DecryptaTechnologies.GovTools.Domain.Contracts;

/// <summary>
/// A Trait which implements the <see cref="ICanCheckHashOutputFileAsync"/> interface by using <see cref="FileSizeBiggerZeroHashOutputFileChecker"/> for implementation in a more reusable style and avoids repetitive stuff
/// </summary>
public interface IFileSizeBiggerZeroHashOutputFileCheckerTrait : ICanCheckHashOutputFileAsync
{

    async Task<bool?> ICanCheckHashOutputFileAsync.CheckHashOutputFileAsync(string filePath)
    {
        return await new FileSizeBiggerZeroHashOutputFileChecker()
            .CheckHashOutputFileAsync(filePath)
            .ConfigureAwait(false);
    }

}
