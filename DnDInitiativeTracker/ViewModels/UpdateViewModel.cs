using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Velopack;

namespace DnDInitiativeTracker.ViewModels;

public sealed partial class UpdateViewModel : ObservableObject
{
    private readonly UpdateManager _updateManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private bool _isCheckingOrDownloading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    private bool _updateReady;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    private bool _hasError;

    [ObservableProperty]
    private int _downloadProgress;

    public bool IsVisible => !string.IsNullOrEmpty(StatusText);
    public bool CanApply => UpdateReady && !IsCheckingOrDownloading;

    public UpdateViewModel(UpdateManager updateManager)
    {
        _updateManager = updateManager;
    }

    [RelayCommand]
    public async Task CheckForUpdatesAsync()
    {
        if (IsCheckingOrDownloading) return;

        try
        {
            IsCheckingOrDownloading = true;
            HasError = false;
            UpdateReady = false;
            StatusText = "Checking for updates…";

            var updateInfo = await _updateManager.CheckForUpdatesAsync();
            if (updateInfo is null)
            {
                StatusText = "You're up to date.";
                await Task.Delay(3000);
                StatusText = string.Empty;
                return;
            }

            StatusText = $"Downloading update v{updateInfo.TargetFullRelease.Version}…";
            DownloadProgress = 0;

            await _updateManager.DownloadUpdatesAsync(
                updateInfo,
                progress => DownloadProgress = progress);

            UpdateReady = true;
            StatusText = $"Update v{updateInfo.TargetFullRelease.Version} ready — click Restart to apply.";
        }
        catch (Exception ex)
        {
            HasError = true;
            StatusText = $"Update failed: {ex.Message}";
        }
        finally
        {
            IsCheckingOrDownloading = false;
        }
    }

    [RelayCommand]
    private void ApplyUpdate()
    {
        if (!UpdateReady) return;
        _updateManager.ApplyUpdatesAndRestart(null);
    }

    [RelayCommand]
    private void Dismiss()
    {
        StatusText = string.Empty;
        HasError = false;
        UpdateReady = false;
    }
}

