using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdvancedWPFGrid.Columns;

namespace AdvancedWPFGrid.Controls;

/// <summary>
/// Represents a single cell in the grid.
/// </summary>
public class GridCell : ContentControl
{
    #region Static Constructor

    static GridCell()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(GridCell),
            new FrameworkPropertyMetadata(typeof(GridCell)));

        FocusableProperty.OverrideMetadata(
            typeof(GridCell),
            new FrameworkPropertyMetadata(true));
    }

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(
        nameof(IsEditing),
        typeof(bool),
        typeof(GridCell),
        new FrameworkPropertyMetadata(false, OnIsEditingChanged));

    public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register(
        nameof(Column),
        typeof(GridColumnBase),
        typeof(GridCell),
        new FrameworkPropertyMetadata(null, OnColumnChanged));

    public static readonly DependencyProperty DataItemProperty = DependencyProperty.Register(
        nameof(DataItem),
        typeof(object),
        typeof(GridCell),
        new FrameworkPropertyMetadata(null, OnDataItemChanged));

    #endregion

    #region Properties

    public bool IsEditing
    {
        get => (bool)GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    public GridColumnBase? Column
    {
        get => (GridColumnBase?)GetValue(ColumnProperty);
        set => SetValue(ColumnProperty, value);
    }

    public object? DataItem
    {
        get => GetValue(DataItemProperty);
        set => SetValue(DataItemProperty, value);
    }

    internal AdvancedGrid? Grid { get; set; }

    private FrameworkElement? _displayElement;
    private FrameworkElement? _editingElement;

    #endregion

    #region Event Handlers

    private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridCell cell)
        {
            cell.UpdateContent();
        }
    }

    private static void OnColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridCell cell)
        {
            cell.UpdateContent();
        }
    }

    private static void OnDataItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridCell cell)
        {
            cell.UpdateContent();
        }
    }

    #endregion

    #region Methods

    private void UpdateContent()
    {
        if (Column == null || DataItem == null)
        {
            Content = null;
            return;
        }

        if (IsEditing && !Column.IsReadOnly)
        {
            _editingElement ??= Column.GenerateEditingElement(this, DataItem);
            Content = _editingElement;
        }
        else
        {
            _displayElement ??= Column.GenerateElement(this, DataItem);
            Content = _displayElement;
        }
    }

    internal void RefreshContent()
    {
        _displayElement = null;
        _editingElement = null;
        UpdateContent();
    }

    internal object? GetCellValue()
    {
        if (DataItem == null || Column?.Binding == null) return null;

        try
        {
            var property = DataItem.GetType().GetProperty(Column.Binding);
            return property?.GetValue(DataItem);
        }
        catch
        {
            return null;
        }
    }

    internal void SetCellValue(object? value)
    {
        if (DataItem == null || Column?.Binding == null) return;

        try
        {
            var property = DataItem.GetType().GetProperty(Column.Binding);
            if (property?.CanWrite == true)
            {
                var convertedValue = ConvertValue(value, property.PropertyType);
                property.SetValue(DataItem, convertedValue);

                // Notify property changed if the data item supports it
                if (DataItem is INotifyPropertyChanged)
                {
                    // The property change notification should be handled by the data item itself
                }
            }
        }
        catch
        {
            // Handle conversion errors
        }
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value == null) return null;
        if (targetType.IsAssignableFrom(value.GetType())) return value;
        
        try
        {
            if (targetType == typeof(double) && value is string strValue)
            {
                return double.Parse(strValue);
            }
            else if (targetType == typeof(int) && value is string strValueInt)
            {
                return int.Parse(strValueInt);
            }
            else if (targetType == typeof(bool) && value is string strValueBool)
            {
                return bool.Parse(strValueBool);
            }
            
            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return null;
        }
    }

    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        if (Column != null && !Column.IsReadOnly)
        {
            BeginEdit();
            e.Handled = true;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.F2 && !IsEditing && Column != null && !Column.IsReadOnly)
        {
            BeginEdit();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && IsEditing)
        {
            CancelEdit();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && IsEditing)
        {
            CommitEdit();
            e.Handled = true;
        }
    }

    public void BeginEdit()
    {
        if (Column != null && !Column.IsReadOnly)
        {
            IsEditing = true;
            
            // Focus the editing element
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_editingElement != null)
                {
                    _editingElement.Focus();
                    
                    if (_editingElement is TextBox textBox)
                    {
                        textBox.SelectAll();
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Input);
        }
    }

    public void CancelEdit()
    {
        IsEditing = false;
        _editingElement = null;
        UpdateContent();
        Focus();
    }

    public void CommitEdit()
    {
        if (Column != null && _editingElement != null)
        {
            // Get value from editing element and set to data item
            Column.CommitCellEdit(_editingElement, DataItem);

            var args = new GridCellEditEndingEventArgs(Column, DataItem!);
            Grid?.RaiseEvent(args);

            if (!args.Cancel)
            {
                IsEditing = false;
                _editingElement = null;
                _displayElement = null;
                UpdateContent();
            }
        }

        Focus();
    }

    #endregion
}
