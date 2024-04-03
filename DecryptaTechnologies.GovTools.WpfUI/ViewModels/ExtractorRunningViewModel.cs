using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.WpfUI.Utils;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class ExtractorRunningViewModel : ScreenViewModelBase
{

    [RelayCommand]
    public void Cancel()
    {
        OnCancelled(EventArgs.Empty);
    }

    protected virtual void OnCancelled(EventArgs e)
    {
        Cancelled?.Invoke(this, e);
    }

    public event EventHandler? Cancelled;

}
