using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Api.Dtos;

/// <summary>
/// Request model for updating an existing habit.
/// Excludes Type (immutable) and other read-only fields.
/// </summary>
public class UpdateHabitDto
{
    /// <summary>
    /// The updated name. Optional on update (if not provided, name is unchanged).
    /// </summary>
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Habit name must be between 1 and 200 characters.")]
    public string? Name { get; set; }

    /// <summary>
    /// The updated description. Optional.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// The updated frequency. Optional.
    /// </summary>
    public FrequencyUpdateDto? Frequency { get; set; }

    /// <summary>
    /// The updated target. Optional.
    /// </summary>
    public TargetUpdateDto? Target { get; set; }

    /// <summary>
    /// The updated end date. Optional.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// The updated reminder time. Optional.
    /// </summary>
    public TimeOnly? ReminderTime { get; set; }
}

/// <summary>
/// Request model for frequency within UpdateHabitDto.
/// </summary>
public class FrequencyUpdateDto
{
    /// <summary>
    /// The frequency type: "Daily", "Weekly", "Monthly", or "Yearly".
    /// </summary>
    [RegularExpression(@"^(Daily|Weekly|Monthly|Yearly)$", ErrorMessage = "Frequency type must be 'Daily', 'Weekly', 'Monthly', or 'Yearly'.")]
    public string? Type { get; set; }

    /// <summary>
    /// How many times per period (must be positive, reasonable max of 365).
    /// </summary>
    [Range(1, 365, ErrorMessage = "Times per period must be between 1 and 365.")]
    public int? TimesPerPeriod { get; set; }
}

/// <summary>
/// Request model for target within UpdateHabitDto.
/// </summary>
public class TargetUpdateDto
{
    /// <summary>
    /// The updated numerical target value. Optional.
    /// </summary>
    public int? Value { get; set; }

    /// <summary>
    /// The updated unit of measurement. Optional.
    /// </summary>
    [StringLength(100, ErrorMessage = "Unit must not exceed 100 characters.")]
    public string? Unit { get; set; }
}
