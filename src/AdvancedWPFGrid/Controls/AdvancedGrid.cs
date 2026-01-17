using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using AdvancedWPFGrid.Columns;
using AdvancedWPFGrid.Data;
using AdvancedWPFGrid.Managers;

namespace AdvancedWPFGrid.Controls;

/// <summary>
/// A high-performance virtualized data grid with grouping, sorting, filtering, and fluent styling.
/// </summary>
public class AdvancedGrid : Control
{
    #region Static Constructor

    static AdvancedGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(AdvancedGrid),
            new FrameworkPropertyMetadata(typeof(AdvancedGrid)));

        KeyboardNavigation.TabNavigationProperty.OverrideMetadata(
            typeof(AdvancedGrid),
            new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));

        KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(
            typeof(AdvancedGrid),
            new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));

        FocusableProperty.OverrideMetadata(
            typeof(AdvancedGrid),
            new FrameworkPropertyMetadata(true));
    }
    
    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
        nameof(Columns),
        typeof(GridColumnCollection),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null, OnColumnsChanged));

    private bool _initialAutoFitDone = false;

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem),
        typeof(object),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
        nameof(SelectedItems),
        typeof(IList),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(
        nameof(SelectionMode),
        typeof(GridSelectionMode),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(GridSelectionMode.Extended));

    public static readonly DependencyProperty CanUserSortProperty = DependencyProperty.Register(
        nameof(CanUserSort),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty CanUserFilterProperty = DependencyProperty.Register(
        nameof(CanUserFilter),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty CanUserResizeColumnsProperty = DependencyProperty.Register(
        nameof(CanUserResizeColumns),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty CanUserReorderColumnsProperty = DependencyProperty.Register(
        nameof(CanUserReorderColumns),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
        nameof(HeaderHeight),
        typeof(double),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(48.0));

    public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register(
        nameof(RowHeight),
        typeof(double),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(32.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty AlternatingRowBackgroundProperty = DependencyProperty.Register(
        nameof(AlternatingRowBackground),
        typeof(Brush),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty GroupDescriptionsProperty = DependencyProperty.Register(
        nameof(GroupDescriptions),
        typeof(ObservableCollection<GroupDescription>),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null, OnGroupDescriptionsChanged));

    public static readonly DependencyProperty IsGridFocusedProperty = DependencyProperty.RegisterAttached(
        "IsGridFocused",
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    public static readonly DependencyProperty RowHeightBindingProperty = DependencyProperty.Register(
        nameof(RowHeightBinding),
        typeof(string),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty FrozenColumnCountProperty = DependencyProperty.Register(
        nameof(FrozenColumnCount),
        typeof(int),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(0));

    public static readonly DependencyProperty DoubleFormatProperty = DependencyProperty.Register(
        nameof(DoubleFormat),
        typeof(string),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null, OnDoubleFormatChanged));

    public static readonly DependencyProperty HasVerticalGridLinesProperty = DependencyProperty.Register(
        nameof(HasVerticalGridLines),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty HasHorizontalGridLinesProperty = DependencyProperty.Register(
        nameof(HasHorizontalGridLines),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty AlternatingRowsProperty = DependencyProperty.Register(
        nameof(AlternatingRows),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion

    #region Properties

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public GridColumnCollection Columns
    {
        get => (GridColumnCollection)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public IList? SelectedItems
    {
        get => (IList?)GetValue(SelectedItemsProperty);
        private set => SetValue(SelectedItemsProperty, value);
    }

    public GridSelectionMode SelectionMode
    {
        get => (GridSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    public bool CanUserSort
    {
        get => (bool)GetValue(CanUserSortProperty);
        set => SetValue(CanUserSortProperty, value);
    }

    public bool CanUserFilter
    {
        get => (bool)GetValue(CanUserFilterProperty);
        set => SetValue(CanUserFilterProperty, value);
    }

    public bool CanUserResizeColumns
    {
        get => (bool)GetValue(CanUserResizeColumnsProperty);
        set => SetValue(CanUserResizeColumnsProperty, value);
    }

    public bool CanUserReorderColumns
    {
        get => (bool)GetValue(CanUserReorderColumnsProperty);
        set => SetValue(CanUserReorderColumnsProperty, value);
    }

    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    public double HeaderHeight
    {
        get => (double)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    public Brush? AlternatingRowBackground
    {
        get => (Brush?)GetValue(AlternatingRowBackgroundProperty);
        set => SetValue(AlternatingRowBackgroundProperty, value);
    }

    public ObservableCollection<GroupDescription>? GroupDescriptions
    {
        get => (ObservableCollection<GroupDescription>?)GetValue(GroupDescriptionsProperty);
        set => SetValue(GroupDescriptionsProperty, value);
    }

    public string? RowHeightBinding
    {
        get => (string?)GetValue(RowHeightBindingProperty);
        set => SetValue(RowHeightBindingProperty, value);
    }

    public int FrozenColumnCount
    {
        get => (int)GetValue(FrozenColumnCountProperty);
        set => SetValue(FrozenColumnCountProperty, value);
    }

    public string? DoubleFormat
    {
        get => (string?)GetValue(DoubleFormatProperty);
        set => SetValue(DoubleFormatProperty, value);
    }

    public bool HasVerticalGridLines
    {
        get => (bool)GetValue(HasVerticalGridLinesProperty);
        set => SetValue(HasVerticalGridLinesProperty, value);
    }

    public bool HasHorizontalGridLines
    {
        get => (bool)GetValue(HasHorizontalGridLinesProperty);
        set => SetValue(HasHorizontalGridLinesProperty, value);
    }

    public bool AlternatingRows
    {
        get => (bool)GetValue(AlternatingRowsProperty);
        set => SetValue(AlternatingRowsProperty, value);
    }

    private static void OnDoubleFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdvancedGrid grid)
        {
            grid.RefreshView();
        }
    }

    public static bool GetIsGridFocused(DependencyObject obj) => (bool)obj.GetValue(IsGridFocusedProperty);
    public static void SetIsGridFocused(DependencyObject obj, bool value) => obj.SetValue(IsGridFocusedProperty, value);

    #endregion

    #region Internal Properties

    public void AutoFitColumn(GridColumnBase column)
    {
        if (column == null) return;

        double maxWidth = 0;

        // 1. Measure Header
        if (!string.IsNullOrEmpty(column.Header))
        {
            var formattedHeader = new FormattedText(
                column.Header,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Foreground,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            maxWidth = formattedHeader.Width;
        }

        // 2. Measure Data
        if (ItemsSource != null)
        {
            var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            // Measure first 500 items to avoid performance hit on large datasets
            int count = 0;
            foreach (var item in ItemsSource)
            {
                if (count++ > 500) break;

                var value = column.GetCellValue(item);
                if (value != null)
                {
                    string text = value.ToString() ?? string.Empty;
                    // Format if the column has a string format? 
                    // GridColumnBase doesn't expose StringFormat universally, check if it's a specific column type?
                    // For now, simple ToString().
                    
                    if (!string.IsNullOrEmpty(text))
                    {
                         var formattedText = new FormattedText(
                            text,
                            System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            typeface,
                            FontSize,
                            Foreground,
                            dpi);

                        if (formattedText.Width > maxWidth)
                        {
                            maxWidth = formattedText.Width;
                        }
                    }
                }
            }
        }

        // Add sufficient padding for:
        // - Cell Padding (left/right)
        // - Header Padding (10+8=18px)
        // - Two Large Buttons (28x2 = 56px)
        // - Margins & Spacing (~10px)
        // - Buffer for Font Variations (Bold etc)
        // Total needed ~100px.
        double extraWidth = 100.0;
        
        column.Width = maxWidth + extraWidth;
    }

    public void AutoFitAllColumns()
    {
        if (Columns == null) return;
        foreach (var column in Columns)
        {
            AutoFitColumn(column);
        }
    }
    
    public SortManager SortManager { get; }
    public FilterManager FilterManager { get; }
    public GroupManager GroupManager { get; }
    internal ICollectionView? CollectionView { get; private set; }
    internal VirtualizingGridPanel? ItemsHost { get; private set; }
    internal GridHeaderPresenter? HeaderPresenter { get; private set; }
    internal ScrollViewer? ScrollViewer { get; private set; }

    private readonly ObservableCollection<object> _selectedItems = new ObservableCollection<object>();

    #endregion

    #region Constructor

    public AdvancedGrid()
    {
        Columns = new GridColumnCollection();
        GroupDescriptions = new ObservableCollection<GroupDescription>();
        SelectedItems = _selectedItems;

        SortManager = new SortManager(this);
        FilterManager = new FilterManager(this);
        GroupManager = new GroupManager(this);

        Loaded += OnGridLoaded;
        Unloaded += OnGridUnloaded;
        GotFocus += OnGridGotFocus;
        LostFocus += OnGridLostFocus;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (Columns != null)
        {
            foreach (var column in Columns)
            {
                column.DataContext = e.NewValue;
            }
        }
    }

    #endregion

    #region Template

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        ItemsHost = GetTemplateChild("PART_ItemsHost") as VirtualizingGridPanel;
        HeaderPresenter = GetTemplateChild("PART_HeaderPresenter") as GridHeaderPresenter;
        ScrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;

        if (HeaderPresenter != null)
        {
            HeaderPresenter.Grid = this;
        }

        if (ItemsHost != null)
        {
            ItemsHost.Grid = this;
        }

        if (ScrollViewer != null)
        {
            ScrollViewer.ScrollChanged += OnScrollChanged;
        }

        RefreshView();
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (HeaderPresenter != null)
        {
            HeaderPresenter.RenderTransform = new TranslateTransform(-e.HorizontalOffset, 0);
        }
    }

    #endregion

    #region Event Handlers

    private void OnGridLoaded(object sender, RoutedEventArgs e)
    {
        RefreshView();
        
        if (!_initialAutoFitDone && ItemsSource != null)
        {
            Dispatcher.BeginInvoke(new Action(() => {
                if (!_initialAutoFitDone) {
                    AutoFitAllColumns();
                    _initialAutoFitDone = true;
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private void OnGridUnloaded(object sender, RoutedEventArgs e)
    {
        // Cleanup
    }

    private void OnGridGotFocus(object sender, RoutedEventArgs e)
    {
        SetIsGridFocused(this, true);
    }

    private void OnGridLostFocus(object sender, RoutedEventArgs e)
    {
        if (!IsKeyboardFocusWithin)
        {
            SetIsGridFocused(this, false);
        }
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdvancedGrid grid)
        {
            grid.OnItemsSourceChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
        }
    }

    private void OnItemsSourceChanged(IEnumerable? oldValue, IEnumerable? newValue)
    {
        if (oldValue is INotifyCollectionChanged oldNcc)
        {
            oldNcc.CollectionChanged -= OnSourceCollectionChanged;
        }

        if (newValue is INotifyCollectionChanged newNcc)
        {
            newNcc.CollectionChanged += OnSourceCollectionChanged;
        }

        CollectionView = newValue != null ? CollectionViewSource.GetDefaultView(newValue) : null;
        
        if (CollectionView != null)
        {
            // Apply grouping
            GroupManager?.ApplyGrouping();
            // Apply sorting
            SortManager?.ApplySorting();
            // Apply filtering
            FilterManager?.ApplyFiltering();
        }

        RefreshView();

        if (IsLoaded && !_initialAutoFitDone && newValue != null)
        {
            Dispatcher.BeginInvoke(new Action(() => {
                if (!_initialAutoFitDone) {
                    AutoFitAllColumns();
                    _initialAutoFitDone = true;
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshView();
    }

    private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdvancedGrid grid)
        {
            if (e.OldValue is GridColumnCollection oldColumns)
            {
                oldColumns.CollectionChanged -= grid.OnColumnsCollectionChanged;
                foreach (var column in oldColumns)
                {
                    column.Grid = null;
                }
            }

            if (e.NewValue is GridColumnCollection newColumns)
            {
                newColumns.CollectionChanged += grid.OnColumnsCollectionChanged;
                foreach (var column in newColumns)
                {
                    column.Grid = grid;
                    column.DataContext = grid.DataContext;
                }
            }

            grid.RefreshView();
        }
    }

    private void OnColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (GridColumnBase column in e.NewItems)
            {
                column.Grid = this;
                column.DataContext = this.DataContext;
            }
        }

        if (e.OldItems != null)
        {
            foreach (GridColumnBase column in e.OldItems)
            {
                column.Grid = null;
            }
        }

        HeaderPresenter?.InvalidateMeasure();
        RefreshView();
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdvancedGrid grid)
        {
            grid.OnSelectedItemChanged(e.OldValue, e.NewValue);
        }
    }

    private void OnSelectedItemChanged(object? oldValue, object? newValue)
    {
        if (SelectionMode == GridSelectionMode.Single)
        {
            _selectedItems.Clear();
            if (newValue != null)
            {
                _selectedItems.Add(newValue);
            }
        }

        RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, 
            oldValue != null ? new List<object> { oldValue } : new List<object>(), 
            newValue != null ? new List<object> { newValue } : new List<object>()));
    }

    private static void OnGroupDescriptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdvancedGrid grid)
        {
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= grid.OnGroupDescriptionsCollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += grid.OnGroupDescriptionsCollectionChanged;
            }

            // Only apply grouping if GroupManager is initialized
            grid.GroupManager?.ApplyGrouping();
        }
    }

    private void OnGroupDescriptionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        GroupManager?.ApplyGrouping();
    }

    #endregion

    #region Selection

    public void SelectItem(object item, bool addToSelection = false)
    {
        if (!addToSelection || SelectionMode == GridSelectionMode.Single)
        {
            _selectedItems.Clear();
        }

        if (!_selectedItems.Contains(item))
        {
            _selectedItems.Add(item);
        }

        SelectedItem = item;
        ItemsHost?.InvalidateVisual();
    }

    public void DeselectItem(object item)
    {
        _selectedItems.Remove(item);
        
        if (SelectedItem == item)
        {
            SelectedItem = _selectedItems.Count > 0 ? _selectedItems[0] : null;
        }

        ItemsHost?.InvalidateVisual();
    }

    public void SelectAll()
    {
        if (SelectionMode != GridSelectionMode.Single && CollectionView != null)
        {
            _selectedItems.Clear();
            foreach (var item in CollectionView)
            {
                _selectedItems.Add(item);
            }
            ItemsHost?.InvalidateVisual();
        }
    }

    public void ClearSelection()
    {
        _selectedItems.Clear();
        SelectedItem = null;
        ItemsHost?.InvalidateVisual();
    }

    public bool IsItemSelected(object item) => _selectedItems.Contains(item);

    #endregion

    #region Public Methods

    public void RefreshView()
    {
        if (ItemsHost is VirtualizingGridPanel panel)
        {
            panel.ClearRealizedRows();
        }
        
        // Refresh header sort/filter indicators
        if (HeaderPresenter != null)
        {
            foreach (UIElement child in HeaderPresenter.Children)
            {
                if (child is GridHeaderCell cell)
                {
                    cell.UpdateSortIndicator();
                }
            }
        }

        ItemsHost?.InvalidateMeasure();
        ItemsHost?.InvalidateVisual();
        HeaderPresenter?.InvalidateMeasure();

        if (ItemsHost != null)
        {
            foreach (UIElement child in ItemsHost.PublicInternalChildren)
            {
                if (child is GridRow row)
                {
                    row.UpdateCells();
                }
            }
        }
    }

    public void ScrollIntoView(object item)
    {
        if (CollectionView == null || ItemsHost == null) return;

        var items = CollectionView.Cast<object>().ToList();
        var index = items.IndexOf(item);
        
        if (index >= 0)
        {
            var offset = index * RowHeight;
            ScrollViewer?.ScrollToVerticalOffset(offset);
        }
    }

    public void BeginEdit()
    {
        // Start editing the current cell
    }

    public void CancelEdit()
    {
        // Cancel current edit
    }

    public void CommitEdit()
    {
        // Commit current edit
    }

    #endregion

    #region Routed Events

    public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(SelectionChanged),
        RoutingStrategy.Bubble,
        typeof(SelectionChangedEventHandler),
        typeof(AdvancedGrid));

    public event SelectionChangedEventHandler SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    public static readonly RoutedEvent CellEditEndingEvent = EventManager.RegisterRoutedEvent(
        nameof(CellEditEnding),
        RoutingStrategy.Bubble,
        typeof(EventHandler<GridCellEditEndingEventArgs>),
        typeof(AdvancedGrid));

    public event EventHandler<GridCellEditEndingEventArgs> CellEditEnding
    {
        add => AddHandler(CellEditEndingEvent, value);
        remove => RemoveHandler(CellEditEndingEvent, value);
    }

    #endregion

    #region Keyboard Navigation

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (CollectionView == null) return;

        var items = CollectionView.Cast<object>().ToList();
        var currentIndex = SelectedItem != null ? items.IndexOf(SelectedItem) : -1;

        switch (e.Key)
        {
            case Key.Up:
                if (currentIndex > 0)
                {
                    SelectItem(items[currentIndex - 1], Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
                    ScrollIntoView(items[currentIndex - 1]);
                }
                e.Handled = true;
                break;

            case Key.Down:
                if (currentIndex < items.Count - 1)
                {
                    var newIndex = currentIndex < 0 ? 0 : currentIndex + 1;
                    SelectItem(items[newIndex], Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
                    ScrollIntoView(items[newIndex]);
                }
                e.Handled = true;
                break;

            case Key.Home:
                if (items.Count > 0)
                {
                    SelectItem(items[0], Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
                    ScrollIntoView(items[0]);
                }
                e.Handled = true;
                break;

            case Key.End:
                if (items.Count > 0)
                {
                    SelectItem(items[^1], Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
                    ScrollIntoView(items[^1]);
                }
                e.Handled = true;
                break;

            case Key.PageUp:
                if (ScrollViewer != null && items.Count > 0)
                {
                    var pageSize = (int)(ScrollViewer.ViewportHeight / RowHeight);
                    var newIndex = Math.Max(0, currentIndex - pageSize);
                    SelectItem(items[newIndex], Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
                    ScrollIntoView(items[newIndex]);
                }
                e.Handled = true;
                break;

            case Key.PageDown:
                if (ScrollViewer != null && items.Count > 0)
                {
                    var pageSize = (int)(ScrollViewer.ViewportHeight / RowHeight);
                    var newIndex = Math.Min(items.Count - 1, currentIndex + pageSize);
                    SelectItem(items[newIndex], Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
                    ScrollIntoView(items[newIndex]);
                }
                e.Handled = true;
                break;

            case Key.A:
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    SelectAll();
                    e.Handled = true;
                }
                break;

            case Key.Space:
                if (SelectedItem != null)
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        if (IsItemSelected(SelectedItem))
                        {
                            DeselectItem(SelectedItem);
                        }
                        else
                        {
                            SelectItem(SelectedItem, true);
                        }
                    }
                    e.Handled = true;
                }
                break;
        }
    }

    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        if (!e.Handled && CanUserResizeColumns)
        {
            var pos = e.GetPosition(this);
            if (pos.Y <= HeaderHeight)
            {
                // If it's in the header area but not handled by cells, auto-fit all
                AutoFitAllColumns();
                e.Handled = true;
            }
        }
    }

    #endregion
}

/// <summary>
/// Grid selection mode enumeration.
/// </summary>
public enum GridSelectionMode
{
    Single,
    Multiple,
    Extended
}

/// <summary>
/// Event arguments for cell edit ending event.
/// </summary>
public class GridCellEditEndingEventArgs : RoutedEventArgs
{
    public GridColumnBase Column { get; }
    public object DataItem { get; }
    public bool Cancel { get; set; }

    public GridCellEditEndingEventArgs(GridColumnBase column, object dataItem)
        : base(AdvancedGrid.CellEditEndingEvent)
    {
        Column = column;
        DataItem = dataItem;
    }
}
