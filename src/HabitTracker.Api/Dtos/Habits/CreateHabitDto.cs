using System.ComponentModel.DataAnnotations;
using HabitTracker.Api.Dtos.Models;
using HabitTracker.Api.Entities;

namespace HabitTracker.Api.Dtos.Habits;

/// <summary>
/// Request model for creating a new habit.
/// </summary>
public record CreateHabitDto(
    [Required(ErrorMessage = "Habit name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Habit name must be between 1 and 200 characters.")]
    string Name,
    [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
    string? Description,
    [Required(ErrorMessage = "Habit type is required.")]
    [RegularExpression(@"^(Binary|Measurable)$", ErrorMessage = "Habit type must be 'Binary' or 'Measurable'.")]
    string Type,
    [Required(ErrorMessage = "Frequency is required.")]
    FrequencyCreateDto? Frequency,
    TargetCreateDto? Target,
    DateOnly? EndDate,
    TimeOnly? ReminderTime)
{
    /// <summary>
    /// Maps this DTO to a domain Habit entity using the factory method.
    /// </summary>
    /// <param name="userId">The ID of the user who owns this habit.</param>
    /// <returns>A new Habit instance.</returns>
    public Habit ToHabit(Guid userId)
    {
        var frequency = new Frequency
        {
            Type = Enum.Parse<FrequencyType>(Frequency!.Type),
            TimesPerPeriod = Frequency.TimesPerPeriod ?? 1
        };

        var target = Target != null
            ? new Target { Value = Target.Value, Unit = Target.Unit }
            : new Target();

        return Habit.Create(
            userId,
            Name,
            Description,
            Enum.Parse<HabitType>(Type),
            frequency,
            target,
            EndDate,
            ReminderTime
        );
    }
}

/// <summary>
/// Request model for the frequency within CreateHabitDto.
/// </summary>
public record FrequencyCreateDto(
    [Required(ErrorMessage = "Frequency type is required.")]
    [RegularExpression(@"^(Daily|Weekly|Monthly|Yearly)$", ErrorMessage = "Frequency type must be 'Daily', 'Weekly', 'Monthly', or 'Yearly'.")]
    string Type,
    [Required(ErrorMessage = "Times per period is required.")]
    [Range(1, 365, ErrorMessage = "Times per period must be between 1 and 365.")]
    int? TimesPerPeriod);

/// <summary>
/// Request model for the target within CreateHabitDto.
/// </summary>
public record TargetCreateDto(
    int? Value,
    [StringLength(100, ErrorMessage = "Unit must not exceed 100 characters.")]
    string? Unit);
