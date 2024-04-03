using Autofac;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.Domain.ValueObjects;
using IContainer = Autofac.IContainer;

namespace DecryptaTechnologies.GovTools.Domain.Models;

public class ExtractorManager : IExtractorManager
{

    IContainer? _container;

    IDictionary<ExtractorTypes, Type> _extractorTypes;

    public ExtractorManager()
    {
        _extractorTypes = new Dictionary<ExtractorTypes, Type>();
    }

    public IExtractorManager Init(IContainer container)
    {
        _container = container;
        return this;
    }

    public IExtractorManager RegisterExtractor(ExtractorTypes extractorType, Type type)
    {
        if (!_extractorTypes.ContainsKey(extractorType))
            _extractorTypes.Add(extractorType, type);
        return this;
    }

    public IExtractor? TryGetExtractorForType(ExtractorTypes extractorType)
    {
        if (_extractorTypes.TryGetValue(extractorType, out var type))
            return (IExtractor) _container!.Resolve(type);
        return null;
    }

}
