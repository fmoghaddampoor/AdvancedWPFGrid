using System.Windows.Data;
using AdvancedWPFGrid.Controls;
using AdvancedWPFGrid.Data;

namespace AdvancedWPFGrid.Managers;

/// <summary>
/// Manages three-level grouping for the grid.
/// </summary>
public class GroupManager
{
    private readonly AdvancedGrid _grid;
    private readonly List<GridGroupItem> _groups = [];

    public GroupManager(AdvancedGrid grid)
    {
        _grid = grid;
    }

    /// <summary>
    /// Gets the root-level groups.
    /// </summary>
    public IReadOnlyList<GridGroupItem> Groups => _groups.AsReadOnly();

    /// <summary>
    /// Applies grouping based on the grid's GroupDescriptions.
    /// </summary>
    public void ApplyGrouping()
    {
        _groups.Clear();

        if (_grid.CollectionView == null || _grid.GroupDescriptions == null || _grid.GroupDescriptions.Count == 0)
        {
            if (_grid.CollectionView != null)
            {
                _grid.CollectionView.GroupDescriptions.Clear();
            }
            _grid.RefreshView();
            return;
        }

        // Limit to 3 levels
        var descriptions = _grid.GroupDescriptions.Take(3).ToList();

        using (_grid.CollectionView.DeferRefresh())
        {
            _grid.CollectionView.GroupDescriptions.Clear();

            foreach (var groupDesc in descriptions)
            {
                if (groupDesc is PropertyGroupDescription propGroup)
                {
                    _grid.CollectionView.GroupDescriptions.Add(propGroup);
                }
            }
        }

        // Build our internal group structure for virtualization
        BuildGroupStructure();

        _grid.RefreshView();
    }

    /// <summary>
    /// Builds the internal group structure from the CollectionView groups.
    /// </summary>
    private void BuildGroupStructure()
    {
        _groups.Clear();

        if (_grid.CollectionView?.Groups == null) return;

        foreach (var group in _grid.CollectionView.Groups)
        {
            if (group is CollectionViewGroup cvGroup)
            {
                var groupItem = CreateGridGroupItem(cvGroup, 0);
                _groups.Add(groupItem);
            }
        }
    }

    private GridGroupItem CreateGridGroupItem(CollectionViewGroup cvGroup, int level)
    {
        var groupItem = new GridGroupItem
        {
            Key = cvGroup.Name,
            Level = level,
            IsExpanded = true,
            PropertyName = GetGroupPropertyName(level)
        };

        if (cvGroup.IsBottomLevel)
        {
            foreach (var item in cvGroup.Items)
            {
                groupItem.Items.Add(item);
            }
        }
        else
        {
            foreach (var subGroup in cvGroup.Items)
            {
                if (subGroup is CollectionViewGroup cvSubGroup)
                {
                    groupItem.SubGroups.Add(CreateGridGroupItem(cvSubGroup, level + 1));
                }
            }
        }

        return groupItem;
    }

    private string GetGroupPropertyName(int level)
    {
        if (_grid.GroupDescriptions == null || level >= _grid.GroupDescriptions.Count)
            return string.Empty;

        var desc = _grid.GroupDescriptions[level];
        if (desc is PropertyGroupDescription propGroup)
        {
            return propGroup.PropertyName ?? string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets a flattened list of groups for display.
    /// </summary>
    public IList<GridGroupItem> GetFlattenedGroups()
    {
        var result = new List<GridGroupItem>();
        FlattenGroups(_groups, result);
        return result;
    }

    private void FlattenGroups(IList<GridGroupItem> groups, List<GridGroupItem> result)
    {
        foreach (var group in groups)
        {
            result.Add(group);

            if (group.IsExpanded && group.SubGroups.Count > 0)
            {
                FlattenGroups(group.SubGroups, result);
            }
        }
    }

    /// <summary>
    /// Expands all groups.
    /// </summary>
    public void ExpandAllGroups()
    {
        SetExpandState(_groups, true);
        _grid.RefreshView();
    }

    /// <summary>
    /// Collapses all groups.
    /// </summary>
    public void CollapseAllGroups()
    {
        SetExpandState(_groups, false);
        _grid.RefreshView();
    }

    /// <summary>
    /// Expands groups to the specified level.
    /// </summary>
    public void ExpandToLevel(int level)
    {
        SetExpandStateToLevel(_groups, level);
        _grid.RefreshView();
    }

    private void SetExpandState(IList<GridGroupItem> groups, bool isExpanded)
    {
        foreach (var group in groups)
        {
            group.IsExpanded = isExpanded;
            if (group.SubGroups.Count > 0)
            {
                SetExpandState(group.SubGroups, isExpanded);
            }
        }
    }

    private void SetExpandStateToLevel(IList<GridGroupItem> groups, int targetLevel)
    {
        foreach (var group in groups)
        {
            group.IsExpanded = group.Level < targetLevel;
            if (group.SubGroups.Count > 0)
            {
                SetExpandStateToLevel(group.SubGroups, targetLevel);
            }
        }
    }

    /// <summary>
    /// Toggles the expanded state of a group.
    /// </summary>
    public void ToggleGroupExpanded(GridGroupItem group)
    {
        group.IsExpanded = !group.IsExpanded;
        _grid.RefreshView();
    }

    /// <summary>
    /// Clears all grouping.
    /// </summary>
    public void ClearGrouping()
    {
        _grid.GroupDescriptions?.Clear();
        _groups.Clear();
        
        if (_grid.CollectionView != null)
        {
            _grid.CollectionView.GroupDescriptions.Clear();
        }
        
        _grid.RefreshView();
    }

    /// <summary>
    /// Adds a grouping level by property name.
    /// </summary>
    public void AddGrouping(string propertyName)
    {
        if (_grid.GroupDescriptions == null) return;
        if (_grid.GroupDescriptions.Count >= 3) return; // Max 3 levels

        _grid.GroupDescriptions.Add(new PropertyGroupDescription(propertyName));
        ApplyGrouping();
    }

    /// <summary>
    /// Removes a grouping level by property name.
    /// </summary>
    public void RemoveGrouping(string propertyName)
    {
        if (_grid.GroupDescriptions == null) return;

        var toRemove = _grid.GroupDescriptions
            .OfType<PropertyGroupDescription>()
            .FirstOrDefault(g => g.PropertyName == propertyName);

        if (toRemove != null)
        {
            _grid.GroupDescriptions.Remove(toRemove);
            ApplyGrouping();
        }
    }
}
