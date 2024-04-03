using DecryptaTechnologies.GovTools.Domain.ValueObjects;

namespace DecryptaTechnologies.GovTools.Domain.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExtractorTypeAttribute : Attribute
{

    public ExtractorTypes ExtractorType { get; set; }

    public ExtractorTypeAttribute(
        ExtractorTypes extractorType
    )
    {
        ExtractorType = extractorType;
    }

}
