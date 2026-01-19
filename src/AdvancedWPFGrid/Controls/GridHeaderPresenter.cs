using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Data;
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
                Width = column.ActualWidth,
                ToolTip = column.HeaderToolTip ?? column.Header
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
        double horizontalOffset = Grid?.HorizontalOffset ?? 0;
        int frozenCount = Grid?.FrozenColumnCount ?? 0;
        int currentCount = 0;

        foreach (UIElement child in Children)
        {
            if (child is GridHeaderCell headerCell)
            {
                var width = headerCell.Column?.ActualWidth ?? 100;
                
                if (currentCount < frozenCount)
                {
                    headerCell.IsFrozen = true;
                    headerCell.IsLastFrozen = (currentCount == frozenCount - 1);
                    child.Arrange(new Rect(x, 0, width, height));
                    Panel.SetZIndex(child, 1000 + (frozenCount - currentCount));
                }
                else
                {
                    headerCell.IsFrozen = false;
                    headerCell.IsLastFrozen = false;
                    child.Arrange(new Rect(x - horizontalOffset, 0, width, height));
                    Panel.SetZIndex(child, 0);
                }

                x += width;
                currentCount++;
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

    public static readonly DependencyProperty IsSelectionColumnProperty = DependencyProperty.Register(
        nameof(IsSelectionColumn),
        typeof(bool),
        typeof(GridHeaderCell),
        new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsDragOverProperty = DependencyProperty.Register(
        nameof(IsDragOver),
        typeof(bool),
        typeof(GridHeaderCell),
        new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsFrozenProperty = DependencyProperty.Register(
        nameof(IsFrozen),
        typeof(bool),
        typeof(GridHeaderCell),
        new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsLastFrozenProperty = DependencyProperty.Register(
        nameof(IsLastFrozen),
        typeof(bool),
        typeof(GridHeaderCell),
        new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty GridProperty = DependencyProperty.Register(
        nameof(Grid),
        typeof(AdvancedGrid),
        typeof(GridHeaderCell),
        new FrameworkPropertyMetadata(null, OnGridChanged));

    private static void OnGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridHeaderCell cell && e.NewValue is AdvancedGrid grid)
        {
            if (cell.Column != null)
            {
                cell.CanSort = cell.Column.CanSort && grid.CanUserSort;
                cell.CanFilter = cell.Column.CanFilter && grid.CanUserFilter;
                cell.UpdateSortIndicator();
                cell.UpdateFilterIndicator();
            }
        }
    }

    private static void OnColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridHeaderCell cell && e.NewValue is GridColumnBase column)
        {
            cell.CanSort = column.CanSort && (cell.Grid?.CanUserSort ?? true);
            cell.CanFilter = column.CanFilter && (cell.Grid?.CanUserFilter ?? true);
            cell.IsSelectionColumn = column.IsSelectionColumn;
            cell.UpdateSortIndicator();
            cell.UpdateFilterIndicator();
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

    public bool IsSelectionColumn
    {
        get => (bool)GetValue(IsSelectionColumnProperty);
        set => SetValue(IsSelectionColumnProperty, value);
    }

    public bool IsDragOver
    {
        get => (bool)GetValue(IsDragOverProperty);
        set => SetValue(IsDragOverProperty, value);
    }

    public bool IsFrozen
    {
        get => (bool)GetValue(IsFrozenProperty);
        set => SetValue(IsFrozenProperty, value);
    }

    public bool IsLastFrozen
    {
        get => (bool)GetValue(IsLastFrozenProperty);
        set => SetValue(IsLastFrozenProperty, value);
    }

    public AdvancedGrid? Grid
    {
        get => (AdvancedGrid?)GetValue(GridProperty);
        set => SetValue(GridProperty, value);
    }

    private Thumb? _resizeGrip;
    private Button? _sortButton;
    private Button? _filterButton;
    private bool _isDragging;
    private Point _dragStartPoint;
    private const double DragThreshold = 5.0;
    private static GridHeaderCell? _currentDragOverCell;

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

        // Update indicators
        UpdateSortIndicator();
        UpdateFilterIndicator();
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

    internal void UpdateFilterIndicator()
    {
        if (Grid?.FilterManager != null && Column != null)
        {
            IsFiltered = Grid.FilterManager.HasFilter(Column.Binding ?? string.Empty);
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
        if (Column != null && CanSort)
        {
            Grid?.SortManager.ToggleSort(Column.Binding);
            UpdateSortIndicator();
        }
    }

    #endregion

    #region Drag and Drop Column Reordering

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        
        // Only track for potential drag, don't capture yet
        if (Grid?.CanUserReorderColumns == true && Column != null && !Column.IsSelectionColumn)
        {
            _dragStartPoint = e.GetPosition(this);
            _isDragging = false;
            // Don't capture mouse here - let other elements handle clicks first
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        
        if (e.LeftButton == MouseButtonState.Pressed && Grid?.CanUserReorderColumns == true && 
            Column != null && !Column.IsSelectionColumn)
        {
            var currentPos = e.GetPosition(this);
            var diff = currentPos - _dragStartPoint;
            
            // Only start dragging after exceeding threshold
            if (!_isDragging && (Math.Abs(diff.X) > DragThreshold || Math.Abs(diff.Y) > DragThreshold))
            {
                _isDragging = true;
                CaptureMouse();
                Cursor = Cursors.SizeWE;
            }
            
            // Update drag-over highlight during drag
            if (_isDragging && Grid != null)
            {
                var mousePos = e.GetPosition(Grid);
                var targetCell = FindHeaderCellAtPosition(mousePos);
                
                // Clear previous highlight
                if (_currentDragOverCell != null && _currentDragOverCell != targetCell)
                {
                    _currentDragOverCell.IsDragOver = false;
                }
                
                // Set new highlight (don't highlight self or selection column)
                if (targetCell != null && targetCell != this && 
                    targetCell.Column != null && !targetCell.Column.IsSelectionColumn)
                {
                    targetCell.IsDragOver = true;
                    _currentDragOverCell = targetCell;
                }
                else
                {
                    _currentDragOverCell = null;
                }
            }
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        
        // Clear drag-over highlight
        if (_currentDragOverCell != null)
        {
            _currentDragOverCell.IsDragOver = false;
            _currentDragOverCell = null;
        }
        
        if (_isDragging && IsMouseCaptured)
        {
            ReleaseMouseCapture();
            
            if (Grid?.CanUserReorderColumns == true && Column != null)
            {
                // Find the target header cell under the mouse
                var mousePos = e.GetPosition(Grid);
                var targetCell = FindHeaderCellAtPosition(mousePos);
                
                if (targetCell != null && targetCell != this && targetCell.Column != null && 
                    !targetCell.Column.IsSelectionColumn)
                {
                    MoveColumn(Column, targetCell.Column);
                }
            }
            
            _isDragging = false;
            Cursor = Grid?.CanUserReorderColumns == true ? Cursors.Hand : Cursors.Arrow;
        }
    }

    private GridHeaderCell? FindHeaderCellAtPosition(Point position)
    {
        if (Grid == null) return null;
        
        // Find the GridHeaderPresenter
        var presenter = Grid.Template?.FindName("PART_HeaderPresenter", Grid) as GridHeaderPresenter;
        if (presenter == null) return null;
        
        // Iterate through header cells using VisualTreeHelper
        var childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(presenter);
        for (int i = 0; i < childCount; i++)
        {
            if (System.Windows.Media.VisualTreeHelper.GetChild(presenter, i) is GridHeaderCell child)
            {
                var cellBounds = child.TransformToAncestor(Grid).TransformBounds(
                    new Rect(0, 0, child.ActualWidth, child.ActualHeight));
                
                if (cellBounds.Contains(position))
                {
                    return child;
                }
            }
        }
        
        return null;
    }

    private void MoveColumn(GridColumnBase sourceColumn, GridColumnBase targetColumn)
    {
        if (Grid?.Columns == null) return;
        
        var columns = Grid.Columns;
        var sourceIndex = columns.IndexOf(sourceColumn);
        var targetIndex = columns.IndexOf(targetColumn);
        
        if (sourceIndex >= 0 && targetIndex >= 0 && sourceIndex != targetIndex)
        {
            // True swap: exchange positions by assigning directly
            // We need to use a temporary approach since ObservableCollection doesn't have direct swap
            columns[sourceIndex] = targetColumn;
            columns[targetIndex] = sourceColumn;
            
            // Rebuild headers
            var presenter = Grid.Template?.FindName("PART_HeaderPresenter", Grid) as GridHeaderPresenter;
            presenter?.UpdateHeaders();
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
        if (Column == null || Grid?.FilterManager == null) return;

        // 1. Get Distinct Values
        var allValues = Grid.FilterManager.GetDistinctValues(Column.Binding);
        var currentFilter = Grid.FilterManager.Filters.TryGetValue(Column.Binding ?? string.Empty, out var descriptor) 
            ? descriptor : null;

        // 2. Create Popup
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
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12),
            MinWidth = 240,
            MaxHeight = 400,
            Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 15, Opacity = 0.2, ShadowDepth = 5 }
        };

        var mainStack = new StackPanel();

        // Header
        mainStack.Children.Add(new TextBlock
        {
            Text = $"Filter: {Column.Header}",
            FontWeight = FontWeights.Bold,
            FontSize = 14,
            Foreground = Application.Current.TryFindResource("FluentTextPrimaryBrush") as System.Windows.Media.Brush,
            Margin = new Thickness(0, 0, 0, 10)
        });

        // Search Box
        var searchBox = new TextBox
        {
            Style = Application.Current.TryFindResource("FluentGridTextBoxStyle") as Style,
            Margin = new Thickness(0, 0, 0, 8),
            Tag = "Search values..."
        };
        mainStack.Children.Add(searchBox);

        // ListBox with Checkboxes
        var listBox = new ListBox
        {
            Style = Application.Current.TryFindResource("FluentFilterListBoxStyle") as Style,
            MaxHeight = 200,
            Margin = new Thickness(0, 0, 0, 8)
        };

        // Data Structure for Items
        var filterItems = new System.Collections.ObjectModel.ObservableCollection<FilterItem>();
        
        // Add "Select All"
        var selectAllItem = new FilterItem { Text = "(Select All)", IsChecked = true };
        filterItems.Add(selectAllItem);

        foreach (var val in allValues)
        {
            var isChecked = currentFilter == null || 
                          (currentFilter.Operator == FilterOperator.In && currentFilter.FilterValues.Contains(val)) ||
                          (currentFilter.Operator != FilterOperator.In && val.Contains(currentFilter.FilterValue ?? string.Empty, StringComparison.OrdinalIgnoreCase));
            
            filterItems.Add(new FilterItem { Text = val, IsChecked = isChecked });
        }

        // Handle Select All logic
        selectAllItem.IsChecked = filterItems.Skip(1).All(x => x.IsChecked);
        
        listBox.ItemsSource = filterItems;

        // Synchronize Select All
        bool isUpdating = false;
        selectAllItem.PropertyChanged += (s, ev) =>
        {
            if (isUpdating) return;
            if (ev.PropertyName == nameof(FilterItem.IsChecked))
            {
                isUpdating = true;
                foreach (var item in filterItems.Skip(1))
                    item.IsChecked = selectAllItem.IsChecked;
                isUpdating = false;
            }
        };

        foreach (var item in filterItems.Skip(1))
        {
            item.PropertyChanged += (s, ev) =>
            {
                if (isUpdating) return;
                if (ev.PropertyName == nameof(FilterItem.IsChecked))
                {
                    isUpdating = true;
                    selectAllItem.IsChecked = filterItems.Skip(1).All(x => x.IsChecked);
                    isUpdating = false;
                }
            };
        }

        // Item Template (Checkbox)
        var itemTemplate = new DataTemplate();
        var factory = new FrameworkElementFactory(typeof(CheckBox));
        factory.SetBinding(CheckBox.ContentProperty, new Binding("Text"));
        factory.SetBinding(CheckBox.IsCheckedProperty, new Binding("IsChecked") { Mode = BindingMode.TwoWay });
        factory.SetValue(CheckBox.StyleProperty, Application.Current.TryFindResource("FluentFilterCheckBoxStyle"));
        factory.SetValue(FrameworkElement.MarginProperty, new Thickness(2));
        itemTemplate.VisualTree = factory;
        listBox.ItemTemplate = itemTemplate;

        mainStack.Children.Add(listBox);

        // Filter Logic for Search
        searchBox.TextChanged += (s, ev) =>
        {
            var text = searchBox.Text.ToLower();
            foreach (var item in filterItems.Skip(1))
            {
                var viewItem = listBox.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
                if (viewItem != null)
                {
                    viewItem.Visibility = item.Text.ToLower().Contains(text) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        };

        // Button Bar
        var buttonStack = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        
        var clearBtn = new Button { Content = "Clear", Style = Application.Current.TryFindResource("FluentGridButtonStyle") as Style, Margin = new Thickness(0, 0, 8, 0) };
        var applyBtn = new Button { Content = "Apply", Style = Application.Current.TryFindResource("FluentGridButtonStyle") as Style, FontWeight = FontWeights.Bold };

        applyBtn.Click += (s, ev) =>
        {
            var selectedValues = filterItems.Skip(1).Where(x => x.IsChecked).Select(x => x.Text).ToList();
            if (selectedValues.Count == allValues.Count)
            {
                Grid.FilterManager.ClearFilter(Column.Binding);
            }
            else
            {
                var desc = new FilterDescriptor
                {
                    PropertyName = Column.Binding ?? string.Empty,
                    Operator = FilterOperator.In,
                    FilterValues = selectedValues
                };
                Grid.FilterManager.SetFilter(desc.PropertyName, string.Empty, FilterOperator.In); // Placeholder call to trigger logic
                // Actually we need to update FilterManager to accept a descriptor or fix the SetFilter signature
                // For now, let's hack the SetFilter to support this if we can, or just reach into the manager.
                
                // Better: Update FilterManager to have an overload for SetFilter(FilterDescriptor)
                Grid.FilterManager.SetFilter(desc); 
            }
            UpdateFilterIndicator();
            popup.IsOpen = false;
        };

        clearBtn.Click += (s, ev) =>
        {
            Grid.FilterManager.ClearFilter(Column.Binding);
            UpdateFilterIndicator();
            popup.IsOpen = false;
        };

        buttonStack.Children.Add(clearBtn);
        buttonStack.Children.Add(applyBtn);
        mainStack.Children.Add(buttonStack);

        border.Child = mainStack;
        popup.Child = border;
        popup.IsOpen = true;
    }

    private class FilterItem : INotifyPropertyChanged
    {
        private bool _isChecked;
        public string Text { get; set; } = string.Empty;
        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked))); }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
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
