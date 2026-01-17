using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AdvancedWPFGrid.Controls;

namespace AdvancedWPFGrid.Columns;

/// <summary>
/// A column that displays a combobox for selecting from a list of values.
/// </summary>
public class ComboBoxColumn : GridColumnBase
{
    #region Dependency Properties

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(IEnumerable),
        typeof(ComboBoxColumn),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ItemsSourceBindingProperty = DependencyProperty.Register(
        nameof(ItemsSourceBinding),
        typeof(string),
        typeof(ComboBoxColumn),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
        nameof(DisplayMemberPath),
        typeof(string),
        typeof(ComboBoxColumn),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SelectedValuePathProperty = DependencyProperty.Register(
        nameof(SelectedValuePath),
        typeof(string),
        typeof(ComboBoxColumn),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register(
        nameof(IsEditable),
        typeof(bool),
        typeof(ComboBoxColumn),
        new FrameworkPropertyMetadata(false));

    #endregion

    #region Properties

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string? ItemsSourceBinding
    {
        get => (string?)GetValue(ItemsSourceBindingProperty);
        set => SetValue(ItemsSourceBindingProperty, value);
    }

    public string? DisplayMemberPath
    {
        get => (string?)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public string? SelectedValuePath
    {
        get => (string?)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    public bool IsEditable
    {
        get => (bool)GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
    }

    #endregion

    #region Methods

    public override FrameworkElement GenerateElement(GridCell cell, object dataItem)
    {
        var value = GetCellValue(dataItem);
        var items = GetItemsSource(dataItem);

        var comboBox = new ComboBox
        {
            ItemsSource = items,
            DisplayMemberPath = DisplayMemberPath,
            SelectedValuePath = SelectedValuePath,
            IsEditable = IsEditable,
            VerticalContentAlignment = VerticalAlignment.Center,
            IsHitTestVisible = !IsReadOnly,
            Focusable = !IsReadOnly,
            Style = Application.Current.TryFindResource("FluentGridComboBoxStyle") as Style
        };

        if (!string.IsNullOrEmpty(SelectedValuePath))
        {
            comboBox.SelectedValue = value;
        }
        else
        {
            comboBox.SelectedItem = value;
        }

        comboBox.SelectionChanged += (s, e) =>
        {
            if (!IsReadOnly)
            {
                OnSelectionChanged(dataItem, comboBox);
            }
        };

        return comboBox;
    }

    public override FrameworkElement GenerateEditingElement(GridCell cell, object dataItem)
    {
        // ComboBox is always interactive in display mode
        return GenerateElement(cell, dataItem);
    }

    public override void CommitCellEdit(FrameworkElement editingElement, object? dataItem)
    {
        // Commits immediately on selection change
    }

    private IEnumerable? GetItemsSource(object dataItem)
    {
        // First try column-level ItemsSource
        if (ItemsSource != null)
        {
            return ItemsSource;
        }

        // Then try binding from data item
        if (!string.IsNullOrEmpty(ItemsSourceBinding))
        {
            try
            {
                var property = dataItem.GetType().GetProperty(ItemsSourceBinding);
                return property?.GetValue(dataItem) as IEnumerable;
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private string GetDisplayText(object? value, object dataItem)
    {
        if (value == null) return string.Empty;

        // If DisplayMemberPath is set and value is an object, get the display property
        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            try
            {
                var property = value.GetType().GetProperty(DisplayMemberPath);
                return property?.GetValue(value)?.ToString() ?? value.ToString() ?? string.Empty;
            }
            catch
            {
                return value.ToString() ?? string.Empty;
            }
        }

        return value.ToString() ?? string.Empty;
    }

    private void OnSelectionChanged(object dataItem, ComboBox comboBox)
    {
        if (Binding == null) return;

        object? valueToSet;
        if (!string.IsNullOrEmpty(SelectedValuePath))
        {
            valueToSet = comboBox.SelectedValue;
        }
        else
        {
            valueToSet = comboBox.SelectedItem;
        }

        SetCellValue(dataItem, valueToSet);
    }

    #endregion
}
