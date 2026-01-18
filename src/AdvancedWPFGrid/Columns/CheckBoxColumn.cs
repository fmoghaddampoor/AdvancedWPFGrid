using System.Windows;
using System.Windows.Controls;
using AdvancedWPFGrid.Controls;

namespace AdvancedWPFGrid.Columns;

/// <summary>
/// A column that displays a checkbox for boolean values.
/// </summary>
public class CheckBoxColumn : GridColumnBase
{
    #region Dependency Properties

    public static readonly DependencyProperty IsThreeStateProperty = DependencyProperty.Register(
        nameof(IsThreeState),
        typeof(bool),
        typeof(CheckBoxColumn),
        new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
        nameof(Content),
        typeof(object),
        typeof(CheckBoxColumn),
        new FrameworkPropertyMetadata(null));

    #endregion

    #region Properties

    public bool IsThreeState
    {
        get => (bool)GetValue(IsThreeStateProperty);
        set => SetValue(IsThreeStateProperty, value);
    }

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    #endregion

    #region Constructor

    public CheckBoxColumn()
    {

        CanSort = true;
        CanFilter = true;
        HorizontalAlignment = HorizontalAlignment.Center;
    }

    #endregion

    #region Methods

    public override FrameworkElement GenerateElement(GridCell cell, object dataItem)
    {
        var value = GetCellValue(dataItem);
        bool? isChecked = value switch
        {
            bool b => b,
            int i => i != 0,
            string s => bool.TryParse(s, out var result) && result,
            null => IsThreeState ? null : false,
            _ => false
        };

        var checkBox = new CheckBox
        {
            IsChecked = isChecked,
            IsThreeState = IsThreeState,
            Content = Content,
            HorizontalAlignment = HorizontalAlignment,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = !IsReadOnly,
            Focusable = !IsReadOnly,
            Style = Application.Current.TryFindResource("FluentGridCheckBoxStyle") as Style
        };

        if (!IsReadOnly)
        {
            checkBox.Checked += (s, e) => OnCheckChanged(dataItem, true);
            checkBox.Unchecked += (s, e) => OnCheckChanged(dataItem, false);
            checkBox.Indeterminate += (s, e) => OnCheckChanged(dataItem, null);
        }

        return checkBox;
    }

    public override FrameworkElement GenerateEditingElement(GridCell cell, object dataItem)
    {
        // Checkbox is always editable in display mode
        return GenerateElement(cell, dataItem);
    }

    public override void CommitCellEdit(FrameworkElement editingElement, object? dataItem)
    {
        // Checkbox commits immediately on click
    }

    private void OnCheckChanged(object dataItem, bool? value)
    {
        if (Binding == null) return;

        try
        {
            var property = dataItem.GetType().GetProperty(Binding);
            if (property?.CanWrite == true)
            {
                if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(dataItem, value ?? false);
                }
                else if (property.PropertyType == typeof(bool?))
                {
                    property.SetValue(dataItem, value);
                }
                else if (property.PropertyType == typeof(int))
                {
                    property.SetValue(dataItem, value == true ? 1 : 0);
                }
            }
        }
        catch
        {
            // Handle conversion errors
        }
    }

    #endregion
}
