using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.WpfUI.Utils;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class SetupScreenViewModelBase : ScreenViewModelBase
{

    public SetupScreenViewModelBase()
    {
        
    }

    [RelayCommand(CanExecute = nameof(CanBackAsync))]
    protected virtual Task BackAsync()
    {
        Backed?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    protected virtual bool CanBackAsync()
        => true;

    [RelayCommand(CanExecute = nameof(CanContinueAsync))]
    protected virtual Task ContinueAsync()
    {
        Continued?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    protected virtual bool CanContinueAsync()
        => false;

    public event EventHandler? Backed;

    public event EventHandler? Continued;

}
