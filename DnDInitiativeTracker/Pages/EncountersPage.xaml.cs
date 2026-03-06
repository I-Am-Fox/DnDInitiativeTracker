using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DnDInitiativeTracker.Core.Models;
using DnDInitiativeTracker.ViewModels;

namespace DnDInitiativeTracker.Pages;

public partial class EncountersPage : Page
{
    public EncountersPage(EncounterViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void EncounterCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: Encounter encounter }
            && DataContext is EncounterViewModel vm)
        {
            vm.SelectEncounterCommand.Execute(encounter);
        }
    }
}

