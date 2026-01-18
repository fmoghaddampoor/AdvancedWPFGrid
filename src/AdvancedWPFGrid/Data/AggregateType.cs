namespace AdvancedWPFGrid.Data;

/// <summary>
/// Specifies the type of aggregation function to perform.
/// </summary>
public enum AggregateType
{
    /// <summary>
    /// Calculates the sum of values.
    /// </summary>
    Sum,

    /// <summary>
    /// Calculates the average of values.
    /// </summary>
    Average,

    /// <summary>
    /// Calculates the count of items.
    /// </summary>
    Count,

    /// <summary>
    /// Finds the minimum value.
    /// </summary>
    Min,

    /// <summary>
    /// Finds the maximum value.
    /// </summary>
    Max
}
