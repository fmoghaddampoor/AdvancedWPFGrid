using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AdvancedWPFGrid.Controls;

namespace AdvancedWPFGrid.Columns;

/// <summary>
/// A column that displays text and allows text editing.
/// </summary>
public class TextColumn : GridColumnBase
{
    #region Dependency Properties

    public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(
        nameof(MaxLength),
        typeof(int),
        typeof(TextColumn),
        new FrameworkPropertyMetadata(0));

    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
        nameof(TextWrapping),
        typeof(TextWrapping),
        typeof(TextColumn),
        new FrameworkPropertyMetadata(TextWrapping.NoWrap));

    public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
        nameof(TextTrimming),
        typeof(TextTrimming),
        typeof(TextColumn),
        new FrameworkPropertyMetadata(TextTrimming.CharacterEllipsis));

    #endregion

    #region Properties

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }

    #endregion

    #region Methods

    public override FrameworkElement GenerateElement(GridCell cell, object dataItem)
    {
        var textBlock = new TextBlock
        {
            Text = GetCellValue(dataItem)?.ToString() ?? string.Empty,
            TextWrapping = TextWrapping,
            TextTrimming = TextTrimming,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment,
            Foreground = Application.Current.TryFindResource("FluentTextPrimaryBrush") as Brush
        };

        return textBlock;
    }

    public override FrameworkElement GenerateEditingElement(GridCell cell, object dataItem)
    {
        var textBox = new TextBox
        {
            Text = GetCellValue(dataItem)?.ToString() ?? string.Empty,
            MaxLength = MaxLength > 0 ? MaxLength : int.MaxValue,
            VerticalContentAlignment = VerticalAlignment.Center,
            Style = Application.Current.TryFindResource("FluentGridTextBoxStyle") as Style
        };

        return textBox;
    }

    public override void CommitCellEdit(FrameworkElement editingElement, object? dataItem)
    {
        if (editingElement is TextBox textBox && dataItem != null)
        {
            SetCellValue(dataItem, textBox.Text);
        }
    }

    #endregion
}
