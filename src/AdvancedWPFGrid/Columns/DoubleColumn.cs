using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AdvancedWPFGrid.Controls;

namespace AdvancedWPFGrid.Columns;

/// <summary>
/// A column that displays and edits double (numeric) values with formatting.
/// </summary>
public class DoubleColumn : GridColumnBase
{
    #region Dependency Properties

    public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(
        nameof(StringFormat),
        typeof(string),
        typeof(DoubleColumn),
        new FrameworkPropertyMetadata("N2"));

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum),
        typeof(double),
        typeof(DoubleColumn),
        new FrameworkPropertyMetadata(double.MinValue));

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum),
        typeof(double),
        typeof(DoubleColumn),
        new FrameworkPropertyMetadata(double.MaxValue));

    public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
        nameof(Increment),
        typeof(double),
        typeof(DoubleColumn),
        new FrameworkPropertyMetadata(1.0));

    public static readonly DependencyProperty ShowThousandsSeparatorProperty = DependencyProperty.Register(
        nameof(ShowThousandsSeparator),
        typeof(bool),
        typeof(DoubleColumn),
        new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register(
        nameof(DecimalPlaces),
        typeof(int),
        typeof(DoubleColumn),
        new FrameworkPropertyMetadata(2));

    #endregion

    #region Properties

    public string StringFormat
    {
        get => (string)GetValue(StringFormatProperty);
        set => SetValue(StringFormatProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Increment
    {
        get => (double)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    public bool ShowThousandsSeparator
    {
        get => (bool)GetValue(ShowThousandsSeparatorProperty);
        set => SetValue(ShowThousandsSeparatorProperty, value);
    }

    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    #endregion

    #region Constructor

    public DoubleColumn()
    {
        HorizontalAlignment = HorizontalAlignment.Right;
    }

    #endregion

    #region Methods

    public override FrameworkElement GenerateElement(GridCell cell, object dataItem)
    {
        var value = GetCellValue(dataItem);
        double numericValue = ConvertToDouble(value);

        string displayText = FormatValue(numericValue);

        var textBlock = new TextBlock
        {
            Text = displayText,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment,
            Foreground = Application.Current.TryFindResource("FluentTextPrimaryBrush") as Brush
        };

        return textBlock;
    }

    public override FrameworkElement GenerateEditingElement(GridCell cell, object dataItem)
    {
        var value = GetCellValue(dataItem);
        double numericValue = ConvertToDouble(value);

        var textBox = new TextBox
        {
            Text = numericValue.ToString(CultureInfo.CurrentCulture),
            TextAlignment = TextAlignment.Right,
            VerticalContentAlignment = VerticalAlignment.Center,
            Style = Application.Current.TryFindResource("FluentGridTextBoxStyle") as Style
        };

        // Add input validation
        textBox.PreviewTextInput += (s, e) =>
        {
            var newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            e.Handled = !IsValidNumericInput(newText);
        };

        return textBox;
    }

    public override void CommitCellEdit(FrameworkElement editingElement, object? dataItem)
    {
        if (editingElement is TextBox textBox && dataItem != null)
        {
            if (double.TryParse(textBox.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var value))
            {
                // Clamp to min/max
                value = Math.Max(Minimum, Math.Min(Maximum, value));
                SetValueToDataItem(dataItem, value);
            }
        }
    }

    private double ConvertToDouble(object? value)
    {
        return value switch
        {
            double d => d,
            float f => f,
            int i => i,
            long l => l,
            decimal dec => (double)dec,
            string s when double.TryParse(s, out var result) => result,
            _ => 0.0
        };
    }

    private string FormatValue(double value)
    {
        if (!string.IsNullOrEmpty(StringFormat))
        {
            try
            {
                return value.ToString(StringFormat, CultureInfo.CurrentCulture);
            }
            catch
            {
                // Fall through to default formatting
            }
        }

        if (ShowThousandsSeparator)
        {
            return value.ToString($"N{DecimalPlaces}", CultureInfo.CurrentCulture);
        }
        else
        {
            return value.ToString($"F{DecimalPlaces}", CultureInfo.CurrentCulture);
        }
    }

    private bool IsValidNumericInput(string text)
    {
        if (string.IsNullOrEmpty(text)) return true;
        if (text == "-" || text == "." || text == "-." || text == ",") return true;
        
        return double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out _);
    }

    private void SetValueToDataItem(object dataItem, double value)
    {
        if (Binding == null) return;

        try
        {
            var property = dataItem.GetType().GetProperty(Binding);
            if (property?.CanWrite != true) return;

            object convertedValue = property.PropertyType.Name switch
            {
                nameof(Double) => value,
                nameof(Single) => (float)value,
                nameof(Int32) => (int)Math.Round(value),
                nameof(Int64) => (long)Math.Round(value),
                nameof(Decimal) => (decimal)value,
                _ => value
            };

            property.SetValue(dataItem, convertedValue);
        }
        catch
        {
            // Handle conversion errors
        }
    }

    #endregion
}
