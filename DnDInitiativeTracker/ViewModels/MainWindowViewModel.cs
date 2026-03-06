using CommunityToolkit.Mvvm.ComponentModel;

namespace DnDInitiativeTracker.ViewModels;

/// <summary>
/// Top-level view model for the main window.
/// </summary>
public sealed partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _applicationTitle = "D&D Initiative Tracker";

    public UpdateViewModel Update { get; }
    public ShellViewModel Shell { get; }

    public MainWindowViewModel(UpdateViewModel updateViewModel, ShellViewModel shellViewModel)
    {
        Update = updateViewModel;
        Shell = shellViewModel;
    }
}

