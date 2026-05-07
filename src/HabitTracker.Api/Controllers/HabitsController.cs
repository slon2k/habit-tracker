using HabitTracker.Api.Data;
using HabitTracker.Api.Dtos;
using HabitTracker.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Api.Controllers;

[ApiController]
[Route("api/habits")]
public class HabitsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Placeholder for authenticated user's ID. 
    /// Implementation note: Extract from JWT claims/identity when authentication is implemented.
    /// </summary>
#pragma warning disable S1135 // Suppress SonarAnalyzer TODO rule
    private readonly Guid _currentUserId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001");
#pragma warning restore S1135

    public HabitsController(ApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    /// <summary>
    /// Get all habits for the current user.
    /// </summary>
    /// <returns>List of habits as DTOs.</returns>
    [HttpGet]
    public IActionResult GetHabits()
    {
        var habits = _dbContext.Habits
            .Where(h => h.UserId == _currentUserId)
            .OrderByDescending(h => h.CreatedAtUtc)
            .ToList();

        var dtos = habits.Select(HabitDto.FromEntity).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Get a single habit by ID.
    /// </summary>
    /// <param name="habitId">The habit ID.</param>
    /// <returns>The habit as a DTO, or 404 if not found or not owned by the user.</returns>
    [HttpGet("{habitId:guid}")]
    public IActionResult GetHabit(Guid habitId)
    {
        var habit = _dbContext.Habits.FirstOrDefault(h => h.Id == habitId && h.UserId == _currentUserId);
        return habit == null ? NotFound() : Ok(HabitDto.FromEntity(habit));
    }

    /// <summary>
    /// Create a new habit for the current user.
    /// </summary>
    /// <param name="request">The habit creation request.</param>
    /// <returns>The created habit as a DTO with 201 Created status.</returns>
    [HttpPost]
    public IActionResult CreateHabit([FromBody] CreateHabitDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Map DTO to domain objects
        var frequency = new Frequency
        {
            Type = Enum.Parse<FrequencyType>(request.Frequency!.Type),
            TimesPerPeriod = request.Frequency.TimesPerPeriod
        };

        var target = request.Target != null
            ? new Target { Value = request.Target.Value, Unit = request.Target.Unit }
            : new Target();

        // Use factory method to create and validate the habit
        var habit = Habit.Create(
            _currentUserId,
            request.Name,
            request.Description,
            Enum.Parse<HabitType>(request.Type),
            frequency,
            target,
            request.EndDate,
            request.ReminderTime
        );

        _dbContext.Habits.Add(habit);
        _dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetHabit), new { habitId = habit.Id }, HabitDto.FromEntity(habit));
    }

    /// <summary>
    /// Update an existing habit.
    /// </summary>
    /// <param name="habitId">The habit ID.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The updated habit as a DTO, or 404 if not found or not owned by the user.</returns>
    [HttpPut("{habitId:guid}")]
    public IActionResult UpdateHabit(Guid habitId, [FromBody] UpdateHabitDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var habit = _dbContext.Habits.FirstOrDefault(h => h.Id == habitId && h.UserId == _currentUserId);
        if (habit == null)
        {
            return NotFound();
        }

        // Apply updates (only fields that are set in the request)
        if (!string.IsNullOrEmpty(request.Name))
        {
            habit.UpdateName(request.Name);
        }

        if (request.Description != null)
        {
            habit.UpdateDescription(request.Description);
        }

        if (request.Frequency != null && 
            (!string.IsNullOrEmpty(request.Frequency.Type) || request.Frequency.TimesPerPeriod.HasValue))
        {
            var frequency = new Frequency
            {
                Type = string.IsNullOrEmpty(request.Frequency.Type) 
                    ? habit.Frequency.Type 
                    : Enum.Parse<FrequencyType>(request.Frequency.Type),
                TimesPerPeriod = request.Frequency.TimesPerPeriod ?? habit.Frequency.TimesPerPeriod
            };
            habit.UpdateFrequency(frequency);
        }

        if (request.Target != null)
        {
            var target = new Target
            {
                Value = request.Target.Value ?? habit.Target.Value,
                Unit = request.Target.Unit ?? habit.Target.Unit
            };
            habit.UpdateTarget(target);
        }

        if (request.ReminderTime != null)
        {
            habit.UpdateReminderTime(request.ReminderTime);
        }

        if (request.EndDate != null)
        {
            habit.UpdateEndDate(request.EndDate);
        }

        _dbContext.SaveChanges();
        return Ok(HabitDto.FromEntity(habit));
    }

    /// <summary>
    /// Delete (archive) a habit.
    /// </summary>
    /// <param name="habitId">The habit ID.</param>
    /// <returns>204 No Content on success, 404 if not found or not owned by the user.</returns>
    [HttpDelete("{habitId:guid}")]
    public IActionResult DeleteHabit(Guid habitId)
    {
        var habit = _dbContext.Habits.FirstOrDefault(h => h.Id == habitId && h.UserId == _currentUserId);
        if (habit == null)
        {
            return NotFound();
        }

        // Soft delete via domain method
        habit.Archive();
        _dbContext.SaveChanges();

        return NoContent();
    }
}