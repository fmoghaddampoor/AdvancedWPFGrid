namespace AdvancedWPFGrid.Data;

/// <summary>
/// Represents a group of items in the grid.
/// </summary>
public class GridGroupItem
{
    /// <summary>
    /// The key value for this group.
    /// </summary>
    public object? Key { get; set; }

    /// <summary>
    /// The grouping level (0, 1, or 2 for three-level grouping).
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Whether this group is expanded.
    /// </summary>
    public bool IsExpanded { get; set; } = true;

    /// <summary>
    /// The property name used for grouping.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Child items (data items or sub-groups).
    /// </summary>
    public List<object> Items { get; set; } = [];

    /// <summary>
    /// Child sub-groups.
    /// </summary>
    public List<GridGroupItem> SubGroups { get; set; } = [];

    /// <summary>
    /// Gets the total count of data items in this group (recursive).
    /// </summary>
    public int ItemCount
    {
        get
        {
            if (SubGroups.Count > 0)
            {
                return SubGroups.Sum(g => g.ItemCount);
            }
            return Items.Count;
        }
    }

    /// <summary>
    /// Gets the display text for this group header.
    /// </summary>
    public string DisplayText => $"{PropertyName}: {Key?.ToString() ?? "(null)"}";
}

/// <summary>
/// Describes a filter operation for a column.
/// </summary>
public class FilterDescriptor
{
    /// <summary>
    /// The property name to filter on.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// The filter value.
    /// </summary>
    public string? FilterValue { get; set; }

    /// <summary>
    /// The filter operator.
    /// </summary>
    public FilterOperator Operator { get; set; } = FilterOperator.Contains;

    /// <summary>
    /// Whether the filter is case-sensitive.
    /// </summary>
    public bool IsCaseSensitive { get; set; } = false;
}

/// <summary>
/// Filter operators.
/// </summary>
public enum FilterOperator
{
    Equals,
    NotEquals,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual,
    IsNull,
    IsNotNull
}

/// <summary>
/// Describes a sort operation for a column.
/// </summary>
public class SortDescriptor
{
    /// <summary>
    /// The property name to sort on.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// The sort direction.
    /// </summary>
    public System.ComponentModel.ListSortDirection Direction { get; set; } = System.ComponentModel.ListSortDirection.Ascending;
}

/// <summary>
/// Describes a grouping operation for a column.
/// </summary>  
public class GroupDescriptor
{
    /// <summary>
    /// The property name to group by.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the group.
    /// </summary>
    public string? DisplayName { get; set; }
}
