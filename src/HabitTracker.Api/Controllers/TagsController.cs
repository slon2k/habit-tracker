using HabitTracker.Api.Data;
using HabitTracker.Api.Dtos.Common;
using HabitTracker.Api.Dtos.Tags;
using HabitTracker.Api.Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Controllers;

[Authorize]
[Route("api/tags")]
public class TagsController(ApplicationDbContext dbContext) : BaseApiController(dbContext)
{
    /// <summary>
    /// Get all tags for the current user.
    /// </summary>
    /// <returns>List of tags as DTOs, ordered by creation date.</returns>
    [HttpGet]
    public async Task<IActionResult> GetTags(CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        if (currentUser == null)
            return Unauthorized();

        var tags = await ApplicationDbContext.Tags
            .Where(t => t.UserId == currentUser.Id)
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
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        if (currentUser == null)
            return Unauthorized();

        var tag = await ApplicationDbContext.Tags.FirstOrDefaultAsync(
            t => t.Id == tagId && t.UserId == currentUser.Id,
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

        var currentUser = await GetCurrentUserAsync(cancellationToken);
        if (currentUser == null)
            return Unauthorized();

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        // Check for duplicate tag name (unique per user)
        var existingTag = await ApplicationDbContext.Tags.FirstOrDefaultAsync(
            t => t.UserId == currentUser.Id && t.Name == request.Name,
            cancellationToken);

        if (existingTag != null)
        {
            return Conflict(new { error = "A tag with this name already exists." });
        }

        var tagId = Guid.NewGuid();
        var tag = new Tag(tagId, currentUser.Id, request.Name);

        ApplicationDbContext.Tags.Add(tag);
        await ApplicationDbContext.SaveChangesAsync(cancellationToken);

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

        var (currentUser, error) = await GetCurrentUserOrUnauthorizedAsync(cancellationToken);
        if (error != null)
            return error;

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var tag = await ApplicationDbContext.Tags.FirstOrDefaultAsync(
            t => t.Id == tagId && t.UserId == currentUser!.Id,
            cancellationToken);

        if (tag == null)
        {
            return NotFound();
        }

        // Check for duplicate tag name (unique per user, but allow same name if updating the same tag)
        var conflictingTag = await ApplicationDbContext.Tags.FirstOrDefaultAsync(
            t => t.UserId == currentUser!.Id && t.Name == request.Name && t.Id != tagId,
            cancellationToken);

        if (conflictingTag != null)
        {
            return Conflict(new { error = "A tag with this name already exists." });
        }

        tag.UpdateName(request.Name);
        await ApplicationDbContext.SaveChangesAsync(cancellationToken);

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
        var (currentUser, error) = await GetCurrentUserOrUnauthorizedAsync(cancellationToken);
        if (error != null)
            return error;

        var tag = await ApplicationDbContext.Tags.FirstOrDefaultAsync(
            t => t.Id == tagId && t.UserId == currentUser!.Id,
            cancellationToken);

        if (tag == null)
        {
            return NotFound();
        }

        ApplicationDbContext.Tags.Remove(tag);
        await ApplicationDbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
