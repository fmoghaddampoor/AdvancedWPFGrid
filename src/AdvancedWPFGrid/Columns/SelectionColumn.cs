using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AdvancedWPFGrid.Controls;

namespace AdvancedWPFGrid.Columns;

/// <summary>
/// A column that displays checkboxes for row selection and a "select all" checkbox in the header.
/// </summary>
public class SelectionColumn : GridColumnBase
{
    public SelectionColumn()
    {
        Header = "Select";
        Width = 40;
        CanSort = false;
        CanFilter = false;
        IsSelectionColumn = true;
    }

    public override FrameworkElement GenerateElement(GridCell cell, object dataItem)
    {
        var checkBox = new CheckBox
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0),
            Style = Application.Current.TryFindResource("FluentGridCheckBoxStyle") as Style
        };

        // Bind to GridRow.IsSelected instead of dataItem directly, 
        // as the Grid handles selection state on the row container.
        var binding = new Binding("IsSelected")
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(GridRow), 1),
            Mode = BindingMode.TwoWay
        };
        checkBox.SetBinding(CheckBox.IsCheckedProperty, binding);

        return checkBox;
    }

    public override FrameworkElement GenerateEditingElement(GridCell cell, object dataItem)
    {
        return GenerateElement(cell, dataItem);
    }

    public override void CommitCellEdit(FrameworkElement editingElement, object? dataItem)
    {
        // Selection is maintained by the CheckBox binding to GridRow.IsSelected
    }
}
