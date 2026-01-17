using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using AdvancedWPFGrid.Data;

namespace AdvancedWPFGrid.Controls;

/// <summary>
/// A virtualizing panel that efficiently handles millions of rows by only creating
/// UI elements for visible items plus a small buffer.
/// </summary>
public class VirtualizingGridPanel : VirtualizingPanel, IScrollInfo
{
    #region Fields

    private readonly Dictionary<int, GridRow> _realizedRows = new();
    private readonly Queue<GridRow> _recycledRows = new();
    private readonly Dictionary<int, GridGroupRow> _realizedGroupRows = new();
    private readonly Queue<GridGroupRow> _recycledGroupRows = new();

    private ScrollViewer? _scrollOwner;
    private Size _extent = new(0, 0);
    private Size _viewport = new(0, 0);
    private Point _offset = new(0, 0);

    private const int BufferSize = 5; // Extra rows to realize above/below viewport

    #endregion

    #region Properties

    internal AdvancedGrid? Grid { get; set; }

    public UIElementCollection PublicInternalChildren => InternalChildren;

    private double RowHeight => Grid?.RowHeight ?? 32;
    private int TotalItemCount => GetFlattenedItemCount();

    #endregion

    #region Constructor

    public VirtualizingGridPanel()
    {
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
        CanHorizontallyScroll = true;
        CanVerticallyScroll = true;
    }

    #endregion

    #region Measure & Arrange

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Grid?.CollectionView == null)
        {
            _extent = new Size(0, 0);
            return new Size(0, 0);
        }

        // Calculate total extent
        var flatItems = GetFlattenedItems();
        var totalHeight = CalculateTotalHeight(flatItems);
        var totalWidth = CalculateTotalWidth();

        _extent = new Size(totalWidth, totalHeight);
        _viewport = availableSize;

        // Determine visible range
        var (startIndex, endIndex) = GetVisibleRange(availableSize.Height, flatItems);

        // Recycle rows that are no longer visible
        RecycleInvisibleRows(startIndex, endIndex);

        // Realize visible rows
        RealizeVisibleRows(startIndex, endIndex, flatItems, availableSize.Width);

        // Measure children
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(totalWidth, GetItemHeight(child)));
        }

        UpdateScrollInfo();

        return new Size(
            double.IsInfinity(availableSize.Width) ? totalWidth : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? totalHeight : availableSize.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Grid?.CollectionView == null) return finalSize;

        var flatItems = GetFlattenedItems();
        double y = 0;

        foreach (var item in flatItems)
        {
            UIElement? element = null;
            double itemHeight = RowHeight;

            if (item is GridGroupItem groupItem)
            {
                itemHeight = RowHeight;
                if (_realizedGroupRows.TryGetValue(flatItems.IndexOf(item), out var groupRow))
                {
                    element = groupRow;
                }
            }
            else
            {
                var index = flatItems.IndexOf(item);
                if (_realizedRows.TryGetValue(index, out var row))
                {
                    element = row;
                    itemHeight = GetDataItemHeight(item);
                }
            }

            if (element != null)
            {
                var rect = new Rect(-_offset.X, y - _offset.Y, Math.Max(finalSize.Width, _extent.Width), itemHeight);
                element.Arrange(rect);
            }

            y += itemHeight;
        }

        return finalSize;
    }

    #endregion

    #region Virtualization

    private List<object> GetFlattenedItems()
    {
        var result = new List<object>();

        if (Grid?.CollectionView == null) return result;

        var groups = Grid.GroupManager.Groups;
        
        if (groups.Count > 0)
        {
            AddGroupedItems(groups, result);
        }
        else
        {
            foreach (var item in Grid.CollectionView)
            {
                result.Add(item);
            }
        }

        return result;
    }

    private void AddGroupedItems(IReadOnlyList<GridGroupItem> groups, List<object> result)
    {
        foreach (var group in groups)
        {
            result.Add(group);

            if (group.IsExpanded)
            {
                if (group.SubGroups.Count > 0)
                {
                    AddGroupedItems(group.SubGroups, result);
                }
                else
                {
                    foreach (var item in group.Items)
                    {
                        result.Add(item);
                    }
                }
            }
        }
    }

    private int GetFlattenedItemCount()
    {
        return GetFlattenedItems().Count;
    }

    private double CalculateTotalHeight(List<object> items)
    {
        double totalHeight = 0;

        foreach (var item in items)
        {
            if (item is GridGroupItem)
            {
                totalHeight += RowHeight;
            }
            else
            {
                totalHeight += GetDataItemHeight(item);
            }
        }

        return totalHeight;
    }

    private double CalculateTotalWidth()
    {
        if (Grid?.Columns == null) return 0;
        return Grid.Columns.Sum(c => c.ActualWidth);
    }

    private double GetDataItemHeight(object item)
    {
        // Support variable row heights if RowHeightBinding is set
        if (!string.IsNullOrEmpty(Grid?.RowHeightBinding))
        {
            try
            {
                var property = item.GetType().GetProperty(Grid.RowHeightBinding);
                if (property?.GetValue(item) is double height)
                {
                    return height;
                }
            }
            catch
            {
                // Fall back to default
            }
        }

        return RowHeight;
    }

    private double GetItemHeight(UIElement element)
    {
        if (element is GridRow row && row.DataItem != null)
        {
            return GetDataItemHeight(row.DataItem);
        }
        return RowHeight;
    }

    private (int startIndex, int endIndex) GetVisibleRange(double viewportHeight, List<object> items)
    {
        double y = 0;
        int startIndex = -1;
        int endIndex = -1;

        for (int i = 0; i < items.Count; i++)
        {
            var itemHeight = items[i] is GridGroupItem ? RowHeight : GetDataItemHeight(items[i]);

            if (startIndex < 0 && y + itemHeight > _offset.Y)
            {
                startIndex = Math.Max(0, i - BufferSize);
            }

            if (y > _offset.Y + viewportHeight)
            {
                endIndex = Math.Min(items.Count - 1, i + BufferSize);
                break;
            }

            y += itemHeight;
        }

        if (startIndex < 0) startIndex = 0;
        if (endIndex < 0) endIndex = Math.Min(items.Count - 1, startIndex + (int)(viewportHeight / RowHeight) + BufferSize * 2);

        return (startIndex, endIndex);
    }

    private void RecycleInvisibleRows(int startIndex, int endIndex)
    {
        // Recycle data rows
        var keysToRemove = _realizedRows.Keys.Where(k => k < startIndex || k > endIndex).ToList();
        foreach (var key in keysToRemove)
        {
            var row = _realizedRows[key];
            InternalChildren.Remove(row);
            _recycledRows.Enqueue(row);
            _realizedRows.Remove(key);
        }

        // Recycle group rows
        var groupKeysToRemove = _realizedGroupRows.Keys.Where(k => k < startIndex || k > endIndex).ToList();
        foreach (var key in groupKeysToRemove)
        {
            var groupRow = _realizedGroupRows[key];
            InternalChildren.Remove(groupRow);
            _recycledGroupRows.Enqueue(groupRow);
            _realizedGroupRows.Remove(key);
        }
    }

    internal void ClearRealizedRows()
    {
        // Recycle all data rows
        foreach (var row in _realizedRows.Values)
        {
            InternalChildren.Remove(row);
            _recycledRows.Enqueue(row);
        }
        _realizedRows.Clear();

        // Recycle all group rows
        foreach (var row in _realizedGroupRows.Values)
        {
            InternalChildren.Remove(row);
            _recycledGroupRows.Enqueue(row);
        }
        _realizedGroupRows.Clear();
    }

    private void RealizeVisibleRows(int startIndex, int endIndex, List<object> items, double width)
    {
        for (int i = startIndex; i <= endIndex && i < items.Count; i++)
        {
            var item = items[i];

            if (item is GridGroupItem groupItem)
            {
                RealizeGroupRow(i, groupItem);
            }
            else
            {
                RealizeDataRow(i, item, startIndex);
            }
        }
    }

    private void RealizeDataRow(int index, object item, int startIndex)
    {
        if (_realizedRows.ContainsKey(index)) return;

        GridRow row;
        if (_recycledRows.Count > 0)
        {
            row = _recycledRows.Dequeue();
            row.DataItem = item;
            row.RowIndex = index;
            row.IsAlternate = index % 2 == 1;
            row.IsSelected = Grid?.IsItemSelected(item) ?? false;
            row.RowHeight = GetDataItemHeight(item);
            row.UpdateCells();
        }
        else
        {
            row = new GridRow
            {
                Grid = Grid,
                DataItem = item,
                RowIndex = index,
                IsAlternate = index % 2 == 1,
                IsSelected = Grid?.IsItemSelected(item) ?? false,
                RowHeight = GetDataItemHeight(item)
            };
        }

        _realizedRows[index] = row;
        InternalChildren.Add(row);
    }

    private void RealizeGroupRow(int index, GridGroupItem groupItem)
    {
        if (_realizedGroupRows.ContainsKey(index)) return;

        GridGroupRow groupRow;
        if (_recycledGroupRows.Count > 0)
        {
            groupRow = _recycledGroupRows.Dequeue();
            groupRow.GridGroupItem = groupItem;
        }
        else
        {
            groupRow = new GridGroupRow
            {
                Grid = Grid,
                GridGroupItem = groupItem
            };
        }

        _realizedGroupRows[index] = groupRow;
        InternalChildren.Add(groupRow);
    }

    #endregion

    #region IScrollInfo Implementation

    public bool CanVerticallyScroll { get; set; }
    public bool CanHorizontallyScroll { get; set; }

    public double ExtentWidth => _extent.Width;
    public double ExtentHeight => _extent.Height;
    public double ViewportWidth => _viewport.Width;
    public double ViewportHeight => _viewport.Height;
    public double HorizontalOffset => _offset.X;
    public double VerticalOffset => _offset.Y;

    public ScrollViewer? ScrollOwner
    {
        get => _scrollOwner;
        set => _scrollOwner = value;
    }

    public void LineUp() => SetVerticalOffset(_offset.Y - RowHeight);
    public void LineDown() => SetVerticalOffset(_offset.Y + RowHeight);
    public void LineLeft() => SetHorizontalOffset(_offset.X - 20);
    public void LineRight() => SetHorizontalOffset(_offset.X + 20);

    public void PageUp() => SetVerticalOffset(_offset.Y - _viewport.Height);
    public void PageDown() => SetVerticalOffset(_offset.Y + _viewport.Height);
    public void PageLeft() => SetHorizontalOffset(_offset.X - _viewport.Width);
    public void PageRight() => SetHorizontalOffset(_offset.X + _viewport.Width);

    public void MouseWheelUp() => SetVerticalOffset(_offset.Y - RowHeight * 3);
    public void MouseWheelDown() => SetVerticalOffset(_offset.Y + RowHeight * 3);
    public void MouseWheelLeft() => SetHorizontalOffset(_offset.X - 20);
    public void MouseWheelRight() => SetHorizontalOffset(_offset.X + 20);

    public void SetHorizontalOffset(double offset)
    {
        offset = Math.Max(0, Math.Min(offset, _extent.Width - _viewport.Width));
        if (offset != _offset.X)
        {
            _offset.X = offset;
            InvalidateMeasure();
            _scrollOwner?.InvalidateScrollInfo();
        }
    }

    public void SetVerticalOffset(double offset)
    {
        offset = Math.Max(0, Math.Min(offset, _extent.Height - _viewport.Height));
        if (offset != _offset.Y)
        {
            _offset.Y = offset;
            InvalidateMeasure();
            _scrollOwner?.InvalidateScrollInfo();
        }
    }

    public Rect MakeVisible(Visual visual, Rect rectangle)
    {
        if (visual is FrameworkElement element)
        {
            var transform = element.TransformToAncestor(this);
            var bounds = transform.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

            if (bounds.Top < 0)
            {
                SetVerticalOffset(_offset.Y + bounds.Top);
            }
            else if (bounds.Bottom > _viewport.Height)
            {
                SetVerticalOffset(_offset.Y + bounds.Bottom - _viewport.Height);
            }

            if (bounds.Left < 0)
            {
                SetHorizontalOffset(_offset.X + bounds.Left);
            }
            else if (bounds.Right > _viewport.Width)
            {
                SetHorizontalOffset(_offset.X + bounds.Right - _viewport.Width);
            }
        }

        return rectangle;
    }

    private void UpdateScrollInfo()
    {
        _scrollOwner?.InvalidateScrollInfo();
    }

    #endregion

    #region Override

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        _viewport = sizeInfo.NewSize;
        UpdateScrollInfo();
    }

    #endregion
}
