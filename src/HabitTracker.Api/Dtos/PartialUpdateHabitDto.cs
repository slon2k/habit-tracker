using System.ComponentModel.DataAnnotations;
using HabitTracker.Api.Entities;

namespace HabitTracker.Api.Dtos;

/// <summary>
/// Request model for partially updating an existing habit (PATCH).
/// All fields are optional; only provided fields are updated.
/// Excludes Type (immutable).
/// </summary>
public record PartialUpdateHabitDto(
    [property: StringLength(200, MinimumLength = 1, ErrorMessage = "Habit name must be between 1 and 200 characters.")]
    string? Name,
    [property: StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
    string? Description,
    FrequencyPartialUpdateDto? Frequency,
    TargetPartialUpdateDto? Target,
    DateOnly? EndDate,
    TimeOnly? ReminderTime,
    PartialUpdateMilestoneDto? Milestone = null)
{
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
public record FrequencyPartialUpdateDto(
    [property: RegularExpression(@"^(Daily|Weekly|Monthly|Yearly)$", ErrorMessage = "Frequency type must be 'Daily', 'Weekly', 'Monthly', or 'Yearly'.")]
    string? Type,
    [property: Range(1, 365, ErrorMessage = "Times per period must be between 1 and 365.")]
    int? TimesPerPeriod);

/// <summary>
/// Request model for target within PartialUpdateHabitDto.
/// </summary>
public record TargetPartialUpdateDto(
    int? Value,
    [property: StringLength(100, ErrorMessage = "Unit must not exceed 100 characters.")]
    string? Unit);

/// <summary>
/// Request model for milestone within PartialUpdateHabitDto.
/// Only Target is updatable; Current is managed by the domain.
/// </summary>
public record PartialUpdateMilestoneDto(
    [property: Range(0, int.MaxValue, ErrorMessage = "Milestone target must be non-negative.")]
    int? Target);
