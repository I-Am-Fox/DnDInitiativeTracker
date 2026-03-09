using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DnDInitiativeTracker.Core.Interfaces.Services;
using DnDInitiativeTracker.Core.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace DnDInitiativeTracker.ViewModels;

public sealed partial class UpdateViewModel : ObservableObject
{
    private readonly IUpdateService _updateService;
    private readonly ISnackbarService _snackbar;
    private UpdateInfo? _pendingUpdate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    private string _statusText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    private bool _isCheckingOrDownloading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    private bool _updateReady;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVisible))]
    private bool _hasError;

    public bool IsVisible => !string.IsNullOrEmpty(StatusText);
    public bool CanApply => UpdateReady && !IsCheckingOrDownloading;

    public UpdateViewModel(IUpdateService updateService, ISnackbarService snackbar)
    {
        _updateService = updateService;
        _snackbar = snackbar;
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

            var updateInfo = await _updateService.CheckForUpdatesAsync();
            if (updateInfo is null)
            {
                StatusText = "You're up to date.";
                await Task.Delay(3000);
                StatusText = string.Empty;
                return;
            }

            _pendingUpdate = updateInfo;
            UpdateReady = true;
            StatusText = $"Update v{updateInfo.Version} ready — click Restart to apply.";

            _snackbar.Show(
                "Update Available",
                $"Version {updateInfo.Version} is available. Check Settings to update.",
                ControlAppearance.Info,
                new SymbolIcon(SymbolRegular.ArrowSync24),
                TimeSpan.FromSeconds(5));
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
    private async Task ApplyUpdateAsync()
    {
        if (!UpdateReady || _pendingUpdate is null) return;

        try
        {
            IsCheckingOrDownloading = true;
            StatusText = "Downloading and applying update…";

            await _updateService.DownloadAndInstallUpdateAsync(_pendingUpdate);
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
    private void Dismiss()
    {
        StatusText = string.Empty;
        HasError = false;
        UpdateReady = false;
        _pendingUpdate = null;
    }
}

