using CommunityToolkit.Mvvm.ComponentModel;
using DnDInitiativeTracker.Core.Config;

namespace DnDInitiativeTracker.ViewModels;

/// <summary>
/// Top-level view model for the main window.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _applicationTitle = "D&D Initiative Tracker";

    [ObservableProperty]
    private string _applicationVersion = $"{AppInfo.GetApplicationVersion()} - Ready";

    public UpdateViewModel Update { get; }
    public ShellViewModel Shell { get; }

    public MainWindowViewModel(UpdateViewModel updateViewModel, ShellViewModel shellViewModel)
    {
        Update = updateViewModel;
        Shell = shellViewModel;
    }
}

