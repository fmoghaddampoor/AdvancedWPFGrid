using System.ComponentModel;

namespace AdvancedWPFGrid.Data;

/// <summary>
/// Defines an aggregation function to be performed on a specific property.
/// </summary>
public class AggregateFunction : INotifyPropertyChanged
{
    private string? _propertyName;
    private AggregateType _functionType = AggregateType.Sum;
    private string? _caption;
    private string? _stringFormat;

    /// <summary>
    /// Gets or sets the name of the property to aggregate.
    /// </summary>
    public string? PropertyName
    {
        get => _propertyName;
        set
        {
            _propertyName = value;
            OnPropertyChanged(nameof(PropertyName));
        }
    }

    /// <summary>
    /// Gets or sets the type of aggregation to perform.
    /// </summary>
    public AggregateType FunctionType
    {
        get => _functionType;
        set
        {
            _functionType = value;
            OnPropertyChanged(nameof(FunctionType));
        }
    }

    /// <summary>
    /// Gets or sets an optional caption to display (e.g., "Total").
    /// </summary>
    public string? Caption
    {
        get => _caption;
        set
        {
            _caption = value;
            OnPropertyChanged(nameof(Caption));
        }
    }

    /// <summary>
    /// Gets or sets the string format for the result.
    /// </summary>
    public string? StringFormat
    {
        get => _stringFormat;
        set
        {
            _stringFormat = value;
            OnPropertyChanged(nameof(StringFormat));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
