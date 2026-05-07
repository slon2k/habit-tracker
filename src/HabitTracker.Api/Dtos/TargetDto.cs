namespace HabitTracker.Api.Dtos;

/// <summary>
/// Represents a target value and unit for measurable habits.
/// </summary>
public class TargetDto
{
    /// <summary>
    /// The numerical target value (e.g., 30, 100).
    /// </summary>
    public int? Value { get; set; }

    /// <summary>
    /// The unit of measurement (e.g., "pages", "km", "minutes").
    /// </summary>
    public string? Unit { get; set; }
}
