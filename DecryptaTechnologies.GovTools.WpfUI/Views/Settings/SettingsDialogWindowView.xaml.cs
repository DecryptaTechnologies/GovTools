using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.WpfUI.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace DecryptaTechnologies.GovTools.WpfUI.Views.Settings;

/// <summary>
/// Interaction logic for SettingsDialogWindowView.xaml
/// </summary>
public partial class SettingsDialogWindowView : Window
{

    public SettingsDialogWindowView()
    {
        InitializeComponent();
        Loaded += SettingsDialogWindowView_Loaded;
        Closing += SettingsDialogWindowView_Closing;
    }

    private async void SettingsDialogWindowView_Loaded(object sender, RoutedEventArgs e)
    {
        var view = (Window)sender;
        view.Loaded -= SettingsDialogWindowView_Loaded;
        var screen = view.DataContext as SettingsDialogWindowViewModel;
        if (screen == null)
            return;
        await screen.ActivateAsync();
    }

    private async void SettingsDialogWindowView_Closing(object? sender, CancelEventArgs e)
    {
        var view = sender as Window;
        if (view == null)
            return;
        view.Closing -= SettingsDialogWindowView_Closing;
        var screen = view.DataContext as IScreen;
        if (screen == null)
            return;
        await screen.DeactivateAsync();
    }

}
