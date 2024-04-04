using Autofac;
using DecryptaTechnologies.GovTools.Domain.Attributes;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.Domain.ValueObjects;
using DecryptaTechnologies.GovTools.WpfUI.ViewModels;
using rskibbe.I18n.Contracts;
using rskibbe.Ini.Contracts;
using System.Diagnostics;

namespace DecryptaTechnologies.GovTools.WpfUI.Extractors;

[ExtractorType(ExtractorTypes.MetaMaskSeed)]
public partial class MetaMaskSeedExtractor : ExtractorBase
{

    protected Process? _process;

    public override string? ImageUrl => "/Resources/extractor-metamask.png";

    public override string Hint => $"{_translator.Translate("Extractors.MetaMaskSeed")}";

    public override string Name => "MetaMask (Seeds)";

    public MetaMaskSeedExtractor(
        IIniFile iniFile,
        ITranslator translator
    ) : base(iniFile, translator)
    {
    }

    public override List<IExtractorStepViewModel> GetRequiredScreens()
    {
        var metaMaskSeedExtractorViewModel = App.Container.Resolve<MetaMaskSeedExtractorViewModel>();
        return [
            metaMaskSeedExtractorViewModel,
        ];
    }

    public bool SupportsFileName(string fileName)
        => true;

    public override Task<bool?> RunAsync()
        => Task.FromResult((bool?)true);

}
