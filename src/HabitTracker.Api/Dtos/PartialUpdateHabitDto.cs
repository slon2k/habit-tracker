using System.ComponentModel.DataAnnotations;
using HabitTracker.Api.Entities;

namespace HabitTracker.Api.Dtos;

/// <summary>
/// Request model for partially updating an existing habit (PATCH).
/// All fields are optional; only provided fields are updated.
/// Excludes Type (immutable).
/// </summary>
public class PartialUpdateHabitDto
{
    /// <summary>
    /// The updated name. Optional.
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
    public FrequencyPartialUpdateDto? Frequency { get; set; }

    /// <summary>
    /// The updated target. Optional.
    /// </summary>
    public TargetPartialUpdateDto? Target { get; set; }

    /// <summary>
    /// The updated end date. Optional.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// The updated reminder time. Optional.
    /// </summary>
    public TimeOnly? ReminderTime { get; set; }

    /// <summary>
    /// Optional milestone target update. If provided, only Target is used.
    /// </summary>
    public PartialUpdateMilestoneDto? Milestone { get; set; }

    /// <summary>
    /// Applies selective fields from this DTO to an existing habit (partial update/merge).
    /// </summary>
    public void ApplyToHabit(Habit habit)
    {
        ArgumentNullException.ThrowIfNull(habit);

        if (!string.IsNullOrEmpty(Name))
        {
            habit.UpdateName(Name);
        }

        if (Description != null)
        {
            habit.UpdateDescription(Description);
        }

        if (Frequency != null && 
            (!string.IsNullOrEmpty(Frequency.Type) || Frequency.TimesPerPeriod.HasValue))
        {
            var frequency = new Frequency
            {
                Type = string.IsNullOrEmpty(Frequency.Type) 
                    ? habit.Frequency.Type 
                    : Enum.Parse<FrequencyType>(Frequency.Type),
                TimesPerPeriod = Frequency.TimesPerPeriod ?? habit.Frequency.TimesPerPeriod
            };
            habit.UpdateFrequency(frequency);
        }

        if (Target != null)
        {
            var target = new Target
            {
                Value = Target.Value ?? habit.Target.Value,
                Unit = Target.Unit ?? habit.Target.Unit
            };
            habit.UpdateTarget(target);
        }

        if (ReminderTime != null)
        {
            habit.UpdateReminderTime(ReminderTime);
        }

        if (EndDate != null)
        {
            habit.UpdateEndDate(EndDate);
        }

        if (Milestone?.Target.HasValue == true)
        {
            habit.UpdateMilestoneTarget(Milestone.Target.Value);
        }
    }
}

/// <summary>
/// Request model for frequency within PartialUpdateHabitDto.
/// </summary>
public class FrequencyPartialUpdateDto
{
    /// <summary>
    /// The frequency type: "Daily", "Weekly", "Monthly", or "Yearly". Optional.
    /// </summary>
    [RegularExpression(@"^(Daily|Weekly|Monthly|Yearly)$", ErrorMessage = "Frequency type must be 'Daily', 'Weekly', 'Monthly', or 'Yearly'.")]
    public string? Type { get; set; }

    /// <summary>
    /// How many times per period (must be positive, reasonable max of 365). Optional.
    /// </summary>
    [Range(1, 365, ErrorMessage = "Times per period must be between 1 and 365.")]
    public int? TimesPerPeriod { get; set; }
}

/// <summary>
/// Request model for target within PartialUpdateHabitDto.
/// </summary>
public class TargetPartialUpdateDto
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

/// <summary>
/// Request model for milestone within PartialUpdateHabitDto.
/// Only Target is updatable; Current is managed by the domain.
/// </summary>
public class PartialUpdateMilestoneDto
{
    /// <summary>
    /// The target milestone value. Optional.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Milestone target must be non-negative.")]
    public int? Target { get; set; }
}
