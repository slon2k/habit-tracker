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
            return ValidationProblem(ModelState);
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
            return ValidationProblem(ModelState);
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
            return ValidationProblem(ModelState);
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

    /// <summary>
    /// Upsert tags to a habit. Replaces all existing tags with the provided list.
    /// </summary>
    /// <param name="habitId">The habit ID.</param>
    /// <param name="request">Request containing list of tag IDs to associate with the habit.</param>
    /// <returns>200 OK with the list of tags now associated with the habit, or 404 if habit not found.</returns>
    [HttpPut("{habitId:guid}/tags")]
    public IActionResult UpsertHabitTags(Guid habitId, [FromBody] UpsertHabitTagsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var habit = _dbContext.Habits.FirstOrDefault(h => h.Id == habitId && h.UserId == _currentUserId);
        if (habit == null)
        {
            return NotFound();
        }

        if (request.TagIds == null || request.TagIds.Count == 0)
        {
            // Clear all tags if empty list provided
            var existingJoins = _dbContext.HabitTags.Where(ht => ht.HabitId == habitId).ToList();
            _dbContext.HabitTags.RemoveRange(existingJoins);
            _dbContext.SaveChanges();
            return Ok(new List<TagDto>());
        }

        // Verify all provided tag IDs belong to current user
        var tagIds = request.TagIds.Distinct().ToList();
        var userTags = _dbContext.Tags
            .Where(t => t.UserId == _currentUserId && tagIds.Contains(t.Id))
            .ToList();

        if (userTags.Count != tagIds.Count)
        {
            return BadRequest(new { error = "One or more tag IDs do not exist or do not belong to the current user." });
        }

        // Remove existing joins for this habit
        var existingJoins2 = _dbContext.HabitTags.Where(ht => ht.HabitId == habitId).ToList();
        _dbContext.HabitTags.RemoveRange(existingJoins2);

        // Add new joins
        foreach (var tagId in tagIds)
        {
            var habitTag = new HabitTag(habitId, tagId);
            _dbContext.HabitTags.Add(habitTag);
        }

        _dbContext.SaveChanges();

        // Return the updated list of tags
        var tagsForHabit = _dbContext.Tags
            .Where(t => tagIds.Contains(t.Id))
            .Select(TagDto.FromEntity)
            .ToList();

        return Ok(tagsForHabit);
    }

    /// <summary>
    /// Remove a single tag from a habit.
    /// </summary>
    /// <param name="habitId">The habit ID.</param>
    /// <param name="tagId">The tag ID to remove.</param>
    /// <returns>204 No Content on success, or 404 if habit or relationship not found or not owned by the user.</returns>
    [HttpDelete("{habitId:guid}/tags/{tagId:guid}")]
    public IActionResult RemoveTagFromHabit(Guid habitId, Guid tagId)
    {
        var habit = _dbContext.Habits.FirstOrDefault(h => h.Id == habitId && h.UserId == _currentUserId);
        if (habit == null)
        {
            return NotFound();
        }

        var habitTag = _dbContext.HabitTags.FirstOrDefault(ht => ht.HabitId == habitId && ht.TagId == tagId);
        if (habitTag == null)
        {
            return NotFound();
        }

        // Verify the tag belongs to the current user (security check)
        var tag = _dbContext.Tags.FirstOrDefault(t => t.Id == tagId && t.UserId == _currentUserId);
        if (tag == null)
        {
            return NotFound();
        }

        _dbContext.HabitTags.Remove(habitTag);
        _dbContext.SaveChanges();

        return NoContent();
    }
}