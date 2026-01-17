using System.ComponentModel;
using System.Globalization;
using AdvancedWPFGrid.Controls;
using AdvancedWPFGrid.Data;

namespace AdvancedWPFGrid.Managers;

/// <summary>
/// Manages filtering for the grid.
/// </summary>
public class FilterManager
{
    private readonly AdvancedGrid _grid;
    private readonly Dictionary<string, FilterDescriptor> _filters = new();

    public FilterManager(AdvancedGrid grid)
    {
        _grid = grid;
    }

    /// <summary>
    /// Gets the current filter descriptors.
    /// </summary>
    public IReadOnlyDictionary<string, FilterDescriptor> Filters => _filters;

    /// <summary>
    /// Sets a filter for the specified property.
    /// </summary>
    public void SetFilter(string? propertyName, string filterValue, FilterOperator op = FilterOperator.Contains)
    {
        if (string.IsNullOrEmpty(propertyName)) return;

        _filters[propertyName] = new FilterDescriptor
        {
            PropertyName = propertyName,
            FilterValue = filterValue,
            Operator = op
        };

        ApplyFiltering();
    }

    /// <summary>
    /// Clears the filter for the specified property.
    /// </summary>
    public void ClearFilter(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName)) return;

        _filters.Remove(propertyName);
        ApplyFiltering();
    }

    /// <summary>
    /// Clears all filters.
    /// </summary>
    public void ClearAllFilters()
    {
        _filters.Clear();
        ApplyFiltering();
    }

    /// <summary>
    /// Checks if the specified property has an active filter.
    /// </summary>
    public bool HasFilter(string propertyName)
    {
        return _filters.ContainsKey(propertyName);
    }

    /// <summary>
    /// Applies the current filters to the collection view.
    /// </summary>
    public void ApplyFiltering()
    {
        if (_grid.CollectionView == null) return;

        if (_filters.Count == 0)
        {
            _grid.CollectionView.Filter = null;
        }
        else
        {
            _grid.CollectionView.Filter = FilterPredicate;
        }

        _grid.RefreshView();
    }

    private bool FilterPredicate(object item)
    {
        foreach (var filter in _filters.Values)
        {
            if (!MatchesFilter(item, filter))
            {
                return false;
            }
        }
        return true;
    }

    private bool MatchesFilter(object item, FilterDescriptor filter)
    {
        try
        {
            var property = item.GetType().GetProperty(filter.PropertyName);
            if (property == null) return true;

            var value = property.GetValue(item);
            var filterValue = filter.FilterValue ?? string.Empty;

            // Handle null values
            if (filter.Operator == FilterOperator.IsNull)
            {
                return value == null;
            }
            
            if (filter.Operator == FilterOperator.IsNotNull)
            {
                return value != null;
            }

            if (value == null) return false;

            var stringValue = value.ToString() ?? string.Empty;

            // String comparison
            var comparison = filter.IsCaseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            switch (filter.Operator)
            {
                case FilterOperator.Equals:
                    return stringValue.Equals(filterValue, comparison);

                case FilterOperator.NotEquals:
                    return !stringValue.Equals(filterValue, comparison);

                case FilterOperator.Contains:
                    return stringValue.Contains(filterValue, comparison);

                case FilterOperator.StartsWith:
                    return stringValue.StartsWith(filterValue, comparison);

                case FilterOperator.EndsWith:
                    return stringValue.EndsWith(filterValue, comparison);

                case FilterOperator.GreaterThan:
                    return CompareValues(value, filterValue) > 0;

                case FilterOperator.LessThan:
                    return CompareValues(value, filterValue) < 0;

                case FilterOperator.GreaterThanOrEqual:
                    return CompareValues(value, filterValue) >= 0;

                case FilterOperator.LessThanOrEqual:
                    return CompareValues(value, filterValue) <= 0;

                default:
                    return true;
            }
        }
        catch
        {
            return true;
        }
    }

    private int CompareValues(object value, string filterValue)
    {
        // Try numeric comparison first
        if (value is IComparable comparable)
        {
            if (double.TryParse(filterValue, NumberStyles.Any, CultureInfo.CurrentCulture, out var numericFilter))
            {
                if (value is double d)
                    return d.CompareTo(numericFilter);
                if (value is int i)
                    return ((double)i).CompareTo(numericFilter);
                if (value is long l)
                    return ((double)l).CompareTo(numericFilter);
                if (value is decimal dec)
                    return ((double)dec).CompareTo(numericFilter);
            }

            // Try date comparison
            if (DateTime.TryParse(filterValue, out var dateFilter))
            {
                if (value is DateTime dt)
                    return dt.CompareTo(dateFilter);
            }

            // Fall back to string comparison
            return string.Compare(value.ToString(), filterValue, StringComparison.OrdinalIgnoreCase);
        }

        return 0;
    }
}
