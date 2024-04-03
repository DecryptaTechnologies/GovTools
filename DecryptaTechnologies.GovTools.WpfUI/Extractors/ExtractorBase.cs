using DecryptaTechnologies.GovTools.Domain.Contracts;
using rskibbe.I18n.Contracts;
using rskibbe.Ini.Contracts;

namespace DecryptaTechnologies.GovTools.WpfUI.Extractors;

public abstract class ExtractorBase : IExtractor
{

    protected IIniFile _iniFile;

    protected ITranslator _translator;

    protected bool _stopped;

    public virtual string Name => "";

    public virtual string? ImageUrl => "/Resources/extractor-default.png";

    public virtual string Hint => "";

    protected ExtractorBase(
        IIniFile iniFile,
        ITranslator translator
    )
    {
        _iniFile = iniFile;
        _translator = translator;
    }

    public virtual List<IExtractorStepViewModel> GetRequiredScreens()
        => new();

    public virtual void Reset()
    {
        _stopped = false;
    }

    public abstract Task<bool?> RunAsync();

    public string GetHashoutDirectory()
    {
        var govCrackerPath = _iniFile.GetString("General", "GovCrackerPath");
        if (string.IsNullOrEmpty(govCrackerPath))
            return AppContext.BaseDirectory;
        else
            return govCrackerPath;
    }

    public virtual void Stop()
    {
        // NO-OP
    }

    public override string ToString()
        => Name;

}
