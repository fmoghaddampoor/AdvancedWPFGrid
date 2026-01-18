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
using AdvancedWPFGrid.Common;
using AdvancedWPFGrid.Export;
using AdvancedWPFGrid.Export.ExportServices;
using AdvancedWPFGrid.Export.UI;
using Microsoft.Win32;
using System;
using System.Linq;

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

    /// <summary>
    /// Identifies the <see cref="ItemsSource"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null, OnItemsSourceChanged));

    /// <summary>
    /// Identifies the <see cref="Columns"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
        nameof(Columns),
        typeof(GridColumnCollection),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null, OnColumnsChanged));

    /// <summary>
    /// Tracks whether initial auto-fit of columns has been performed.
    /// </summary>
    private bool _initialAutoFitDone = false;

    /// <summary>
    /// Identifies the <see cref="SelectedItem"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem),
        typeof(object),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    /// <summary>
    /// Identifies the <see cref="SelectedItems"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
        nameof(SelectedItems),
        typeof(IList),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Identifies the <see cref="SelectionMode"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(
        nameof(SelectionMode),
        typeof(GridSelectionMode),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(GridSelectionMode.Extended));

    /// <summary>
    /// Identifies the <see cref="CanUserSort"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CanUserSortProperty = DependencyProperty.Register(
        nameof(CanUserSort),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true, OnCanUserSortChanged));

    /// <summary>
    /// Identifies the <see cref="CanUserFilter"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CanUserFilterProperty = DependencyProperty.Register(
        nameof(CanUserFilter),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true, OnCanUserFilterChanged));

    /// <summary>
    /// Identifies the <see cref="CanUserResizeColumns"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CanUserResizeColumnsProperty = DependencyProperty.Register(
        nameof(CanUserResizeColumns),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Identifies the <see cref="CanUserReorderColumns"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CanUserReorderColumnsProperty = DependencyProperty.Register(
        nameof(CanUserReorderColumns),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true));

    /// <summary>
    /// Identifies the <see cref="HeaderHeight"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
        nameof(HeaderHeight),
        typeof(double),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(48.0));

    /// <summary>
    /// Identifies the <see cref="RowHeight"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register(
        nameof(RowHeight),
        typeof(double),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(32.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Identifies the <see cref="AlternatingRowBackground"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty AlternatingRowBackgroundProperty = DependencyProperty.Register(
        nameof(AlternatingRowBackground),
        typeof(Brush),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Identifies the <see cref="GroupDescriptions"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty GroupDescriptionsProperty = DependencyProperty.Register(
        nameof(GroupDescriptions),
        typeof(ObservableCollection<GroupDescription>),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null, OnGroupDescriptionsChanged));

    /// <summary>
    /// Identifies the IsGridFocused attached dependency property.
    /// </summary>
    public static readonly DependencyProperty IsGridFocusedProperty = DependencyProperty.RegisterAttached(
        "IsGridFocused",
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// Identifies the <see cref="RowHeightBinding"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty RowHeightBindingProperty = DependencyProperty.Register(
        nameof(RowHeightBinding),
        typeof(string),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Identifies the <see cref="FrozenColumnCount"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty FrozenColumnCountProperty = DependencyProperty.Register(
        nameof(FrozenColumnCount),
        typeof(int),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(0));

    /// <summary>
    /// Identifies the <see cref="SelectionState"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SelectionStateProperty = DependencyProperty.Register(
        nameof(SelectionState),
        typeof(bool?),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(false, OnSelectionStateChanged));

    /// <summary>
    /// Identifies the <see cref="DoubleFormat"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DoubleFormatProperty = DependencyProperty.Register(
        nameof(DoubleFormat),
        typeof(string),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null, OnDoubleFormatChanged));

    /// <summary>
    /// Identifies the <see cref="HasVerticalGridLines"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HasVerticalGridLinesProperty = DependencyProperty.Register(
        nameof(HasVerticalGridLines),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Identifies the <see cref="HasHorizontalGridLines"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HasHorizontalGridLinesProperty = DependencyProperty.Register(
        nameof(HasHorizontalGridLines),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Identifies the <see cref="AlternatingRows"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty AlternatingRowsProperty = DependencyProperty.Register(
        nameof(AlternatingRows),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    private static void OnCanUserFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdvancedGrid grid)
        {
            grid.RefreshView();
        }
    }

    private static void OnCanUserSortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdvancedGrid grid)
        {
            grid.RefreshView();
        }
    }

    /// <summary>
    /// Identifies the <see cref="AggregateFunctions"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty AggregateFunctionsProperty = DependencyProperty.Register(
        nameof(AggregateFunctions),
        typeof(ObservableCollection<AggregateFunction>),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null, OnAggregateFunctionsChanged));

    /// <summary>
    /// Identifies the <see cref="SummaryResults"/> dependency property.
    /// </summary>
    private static readonly DependencyPropertyKey SummaryResultsPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(SummaryResults),
        typeof(ObservableCollection<AggregateResult>),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SummaryResultsProperty = SummaryResultsPropertyKey.DependencyProperty;

    /// <summary>
    /// Identifies the <see cref="ShowSummaryRow"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ShowSummaryRowProperty = DependencyProperty.Register(
        nameof(ShowSummaryRow),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>
    /// Identifies the <see cref="SummaryAlignment"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SummaryAlignmentProperty = DependencyProperty.Register(
        nameof(SummaryAlignment),
        typeof(HorizontalAlignment),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>
    /// Identifies the <see cref="CanUserExport"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CanUserExportProperty = DependencyProperty.Register(
        nameof(CanUserExport),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true));

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the collection of items to display in the grid.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of columns defined for this grid.
    /// </summary>
    public GridColumnCollection Columns
    {
        get => (GridColumnCollection)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets the currently selected item.
    /// </summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Gets the collection of currently selected items.
    /// </summary>
    public IList? SelectedItems
    {
        get => (IList?)GetValue(SelectedItemsProperty);
        private set => SetValue(SelectedItemsProperty, value);
    }

    /// <summary>
    /// Gets or sets the selection mode for the grid.
    /// </summary>
    public GridSelectionMode SelectionMode
    {
        get => (GridSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of aggregation functions to perform.
    /// </summary>
    public ObservableCollection<AggregateFunction> AggregateFunctions
    {
        get => (ObservableCollection<AggregateFunction>)GetValue(AggregateFunctionsProperty);
        set => SetValue(AggregateFunctionsProperty, value);
    }

    /// <summary>
    /// Gets the calculated summary results.
    /// </summary>
    public ObservableCollection<AggregateResult> SummaryResults
    {
        get => (ObservableCollection<AggregateResult>)GetValue(SummaryResultsProperty);
        private set => SetValue(SummaryResultsPropertyKey, value);
    }

    /// <summary>
    /// Gets or sets whether to display the summary row at the bottom of the grid.
    /// </summary>
    public bool ShowSummaryRow
    {
        get => (bool)GetValue(ShowSummaryRowProperty);
        set => SetValue(ShowSummaryRowProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment for summary row values.
    /// </summary>
    public HorizontalAlignment SummaryAlignment
    {
        get => (HorizontalAlignment)GetValue(SummaryAlignmentProperty);
        set => SetValue(SummaryAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets whether users can sort columns by clicking headers.
    /// </summary>
    public bool CanUserSort
    {
        get => (bool)GetValue(CanUserSortProperty);
        set => SetValue(CanUserSortProperty, value);
    }

    /// <summary>
    /// Gets or sets whether users can filter columns.
    /// </summary>
    public bool CanUserFilter
    {
        get => (bool)GetValue(CanUserFilterProperty);
        set => SetValue(CanUserFilterProperty, value);
    }

    /// <summary>
    /// Gets or sets whether users can resize columns by dragging.
    /// </summary>
    public bool CanUserResizeColumns
    {
        get => (bool)GetValue(CanUserResizeColumnsProperty);
        set => SetValue(CanUserResizeColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets whether users can reorder columns by dragging.
    /// </summary>
    public bool CanUserReorderColumns
    {
        get => (bool)GetValue(CanUserReorderColumnsProperty);
        set => SetValue(CanUserReorderColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of each data row in pixels.
    /// </summary>
    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the header row in pixels.
    /// </summary>
    public double HeaderHeight
    {
        get => (double)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush used for alternating row backgrounds.
    /// </summary>
    public Brush? AlternatingRowBackground
    {
        get => (Brush?)GetValue(AlternatingRowBackgroundProperty);
        set => SetValue(AlternatingRowBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the user can export data.
    /// </summary>
    public bool CanUserExport
    {
        get => (bool)GetValue(CanUserExportProperty);
        set => SetValue(CanUserExportProperty, value);
    }

    /// <summary>
    /// Gets or sets the collection of group descriptions for hierarchical grouping.
    /// </summary>
    public ObservableCollection<GroupDescription>? GroupDescriptions
    {
        get => (ObservableCollection<GroupDescription>?)GetValue(GroupDescriptionsProperty);
        set => SetValue(GroupDescriptionsProperty, value);
    }

    /// <summary>
    /// Gets or sets the property name to bind row heights to for variable row heights.
    /// </summary>
    public string? RowHeightBinding
    {
        get => (string?)GetValue(RowHeightBindingProperty);
        set => SetValue(RowHeightBindingProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of columns frozen on the left side during horizontal scrolling.
    /// </summary>
    public int FrozenColumnCount
    {
        get => (int)GetValue(FrozenColumnCountProperty);
        set => SetValue(FrozenColumnCountProperty, value);
    }

    /// <summary>
    /// Gets or sets the format string for double values in the grid.
    /// </summary>
    public string? DoubleFormat
    {
        get => (string?)GetValue(DoubleFormatProperty);
        set => SetValue(DoubleFormatProperty, value);
    }

    /// <summary>
    /// Gets or sets whether vertical grid lines are displayed.
    /// </summary>
    public bool HasVerticalGridLines
    {
        get => (bool)GetValue(HasVerticalGridLinesProperty);
        set => SetValue(HasVerticalGridLinesProperty, value);
    }

    /// <summary>
    /// Gets or sets whether horizontal grid lines are displayed.
    /// </summary>
    public bool HasHorizontalGridLines
    {
        get => (bool)GetValue(HasHorizontalGridLinesProperty);
        set => SetValue(HasHorizontalGridLinesProperty, value);
    }

    /// <summary>
    /// Identifies the IsSearchPanelVisible dependency property.
    /// </summary>
    public static readonly DependencyProperty IsSearchPanelVisibleProperty = DependencyProperty.Register(
        nameof(IsSearchPanelVisible),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(false));

    public bool IsSearchPanelVisible
    {
        get => (bool)GetValue(IsSearchPanelVisibleProperty);
        set => SetValue(IsSearchPanelVisibleProperty, value);
    }

    /// <summary>
    /// Identifies the SearchText dependency property.
    /// </summary>
    public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
        nameof(SearchText),
        typeof(string),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(string.Empty, OnSearchTextChanged));

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <summary>
    /// Identifies the IsAutoSearchEnabled dependency property.
    /// </summary>
    public static readonly DependencyProperty IsAutoSearchEnabledProperty = DependencyProperty.Register(
        nameof(IsAutoSearchEnabled),
        typeof(bool),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(true));

    public bool IsAutoSearchEnabled
    {
        get => (bool)GetValue(IsAutoSearchEnabledProperty);
        set => SetValue(IsAutoSearchEnabledProperty, value);
    }

    /// <summary>
    /// Identifies the SearchDelay dependency property.
    /// </summary>
    public static readonly DependencyProperty SearchDelayProperty = DependencyProperty.Register(
        nameof(SearchDelay),
        typeof(TimeSpan),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(TimeSpan.FromMilliseconds(500)));

    public TimeSpan SearchDelay
    {
        get => (TimeSpan)GetValue(SearchDelayProperty);
        set => SetValue(SearchDelayProperty, value);
    }

    /// <summary>
    /// Identifies the SearchCountMode dependency property.
    /// </summary>
    public static readonly DependencyProperty SearchCountModeProperty = DependencyProperty.Register(
        nameof(SearchCountMode),
        typeof(SearchCountMode),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(SearchCountMode.Rows, OnSearchCountModeChanged));

    public SearchCountMode SearchCountMode
    {
        get => (SearchCountMode)GetValue(SearchCountModeProperty);
        set => SetValue(SearchCountModeProperty, value);
    }

    /// <summary>
    /// Identifies the SearchMatchCount dependency property (Read-Only).
    /// </summary>
    public static readonly DependencyPropertyKey SearchMatchCountPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(SearchMatchCount),
        typeof(int),
        typeof(AdvancedGrid),
        new FrameworkPropertyMetadata(0));

    public static readonly DependencyProperty SearchMatchCountProperty = SearchMatchCountPropertyKey.DependencyProperty;

    public int SearchMatchCount
    {
        get => (int)GetValue(SearchMatchCountProperty);
        private set => SetValue(SearchMatchCountPropertyKey, value);
    }


    /// <summary>
    /// Gets or sets whether alternating row styling is enabled.
    /// </summary>
    public bool AlternatingRows
    {
        get => (bool)GetValue(AlternatingRowsProperty);
        set => SetValue(AlternatingRowsProperty, value);
    }

    /// <summary>
    /// Gets or sets the selection state: true (all selected), false (none), null (some selected).
    /// </summary>
    public bool? SelectionState
    {
        get => (bool?)GetValue(SelectionStateProperty);
        set => SetValue(SelectionStateProperty, value);
    }

    private static void OnDoubleFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdvancedGrid grid)
        {
            grid.RefreshView();
        }
    }

    /// <summary>
    /// Gets the IsGridFocused attached property value.
    /// </summary>
    /// <param name="obj">The dependency object to query.</param>
    /// <returns>True if the grid is focused; otherwise, false.</returns>
    public static bool GetIsGridFocused(DependencyObject obj) => (bool)obj.GetValue(IsGridFocusedProperty);

    /// <summary>
    /// Sets the IsGridFocused attached property value.
    /// </summary>
    /// <param name="obj">The dependency object to set.</param>
    /// <param name="value">The value to set.</param>
    public static void SetIsGridFocused(DependencyObject obj, bool value) => obj.SetValue(IsGridFocusedProperty, value);

    #endregion

    #region Internal Properties

    /// <summary>
    /// Automatically adjusts the width of the specified column to fit its content.
    /// </summary>
    /// <param name="column">The column to auto-fit.</param>
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

    /// <summary>
    /// Automatically adjusts the width of all columns to fit their content.
    /// </summary>
    public void AutoFitAllColumns()
    {
        if (Columns == null) return;
        foreach (var column in Columns)
        {
            AutoFitColumn(column);
        }
    }
    
    /// <summary>
    /// Gets the sort manager responsible for column sorting operations.
    /// </summary>
    public SortManager SortManager { get; }

    /// <summary>
    /// Gets the filter manager responsible for column filtering operations.
    /// </summary>
    public FilterManager FilterManager { get; }

    /// <summary>
    /// Gets the group manager responsible for row grouping operations.
    /// </summary>
    public GroupManager GroupManager { get; }

    /// <summary>
    /// Gets the collection view wrapping the ItemsSource for sorting, filtering, and grouping.
    /// </summary>
    internal ICollectionView? CollectionView { get; private set; }

    /// <summary>
    /// Gets the collection of available export providers.
    /// </summary>
    public ObservableCollection<IExportService> ExportProviders { get; } = new();

    /// <summary>
    /// Gets the command used to export grid data.
    /// </summary>
    public ICommand ExportCommand { get; }

    /// <summary>
    /// Gets the items host.
    /// </summary>
    private VirtualizingGridPanel? ItemsHost { get; set; }
    private GridHeaderPresenter? HeaderPresenter { get; set; }
    private GridSummaryPresenter? SummaryPresenter { get; set; }
    private ScrollViewer? ScrollViewer { get; set; }
    private ScrollViewer? SummaryScrollViewer { get; set; }
    private bool _isRefreshing;

    /// <summary>
    /// Internal collection of selected items.
    /// </summary>
    private readonly ObservableCollection<object> _selectedItems = new ObservableCollection<object>();

    /// <summary>
    /// The anchor item for shift-click range selection.
    /// </summary>
    private object? _selectionAnchor;

    private readonly System.Windows.Threading.DispatcherTimer _searchTimer;

    #endregion

    #region Search Methods

    private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdvancedGrid grid)
        {
            if (grid.IsAutoSearchEnabled)
            {
                grid._searchTimer.Interval = grid.SearchDelay;
                grid._searchTimer.Stop();
                grid._searchTimer.Start();
            }
            else
            {
                // Manual search only (Enter key or button)
                // But for now, if IsAutoSearchEnabled is false, we might wait for explicit command.
                // The implementation plan says "automatic search if available... search box will search after user pauses"
                // which implies AutoSearch. If IsAutoSearchEnabled is false, we don't trigger here.
                // We'll rely on the KeyDown handler or a Search Command.
            }
        }
    }

    private static void OnSearchCountModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdvancedGrid grid)
        {
            grid.UpdateSearchMatchCount();
        }
    }

    private void OnSearchTimerTick(object? sender, EventArgs e)
    {
        _searchTimer.Stop();
        PerformSearch();
    }

    private void OnFindExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        PerformSearch();
    }

    private void OnDeleteExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        SearchText = string.Empty;
        PerformSearch();
    }

    /// <summary>
    /// Performs the search operation using the current SearchText.
    /// </summary>
    public void PerformSearch()
    {
        _searchTimer.Stop();
        FilterManager.GlobalSearchText = SearchText;
        FilterManager.ApplyFiltering();
        UpdateAggregates();
    }

    private static void OnAggregateFunctionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdvancedGrid grid)
        {
            if (e.OldValue is ObservableCollection<AggregateFunction> oldColl)
            {
                oldColl.CollectionChanged -= grid.OnAggregateFunctionsCollectionChanged;
            }

            if (e.NewValue is ObservableCollection<AggregateFunction> newColl)
            {
                newColl.CollectionChanged += grid.OnAggregateFunctionsCollectionChanged;
            }

            grid.UpdateAggregates();
        }
    }

    private void OnAggregateFunctionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateAggregates();
    }

    /// <summary>
    /// Recalculates all defined aggregates for the current view.
    /// </summary>
    public void UpdateAggregates()
    {
        if (AggregateFunctions == null || AggregateFunctions.Count == 0 || CollectionView == null)
        {
            SummaryResults = new ObservableCollection<AggregateResult>();
            return;
        }

        var results = new ObservableCollection<AggregateResult>();
        var items = CollectionView.Cast<object>().ToList();

        foreach (var func in AggregateFunctions)
        {
            if (string.IsNullOrEmpty(func.PropertyName)) continue;

            var result = InternalCalculateAggregate(items, func);
            if (result != null)
            {
                results.Add(result);
            }
        }

        SummaryResults = results;
    }

    internal void UpdateGroupAggregates()
    {
        GroupManager?.UpdateGroupAggregates();
    }

    internal AggregateResult? InternalCalculateAggregate(List<object> items, AggregateFunction func)
    {
        if (items.Count == 0 && func.FunctionType != AggregateType.Count) return null;

        object? finalValue = null;
        
        try
        {
            switch (func.FunctionType)
            {
                case AggregateType.Count:
                    finalValue = items.Count;
                    break;

                case AggregateType.Sum:
                    finalValue = items.Sum(item => GetNumericValue(item, func.PropertyName));
                    break;

                case AggregateType.Average:
                    if (items.Count > 0)
                        finalValue = items.Average(item => GetNumericValue(item, func.PropertyName));
                    break;

                case AggregateType.Min:
                    finalValue = items.Min(item => GetComparableValue(item, func.PropertyName));
                    break;

                case AggregateType.Max:
                    finalValue = items.Max(item => GetComparableValue(item, func.PropertyName));
                    break;
            }
        }
        catch
        {
            // Silently fail for incompatible types
            return null;
        }

        if (finalValue == null) return null;

        // Default to basic ToString
        var formattedValue = finalValue.ToString() ?? string.Empty;
        
        // Apply format if StringFormat is provided
        if (!string.IsNullOrEmpty(func.StringFormat))
        {
            if (func.StringFormat.Contains("{0"))
            {
                 formattedValue = string.Format(func.StringFormat, finalValue);
            }
            else
            {
                formattedValue = string.Format("{0:" + func.StringFormat + "}", finalValue);
            }
        }
        // Otherwise, apply grid's DoubleFormat to numeric values if available
        else if (finalValue is double && !string.IsNullOrEmpty(DoubleFormat))
        {
            formattedValue = string.Format("{0:" + DoubleFormat + "}", finalValue);
        }
        
        // Add caption prefix if provided
        if (!string.IsNullOrEmpty(func.Caption) && string.IsNullOrEmpty(func.StringFormat))
        {
            formattedValue = $"{func.Caption}: {formattedValue}";
        }

        return new AggregateResult
        {
            PropertyName = func.PropertyName,
            Value = finalValue,
            FormattedValue = formattedValue
        };
    }

    private double GetNumericValue(object item, string propertyName)
    {
        var prop = item.GetType().GetProperty(propertyName);
        var val = prop?.GetValue(item);
        if (val == null) return 0;

        try
        {
            return Convert.ToDouble(val);
        }
        catch
        {
            return 0;
        }
    }

    private object? GetComparableValue(object item, string propertyName)
    {
        var prop = item.GetType().GetProperty(propertyName);
        return prop?.GetValue(item);
    }
    internal void UpdateSearchMatchCount()
    {
        if (CollectionView == null)
        {
            SearchMatchCount = 0;
            return;
        }

        // If no global search and no column filters, hide the count
        if (string.IsNullOrWhiteSpace(SearchText) && (FilterManager == null || FilterManager.Filters.Count == 0))
        {
            SearchMatchCount = 0;
            return;
        }

        if (SearchCountMode == SearchCountMode.Rows)
        {
            SearchMatchCount = CollectionView.Cast<object>().Count();
        }
        else // Cells
        {
            int cellCount = 0;
            var searchText = SearchText;
            
            // If explicit search text is empty, do we count all cells? 
            // Usually "0 found" if no search, or total cells? 
            // Let's assume matches for the search text.
            if (string.IsNullOrWhiteSpace(searchText))
            {
                SearchMatchCount = 0; 
                return;
            }

            foreach (var item in CollectionView)
            {
                if (Columns == null) continue;
                foreach (var column in Columns)
                {
                    if (string.IsNullOrEmpty(column.Binding)) continue;
                    
                    var value = column.GetCellValue(item);
                    if (value != null)
                    {
                         var stringValue = value.ToString() ?? string.Empty;
                         if (stringValue.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                         {
                             cellCount++;
                         }
                    }
                }
            }
            SearchMatchCount = cellCount;
        }
    }

    #endregion

    #region Constructor

    public AdvancedGrid()
    {
        Columns = new GridColumnCollection();
        GroupDescriptions = new ObservableCollection<GroupDescription>();
        AggregateFunctions = new ObservableCollection<AggregateFunction>();
        SelectedItems = _selectedItems;

        ExportProviders.Add(new ExcelExportService());
        ExportProviders.Add(new CsvExportService());
        ExportProviders.Add(new PdfExportService());
        ExportCommand = new RelayCommand(ExecuteExport, CanExecuteExport);

        _searchTimer = new System.Windows.Threading.DispatcherTimer();
        _searchTimer.Tick += OnSearchTimerTick;

        SortManager = new SortManager(this);
        FilterManager = new FilterManager(this);
        GroupManager = new GroupManager(this);

        _selectedItems.CollectionChanged += OnSelectedItemsChanged;

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Find, OnFindExecuted));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, OnDeleteExecuted));

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
        SummaryPresenter = GetTemplateChild("PART_SummaryPresenter") as GridSummaryPresenter;
        ScrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
        SummaryScrollViewer = GetTemplateChild("PART_SummaryScrollViewer") as ScrollViewer;

        if (HeaderPresenter != null)
        {
            HeaderPresenter.Grid = this;
        }

        if (SummaryPresenter != null)
        {
            SummaryPresenter.Grid = this;
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
        if (e.HorizontalChange != 0)
        {
            HeaderPresenter?.InvalidateArrange();
            if (SummaryScrollViewer != null)
            {
                SummaryScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
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

    /// <summary>
    /// Selects an item in the grid, optionally handling modifier keys for multi-selection.
    /// </summary>
    /// <param name="item">The item to select.</param>
    /// <param name="isControlPressed">True if the Control key is pressed (toggles selection).</param>
    /// <param name="isShiftPressed">True if the Shift key is pressed (extends selection).</param>
    public void SelectItem(object item, bool isControlPressed, bool isShiftPressed)
    {
        if (CollectionView == null) return;

        if (isShiftPressed && _selectionAnchor != null && SelectionMode != GridSelectionMode.Single)
        {
            // Range Selection
            var items = CollectionView.Cast<object>().ToList();
            var start = items.IndexOf(_selectionAnchor);
            var end = items.IndexOf(item);

            if (start >= 0 && end >= 0)
            {
                if (!isControlPressed)
                {
                    _selectedItems.Clear();
                }

                var from = Math.Min(start, end);
                var to = Math.Max(start, end);

                for (int i = from; i <= to; i++)
                {
                    if (!_selectedItems.Contains(items[i]))
                    {
                        _selectedItems.Add(items[i]);
                    }
                }
            }
        }
        else if (isControlPressed && SelectionMode != GridSelectionMode.Single)
        {
            // Toggle Selection
            if (_selectedItems.Contains(item))
            {
                _selectedItems.Remove(item);
            }
            else
            {
                _selectedItems.Add(item);
            }
            _selectionAnchor = item;
        }
        else
        {
            // Single Selection
            _selectedItems.Clear();
            _selectedItems.Add(item);
            _selectionAnchor = item;
        }

        SelectedItem = item;
        ItemsHost?.InvalidateVisual();
    }

    /// <summary>
    /// Selects an item in the grid.
    /// </summary>
    /// <param name="item">The item to select.</param>
    /// <param name="addToSelection">True to add to existing selection; False to replace selection.</param>
    public void SelectItem(object item, bool addToSelection = false)
    {
        SelectItem(item, addToSelection, false);
    }

    private bool CanExecuteExport(object? parameter) => CollectionView != null;

    private async void ExecuteExport(object? parameter)
    {
        if (parameter is not IExportService service) return;

        try
        {
            var view = CollectionView;
            if (view == null) return;

            // 1. Prepare Column Metadata
            var exportColumns = Columns.Select(c => 
            {
                string? format = null;
                if (c is DoubleColumn dc)
                {
                    if (!string.IsNullOrEmpty(dc.StringFormat)) format = dc.StringFormat;
                    else if (!string.IsNullOrEmpty(DoubleFormat)) format = DoubleFormat;
                    else format = dc.ShowThousandsSeparator ? $"N{dc.DecimalPlaces}" : $"F{dc.DecimalPlaces}";

                    if (!string.IsNullOrEmpty(format) && !format.Contains("{0"))
                        format = "{0:" + format + "}";
                }

                return new GridExportColumn
                {
                    Header = c.Header ?? string.Empty,
                    Binding = c.Binding,
                    IsSelected = c.IsVisible,
                    StringFormat = format
                };
            }).ToList();

            // 2. Show Settings Dialog
            var options = new ExportOptions { Title = "Grid Export" };
            var settingsVm = new ExportSettingsViewModel(options, exportColumns);
            var settingsWindow = new ExportSettingsWindow(settingsVm) { Owner = Window.GetWindow(this) };
            
            if (settingsWindow.ShowDialog() != true) return;

            // 3. Select File
            var dialog = new SaveFileDialog
            {
                Filter = service.FileFilter,
                FileName = $"Export_{DateTime.Now:yyyyMMdd_HHmmss}",
                DefaultExt = service.FileExtension
            };

            if (dialog.ShowDialog() != true) return;
            options.FilePath = dialog.FileName;

            // 4. Extract Data (UI Thread)
            IEnumerable dataToExport;
            if (options.VisibleRowsOnly)
            {
                dataToExport = view.Cast<object>().ToList();
            }
            else
            {
                // To export ALL data (ignoring filters), we use ItemsSource
                dataToExport = ItemsSource?.Cast<object>().ToList() ?? new List<object>();
            }

            var finalColumns = exportColumns.Where(c => c.IsSelected).ToList();
            if (finalColumns.Count == 0)
            {
                MessageBox.Show("Please select at least one column to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dataList = dataToExport.Cast<object>().ToList();
            if (dataList.Count == 0)
            {
                MessageBox.Show("No data found to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 5. Run Export with Progress
            var progressWindow = new ExportProgressWindow 
            { 
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner 
            };
            progressWindow.Show();

            try
            {
                await service.ExportAsync(dataList, finalColumns, options, progressWindow);
                // Tiny delay so user sees 100%
                await Task.Delay(200);
            }
            finally
            {
                progressWindow.Close();
            }

            // 6. Show Result
            var resultWindow = new ExportResultWindow(options.FilePath!) { Owner = Window.GetWindow(this) };
            resultWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Deselects a specific item.
    /// </summary>
    /// <param name="item">The item to deselect.</param>
    public void DeselectItem(object item)
    {
        _selectedItems.Remove(item);
        
        if (SelectedItem == item)
        {
            SelectedItem = _selectedItems.Count > 0 ? _selectedItems[0] : null;
        }

        ItemsHost?.InvalidateVisual();
    }

    /// <summary>
    /// Selects all items in the grid, if multiple selection is enabled.
    /// </summary>
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

    /// <summary>
    /// Clears all selected items.
    /// </summary>
    public void ClearSelection()
    {
        _selectedItems.Clear();
        SelectedItem = null;
        ItemsHost?.InvalidateVisual();
    }

    /// <summary>
    /// Determines whether the specified item is selected.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>True if the item is selected; otherwise, false.</returns>
    public bool IsItemSelected(object item) => _selectedItems.Contains(item);

    private bool _isUpdatingSelectionState = false;

    private void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isUpdatingSelectionState) return;

        UpdateSelectionState();

        // Sync materialized rows
        if (ItemsHost != null)
        {
            foreach (UIElement child in ItemsHost.PublicInternalChildren)
            {
                if (child is GridRow row && row.DataItem != null)
                {
                    row.IsSelected = IsItemSelected(row.DataItem);
                }
            }
        }
    }

    private void UpdateSelectionState()
    {
        if (CollectionView == null)
        {
            _isUpdatingSelectionState = true;
            SelectionState = false;
            _isUpdatingSelectionState = false;
            return;
        }

        var totalCount = CollectionView.Cast<object>().Count();
        var selectedCount = _selectedItems.Count;

        _isUpdatingSelectionState = true;
        if (selectedCount == 0)
        {
            SelectionState = false;
        }
        else if (selectedCount >= totalCount)
        {
            SelectionState = true;
        }
        else
        {
            SelectionState = null; // Indeterminate
        }
        _isUpdatingSelectionState = false;
    }

    private static void OnSelectionStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AdvancedGrid grid && !grid._isUpdatingSelectionState)
        {
            var newState = (bool?)e.NewValue;
            if (newState == true)
            {
                grid.SelectAll();
            }
            else
            {
                // Both false and null (indeterminate) should clear selection
                grid.ClearSelection();
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Refreshes the grid view, including headers and rows.
    /// </summary>
    public void RefreshView()
    {
        if (_isRefreshing) return;
        _isRefreshing = true;

        try
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
                    if (child is GridHeaderCell cell && cell.Column != null)
                    {
                        cell.CanSort = cell.Column.CanSort && CanUserSort;
                        cell.CanFilter = cell.Column.CanFilter && CanUserFilter;
                        cell.UpdateSortIndicator();
                        cell.UpdateFilterIndicator();
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

            UpdateAggregates();
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    /// <summary>
    /// Scrolls the specified item into view.
    /// </summary>
    /// <param name="item">The item to scroll to.</param>
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

    /// <summary>
    /// Begins editing the current cell.
    /// </summary>
    public void BeginEdit()
    {
        // Start editing the current cell
    }

    /// <summary>
    /// Cancels the current edit operation.
    /// </summary>
    public void CancelEdit()
    {
        // Cancel current edit
    }

    /// <summary>
    /// Commits the current edit operation.
    /// </summary>
    public void CommitEdit()
    {
        // Commit current edit
    }

    #endregion

    #region Routed Events

    /// <summary>
    /// Identifies the <see cref="SelectionChanged"/> routed event.
    /// </summary>
    public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(SelectionChanged),
        RoutingStrategy.Bubble,
        typeof(SelectionChangedEventHandler),
        typeof(AdvancedGrid));

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event SelectionChangedEventHandler SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    /// <summary>
    /// Identifies the <see cref="CellEditEnding"/> routed event.
    /// </summary>
    public static readonly RoutedEvent CellEditEndingEvent = EventManager.RegisterRoutedEvent(
        nameof(CellEditEnding),
        RoutingStrategy.Bubble,
        typeof(EventHandler<GridCellEditEndingEventArgs>),
        typeof(AdvancedGrid));

    /// <summary>
    /// Occurs when a cell edit is ending.
    /// </summary>
    public event EventHandler<GridCellEditEndingEventArgs> CellEditEnding
    {
        add => AddHandler(CellEditEndingEvent, value);
        remove => RemoveHandler(CellEditEndingEvent, value);
    }

    #endregion

    #region Keyboard Navigation

    /// <summary>
    /// Handles the <see cref="UIElement.KeyDown"/> routed event to support keyboard navigation.
    /// </summary>
    /// <param name="e">The key event arguments.</param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (CollectionView == null) return;

        var items = CollectionView.Cast<object>().ToList();
        var currentIndex = SelectedItem != null ? items.IndexOf(SelectedItem) : -1;

        var isControl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        var isShift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

        switch (e.Key)
        {
            case Key.Up:
                if (currentIndex > 0)
                {
                    SelectItem(items[currentIndex - 1], isControl, isShift);
                    ScrollIntoView(items[currentIndex - 1]);
                }
                e.Handled = true;
                break;

            case Key.Down:
                if (currentIndex < items.Count - 1)
                {
                    var newIndex = currentIndex < 0 ? 0 : currentIndex + 1;
                    SelectItem(items[newIndex], isControl, isShift);
                    ScrollIntoView(items[newIndex]);
                }
                e.Handled = true;
                break;

            case Key.Home:
                if (items.Count > 0)
                {
                    SelectItem(items[0], isControl, isShift);
                    ScrollIntoView(items[0]);
                }
                e.Handled = true;
                break;

            case Key.End:
                if (items.Count > 0)
                {
                    SelectItem(items[^1], isControl, isShift);
                    ScrollIntoView(items[^1]);
                }
                e.Handled = true;
                break;

            case Key.PageUp:
                if (ScrollViewer != null && items.Count > 0)
                {
                    var pageSize = (int)(ScrollViewer.ViewportHeight / RowHeight);
                    var newIndex = Math.Max(0, currentIndex - pageSize);
                    SelectItem(items[newIndex], isControl, isShift);
                    ScrollIntoView(items[newIndex]);
                }
                e.Handled = true;
                break;

            case Key.PageDown:
                if (ScrollViewer != null && items.Count > 0)
                {
                    var pageSize = (int)(ScrollViewer.ViewportHeight / RowHeight);
                    var newIndex = Math.Min(items.Count - 1, currentIndex + pageSize);
                    SelectItem(items[newIndex], isControl, isShift);
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

    /// <summary>
    /// Handles the <see cref="Control.MouseDoubleClick"/> routed event.
    /// </summary>
    /// <param name="e">The mouse button event arguments.</param>
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
