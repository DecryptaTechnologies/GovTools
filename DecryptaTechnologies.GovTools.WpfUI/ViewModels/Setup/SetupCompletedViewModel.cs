
using rskibbe.Ini.Contracts;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class SetupCompletedViewModel : SetupScreenViewModelBase
{

    protected IIniFile _iniFile;

    public SetupCompletedViewModel(
        IIniFile iniFile    
    )
    {
        _iniFile = iniFile;
    }

    protected async override Task ContinueAsync()
    {
        _iniFile.SetBoolean("General", "SetupCompleted", true);
        await _iniFile.SaveAsync();
        await base.ContinueAsync();
    }

    protected override bool CanContinueAsync()
    {
        return true;
    }

}
