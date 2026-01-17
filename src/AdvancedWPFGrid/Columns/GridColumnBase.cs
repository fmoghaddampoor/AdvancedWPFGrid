using System.Collections.ObjectModel;
using System.Windows;
using AdvancedWPFGrid.Controls;

namespace AdvancedWPFGrid.Columns;

/// <summary>
/// Base class for all grid columns.
/// </summary>
public abstract class GridColumnBase : DependencyObject
{
    #region Dependency Properties

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header),
        typeof(string),
        typeof(GridColumnBase),
        new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty BindingProperty = DependencyProperty.Register(
        nameof(Binding),
        typeof(string),
        typeof(GridColumnBase),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty HeaderToolTipProperty = DependencyProperty.Register(
        nameof(HeaderToolTip),
        typeof(object),
        typeof(GridColumnBase),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
        nameof(Width),
        typeof(double),
        typeof(GridColumnBase),
        new FrameworkPropertyMetadata(100.0, OnWidthChanged));

    public static readonly DependencyProperty MinWidthProperty = DependencyProperty.Register(
        nameof(MinWidth),
        typeof(double),
        typeof(GridColumnBase),
        new FrameworkPropertyMetadata(30.0));

    public static readonly DependencyProperty MaxWidthProperty = DependencyProperty.Register(
        nameof(MaxWidth),
        typeof(double),
        typeof(GridColumnBase),
        new FrameworkPropertyMetadata(double.MaxValue));

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly),
        typeof(bool),
        typeof(GridColumnBase),
        new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty CanSortProperty = DependencyProperty.Register(
        nameof(CanSort),
        typeof(bool),
        typeof(GridColumnBase),
        new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty CanFilterProperty = DependencyProperty.Register(
        nameof(CanFilter),
        typeof(bool),
        typeof(GridColumnBase),
        new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register(
        nameof(IsVisible),
        typeof(bool),
        typeof(GridColumnBase),
        new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty HorizontalAlignmentProperty = DependencyProperty.Register(
        nameof(HorizontalAlignment),
        typeof(HorizontalAlignment),
        typeof(GridColumnBase),
        new FrameworkPropertyMetadata(HorizontalAlignment.Left));

    public static readonly DependencyProperty DataContextProperty = DependencyProperty.Register(
        nameof(DataContext),
        typeof(object),
        typeof(GridColumnBase),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IsSelectionColumnProperty = DependencyProperty.Register(
        nameof(IsSelectionColumn),
        typeof(bool),
        typeof(GridColumnBase),
        new FrameworkPropertyMetadata(false));

    #endregion

    #region Properties

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string? Binding
    {
        get => (string?)GetValue(BindingProperty);
        set => SetValue(BindingProperty, value);
    }

    public object? HeaderToolTip
    {
        get => GetValue(HeaderToolTipProperty);
        set => SetValue(HeaderToolTipProperty, value);
    }

    public double Width
    {
        get => (double)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    public double MinWidth
    {
        get => (double)GetValue(MinWidthProperty);
        set => SetValue(MinWidthProperty, value);
    }

    public double MaxWidth
    {
        get => (double)GetValue(MaxWidthProperty);
        set => SetValue(MaxWidthProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool CanSort
    {
        get => (bool)GetValue(CanSortProperty);
        set => SetValue(CanSortProperty, value);
    }

    public bool CanFilter
    {
        get => (bool)GetValue(CanFilterProperty);
        set => SetValue(CanFilterProperty, value);
    }

    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    public HorizontalAlignment HorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalAlignmentProperty);
        set => SetValue(HorizontalAlignmentProperty, value);
    }

    public object DataContext
    {
        get => GetValue(DataContextProperty);
        set => SetValue(DataContextProperty, value);
    }

    public double ActualWidth => Math.Max(MinWidth, Math.Min(MaxWidth, Width));

    public bool IsSelectionColumn
    {
        get => (bool)GetValue(IsSelectionColumnProperty);
        set => SetValue(IsSelectionColumnProperty, value);
    }

    internal AdvancedGrid? Grid { get; set; }

    #endregion

    #region Methods

    private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridColumnBase column)
        {
            column.Grid?.RefreshView();
        }
    }

    /// <summary>
    /// Generates the display element for a cell.
    /// </summary>
    public abstract FrameworkElement GenerateElement(GridCell cell, object dataItem);

    /// <summary>
    /// Generates the editing element for a cell.
    /// </summary>
    public abstract FrameworkElement GenerateEditingElement(GridCell cell, object dataItem);

    /// <summary>
    /// Commits the edit from the editing element to the data item.
    /// </summary>
    public abstract void CommitCellEdit(FrameworkElement editingElement, object? dataItem);

    /// <summary>
    /// Gets the display text for a cell in this column.
    /// </summary>
    public virtual string GetCellDisplayText(object dataItem)
    {
        return GetCellValue(dataItem)?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the value from the data item for this column.
    /// </summary>
    internal object? GetCellValue(object dataItem)
    {
        if (Binding == null) return null;

        try
        {
            var property = dataItem.GetType().GetProperty(Binding);
            return property?.GetValue(dataItem);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Sets the value on the data item for this column.
    /// </summary>
    protected void SetCellValue(object dataItem, object? value)
    {
        if (Binding == null) return;

        try
        {
            var property = dataItem.GetType().GetProperty(Binding);
            if (property?.CanWrite == true)
            {
                property.SetValue(dataItem, value);
            }
        }
        catch
        {
            // Handle conversion errors
        }
    }

    #endregion
}

/// <summary>
/// Collection of grid columns.
/// </summary>
public class GridColumnCollection : ObservableCollection<GridColumnBase>
{
}
