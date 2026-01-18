using System.ComponentModel;

namespace AdvancedWPFGrid.Data;

/// <summary>
/// Represents the result of an aggregation calculation.
/// </summary>
public class AggregateResult : INotifyPropertyChanged
{
    private string? _propertyName;
    private object? _value;
    private string? _formattedValue;

    /// <summary>
    /// Gets or sets the property name this result belongs to.
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
    /// Gets or sets the raw calculated value.
    /// </summary>
    public object? Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged(nameof(Value));
        }
    }

    /// <summary>
    /// Gets or sets the human-readable formatted value.
    /// </summary>
    public string? FormattedValue
    {
        get => _formattedValue;
        set
        {
            _formattedValue = value;
            OnPropertyChanged(nameof(FormattedValue));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
