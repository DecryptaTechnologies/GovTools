using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.WpfUI.Utils;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class ExtractorStepBaseViewModel : ScreenViewModelBase, IExtractorStepViewModel
{

    protected virtual void OnCompleted(EventArgs e)
        => Completed?.Invoke(this, e);

    public event EventHandler? Completed;

}
