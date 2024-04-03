using DecryptaTechnologies.GovTools.Domain.Contracts;
using System.Diagnostics;

namespace DecryptaTechnologies.GovTools.WpfUI.Utils;

public abstract class ScreenViewModelBase : ViewModelBase, IScreen
{

    public ScreenViewModelBase()
    {

    }

    public virtual Task BeforeActivateAsync()
    {
        Debug.WriteLine($"{GetType().Name}.{nameof(BeforeActivateAsync)}");
        return Task.CompletedTask;
    }

    public virtual Task ActivateAsync()
    {
        Debug.WriteLine($"{GetType().Name}.{nameof(ActivateAsync)}");
        return Task.CompletedTask;
    }

    public virtual Task DeactivateAsync()
    {
        Debug.WriteLine($"{GetType().Name}.{nameof(DeactivateAsync)}");
        return Task.CompletedTask;
    }

}
