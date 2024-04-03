using DecryptaTechnologies.GovTools.Domain.ValueObjects;

namespace DecryptaTechnologies.GovTools.Domain.Contracts;

public interface IExtractorManager
{

    IExtractor? TryGetExtractorForType(ExtractorTypes extractorType);

}
