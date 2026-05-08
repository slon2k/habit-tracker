using System.ComponentModel.DataAnnotations;
using HabitTracker.Api.Entities;

namespace HabitTracker.Api.Dtos;

/// <summary>
/// Request model for replacing an entire habit (PUT).
/// All fields must be provided for full replacement semantics.
/// </summary>
public record UpdateHabitDto(
    [property: Required(ErrorMessage = "Habit name is required.")]
    [property: StringLength(200, MinimumLength = 1, ErrorMessage = "Habit name must be between 1 and 200 characters.")]
    string Name,
    [property: StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
    string? Description,
    [property: Required(ErrorMessage = "Frequency is required.")]
    FrequencyUpdateDto? Frequency,
    [property: Required(ErrorMessage = "Target is required.")]
    TargetUpdateDto? Target,
    DateOnly? EndDate,
    TimeOnly? ReminderTime,
    UpdateMilestoneDto? Milestone = null)
{
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
public record FrequencyUpdateDto(
    [property: Required(ErrorMessage = "Frequency type is required.")]
    [property: RegularExpression(@"^(Daily|Weekly|Monthly|Yearly)$", ErrorMessage = "Frequency type must be 'Daily', 'Weekly', 'Monthly', or 'Yearly'.")]
    string Type,
    [property: Required(ErrorMessage = "Times per period is required.")]
    [property: Range(1, 365, ErrorMessage = "Times per period must be between 1 and 365.")]
    int TimesPerPeriod);

/// <summary>
/// Request model for target within UpdateHabitDto.
/// </summary>
public record TargetUpdateDto(
    int? Value,
    [property: StringLength(100, ErrorMessage = "Unit must not exceed 100 characters.")]
    string? Unit);

/// <summary>
/// Request model for milestone within UpdateHabitDto.
/// Only Target is updatable; Current is managed by the domain.
/// </summary>
public record UpdateMilestoneDto(
    [property: Required(ErrorMessage = "Milestone target is required.")]
    [property: Range(0, int.MaxValue, ErrorMessage = "Milestone target must be non-negative.")]
    int Target);
