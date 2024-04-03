using DecryptaTechnologies.GovTools.Domain.Contracts;

namespace DecryptaTechnologies.GovTools.WpfUI.ValueObjects;

public class ExtractorEventArgs
{

    public IExtractor Extractor { get; set; }

    public ExtractorEventArgs(IExtractor extractor)
    {
        Extractor = extractor;
    }

}
