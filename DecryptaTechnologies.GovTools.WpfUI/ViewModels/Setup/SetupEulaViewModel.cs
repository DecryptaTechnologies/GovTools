using CommunityToolkit.Mvvm.ComponentModel;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class SetupEulaViewModel : SetupScreenViewModelBase
{

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    bool _accepted;

    public SetupEulaViewModel()
    {
        
    }

    protected override bool CanContinueAsync()
    {
        return Accepted;
    }

}
