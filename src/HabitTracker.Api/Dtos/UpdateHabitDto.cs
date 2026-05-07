using System.ComponentModel.DataAnnotations;
using HabitTracker.Api.Entities;

namespace HabitTracker.Api.Dtos;

/// <summary>
/// Request model for replacing an entire habit (PUT).
/// All fields must be provided for full replacement semantics.
/// </summary>
public class UpdateHabitDto
{
    /// <summary>
    /// The habit name. Required.
    /// </summary>
    [Required(ErrorMessage = "Habit name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Habit name must be between 1 and 200 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description. Optional.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// The frequency. Required.
    /// </summary>
    [Required(ErrorMessage = "Frequency is required.")]
    public FrequencyUpdateDto? Frequency { get; set; }

    /// <summary>
    /// The target. Required.
    /// </summary>
    [Required(ErrorMessage = "Target is required.")]
    public TargetUpdateDto? Target { get; set; }

    /// <summary>
    /// The end date. Optional.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// The reminder time. Optional.
    /// </summary>
    public TimeOnly? ReminderTime { get; set; }

    /// <summary>
    /// Optional milestone target update. If provided, only Target is used.
    /// </summary>
    public UpdateMilestoneDto? Milestone { get; set; }

    /// <summary>
    /// Applies all fields from this DTO to an existing habit (full replacement).
    /// </summary>
    public void ApplyToHabit(Habit habit)
    {
        ArgumentNullException.ThrowIfNull(habit);

        habit.UpdateName(Name);
        habit.UpdateDescription(Description);

        var frequency = new Frequency
        {
            Type = Enum.Parse<FrequencyType>(Frequency!.Type),
            TimesPerPeriod = Frequency.TimesPerPeriod
        };
        habit.UpdateFrequency(frequency);

        var target = new Target
        {
            Value = Target?.Value,
            Unit = Target?.Unit
        };
        habit.UpdateTarget(target);

        if (EndDate.HasValue)
        {
            habit.UpdateEndDate(EndDate);
        }

        if (ReminderTime.HasValue)
        {
            habit.UpdateReminderTime(ReminderTime);
        }

        if (Milestone != null && Milestone.Target > 0)
        {
            habit.UpdateMilestoneTarget(Milestone.Target);
        }
    }
}

/// <summary>
/// Request model for frequency within UpdateHabitDto.
/// </summary>
public class FrequencyUpdateDto
{
    /// <summary>
    /// The frequency type: "Daily", "Weekly", "Monthly", or "Yearly". Required.
    /// </summary>
    [Required(ErrorMessage = "Frequency type is required.")]
    [RegularExpression(@"^(Daily|Weekly|Monthly|Yearly)$", ErrorMessage = "Frequency type must be 'Daily', 'Weekly', 'Monthly', or 'Yearly'.")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// How many times per period (must be positive, reasonable max of 365). Required.
    /// </summary>
    [Required(ErrorMessage = "Times per period is required.")]
    [Range(1, 365, ErrorMessage = "Times per period must be between 1 and 365.")]
    public int TimesPerPeriod { get; set; }
}

/// <summary>
/// Request model for target within UpdateHabitDto.
/// </summary>
public class TargetUpdateDto
{
    /// <summary>
    /// The numerical target value.
    /// </summary>
    public int? Value { get; set; }

    /// <summary>
    /// The unit of measurement.
    /// </summary>
    [StringLength(100, ErrorMessage = "Unit must not exceed 100 characters.")]
    public string? Unit { get; set; }
}

/// <summary>
/// Request model for milestone within UpdateHabitDto.
/// Only Target is updatable; Current is managed by the domain.
/// </summary>
public class UpdateMilestoneDto
{
    /// <summary>
    /// The target milestone value. Required if Milestone is provided.
    /// </summary>
    [Required(ErrorMessage = "Milestone target is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Milestone target must be non-negative.")]
    public int Target { get; set; }
}
