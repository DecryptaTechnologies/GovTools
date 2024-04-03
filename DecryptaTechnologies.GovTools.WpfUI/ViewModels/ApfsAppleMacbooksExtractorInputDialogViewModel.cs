using Autofac;
using CommunityToolkit.Mvvm.Input;
using DecryptaTechnologies.GovTools.WpfUI.Utils;
using Microsoft.Win32;
using MvvmDialogs;
using rskibbe.I18n.Contracts;
using System.Diagnostics;

namespace DecryptaTechnologies.GovTools.WpfUI.ViewModels;

public partial class ApfsAppleMacbooksExtractorInputDialogViewModel : ViewModelBase, IModalDialogViewModel
{

    protected ITranslator _translator;

    protected IDialogService _dialogService;

    bool? _dialogResult;

    public bool? DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }

    string _imageFilePath;

    public string ImageFilePath
    {
        get => _imageFilePath;
        set => SetProperty(ref _imageFilePath, value);
    }

    public ApfsAppleMacbooksExtractorInputDialogViewModel(
        ITranslator translator,
        IDialogService dialogService
    )
    {
        _translator = translator;
        _dialogService = dialogService;
        _imageFilePath = "";
    }

    [RelayCommand]
    public void InstallUbuntu()
    {
        var dialogVm = App.Container.Resolve<ActivateLinuxDialogViewModel>();
        var result = _dialogService.ShowDialog(this, dialogVm);
        if (result == true)
        {
            // linux subsys has been activated successfully..
        }
    }

    [RelayCommand]
    public void Open()
    {
        var ofd = new OpenFileDialog();
        ofd.Filter = "All Files (*.*)| *.*";
        ofd.InitialDirectory = AppContext.BaseDirectory;
        var result = ofd.ShowDialog();
        if (result != true)
            return;
        ImageFilePath = ChangeInput(ofd.FileName);
    }

    private string ChangeInput(string originalFilePath)
    {
        return $"{originalFilePath[..2].ToLower()}{originalFilePath.Substring(2)}"
            .Replace("\\", "/")
            .Replace(":", "");
    }

    [RelayCommand]
    public void Help()
    {
        var helpUrl = "https://github.com/banaanhangwagen/apfs2hashcat";
        var info = new ProcessStartInfo(helpUrl) { UseShellExecute = true };
        Process.Start(info);
    }

}
