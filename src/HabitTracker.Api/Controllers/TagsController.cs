using HabitTracker.Api.Data;
using HabitTracker.Api.Dtos;
using HabitTracker.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Placeholder for authenticated user's ID.
    /// Implementation note: Extract from JWT claims/identity when authentication is implemented.
    /// </summary>
#pragma warning disable S1135 // Suppress SonarAnalyzer TODO rule
    private readonly Guid _currentUserId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001");
#pragma warning restore S1135

    public TagsController(ApplicationDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    /// <summary>
    /// Get all tags for the current user.
    /// </summary>
    /// <returns>List of tags as DTOs, ordered by creation date.</returns>
    [HttpGet]
    public async Task<IActionResult> GetTags(CancellationToken cancellationToken)
    {
        var tags = await _dbContext.Tags
            .Where(t => t.UserId == _currentUserId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var dtos = tags.Select(TagDto.FromEntity).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Get a single tag by ID.
    /// </summary>
    /// <param name="tagId">The tag ID.</param>
    /// <returns>The tag as a DTO, or 404 if not found or not owned by the user.</returns>
    [HttpGet("{tagId:guid}")]
    public async Task<IActionResult> GetTag(Guid tagId, CancellationToken cancellationToken)
    {
        var tag = await _dbContext.Tags.FirstOrDefaultAsync(
            t => t.Id == tagId && t.UserId == _currentUserId,
            cancellationToken);

        return tag == null ? NotFound() : Ok(TagDto.FromEntity(tag));
    }

    /// <summary>
    /// Create a new tag for the current user.
    /// </summary>
    /// <param name="request">The tag creation request.</param>
    /// <returns>The created tag as a DTO with 201 Created status.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagDto request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        // Check for duplicate tag name (unique per user)
        var existingTag = await _dbContext.Tags.FirstOrDefaultAsync(
            t => t.UserId == _currentUserId && t.Name == request.Name,
            cancellationToken);

        if (existingTag != null)
        {
            return Conflict(new { error = "A tag with this name already exists." });
        }

        var tagId = Guid.NewGuid();
        var tag = new Tag(tagId, _currentUserId, request.Name);

        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = TagDto.FromEntity(tag);
        return CreatedAtAction(nameof(GetTag), new { tagId = tag.Id }, dto);
    }

    /// <summary>
    /// Update a tag's name.
    /// </summary>
    /// <param name="tagId">The tag ID.</param>
    /// <param name="request">The update request containing the new name.</param>
    /// <returns>The updated tag as a DTO, or 404 if not found or not owned by the user.</returns>
    [HttpPut("{tagId:guid}")]
    public async Task<IActionResult> UpdateTag(Guid tagId, [FromBody] CreateTagDto request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var tag = await _dbContext.Tags.FirstOrDefaultAsync(
            t => t.Id == tagId && t.UserId == _currentUserId,
            cancellationToken);

        if (tag == null)
        {
            return NotFound();
        }

        // Check for duplicate tag name (unique per user, but allow same name if updating the same tag)
        var conflictingTag = await _dbContext.Tags.FirstOrDefaultAsync(
            t => t.UserId == _currentUserId && t.Name == request.Name && t.Id != tagId,
            cancellationToken);

        if (conflictingTag != null)
        {
            return Conflict(new { error = "A tag with this name already exists." });
        }

        tag.UpdateName(request.Name);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = TagDto.FromEntity(tag);
        return Ok(dto);
    }

    /// <summary>
    /// Delete a tag by ID. This also removes all associated HabitTag relationships via cascade delete.
    /// </summary>
    /// <param name="tagId">The tag ID.</param>
    /// <returns>204 No Content on success, or 404 if not found or not owned by the user.</returns>
    [HttpDelete("{tagId:guid}")]
    public async Task<IActionResult> DeleteTag(Guid tagId, CancellationToken cancellationToken)
    {
        var tag = await _dbContext.Tags.FirstOrDefaultAsync(
            t => t.Id == tagId && t.UserId == _currentUserId,
            cancellationToken);

        if (tag == null)
        {
            return NotFound();
        }

        _dbContext.Tags.Remove(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
