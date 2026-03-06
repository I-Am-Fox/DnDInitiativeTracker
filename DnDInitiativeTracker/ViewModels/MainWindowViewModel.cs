using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDInitiativeTracker.ViewModels;

/// <summary>
/// Top-level view model for the main window. Holds no logic —
/// navigation is handled by WPF-UI's NavigationService.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _applicationTitle = "D&D Initiative Tracker";
}

