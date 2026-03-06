using System.Windows.Controls;
using DnDInitiativeTracker.ViewModels;

namespace DnDInitiativeTracker.Pages;

public partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}

