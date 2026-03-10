using System.Windows.Controls;
using DnDInitiativeTracker.ViewModels;

namespace DnDInitiativeTracker.Pages;

public partial class TranslatorPage : Page
{
    public TranslatorPage(TranslatorViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}

