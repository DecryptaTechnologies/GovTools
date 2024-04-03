using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class MetaMaskSeedExtractorViewModel : ExtractorStepBaseViewModel
{

    public MetaMaskSeedExtractorViewModel()
    {
        
    }

    [RelayCommand]
    public void VisitMetaMaskPage()
    {
        Process.Start(new ProcessStartInfo("https://metamask.github.io/vault-decryptor/") { UseShellExecute = true });
    }

}
