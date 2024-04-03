using Autofac;
using CommunityToolkit.Mvvm.Messaging;
using DecryptaTechnologies.GovTools.Domain.Attributes;
using DecryptaTechnologies.GovTools.Domain.Contracts;
using DecryptaTechnologies.GovTools.Domain.Models;
using DecryptaTechnologies.GovTools.WpfUI.ValueObjects;
using DecryptaTechnologies.GovTools.WpfUI.ViewModels;
using DecryptaTechnologies.GovTools.WpfUI.Views;
using MvvmDialogs;
using rskibbe.I18n.Contracts;
using rskibbe.I18n.Models;
using rskibbe.Ini.Contracts;
using rskibbe.Ini.Models;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
using IContainer = Autofac.IContainer;

namespace DecryptaTechnologies.GovTools.WpfUI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    private static IContainer? _container;

    public static IContainer Container => _container!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _container = SetupDI();
        ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        var viewModel = Container.Resolve<MainWindowViewModel>();
        var view = new MainWindowView() { DataContext = viewModel };
        view.Loaded += View_Loaded;
        view.Closing += View_Closing;
        view.Show();
    }

    private async void View_Loaded(object sender, RoutedEventArgs e)
    {
        var view = (Window)sender;
        view.Loaded -= View_Loaded;
        var screen = view.DataContext as IScreen;
        if (screen == null)
            return;
        await screen.ActivateAsync();
        var iniFile = _container!.Resolve<IIniFile>();
        var startMaximized = iniFile.GetBoolean("General", "StartMaximized", false);
        if (startMaximized)
            view.WindowState = WindowState.Maximized;
    }

    private async void View_Closing(object? sender, CancelEventArgs e)
    {
        var view = sender as Window;
        if (view == null)
            return;
        view.Closing -= View_Closing;
        WeakReferenceMessenger.Default.Send<WindowClosingMessage>();
        var screen = view.DataContext as IScreen;
        if (screen == null)
            return;
        await screen.DeactivateAsync();
        WeakReferenceMessenger.Default.Send<WindowClosedMessage>();
    }

    private static IContainer SetupDI()
    {
        var builder = new ContainerBuilder();

        RegisterTranslation(builder);
        RegisterIni(builder);
        RegisterViewModels(builder);
        RegisterExtractors(builder);
        //RegisterRepositories(builder);
        //RegisterServices(builder);

        // custom naming convention ding dong
        var dialogService = new DialogService(dialogTypeLocator: new Utils.NamingConventionDialogTypeLocator());
        builder
            .RegisterInstance(dialogService)
            .As<IDialogService>();

        builder
            .RegisterType<GovCracker>()
            .As<IGovCracker>()
            .SingleInstance();

        builder
            .RegisterType<Domain.Models.GovTools>()
            .As<IGovTools>()
            .SingleInstance();

        builder.RegisterType<WindowsService>()
            .As<IWindowsService>()
            .SingleInstance();

        return builder.Build();
    }

    private static void RegisterTranslation(ContainerBuilder builder)
    {
        var translator = Translator
            .Builder
            .WithoutLanguage()
            .WithUpdates()
            .Build()
            .StoreInstance();
        builder
            .RegisterInstance(translator)
            .As<ITranslator>()
            .SingleInstance();
    }

    private static void RegisterIni(ContainerBuilder builder)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var iniFilePath = Path.Combine(baseDir, "settings.ini");
        var iniFile = new IniFile(iniFilePath);
        builder.RegisterInstance(iniFile)
            .As<IIniFile>()
            .SingleInstance();
    }

    private static void RegisterViewModels(ContainerBuilder builder)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        builder
            .RegisterAssemblyTypes(executingAssembly)
            .Where(x => x.Name.EndsWith("ViewModel"));
    }

    private static void RegisterExtractors(ContainerBuilder builder)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();

        builder.Register<IExtractorManager>(c =>
        {
            var extractorManager = new ExtractorManager();
            extractorManager.Init(_container!);

            var extractorTypes = executingAssembly
                .GetExportedTypes()
                .Where(x => x.GetInterface(nameof(IExtractor)) != null);
            foreach (var extractorType in extractorTypes)
            {
                var extractorTypeAttribute = extractorType.GetCustomAttribute<ExtractorTypeAttribute>();
                if (extractorTypeAttribute == null)
                    continue;
                extractorManager.RegisterExtractor(extractorTypeAttribute.ExtractorType, extractorType);
            }
            return extractorManager;
        }).SingleInstance();

        builder
            .RegisterAssemblyTypes(executingAssembly)
            .Where(x => x.GetInterface(nameof(IExtractor)) != null);
    }

    //private static void RegisterRepositories(ContainerBuilder builder)
    //{
    //    var filePersistenceAssembly = Assembly.GetAssembly(typeof(Persistence.File.Repositories.FileBasedAttackAlgorithmRepository));
    //    if (filePersistenceAssembly == null)
    //        return;
    //    builder
    //        .RegisterAssemblyTypes(filePersistenceAssembly)
    //        .Where(x => x.Name.EndsWith("Repository"))
    //        .AsImplementedInterfaces()
    //        .SingleInstance();
    //}

    //private static void RegisterServices(ContainerBuilder builder)
    //{
    //    var domainAssembly = Assembly.GetAssembly(typeof(Domain.Services.IService));
    //    if (domainAssembly == null)
    //        return;
    //    builder
    //        .RegisterAssemblyTypes(domainAssembly)
    //        .Where(x => x.Namespace!.EndsWith("Services"))
    //        .AsImplementedInterfaces()
    //        .SingleInstance();
    //}

}

