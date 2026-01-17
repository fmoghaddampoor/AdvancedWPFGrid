using System.ComponentModel;
using AdvancedWPFGrid.Controls;

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
        if (_grid.CollectionView == null) return;

        using (_grid.CollectionView.DeferRefresh())
        {
            _grid.CollectionView.SortDescriptions.Clear();

            foreach (var sort in _sortDescriptors)
            {
                _grid.CollectionView.SortDescriptions.Add(
                    new SortDescription(sort.PropertyName, sort.Direction));
            }
        }

        _grid.RefreshView();
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
