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

        var habit = request.ToHabit(_currentUserId);

        _dbContext.Habits.Add(habit);
        _dbContext.SaveChanges();

        return CreatedAtAction(nameof(GetHabit), new { habitId = habit.Id }, HabitDto.FromEntity(habit));
    }

    /// <summary>
    /// Replace an entire habit (full replacement semantics).
    /// </summary>
    /// <param name="habitId">The habit ID.</param>
    /// <param name="request">The complete replacement request.</param>
    /// <returns>The replaced habit as a DTO, or 404 if not found or not owned by the user.</returns>
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

        // Apply full replacement via DTO mapping
        request.ApplyToHabit(habit);

        _dbContext.SaveChanges();
        return Ok(HabitDto.FromEntity(habit));
    }

    /// <summary>
    /// Partially update an existing habit. Only provided fields are updated; omitted fields are preserved.
    /// </summary>
    /// <param name="habitId">The habit ID.</param>
    /// <param name="request">The partial update request.</param>
    /// <returns>The updated habit as a DTO, or 404 if not found or not owned by the user.</returns>
    [HttpPatch("{habitId:guid}")]
    public IActionResult PatchHabit(Guid habitId, [FromBody] PartialUpdateHabitDto request)
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

        // Apply selective updates via DTO mapping
        request.ApplyToHabit(habit);

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