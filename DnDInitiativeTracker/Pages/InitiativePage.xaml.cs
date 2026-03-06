using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DnDInitiativeTracker.ViewModels;

namespace DnDInitiativeTracker.Pages;

public partial class InitiativePage : Page
{
    private CombatantViewModel? _draggedItem;
    private Point _dragStartPoint;

    public InitiativePage(EncounterViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void CombatantList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        if (_draggedItem is null) return;

        var delta = e.GetPosition(null) - _dragStartPoint;
        if (Math.Abs(delta.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(delta.Y) < SystemParameters.MinimumVerticalDragDistance) return;

        DragDrop.DoDragDrop(CombatantListBox, _draggedItem, DragDropEffects.Move);
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
        _dragStartPoint = e.GetPosition(null);

        var item = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        _draggedItem = item?.DataContext as CombatantViewModel;
    }

    private async void CombatantList_Drop(object sender, DragEventArgs e)
    {
        if (_draggedItem is null) return;

        var target = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        if (target?.DataContext is not CombatantViewModel targetVm) return;
        if (ReferenceEquals(_draggedItem, targetVm)) return;

        if (DataContext is EncounterViewModel vm)
        {
            var items = vm.Combatants;
            var oldIndex = items.IndexOf(_draggedItem);
            var newIndex = items.IndexOf(targetVm);

            if (oldIndex >= 0 && newIndex >= 0)
                await vm.MoveCombatantAsync(oldIndex, newIndex);
        }

        _draggedItem = null;
    }

    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T typed) return typed;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}

