using DnDInitiativeTracker.Core.Interfaces.Repositories;
using DnDInitiativeTracker.Core.Interfaces.Services;
using DnDInitiativeTracker.Core.Repositories;
using DnDInitiativeTracker.Core.Services;
using DnDInitiativeTracker.Pages;
using DnDInitiativeTracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using Velopack;
using Wpf.Ui;
using Wpf.Ui.Appearance;

namespace DnDInitiativeTracker;

public partial class App : Application
{
    private readonly ServiceProvider? _services;

    public IServiceProvider Services => _services;

    [STAThread]
    private static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        App app = new();
        app.InitializeComponent();
        app.Run();
    }

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _services = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Determine the data root: a "Data" folder next to the exe.
        var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                     ?? AppContext.BaseDirectory;

        var dataRoot = Path.Combine(appDir, "Data");

        Directory.CreateDirectory(dataRoot);

        // Repositories
        services.AddSingleton<ICampaignRepository>(_ => new JsonCampaignRepository(dataRoot));
        services.AddSingleton<IEncounterRepository>(_ => new JsonEncounterRepository(dataRoot));
        services.AddSingleton<ICharacterRepository>(_ => new JsonCharacterRepository(dataRoot));

        // Services
        services.AddSingleton<IInitiativeOrderingService, InitiativeOrderingService>();
        services.AddSingleton(_ =>
        {
            var client = new HttpClient { BaseAddress = new Uri("https://www.dnd5eapi.co") };
            return client;
        });
        services.AddSingleton<ICreatureCatalogService, FiveBitsCreatureCatalogService>();

        // Velopack update manager (checks GitHub Releases)
        services.AddSingleton<IUpdateService, VelopackUpdateService>();

        // WPF-UI services (snackbar only — we handle navigation ourselves)
        services.AddSingleton<ISnackbarService, SnackbarService>();

        // ViewModels
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<UpdateViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<EncounterViewModel>();
        services.AddSingleton<CampaignDetailViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<TranslatorViewModel>();
        services.AddSingleton<ShellViewModel>();

        // Pages (transient so fresh instances are created on navigation)
        services.AddTransient<CampaignsPage>();
        services.AddTransient<CampaignDetailPage>();
        services.AddTransient<EncountersPage>();
        services.AddTransient<InitiativePage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<TranslatorPage>();

        // Window
        services.AddSingleton<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

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
