using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using AdvancedWPFGrid.Columns;
using AdvancedWPFGrid.Data;

namespace AdvancedWPFGrid.Controls;

/// <summary>
/// Presents column headers in the grid.
/// </summary>
public class GridHeaderPresenter : Panel
{
    private AdvancedGrid? _grid;
    internal AdvancedGrid? Grid 
    { 
        get => _grid; 
        set
        {
            _grid = value;
            UpdateHeaders();
        }
    }

    public GridHeaderPresenter()
    {
        ClipToBounds = true;
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);
        UpdateHeaders();
    }

    internal void UpdateHeaders()
    {
        Children.Clear();

        if (Grid?.Columns == null) return;

        foreach (var column in Grid.Columns)
        {
            var headerCell = new GridHeaderCell
            {
                Content = column.Header,
                Column = column,
                Grid = Grid,
                Width = column.ActualWidth
            };

            Children.Add(headerCell);
        }

        InvalidateMeasure();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double totalWidth = 0;
        var height = Grid?.HeaderHeight ?? 36;

        foreach (UIElement child in Children)
        {
            if (child is GridHeaderCell headerCell)
            {
                headerCell.Width = headerCell.Column?.ActualWidth ?? 100;
                child.Measure(new Size(headerCell.Width, height));
                totalWidth += headerCell.Width;
            }
        }

        return new Size(Math.Max(totalWidth, availableSize.Width), height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double x = 0;
        var height = Grid?.HeaderHeight ?? 36;

        foreach (UIElement child in Children)
        {
            if (child is GridHeaderCell headerCell)
            {
                var width = headerCell.Column?.ActualWidth ?? 100;
                child.Arrange(new Rect(x, 0, width, height));
                x += width;
            }
        }

        return new Size(Math.Max(x, finalSize.Width), height);
    }
}

/// <summary>
/// Represents a column header cell.
/// </summary>
public class GridHeaderCell : ContentControl
{
    #region Static Constructor

    static GridHeaderCell()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(GridHeaderCell),
            new FrameworkPropertyMetadata(typeof(GridHeaderCell)));
    }

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.Register(
        nameof(SortDirection),
        typeof(ListSortDirection?),
        typeof(GridHeaderCell),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsFilteredProperty = DependencyProperty.Register(
        nameof(IsFiltered),
        typeof(bool),
        typeof(GridHeaderCell),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register(
        nameof(Column),
        typeof(GridColumnBase),
        typeof(GridHeaderCell),
        new FrameworkPropertyMetadata(null, OnColumnChanged));

    public static readonly DependencyProperty CanSortProperty = DependencyProperty.Register(
        nameof(CanSort),
        typeof(bool),
        typeof(GridHeaderCell),
        new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty CanFilterProperty = DependencyProperty.Register(
        nameof(CanFilter),
        typeof(bool),
        typeof(GridHeaderCell),
        new FrameworkPropertyMetadata(true));

    private static void OnColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridHeaderCell cell && e.NewValue is GridColumnBase column)
        {
            cell.CanSort = column.CanSort;
            cell.CanFilter = column.CanFilter;
            cell.UpdateSortIndicator();
        }
    }

    #endregion

    #region Properties

    public ListSortDirection? SortDirection
    {
        get => (ListSortDirection?)GetValue(SortDirectionProperty);
        set => SetValue(SortDirectionProperty, value);
    }

    public bool IsFiltered
    {
        get => (bool)GetValue(IsFilteredProperty);
        set => SetValue(IsFilteredProperty, value);
    }

    public GridColumnBase? Column
    {
        get => (GridColumnBase?)GetValue(ColumnProperty);
        set => SetValue(ColumnProperty, value);
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

    internal AdvancedGrid? Grid { get; set; }

    private Thumb? _resizeGrip;
    private Button? _sortButton;
    private Button? _filterButton;

    #endregion

    #region Template

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _resizeGrip = GetTemplateChild("PART_ResizeGrip") as Thumb;
        if (_resizeGrip != null)
        {
            _resizeGrip.DragDelta += OnResizeGripDragDelta;
        }

        _sortButton = GetTemplateChild("PART_SortButton") as Button;
        if (_sortButton != null)
        {
            _sortButton.Click += OnSortButtonClick;
        }

        _filterButton = GetTemplateChild("PART_FilterButton") as Button;
        if (_filterButton != null)
        {
            _filterButton.Click += OnFilterButtonClick;
        }

        // Update sort direction
        UpdateSortIndicator();
    }

    #endregion

    #region Methods

    internal void UpdateSortIndicator()
    {
        if (Grid?.SortManager != null && Column != null)
        {
            SortDirection = Grid.SortManager.GetSortDirection(Column.Binding);
        }
    }

    private void OnResizeGripDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (Column != null && Grid?.CanUserResizeColumns == true)
        {
            var newWidth = Column.ActualWidth + e.HorizontalChange;
            newWidth = Math.Max(Column.MinWidth, Math.Min(Column.MaxWidth, newWidth));
            Column.Width = newWidth;
            
            Grid.RefreshView();
        }
    }

    private void OnSortButtonClick(object sender, RoutedEventArgs e)
    {
        if (Column != null && Column.CanSort && Grid?.CanUserSort == true)
        {
            Grid.SortManager.ToggleSort(Column.Binding);
            UpdateSortIndicator();
        }
    }

    private void OnFilterButtonClick(object sender, RoutedEventArgs e)
    {
        if (Column != null && Column.CanFilter && Grid?.CanUserFilter == true)
        {
            ShowFilterPopup();
        }
    }

    private void ShowFilterPopup()
    {
        // Create a simple filter popup
        var popup = new Popup
        {
            PlacementTarget = _filterButton ?? (UIElement)this,
            Placement = PlacementMode.Bottom,
            StaysOpen = false,
            AllowsTransparency = true
        };

        var border = new Border
        {
            Background = Application.Current.TryFindResource("FluentSurfaceBrush") as System.Windows.Media.Brush,
            BorderBrush = Application.Current.TryFindResource("FluentBorderBrush") as System.Windows.Media.Brush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8),
            MinWidth = 200
        };

        var stack = new StackPanel();

        var label = new TextBlock
        {
            Text = $"Filter: {Column?.Header}",
            FontWeight = FontWeights.SemiBold,
            Foreground = Application.Current.TryFindResource("FluentTextPrimaryBrush") as System.Windows.Media.Brush,
            Margin = new Thickness(0, 0, 0, 8)
        };

        var textBox = new TextBox
        {
            Style = Application.Current.TryFindResource("FluentGridTextBoxStyle") as Style,
            Margin = new Thickness(0, 0, 0, 8)
        };

        var buttonStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var applyButton = new Button
        {
            Content = "Apply",
            Style = Application.Current.TryFindResource("FluentGridButtonStyle") as Style,
            Padding = new Thickness(12, 4, 12, 4)
        };

        var clearButton = new Button
        {
            Content = "Clear",
            Style = Application.Current.TryFindResource("FluentGridButtonStyle") as Style,
            Padding = new Thickness(12, 4, 12, 4),
            Margin = new Thickness(0, 0, 4, 0)
        };

        applyButton.Click += (s, e) =>
        {
            if (Column != null && Grid != null)
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    Grid.FilterManager.SetFilter(Column.Binding, textBox.Text);
                    IsFiltered = true;
                }
                popup.IsOpen = false;
            }
        };

        clearButton.Click += (s, e) =>
        {
            if (Column != null && Grid != null)
            {
                Grid.FilterManager.ClearFilter(Column.Binding);
                IsFiltered = false;
                popup.IsOpen = false;
            }
        };

        buttonStack.Children.Add(clearButton);
        buttonStack.Children.Add(applyButton);

        stack.Children.Add(label);
        stack.Children.Add(textBox);
        stack.Children.Add(buttonStack);

        border.Child = stack;
        popup.Child = border;
        popup.IsOpen = true;

        textBox.Focus();
    }

    #endregion
}

/// <summary>
/// Represents a group header row in the grid.
/// </summary>
public class GridGroupRow : ContentControl
{
    #region Static Constructor

    static GridGroupRow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(GridGroupRow),
            new FrameworkPropertyMetadata(typeof(GridGroupRow)));
    }

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded),
        typeof(bool),
        typeof(GridGroupRow),
        new FrameworkPropertyMetadata(true, OnIsExpandedChanged));

    public static readonly DependencyProperty GroupLevelProperty = DependencyProperty.Register(
        nameof(GroupLevel),
        typeof(int),
        typeof(GridGroupRow),
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty GridGroupItemProperty = DependencyProperty.Register(
        nameof(GridGroupItem),
        typeof(GridGroupItem),
        typeof(GridGroupRow),
        new FrameworkPropertyMetadata(null, OnGridGroupItemChanged));

    public static readonly DependencyProperty ItemCountTextProperty = DependencyProperty.Register(
        nameof(ItemCountText),
        typeof(string),
        typeof(GridGroupRow),
        new FrameworkPropertyMetadata(string.Empty));

    #endregion

    #region Properties

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public int GroupLevel
    {
        get => (int)GetValue(GroupLevelProperty);
        set => SetValue(GroupLevelProperty, value);
    }

    public GridGroupItem? GridGroupItem
    {
        get => (GridGroupItem?)GetValue(GridGroupItemProperty);
        set => SetValue(GridGroupItemProperty, value);
    }

    public string ItemCountText
    {
        get => (string)GetValue(ItemCountTextProperty);
        set => SetValue(ItemCountTextProperty, value);
    }

    internal AdvancedGrid? Grid { get; set; }

    #endregion

    #region Event Handlers

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridGroupRow groupRow)
        {
            if (groupRow.GridGroupItem != null)
            {
                groupRow.GridGroupItem.IsExpanded = (bool)e.NewValue;
            }
            groupRow.Grid?.RefreshView();
        }
    }

    private static void OnGridGroupItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridGroupRow groupRow && e.NewValue is GridGroupItem item)
        {
            groupRow.Content = item.Key?.ToString() ?? "(null)";
            groupRow.GroupLevel = item.Level;
            groupRow.IsExpanded = item.IsExpanded;
            groupRow.ItemCountText = $"({item.ItemCount} items)";
        }
    }

    #endregion

    #region Methods

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        IsExpanded = !IsExpanded;
        e.Handled = true;
    }

    #endregion
}
