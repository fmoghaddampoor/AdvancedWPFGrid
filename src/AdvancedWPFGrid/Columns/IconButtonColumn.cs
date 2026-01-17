using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using AdvancedWPFGrid.Controls;

namespace AdvancedWPFGrid.Columns;

/// <summary>
/// A column that displays a button with an icon (Path geometry).
/// </summary>
public class IconButtonColumn : GridColumnBase
{
    #region Dependency Properties

    public static readonly DependencyProperty IconDataProperty = DependencyProperty.Register(
        nameof(IconData),
        typeof(Geometry),
        typeof(IconButtonColumn),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IconDataBindingProperty = DependencyProperty.Register(
        nameof(IconDataBinding),
        typeof(string),
        typeof(IconButtonColumn),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
        nameof(IconSize),
        typeof(double),
        typeof(IconButtonColumn),
        new FrameworkPropertyMetadata(16.0));

    public static readonly DependencyProperty IconFillProperty = DependencyProperty.Register(
        nameof(IconFill),
        typeof(Brush),
        typeof(IconButtonColumn),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IconHoverFillProperty = DependencyProperty.Register(
        nameof(IconHoverFill),
        typeof(Brush),
        typeof(IconButtonColumn),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command),
        typeof(ICommand),
        typeof(IconButtonColumn),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ToolTipTextProperty = DependencyProperty.Register(
        nameof(ToolTipText),
        typeof(string),
        typeof(IconButtonColumn),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ToolTipTextBindingProperty = DependencyProperty.Register(
        nameof(ToolTipTextBinding),
        typeof(string),
        typeof(IconButtonColumn),
        new FrameworkPropertyMetadata(null));

    #endregion

    #region Properties

    public Geometry? IconData
    {
        get => (Geometry?)GetValue(IconDataProperty);
        set => SetValue(IconDataProperty, value);
    }

    public string? IconDataBinding
    {
        get => (string?)GetValue(IconDataBindingProperty);
        set => SetValue(IconDataBindingProperty, value);
    }

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public Brush? IconFill
    {
        get => (Brush?)GetValue(IconFillProperty);
        set => SetValue(IconFillProperty, value);
    }

    public Brush? IconHoverFill
    {
        get => (Brush?)GetValue(IconHoverFillProperty);
        set => SetValue(IconHoverFillProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public string? ToolTipText
    {
        get => (string?)GetValue(ToolTipTextProperty);
        set => SetValue(ToolTipTextProperty, value);
    }

    public string? ToolTipTextBinding
    {
        get => (string?)GetValue(ToolTipTextBindingProperty);
        set => SetValue(ToolTipTextBindingProperty, value);
    }

    /// <summary>
    /// Event raised when the button is clicked.
    /// </summary>
    public event EventHandler<ButtonClickEventArgs>? ButtonClick;

    #endregion

    #region Static Icon Geometries (Common Icons)

    public static readonly Geometry EditIcon = Geometry.Parse("M3,17.25V21h3.75L17.81,9.94l-3.75-3.75L3,17.25z M20.71,7.04c0.39-0.39,0.39-1.02,0-1.41l-2.34-2.34c-0.39-0.39-1.02-0.39-1.41,0l-1.83,1.83l3.75,3.75L20.71,7.04z");
    public static readonly Geometry DeleteIcon = Geometry.Parse("M6,19c0,1.1,0.9,2,2,2h8c1.1,0,2-0.9,2-2V7H6V19z M19,4h-3.5l-1-1h-5l-1,1H5v2h14V4z");
    public static readonly Geometry ViewIcon = Geometry.Parse("M12,4.5C7,4.5,2.73,7.61,1,12c1.73,4.39,6,7.5,11,7.5s9.27-3.11,11-7.5c-1.73-4.39-6-7.5-11-7.5z M12,17c-2.76,0-5-2.24-5-5s2.24-5,5-5s5,2.24,5,5S14.76,17,12,17z M12,9c-1.66,0-3,1.34-3,3s1.34,3,3,3s3-1.34,3-3S13.66,9,12,9z");
    public static readonly Geometry AddIcon = Geometry.Parse("M19,13h-6v6h-2v-6H5v-2h6V5h2v6h6V13z");
    public static readonly Geometry RefreshIcon = Geometry.Parse("M17.65,6.35C16.2,4.9,14.21,4,12,4c-4.42,0-7.99,3.58-7.99,8s3.57,8,7.99,8c3.73,0,6.84-2.55,7.73-6h-2.08c-0.82,2.33-3.04,4-5.65,4c-3.31,0-6-2.69-6-6s2.69-6,6-6c1.66,0,3.14,0.69,4.22,1.78L13,11h7V4L17.65,6.35z");
    public static readonly Geometry SettingsIcon = Geometry.Parse("M19.14,12.94c0.04-0.31,0.06-0.63,0.06-0.94c0-0.32-0.02-0.64-0.07-0.94l2.03-1.58c0.18-0.14,0.23-0.41,0.12-0.61l-1.92-3.32c-0.12-0.22-0.37-0.29-0.59-0.22l-2.39,0.96c-0.5-0.38-1.03-0.7-1.62-0.94L14.4,2.81c-0.04-0.24-0.24-0.41-0.48-0.41h-3.84c-0.24,0-0.43,0.17-0.47,0.41L9.25,5.35C8.66,5.59,8.12,5.92,7.63,6.29L5.24,5.33c-0.22-0.08-0.47,0-0.59,0.22L2.74,8.87C2.62,9.08,2.66,9.34,2.86,9.48l2.03,1.58C4.84,11.36,4.8,11.69,4.8,12s0.02,0.64,0.07,0.94l-2.03,1.58c-0.18,0.14-0.23,0.41-0.12,0.61l1.92,3.32c0.12,0.22,0.37,0.29,0.59,0.22l2.39-0.96c0.5,0.38,1.03,0.7,1.62,0.94l0.36,2.54c0.05,0.24,0.24,0.41,0.48,0.41h3.84c0.24,0,0.44-0.17,0.47-0.41l0.36-2.54c0.59-0.24,1.13-0.56,1.62-0.94l2.39,0.96c0.22,0.08,0.47,0,0.59-0.22l1.92-3.32c0.12-0.22,0.07-0.47-0.12-0.61L19.14,12.94z M12,15.6c-1.98,0-3.6-1.62-3.6-3.6s1.62-3.6,3.6-3.6s3.6,1.62,3.6,3.6S13.98,15.6,12,15.6z");

    #endregion

    #region Constructor

    public IconButtonColumn()
    {
        IsReadOnly = true;
        CanSort = false;
        CanFilter = false;
        HorizontalAlignment = HorizontalAlignment.Center;
        Width = 40;
    }

    #endregion

    #region Methods

    public override FrameworkElement GenerateElement(GridCell cell, object dataItem)
    {
        var iconData = GetIconData(dataItem);
        var toolTip = GetToolTipText(dataItem);

        var defaultFill = IconFill ?? Application.Current.TryFindResource("FluentTextSecondaryBrush") as Brush;
        var hoverFill = IconHoverFill ?? Application.Current.TryFindResource("FluentAccentBrush") as Brush;

        var path = new Path
        {
            Data = iconData,
            Width = IconSize,
            Height = IconSize,
            Stretch = Stretch.Uniform,
            Fill = defaultFill
        };

        var button = new Button
        {
            Content = path,
            Command = Command,
            CommandParameter = dataItem,
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            Width = IconSize + 12,
            Height = IconSize + 12,
            HorizontalAlignment = HorizontalAlignment,
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = Cursors.Hand,
            Padding = new Thickness(4),
            Style = null, // Use minimal style
            ToolTip = string.IsNullOrEmpty(toolTip) ? null : toolTip
        };

        // Apply hover effect
        button.MouseEnter += (s, e) => path.Fill = hoverFill;
        button.MouseLeave += (s, e) => path.Fill = defaultFill;

        button.Click += (s, e) =>
        {
            ButtonClick?.Invoke(this, new ButtonClickEventArgs(dataItem));
            e.Handled = true;
        };

        // Create a simple template for the button
        var factory = new FrameworkElementFactory(typeof(Border));
        factory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
        factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
        factory.AppendChild(new FrameworkElementFactory(typeof(ContentPresenter))
        {
            Name = "contentPresenter"
        });

        var template = new ControlTemplate(typeof(Button))
        {
            VisualTree = factory
        };

        button.Template = template;

        return button;
    }

    public override FrameworkElement GenerateEditingElement(GridCell cell, object dataItem)
    {
        // Icon button columns don't have an editing mode
        return GenerateElement(cell, dataItem);
    }

    public override void CommitCellEdit(FrameworkElement editingElement, object? dataItem)
    {
        // Icon button columns don't commit edits
    }

    private Geometry? GetIconData(object dataItem)
    {
        // First try binding
        if (!string.IsNullOrEmpty(IconDataBinding))
        {
            try
            {
                var property = dataItem.GetType().GetProperty(IconDataBinding);
                var value = property?.GetValue(dataItem);
                
                if (value is Geometry geometry)
                {
                    return geometry;
                }
                else if (value is string pathData)
                {
                    return Geometry.Parse(pathData);
                }
            }
            catch
            {
                // Fall through to default
            }
        }

        return IconData;
    }

    private string? GetToolTipText(object dataItem)
    {
        if (!string.IsNullOrEmpty(ToolTipTextBinding))
        {
            try
            {
                var property = dataItem.GetType().GetProperty(ToolTipTextBinding);
                return property?.GetValue(dataItem)?.ToString();
            }
            catch
            {
                // Fall through to default
            }
        }

        return ToolTipText;
    }

    #endregion
}
