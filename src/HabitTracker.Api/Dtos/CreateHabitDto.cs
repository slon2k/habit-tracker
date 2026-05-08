using System.ComponentModel.DataAnnotations;
using HabitTracker.Api.Entities;

namespace HabitTracker.Api.Dtos;

/// <summary>
/// Request model for creating a new habit.
/// </summary>
public record CreateHabitDto(
    [property: Required(ErrorMessage = "Habit name is required.")]
    [property: StringLength(200, MinimumLength = 1, ErrorMessage = "Habit name must be between 1 and 200 characters.")]
    string Name,
    [property: StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
    string? Description,
    [property: Required(ErrorMessage = "Habit type is required.")]
    [property: RegularExpression(@"^(Binary|Measurable)$", ErrorMessage = "Habit type must be 'Binary' or 'Measurable'.")]
    string Type,
    [property: Required(ErrorMessage = "Frequency is required.")]
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
            TimesPerPeriod = Frequency.TimesPerPeriod
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
    [property: Required(ErrorMessage = "Frequency type is required.")]
    [property: RegularExpression(@"^(Daily|Weekly|Monthly|Yearly)$", ErrorMessage = "Frequency type must be 'Daily', 'Weekly', 'Monthly', or 'Yearly'.")]
    string Type,
    [property: Required(ErrorMessage = "Times per period is required.")]
    [property: Range(1, 365, ErrorMessage = "Times per period must be between 1 and 365.")]
    int TimesPerPeriod);

/// <summary>
/// Request model for the target within CreateHabitDto.
/// </summary>
public record TargetCreateDto(
    int? Value,
    [property: StringLength(100, ErrorMessage = "Unit must not exceed 100 characters.")]
    string? Unit);
