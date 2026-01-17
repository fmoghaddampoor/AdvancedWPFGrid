using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AdvancedWPFGrid.Controls;

namespace AdvancedWPFGrid.Columns;

/// <summary>
/// A column that displays text with an optional button beside it.
/// </summary>
public class TextButtonColumn : GridColumnBase
{
    #region Dependency Properties

    public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register(
        nameof(ButtonContent),
        typeof(object),
        typeof(TextButtonColumn),
        new FrameworkPropertyMetadata("..."));

    public static readonly DependencyProperty ButtonWidthProperty = DependencyProperty.Register(
        nameof(ButtonWidth),
        typeof(double),
        typeof(TextButtonColumn),
        new FrameworkPropertyMetadata(28.0));

    public static readonly DependencyProperty ButtonPositionProperty = DependencyProperty.Register(
        nameof(ButtonPosition),
        typeof(ButtonPosition),
        typeof(TextButtonColumn),
        new FrameworkPropertyMetadata(ButtonPosition.Right));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command),
        typeof(ICommand),
        typeof(TextButtonColumn),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ShowButtonOnHoverOnlyProperty = DependencyProperty.Register(
        nameof(ShowButtonOnHoverOnly),
        typeof(bool),
        typeof(TextButtonColumn),
        new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
        nameof(TextTrimming),
        typeof(TextTrimming),
        typeof(TextButtonColumn),
        new FrameworkPropertyMetadata(TextTrimming.CharacterEllipsis));

    #endregion

    #region Properties

    public object ButtonContent
    {
        get => GetValue(ButtonContentProperty);
        set => SetValue(ButtonContentProperty, value);
    }

    public double ButtonWidth
    {
        get => (double)GetValue(ButtonWidthProperty);
        set => SetValue(ButtonWidthProperty, value);
    }

    public ButtonPosition ButtonPosition
    {
        get => (ButtonPosition)GetValue(ButtonPositionProperty);
        set => SetValue(ButtonPositionProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public bool ShowButtonOnHoverOnly
    {
        get => (bool)GetValue(ShowButtonOnHoverOnlyProperty);
        set => SetValue(ShowButtonOnHoverOnlyProperty, value);
    }

    public TextTrimming TextTrimming
    {
        get => (TextTrimming)GetValue(TextTrimmingProperty);
        set => SetValue(TextTrimmingProperty, value);
    }

    /// <summary>
    /// Event raised when the button is clicked.
    /// </summary>
    public event EventHandler<ButtonClickEventArgs>? ButtonClick;

    #endregion

    #region Methods

    public override FrameworkElement GenerateElement(GridCell cell, object dataItem)
    {
        var value = GetCellValue(dataItem)?.ToString() ?? string.Empty;

        var grid = new Grid();

        if (ButtonPosition == ButtonPosition.Left)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ButtonWidth) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }
        else
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ButtonWidth) });
        }

        var textBlock = new TextBlock
        {
            Text = value,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming,
            Foreground = Application.Current.TryFindResource("FluentTextPrimaryBrush") as Brush,
            Margin = new Thickness(4, 0, 4, 0)
        };

        var button = new Button
        {
            Content = ButtonContent,
            Command = Command,
            CommandParameter = dataItem,
            Width = ButtonWidth - 4,
            Height = 24,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Style = Application.Current.TryFindResource("FluentGridButtonStyle") as Style,
            Visibility = ShowButtonOnHoverOnly ? Visibility.Collapsed : Visibility.Visible
        };

        button.Click += (s, e) =>
        {
            ButtonClick?.Invoke(this, new ButtonClickEventArgs(dataItem));
            e.Handled = true;
        };

        if (ButtonPosition == ButtonPosition.Left)
        {
            System.Windows.Controls.Grid.SetColumn(button, 0);
            System.Windows.Controls.Grid.SetColumn(textBlock, 1);
        }
        else
        {
            System.Windows.Controls.Grid.SetColumn(textBlock, 0);
            System.Windows.Controls.Grid.SetColumn(button, 1);
        }

        grid.Children.Add(textBlock);
        grid.Children.Add(button);

        if (ShowButtonOnHoverOnly)
        {
            grid.MouseEnter += (s, e) => button.Visibility = Visibility.Visible;
            grid.MouseLeave += (s, e) => button.Visibility = Visibility.Collapsed;
        }

        return grid;
    }

    public override FrameworkElement GenerateEditingElement(GridCell cell, object dataItem)
    {
        var value = GetCellValue(dataItem)?.ToString() ?? string.Empty;

        var grid = new Grid();

        if (ButtonPosition == ButtonPosition.Left)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ButtonWidth) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }
        else
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ButtonWidth) });
        }

        var textBox = new TextBox
        {
            Text = value,
            VerticalContentAlignment = VerticalAlignment.Center,
            Style = Application.Current.TryFindResource("FluentGridTextBoxStyle") as Style,
            Margin = new Thickness(0, 0, 2, 0)
        };

        var button = new Button
        {
            Content = ButtonContent,
            Command = Command,
            CommandParameter = dataItem,
            Width = ButtonWidth - 4,
            Height = 24,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Style = Application.Current.TryFindResource("FluentGridButtonStyle") as Style
        };

        button.Click += (s, e) =>
        {
            ButtonClick?.Invoke(this, new ButtonClickEventArgs(dataItem));
            e.Handled = true;
        };

        if (ButtonPosition == ButtonPosition.Left)
        {
            System.Windows.Controls.Grid.SetColumn(button, 0);
            System.Windows.Controls.Grid.SetColumn(textBox, 1);
        }
        else
        {
            System.Windows.Controls.Grid.SetColumn(textBox, 0);
            System.Windows.Controls.Grid.SetColumn(button, 1);
        }

        grid.Children.Add(textBox);
        grid.Children.Add(button);

        return grid;
    }

    public override void CommitCellEdit(FrameworkElement editingElement, object? dataItem)
    {
        if (editingElement is Grid grid && dataItem != null)
        {
            foreach (var child in grid.Children)
            {
                if (child is TextBox textBox)
                {
                    SetCellValue(dataItem, textBox.Text);
                    break;
                }
            }
        }
    }

    #endregion
}

/// <summary>
/// Position of the button in a TextButtonColumn.
/// </summary>
public enum ButtonPosition
{
    Left,
    Right
}
