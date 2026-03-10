using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Appearance;

namespace DnDInitiativeTracker.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    public UpdateViewModel Update { get; }

    [ObservableProperty]
    private bool _isDarkMode = true;

    public string AppVersion { get; } =
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    public SettingsViewModel(UpdateViewModel updateViewModel)
    {
        Update = updateViewModel;
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        var theme = value ? ApplicationTheme.Dark : ApplicationTheme.Light;
        ApplicationThemeManager.Apply(theme);
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        await Update.CheckForUpdatesAsync();
    }
}
