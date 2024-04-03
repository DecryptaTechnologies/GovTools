namespace DecryptaTechnologies.GovTools.Domain.Contracts;

public interface ICanPostProcessHashOutputFileAsync
{

    Task<bool> PostProcessHashOutputFileAsync(string filePath);

}
