using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DnDInitiativeTracker.ViewModels;
using Wpf.Ui;

namespace DnDInitiativeTracker;

public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
{
    private readonly ISnackbarService _snackbars;
    private readonly ShellViewModel _shell;
    private bool _suppressNavEvent;

    public MainWindow(MainWindowViewModel vm, ISnackbarService snackbars)
    {
        InitializeComponent();
        DataContext = vm;

        _snackbars = snackbars;
        _shell = vm.Shell;

        _snackbars.SetSnackbarPresenter(SnackbarPresenter);

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.InvokeAsync(() => _shell.Navigate("Campaigns"), DispatcherPriority.Loaded);
    }

    private void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressNavEvent) return;
        if (sender is ListBox listBox && listBox.SelectedItem is ListBoxItem item)
        {
            var tag = item.Tag?.ToString();
            if (!string.IsNullOrEmpty(tag))
            {
                _shell.Navigate(tag);
            }
        }
    }
}

