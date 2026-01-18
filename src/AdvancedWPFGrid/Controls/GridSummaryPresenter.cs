using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AdvancedWPFGrid.Columns;
using AdvancedWPFGrid.Data;

namespace AdvancedWPFGrid.Controls;

/// <summary>
/// Presents column summaries at the bottom of the grid.
/// </summary>
public class GridSummaryPresenter : Panel
{
    private AdvancedGrid? _grid;
    internal AdvancedGrid? Grid 
    { 
        get => _grid; 
        set
        {
            _grid = value;
            UpdateSummaries();
        }
    }

    public GridSummaryPresenter()
    {
        ClipToBounds = true;
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);
        UpdateSummaries();
    }

    internal void UpdateSummaries()
    {
        Children.Clear();

        if (Grid?.Columns == null) return;

        foreach (var column in Grid.Columns)
        {
            var summaryCell = new GridSummaryCell
            {
                Column = column,
                Grid = Grid
            };

            summaryCell.SetBinding(WidthProperty, new Binding("Width") { Source = column });
            summaryCell.SetBinding(MinWidthProperty, new Binding("MinWidth") { Source = column });
            summaryCell.SetBinding(MaxWidthProperty, new Binding("MaxWidth") { Source = column });

            // Bind to the specific result for this property if it exists
            if (!string.IsNullOrEmpty(column.Binding) && column.ShowSummary)
            {
                var binding = new Binding("SummaryResults")
                {
                    Source = Grid,
                    Converter = new SummaryResultConverter(),
                    ConverterParameter = column.Binding
                };
                summaryCell.SetBinding(GridSummaryCell.ResultProperty, binding);
            }

            Children.Add(summaryCell);
        }

        InvalidateMeasure();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double totalWidth = 0;
        var height = 28.0; // Fixed height for summary row for now

        foreach (UIElement child in Children)
        {
            child.Measure(new Size(double.PositiveInfinity, height));
            totalWidth += child.DesiredSize.Width;
        }

        double width = totalWidth;
        if (!double.IsPositiveInfinity(availableSize.Width))
        {
            width = Math.Max(width, availableSize.Width);
        }

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double x = 0;
        var height = 28.0;

        foreach (UIElement child in Children)
        {
            var width = child.DesiredSize.Width;
            child.Arrange(new Rect(x, 0, width, height));
            x += width;
        }

        return new Size(Math.Max(x, finalSize.Width), height);
    }
}

/// <summary>
/// Represents a summary cell at the bottom of a column.
/// </summary>
public class GridSummaryCell : Control
{
    static GridSummaryCell()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(GridSummaryCell),
            new FrameworkPropertyMetadata(typeof(GridSummaryCell)));
    }

    public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register(
        nameof(Column),
        typeof(GridColumnBase),
        typeof(GridSummaryCell),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ResultProperty = DependencyProperty.Register(
        nameof(Result),
        typeof(AggregateResult),
        typeof(GridSummaryCell),
        new FrameworkPropertyMetadata(null));

    public GridColumnBase? Column
    {
        get => (GridColumnBase?)GetValue(ColumnProperty);
        set => SetValue(ColumnProperty, value);
    }

    public AggregateResult? Result
    {
        get => (AggregateResult?)GetValue(ResultProperty);
        set => SetValue(ResultProperty, value);
    }

    public AdvancedGrid? Grid { get; set; }
}

/// <summary>
/// Converter to find a specific AggregateResult in a collection by property name.
/// </summary>
public class SummaryResultConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is System.Collections.IEnumerable results && parameter is string propertyName)
        {
            foreach (var item in results)
            {
                if (item is AggregateResult res && res.PropertyName == propertyName)
                {
                    return res;
                }
            }
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
