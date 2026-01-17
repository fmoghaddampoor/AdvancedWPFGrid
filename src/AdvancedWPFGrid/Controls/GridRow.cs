using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AdvancedWPFGrid.Columns;

namespace AdvancedWPFGrid.Controls;

/// <summary>
/// Represents a row in the grid.
/// </summary>
public class GridRow : Control
{
    #region Static Constructor

    static GridRow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(GridRow),
            new FrameworkPropertyMetadata(typeof(GridRow)));

        FocusableProperty.OverrideMetadata(
            typeof(GridRow),
            new FrameworkPropertyMetadata(true));
    }

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty DataItemProperty = DependencyProperty.Register(
        nameof(DataItem),
        typeof(object),
        typeof(GridRow),
        new FrameworkPropertyMetadata(null, OnDataItemChanged));

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        nameof(IsSelected),
        typeof(bool),
        typeof(GridRow),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsAlternateProperty = DependencyProperty.Register(
        nameof(IsAlternate),
        typeof(bool),
        typeof(GridRow),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RowIndexProperty = DependencyProperty.Register(
        nameof(RowIndex),
        typeof(int),
        typeof(GridRow),
        new FrameworkPropertyMetadata(-1));

    public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register(
        nameof(RowHeight),
        typeof(double),
        typeof(GridRow),
        new FrameworkPropertyMetadata(32.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    #endregion

    #region Properties

    public object? DataItem
    {
        get => GetValue(DataItemProperty);
        set => SetValue(DataItemProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public bool IsAlternate
    {
        get => (bool)GetValue(IsAlternateProperty);
        set => SetValue(IsAlternateProperty, value);
    }

    public int RowIndex
    {
        get => (int)GetValue(RowIndexProperty);
        set => SetValue(RowIndexProperty, value);
    }

    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    internal AdvancedGrid? Grid { get; set; }
    internal GridCellsPresenter? CellsPresenter { get; private set; }

    #endregion

    #region Template

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        CellsPresenter = GetTemplateChild("PART_CellsPresenter") as GridCellsPresenter;
        
        if (CellsPresenter != null)
        {
            CellsPresenter.Row = this;
        }

        UpdateCells();
    }

    #endregion

    #region Methods

    private static void OnDataItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridRow row)
        {
            row.UpdateCells();
        }
    }

    internal void UpdateCells()
    {
        CellsPresenter?.UpdateCells();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (Grid != null && DataItem != null)
        {
            var addToSelection = Grid.SelectionMode != GridSelectionMode.Single &&
                                 (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) ||
                                  Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));

            Grid.SelectItem(DataItem, addToSelection);
            Focus();
            e.Handled = true;
        }
    }

    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        // Begin editing if applicable
        if (Grid != null)
        {
            Grid.BeginEdit();
        }
    }

    protected override Size MeasureOverride(Size constraint)
    {
        var height = RowHeight > 0 ? RowHeight : 32;
        return new Size(constraint.Width, height);
    }

    #endregion
}

/// <summary>
/// Presents cells in a grid row.
/// </summary>
public class GridCellsPresenter : Panel
{
    internal GridRow? Row { get; set; }

    public GridCellsPresenter()
    {
        ClipToBounds = true;
    }

    internal void UpdateCells()
    {
        Children.Clear();

        if (Row?.Grid?.Columns == null || Row.DataItem == null) return;

        foreach (var column in Row.Grid.Columns)
        {
            var cell = new GridCell
            {
                Column = column,
                DataItem = Row.DataItem,
                Width = column.ActualWidth,
                Grid = Row.Grid
            };

            Children.Add(cell);
        }

        InvalidateMeasure();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double totalWidth = 0;
        double maxHeight = Row?.RowHeight ?? 32;

        foreach (UIElement child in Children)
        {
            if (child is GridCell cell)
            {
                cell.Width = cell.Column?.ActualWidth ?? 100;
                child.Measure(new Size(cell.Width, maxHeight));
                totalWidth += cell.Width;
            }
        }

        return new Size(totalWidth, maxHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double x = 0;
        double height = finalSize.Height;

        foreach (UIElement child in Children)
        {
            if (child is GridCell cell)
            {
                var width = cell.Column?.ActualWidth ?? 100;
                child.Arrange(new Rect(x, 0, width, height));
                x += width;
            }
        }

        return new Size(x, height);
    }
}
