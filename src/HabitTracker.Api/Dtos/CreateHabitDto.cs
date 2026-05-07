using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Api.Dtos;

/// <summary>
/// Request model for creating a new habit.
/// </summary>
public class CreateHabitDto
{
    /// <summary>
    /// The name of the habit. Required, non-empty, max 200 characters.
    /// </summary>
    [Required(ErrorMessage = "Habit name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Habit name must be between 1 and 200 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the habit. Max 2000 characters.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// The habit type: "Binary" (done/not done) or "Measurable" (with target). Required.
    /// </summary>
    [Required(ErrorMessage = "Habit type is required.")]
    [RegularExpression(@"^(Binary|Measurable)$", ErrorMessage = "Habit type must be 'Binary' or 'Measurable'.")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// How often the habit should be performed. Required.
    /// </summary>
    [Required(ErrorMessage = "Frequency is required.")]
    public FrequencyCreateDto? Frequency { get; set; }

    /// <summary>
    /// Optional target value and unit for measurable habits.
    /// </summary>
    public TargetCreateDto? Target { get; set; }

    /// <summary>
    /// Optional end date for the habit.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Optional time of day to remind the user about this habit.
    /// </summary>
    public TimeOnly? ReminderTime { get; set; }
}

/// <summary>
/// Request model for the frequency within CreateHabitDto.
/// </summary>
public class FrequencyCreateDto
{
    /// <summary>
    /// The frequency type: "Daily", "Weekly", "Monthly", or "Yearly". Required.
    /// </summary>
    [Required(ErrorMessage = "Frequency type is required.")]
    [RegularExpression(@"^(Daily|Weekly|Monthly|Yearly)$", ErrorMessage = "Frequency type must be 'Daily', 'Weekly', 'Monthly', or 'Yearly'.")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// How many times per period (must be positive, reasonable max of 365).
    /// </summary>
    [Required(ErrorMessage = "Times per period is required.")]
    [Range(1, 365, ErrorMessage = "Times per period must be between 1 and 365.")]
    public int TimesPerPeriod { get; set; }
}

/// <summary>
/// Request model for the target within CreateHabitDto.
/// </summary>
public class TargetCreateDto
{
    /// <summary>
    /// The numerical target value.
    /// </summary>
    public int? Value { get; set; }

    /// <summary>
    /// The unit of measurement. Max 100 characters.
    /// </summary>
    [StringLength(100, ErrorMessage = "Unit must not exceed 100 characters.")]
    public string? Unit { get; set; }
}
