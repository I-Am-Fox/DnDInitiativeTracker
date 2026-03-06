using System.Windows.Controls;
using DnDInitiativeTracker.ViewModels;

namespace DnDInitiativeTracker.Pages;

public partial class CampaignsPage : Page
{
    public CampaignsPage(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}

