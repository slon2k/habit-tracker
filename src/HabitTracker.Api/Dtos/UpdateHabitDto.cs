using System.ComponentModel.DataAnnotations;
using HabitTracker.Api.Entities;

namespace HabitTracker.Api.Dtos;

/// <summary>
/// Request model for replacing an entire habit (PUT).
/// All fields must be provided for full replacement semantics.
/// </summary>
public record UpdateHabitDto(
    [Required(ErrorMessage = "Habit name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Habit name must be between 1 and 200 characters.")]
    string Name,
    [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
    string? Description,
    [Required(ErrorMessage = "Frequency is required.")]
    FrequencyUpdateDto? Frequency,
    [Required(ErrorMessage = "Target is required.")]
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
            TimesPerPeriod = Frequency.TimesPerPeriod ?? 1
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

        if (Milestone != null && Milestone.Target.HasValue && Milestone.Target > 0)
        {
            habit.UpdateMilestoneTarget(Milestone.Target.Value);
        }
    }
}

/// <summary>
/// Request model for frequency within UpdateHabitDto.
/// </summary>
public record FrequencyUpdateDto(
    [Required(ErrorMessage = "Frequency type is required.")]
    [RegularExpression(@"^(Daily|Weekly|Monthly|Yearly)$", ErrorMessage = "Frequency type must be 'Daily', 'Weekly', 'Monthly', or 'Yearly'.")]
    string Type,
    [Required(ErrorMessage = "Times per period is required.")]
    [Range(1, 365, ErrorMessage = "Times per period must be between 1 and 365.")]
    int? TimesPerPeriod);

/// <summary>
/// Request model for target within UpdateHabitDto.
/// </summary>
public record TargetUpdateDto(
    int? Value,
    [StringLength(100, ErrorMessage = "Unit must not exceed 100 characters.")]
    string? Unit);

/// <summary>
/// Request model for milestone within UpdateHabitDto.
/// Only Target is updatable; Current is managed by the domain.
/// </summary>
public record UpdateMilestoneDto(
    [Range(0, int.MaxValue, ErrorMessage = "Milestone target must be non-negative.")]
    int? Target);
