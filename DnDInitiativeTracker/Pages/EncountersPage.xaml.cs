using System.Windows.Controls;
using DnDInitiativeTracker.ViewModels;

namespace DnDInitiativeTracker.Pages;

public partial class EncountersPage : Page
{
    public EncountersPage(EncounterViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}

