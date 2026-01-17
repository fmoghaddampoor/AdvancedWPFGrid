using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdvancedWPFGrid.Controls;

namespace AdvancedWPFGrid.Columns;

/// <summary>
/// A column that displays a button.
/// </summary>
public class ButtonColumn : GridColumnBase
{
    #region Dependency Properties

    public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register(
        nameof(ButtonContent),
        typeof(object),
        typeof(ButtonColumn),
        new FrameworkPropertyMetadata("Click"));

    public static readonly DependencyProperty ButtonContentBindingProperty = DependencyProperty.Register(
        nameof(ButtonContentBinding),
        typeof(string),
        typeof(ButtonColumn),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command),
        typeof(ICommand),
        typeof(ButtonColumn),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
        nameof(CommandParameter),
        typeof(object),
        typeof(ButtonColumn),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty UseDataItemAsCommandParameterProperty = DependencyProperty.Register(
        nameof(UseDataItemAsCommandParameter),
        typeof(bool),
        typeof(ButtonColumn),
        new FrameworkPropertyMetadata(true));

    #endregion

    #region Properties

    public object ButtonContent
    {
        get => GetValue(ButtonContentProperty);
        set => SetValue(ButtonContentProperty, value);
    }

    public string? ButtonContentBinding
    {
        get => (string?)GetValue(ButtonContentBindingProperty);
        set => SetValue(ButtonContentBindingProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public bool UseDataItemAsCommandParameter
    {
        get => (bool)GetValue(UseDataItemAsCommandParameterProperty);
        set => SetValue(UseDataItemAsCommandParameterProperty, value);
    }

    /// <summary>
    /// Event raised when the button is clicked.
    /// </summary>
    public event EventHandler<ButtonClickEventArgs>? ButtonClick;

    #endregion

    #region Constructor

    public ButtonColumn()
    {
        IsReadOnly = true;
        CanSort = false;
        CanFilter = false;
        HorizontalAlignment = HorizontalAlignment.Center;
    }

    #endregion

    #region Methods

    public override FrameworkElement GenerateElement(GridCell cell, object dataItem)
    {
        var content = GetButtonContent(dataItem);

        var button = new Button
        {
            Content = content,
            Command = Command,
            CommandParameter = GetCommandParameter(dataItem),
            HorizontalAlignment = HorizontalAlignment,
            VerticalAlignment = VerticalAlignment.Center,
            Style = Application.Current.TryFindResource("FluentGridButtonStyle") as Style
        };

        button.Click += (s, e) =>
        {
            OnButtonClick(dataItem);
            e.Handled = true;
        };

        return button;
    }

    public override FrameworkElement GenerateEditingElement(GridCell cell, object dataItem)
    {
        // Button columns don't have an editing mode
        return GenerateElement(cell, dataItem);
    }

    public override void CommitCellEdit(FrameworkElement editingElement, object? dataItem)
    {
        // Button columns don't commit edits
    }

    private object GetButtonContent(object dataItem)
    {
        if (!string.IsNullOrEmpty(ButtonContentBinding))
        {
            try
            {
                var property = dataItem.GetType().GetProperty(ButtonContentBinding);
                return property?.GetValue(dataItem) ?? ButtonContent;
            }
            catch
            {
                return ButtonContent;
            }
        }

        return ButtonContent;
    }

    private object? GetCommandParameter(object dataItem)
    {
        if (UseDataItemAsCommandParameter)
        {
            return dataItem;
        }

        return CommandParameter;
    }

    protected virtual void OnButtonClick(object dataItem)
    {
        ButtonClick?.Invoke(this, new ButtonClickEventArgs(dataItem));
    }

    #endregion
}

/// <summary>
/// Event arguments for button click events.
/// </summary>
public class ButtonClickEventArgs : EventArgs
{
    public object DataItem { get; }

    public ButtonClickEventArgs(object dataItem)
    {
        DataItem = dataItem;
    }
}
