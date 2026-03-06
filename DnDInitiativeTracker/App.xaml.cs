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
using Wpf.Ui.Appearance;

namespace DnDInitiativeTracker;

public partial class App : Application
{
    private ServiceProvider? _services;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var collection = new ServiceCollection();

        // Determine the data root: a "Data" folder next to the exe.
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

        // Velopack update manager (checks GitHub Releases)
        collection.AddSingleton(_ => new Velopack.UpdateManager(
            new Velopack.Sources.GithubSource("https://github.com/I-Am-Fox/DnDInitiativeTracker", null, false)));

        // WPF-UI services (snackbar only — we handle navigation ourselves)
        collection.AddSingleton<ISnackbarService, SnackbarService>();

        // ViewModels
        collection.AddSingleton<MainWindowViewModel>();
        collection.AddSingleton<UpdateViewModel>();
        collection.AddSingleton<MainViewModel>();
        collection.AddSingleton<EncounterViewModel>();
        collection.AddSingleton<CampaignDetailViewModel>();
        collection.AddSingleton<SettingsViewModel>();
        collection.AddSingleton<ShellViewModel>();

        // Pages (transient so fresh instances are created on navigation)
        collection.AddTransient<CampaignsPage>();
        collection.AddTransient<CampaignDetailPage>();
        collection.AddTransient<EncountersPage>();
        collection.AddTransient<InitiativePage>();
        collection.AddTransient<SettingsPage>();

        // Window
        collection.AddSingleton<MainWindow>();

        _services = collection.BuildServiceProvider();

        // Apply WPF-UI dark theme
        ApplicationThemeManager.Apply(ApplicationTheme.Dark);

        // Pre-load campaigns
        var mainVm = _services.GetRequiredService<MainViewModel>();
        await mainVm.InitializeAsync();

        // Resolve ShellViewModel and wire up cross-references
        var shell = _services.GetRequiredService<ShellViewModel>();
        mainVm.SetShell(shell);
        var encounterVm = _services.GetRequiredService<EncounterViewModel>();
        encounterVm.SetShell(shell);

        var window = _services.GetRequiredService<MainWindow>();
        window.Show();

        // Check for updates in the background (fire-and-forget, non-blocking)
        var updateVm = _services.GetRequiredService<UpdateViewModel>();
        _ = updateVm.CheckForUpdatesAsync();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}
