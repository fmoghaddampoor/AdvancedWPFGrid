using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using AdvancedWPFGrid.Controls;
using AdvancedWPFGrid.Data;

namespace AdvancedWPFGrid.Managers;

/// <summary>
/// Manages sorting for the grid.
/// </summary>
public class SortManager
{
    private readonly AdvancedGrid _grid;
    private readonly List<SortDescriptor> _sortDescriptors = [];

    public SortManager(AdvancedGrid grid)
    {
        _grid = grid;
    }

    /// <summary>
    /// Gets the current sort descriptors.
    /// </summary>
    public IReadOnlyList<SortDescriptor> SortDescriptors => _sortDescriptors.AsReadOnly();

    /// <summary>
    /// Toggles sorting for the specified property.
    /// </summary>
    public void ToggleSort(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName)) return;

        var existingSort = _sortDescriptors.FirstOrDefault(s => s.PropertyName == propertyName);

        if (existingSort != null)
        {
            if (existingSort.Direction == ListSortDirection.Ascending)
            {
                existingSort.Direction = ListSortDirection.Descending;
            }
            else
            {
                // Ctrl+Click to add to multi-sort, regular click to clear and set single sort
                if (!System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control))
                {
                    _sortDescriptors.Remove(existingSort);
                }
                else
                {
                    existingSort.Direction = ListSortDirection.Ascending;
                }
            }
        }
        else
        {
            if (!System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control))
            {
                _sortDescriptors.Clear();
            }

            _sortDescriptors.Add(new SortDescriptor
            {
                PropertyName = propertyName,
                Direction = ListSortDirection.Ascending
            });
        }

        ApplySorting();
    }

    /// <summary>
    /// Sets an explicit sort for the specified property.
    /// </summary>
    public void SetSort(string propertyName, ListSortDirection direction)
    {
        var existingSort = _sortDescriptors.FirstOrDefault(s => s.PropertyName == propertyName);

        if (existingSort != null)
        {
            existingSort.Direction = direction;
        }
        else
        {
            _sortDescriptors.Add(new SortDescriptor
            {
                PropertyName = propertyName,
                Direction = direction
            });
        }

        ApplySorting();
    }

    /// <summary>
    /// Clears the sort for the specified property.
    /// </summary>
    public void ClearSort(string propertyName)
    {
        _sortDescriptors.RemoveAll(s => s.PropertyName == propertyName);
        ApplySorting();
    }

    /// <summary>
    /// Clears all sorting.
    /// </summary>
    public void ClearAllSorts()
    {
        _sortDescriptors.Clear();
        ApplySorting();
    }

    /// <summary>
    /// Gets the sort direction for the specified property.
    /// </summary>
    public ListSortDirection? GetSortDirection(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName)) return null;
        
        var sort = _sortDescriptors.FirstOrDefault(s => s.PropertyName == propertyName);
        return sort?.Direction;
    }

    /// <summary>
    /// Applies the current sort descriptors to the collection view.
    /// </summary>
    public void ApplySorting()
    {
        if (_grid.CollectionView is ListCollectionView lcv)
        {
            if (_sortDescriptors.Count == 0)
            {
                lcv.CustomSort = null;
                lcv.SortDescriptions.Clear();
            }
            else
            {
                // For simplicity, we'll use CustomSort if any string columns are involved, 
                // but standard SortDescriptions are better for simple types.
                // However, CustomSort overrides SortDescriptions.
                
                var firstSort = _sortDescriptors[0];
                var firstItem = lcv.Cast<object>().FirstOrDefault();
                Type? propertyType = null;
                
                if (firstItem != null)
                {
                    propertyType = firstItem.GetType().GetProperty(firstSort.PropertyName)?.PropertyType;
                }
                
                if (propertyType == typeof(string))
                {
                    lcv.CustomSort = new PropertyNaturalComparer(firstSort.PropertyName, firstSort.Direction == ListSortDirection.Ascending);
                }
                else
                {
                    lcv.CustomSort = null; // Use standard sorting
                    lcv.SortDescriptions.Clear();
                    foreach (var sort in _sortDescriptors)
                    {
                        lcv.SortDescriptions.Add(new SortDescription(sort.PropertyName, sort.Direction));
                    }
                }
            }
        }
        else if (_grid.CollectionView != null)
        {
            using (_grid.CollectionView.DeferRefresh())
            {
                _grid.CollectionView.SortDescriptions.Clear();
                foreach (var sort in _sortDescriptors)
                {
                    _grid.CollectionView.SortDescriptions.Add(
                        new SortDescription(sort.PropertyName, sort.Direction));
                }
            }
        }

        _grid.RefreshView();
    }
}

internal class PropertyNaturalComparer : IComparer
{
    private readonly string _propertyName;
    private readonly bool _ascending;
    private readonly NaturalStringComparer _comparer;

    public PropertyNaturalComparer(string propertyName, bool ascending)
    {
        _propertyName = propertyName;
        _ascending = ascending;
        _comparer = new NaturalStringComparer(ascending);
    }

    public int Compare(object? x, object? y)
    {
        if (x == null || y == null) return 0;
        
        var v1 = x.GetType().GetProperty(_propertyName)?.GetValue(x);
        var v2 = y.GetType().GetProperty(_propertyName)?.GetValue(y);
        
        return _comparer.Compare(v1, v2);
    }
}

/// <summary>
/// Represents a sort descriptor.
/// </summary>
public class SortDescriptor
{
    public string PropertyName { get; set; } = string.Empty;
    public ListSortDirection Direction { get; set; } = ListSortDirection.Ascending;
}
