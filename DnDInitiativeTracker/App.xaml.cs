using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using DnDInitiativeTracker.Core.Repositories;
using DnDInitiativeTracker.Core.Services;
using DnDInitiativeTracker.Pages;
using DnDInitiativeTracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Velopack;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.DependencyInjection;

namespace DnDInitiativeTracker;

public partial class App : Application
{
    private ServiceProvider? _services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var collection = new ServiceCollection();

        // Determine the data root: a "Data" folder next to the exe.
        // Under Velopack the app root is the install directory;
        // in development it's just the build output directory.
        var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                     ?? AppContext.BaseDirectory;
        var dataRoot = Path.Combine(appDir, "Data");
        Directory.CreateDirectory(dataRoot);

        // Repositories
        collection.AddSingleton<ICampaignRepository>(_ => new JsonCampaignRepository(dataRoot));
        collection.AddSingleton<IEncounterRepository>(_ => new JsonEncounterRepository(dataRoot));
        collection.AddSingleton<ICharacterRepository>(_ => new JsonCharacterRepository(dataRoot));

        // Services
        collection.AddSingleton<IInitiativeOrderingService, InitiativeOrderingService>();
        collection.AddSingleton(_ =>
        {
            var client = new HttpClient { BaseAddress = new Uri("https://www.dnd5eapi.co") };
            return client;
        });
        collection.AddSingleton<ICreatureCatalogService, FiveBitsCreatureCatalogService>();

        // WPF-UI services
        collection.AddSingleton<ISnackbarService, SnackbarService>();
        collection.AddSingleton<INavigationService, NavigationService>();
        collection.AddNavigationViewPageProvider();

        // ViewModels
        collection.AddSingleton<MainWindowViewModel>();
        collection.AddSingleton<MainViewModel>();
        collection.AddSingleton<EncounterViewModel>();
        collection.AddSingleton<CampaignDetailViewModel>();
        collection.AddSingleton<ShellViewModel>();

        // Pages (transient so NavigationView creates fresh instances)
        collection.AddTransient<CampaignsPage>();
        collection.AddTransient<CampaignDetailPage>();
        collection.AddTransient<EncountersPage>();
        collection.AddTransient<InitiativePage>();

        // Window
        collection.AddSingleton<MainWindow>();

        _services = collection.BuildServiceProvider();

        // Apply WPF-UI dark theme
        ApplicationThemeManager.Apply(ApplicationTheme.Dark);

        // Pre-load campaigns
        var mainVm = _services.GetRequiredService<MainViewModel>();
        await mainVm.InitializeAsync();

        // Resolve ShellViewModel so it wires up property-change coordination
        _services.GetRequiredService<ShellViewModel>();

        var window = _services.GetRequiredService<MainWindow>();
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}
