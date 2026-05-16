using HabitTracker.Api.Data;
using HabitTracker.Api.Dtos;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Extensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Controllers;

[Authorize]
[Route("api/habits")]
public class HabitsController(ApplicationDbContext dbContext) : BaseApiController(dbContext)
{
    /// <summary>
    /// Get all habits for the current user.
    /// </summary>
    /// <returns>Paginated list of habits as DTOs with pagination metadata. Includes HATEOAS links for HAL clients.</returns>
    [HttpGet]
    [Produces("application/json", "application/hal+json")]
    public async Task<IActionResult> GetHabits([FromQuery] HabitsQueryParameters? queryParameters, CancellationToken cancellationToken)
    {
        var (currentUser, error) = await GetCurrentUserOrUnauthorizedAsync();
        if (error != null)
            return error;

        var parameters = queryParameters ?? new HabitsQueryParameters();

        var result = await ApplicationDbContext.Habits
            .Where(h => h.UserId == currentUser!.Id)
            .ApplyFiltering(parameters)
            .ApplySorting(parameters.Sort)
            .ToPagedResultAsync(parameters.PageNumber, parameters.PageSize, HabitDto.FromEntity, cancellationToken);

        if (AcceptsHalJson())
        {
            var halLinks = BuildPaginationLinks(parameters, result.PageNumber, result.PageSize, result.TotalCount);
            var halResult = new HalPagedResult<HabitDto>(result.Items, result.TotalCount, result.PageNumber, result.PageSize, halLinks);
            return Ok(halResult);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a single habit by ID, including its associated tags.
    /// </summary>
    /// <param name="habitId">The habit ID.</param>
    /// <returns>The habit details as a DTO (including tags), with HATEOAS links for HAL clients, or 404 if not found or not owned by the user.</returns>
    [HttpGet("{habitId:guid}")]
    [Produces("application/json", "application/hal+json")]
    public async Task<IActionResult> GetHabit(Guid habitId, CancellationToken cancellationToken)
    {
        var (currentUser, error) = await GetCurrentUserOrUnauthorizedAsync();
        if (error != null)
            return error;

        var habit = await ApplicationDbContext.Habits.FirstOrDefaultAsync(
            h => h.Id == habitId && h.UserId == currentUser!.Id,
            cancellationToken);

        if (habit == null)
        {
            return NotFound();
        }

        var tags = await ApplicationDbContext.Tags
            .Where(t => t.UserId == currentUser!.Id && t.HabitTags.Any(ht => ht.HabitId == habitId))
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        var habitDetails = HabitDetailsDto.FromEntity(habit, tags);

        if (AcceptsHalJson())
        {
            var halLinks = BuildDetailLinks(habitId, habit.IsArchived);
            var halResult = new HalResult<HabitDetailsDto>(habitDetails, halLinks);
            return Ok(halResult);
        }

        return Ok(habitDetails);
    }

    /// <summary>
    /// Create a new habit for the current user.
    /// </summary>
    /// <param name="request">The habit creation request.</param>
    /// <returns>The created habit as a DTO with 201 Created status, with HATEOAS links for HAL clients.</returns>
    [HttpPost]
    [Produces("application/json", "application/hal+json")]
    public async Task<IActionResult> CreateHabit([FromBody] CreateHabitDto request, CancellationToken cancellationToken)
    {
        var (currentUser, error) = await GetCurrentUserOrUnauthorizedAsync();
        if (error != null)
            return error;
        ArgumentNullException.ThrowIfNull(request);

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var habit = request.ToHabit(currentUser!.Id);

        ApplicationDbContext.Habits.Add(habit);
        await ApplicationDbContext.SaveChangesAsync(cancellationToken);

        var habitDto = HabitDto.FromEntity(habit);

        if (AcceptsHalJson())
        {
            var halLinks = BuildCreatedLinks(habit.Id);
            var halResult = new HalResult<HabitDto>(habitDto, halLinks);
            return CreatedAtAction(nameof(GetHabit), new { habitId = habit.Id }, halResult);
        }

        return CreatedAtAction(nameof(GetHabit), new { habitId = habit.Id }, habitDto);
    }

    /// <summary>
    /// Replace an entire habit (full replacement semantics).
    /// </summary>
    /// <param name="habitId">The habit ID.</param>
    /// <param name="request">The complete replacement request.</param>
    /// <returns>The replaced habit as a DTO, or 404 if not found or not owned by the user.</returns>
    [HttpPut("{habitId:guid}")]
    public async Task<IActionResult> UpdateHabit(Guid habitId, [FromBody] UpdateHabitDto request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (currentUser, error) = await GetCurrentUserOrUnauthorizedAsync();
        if (error != null)
            return error;

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var habit = await ApplicationDbContext.Habits.FirstOrDefaultAsync(
            h => h.Id == habitId && h.UserId == currentUser!.Id,
            cancellationToken);

        if (habit == null)
        {
            return NotFound();
        }

        // Apply full replacement via DTO mapping
        request.ApplyToHabit(habit);

        await ApplicationDbContext.SaveChangesAsync(cancellationToken);
        return Ok(HabitDto.FromEntity(habit));
    }

    /// <summary>
    /// Partially update an existing habit. Only provided fields are updated; omitted fields are preserved.
    /// </summary>
    /// <param name="habitId">The habit ID.</param>
    /// <param name="request">The partial update request.</param>
    /// <returns>The updated habit as a DTO, or 404 if not found or not owned by the user.</returns>
    [HttpPatch("{habitId:guid}")]
    public async Task<IActionResult> PatchHabit(Guid habitId, [FromBody] PartialUpdateHabitDto request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (currentUser, error) = await GetCurrentUserOrUnauthorizedAsync();
        if (error != null)
            return error;

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var habit = await ApplicationDbContext.Habits.FirstOrDefaultAsync(
            h => h.Id == habitId && h.UserId == currentUser!.Id,
            cancellationToken);

        if (habit == null)
        {
            return NotFound();
        }

        // Apply selective updates via DTO mapping
        request.ApplyToHabit(habit);

        await ApplicationDbContext.SaveChangesAsync(cancellationToken);
        return Ok(HabitDto.FromEntity(habit));
    }

    /// <summary>
    /// Delete (archive) a habit.
    /// </summary>
    /// <param name="habitId">The habit ID.</param>
    /// <returns>204 No Content on success, 404 if not found or not owned by the user.</returns>
    [HttpDelete("{habitId:guid}")]
    public async Task<IActionResult> DeleteHabit(Guid habitId, CancellationToken cancellationToken)
    {
        var (currentUser, error) = await GetCurrentUserOrUnauthorizedAsync();
        if (error != null)
            return error;

        var habit = await ApplicationDbContext.Habits.FirstOrDefaultAsync(
            h => h.Id == habitId && h.UserId == currentUser!.Id,
            cancellationToken);

        if (habit == null)
        {
            return NotFound();
        }

        // Soft delete via domain method
        habit.Archive();
        await ApplicationDbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Upsert tags to a habit. Replaces all existing tags with the provided list.
    /// </summary>
    /// <param name="habitId">The habit ID.</param>
    /// <param name="request">Request containing list of tag IDs to associate with the habit.</param>
    /// <returns>200 OK with the list of tags now associated with the habit, or 404 if habit not found.</returns>
    [HttpPut("{habitId:guid}/tags")]
    public async Task<IActionResult> UpsertHabitTags(Guid habitId, [FromBody] UpsertHabitTagsRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (currentUser, error) = await GetCurrentUserOrUnauthorizedAsync();
        if (error != null)
            return error;

        var habit = await ApplicationDbContext.Habits.FirstOrDefaultAsync(
            h => h.Id == habitId && h.UserId == currentUser!.Id,
            cancellationToken);

        if (habit == null)
        {
            return NotFound();
        }

        if (request.TagIds == null || request.TagIds.Count == 0)
        {
            // Clear all tags if empty list provided
            var existingJoins = await ApplicationDbContext.HabitTags
                .Where(ht => ht.HabitId == habitId)
                .ToListAsync(cancellationToken);

            ApplicationDbContext.HabitTags.RemoveRange(existingJoins);
            await ApplicationDbContext.SaveChangesAsync(cancellationToken);
            return Ok(new List<TagDto>());
        }

        // Verify all provided tag IDs belong to current user
        var tagIds = request.TagIds.Distinct().ToList();
        var userTags = await ApplicationDbContext.Tags
            .Where(t => t.UserId == currentUser!.Id && tagIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        if (userTags.Count != tagIds.Count)
        {
            return BadRequest(new { error = "One or more tag IDs do not exist or do not belong to the current user." });
        }

        // Remove existing joins for this habit
        var existingJoins2 = await ApplicationDbContext.HabitTags
            .Where(ht => ht.HabitId == habitId)
            .ToListAsync(cancellationToken);

        ApplicationDbContext.HabitTags.RemoveRange(existingJoins2);

        // Add new joins
        foreach (var tagId in tagIds)
        {
            var habitTag = new HabitTag(habitId, tagId);
            ApplicationDbContext.HabitTags.Add(habitTag);
        }

        await ApplicationDbContext.SaveChangesAsync(cancellationToken);

        // Return the updated list of tags
        var tagsForHabit = await ApplicationDbContext.Tags
            .Where(t => tagIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        return Ok(tagsForHabit.Select(TagDto.FromEntity).ToList());
    }

    /// <summary>
    /// Remove a single tag from a habit.
    /// </summary>
    /// <param name="habitId">The habit ID.</param>
    /// <param name="tagId">The tag ID to remove.</param>
    /// <returns>204 No Content on success, or 404 if habit or relationship not found or not owned by the user.</returns>
    [HttpDelete("{habitId:guid}/tags/{tagId:guid}")]
    public async Task<IActionResult> RemoveTagFromHabit(Guid habitId, Guid tagId, CancellationToken cancellationToken)
    {
        var (currentUser, error) = await GetCurrentUserOrUnauthorizedAsync();
        if (error != null)
            return error;

        var habit = await ApplicationDbContext.Habits.FirstOrDefaultAsync(
            h => h.Id == habitId && h.UserId == currentUser!.Id,
            cancellationToken);

        if (habit == null)
        {
            return NotFound();
        }

        var habitTag = await ApplicationDbContext.HabitTags.FirstOrDefaultAsync(
            ht => ht.HabitId == habitId && ht.TagId == tagId,
            cancellationToken);

        if (habitTag == null)
        {
            return NotFound();
        }

        // Verify the tag belongs to the current user (security check)
        var tag = await ApplicationDbContext.Tags.FirstOrDefaultAsync(
            t => t.Id == tagId && t.UserId == currentUser!.Id,
            cancellationToken);

        if (tag == null)
        {
            return NotFound();
        }

        ApplicationDbContext.HabitTags.Remove(habitTag);
        await ApplicationDbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private List<HateoasLink> BuildPaginationLinks(HabitsQueryParameters parameters, int pageNumber, int pageSize, int totalCount)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var links = new List<HateoasLink>();
        var totalPages = (totalCount + pageSize - 1) / pageSize;

        links.Add(new HateoasLink(
            Href: CreateRouteLink(nameof(GetHabits), BuildHabitsRouteValues(parameters, pageNumber, pageSize)),
            Rel: "self",
            Method: "GET"));

        links.Add(new HateoasLink(
            Href: CreateRouteLink(nameof(GetHabits), BuildHabitsRouteValues(parameters, 1, pageSize)),
            Rel: "first",
            Method: "GET"));

        if (totalPages > 0)
        {
            links.Add(new HateoasLink(
                Href: CreateRouteLink(nameof(GetHabits), BuildHabitsRouteValues(parameters, totalPages, pageSize)),
                Rel: "last",
                Method: "GET"));
        }

        if (pageNumber > 1)
        {
            links.Add(new HateoasLink(
                Href: CreateRouteLink(nameof(GetHabits), BuildHabitsRouteValues(parameters, pageNumber - 1, pageSize)),
                Rel: "prev",
                Method: "GET"));
        }

        if (pageNumber < totalPages)
        {
            links.Add(new HateoasLink(
                Href: CreateRouteLink(nameof(GetHabits), BuildHabitsRouteValues(parameters, pageNumber + 1, pageSize)),
                Rel: "next",
                Method: "GET"));
        }

        links.Add(new HateoasLink(
            Href: CreateRouteLink(nameof(GetHabits), null),
            Rel: "create",
            Method: "POST",
            Title: "Create a new habit"));

        return links;
    }

    private static Dictionary<string, object?> BuildHabitsRouteValues(HabitsQueryParameters parameters, int pageNumber, int pageSize)
    {
        var routeValues = new Dictionary<string, object?>
        {
            ["pageNumber"] = pageNumber,
            ["pageSize"] = pageSize
        };

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            routeValues["search"] = parameters.Search;
        }

        if (parameters.Type.HasValue)
        {
            routeValues["type"] = parameters.Type.Value;
        }

        if (parameters.Status.HasValue)
        {
            routeValues["status"] = parameters.Status.Value;
        }

        if (!string.IsNullOrWhiteSpace(parameters.Sort))
        {
            routeValues["sort"] = parameters.Sort;
        }

        return routeValues;
    }

    private List<HateoasLink> BuildDetailLinks(Guid habitId, bool isArchived)
    {
        List<HateoasLink> links =
        [
            new HateoasLink(
                Href: CreateRouteLink(nameof(GetHabit), new { habitId }),
                Rel: "self",
                Method: "GET"),
            new HateoasLink(
                Href: CreateRouteLink(nameof(UpdateHabit), new { habitId }),
                Rel: "update",
                Method: "PUT",
                Title: "Replace this habit"),
            new HateoasLink(
                Href: CreateRouteLink(nameof(PatchHabit), new { habitId }),
                Rel: "patch",
                Method: "PATCH",
                Title: "Partially update this habit"),
        ];

        if (!isArchived)
        {
            links.Add(new HateoasLink(
                Href: CreateRouteLink(nameof(DeleteHabit), new { habitId }),
                Rel: "delete",
                Method: "DELETE",
                Title: "Archive this habit"));
        }

        links.Add(new HateoasLink(
            Href: CreateRouteLink(nameof(UpsertHabitTags), new { habitId }),
            Rel: "tags",
            Method: "PUT",
            Title: "Replace tags for this habit"));

        links.Add(new HateoasLink(
            Href: CreateRouteLink(nameof(GetHabits), null),
            Rel: "all",
            Method: "GET",
            Title: "List all habits"));

        return links;
    }

    private List<HateoasLink> BuildCreatedLinks(Guid habitId) =>
    [
        new HateoasLink(
            Href: CreateRouteLink(nameof(GetHabit), new { habitId }),
            Rel: "self",
            Method: "GET",
            Title: "The created habit"),
        new HateoasLink(
            Href: CreateRouteLink(nameof(UpdateHabit), new { habitId }),
            Rel: "update",
            Method: "PUT",
            Title: "Replace this habit"),
        new HateoasLink(
            Href: CreateRouteLink(nameof(GetHabits), null),
            Rel: "all",
            Method: "GET",
            Title: "List all habits"),
    ];

    private string CreateRouteLink(string actionName, object? routeValues) =>
        Url.Action(actionName, values: routeValues) 
            ?? throw new InvalidOperationException($"Unable to generate route link for action '{actionName}'.");

    private bool AcceptsHalJson() => Request.GetTypedHeaders().Accept switch
    {
        null => false,
        { Count: 0 } => false,
        var acceptedMediaTypes => acceptedMediaTypes.Any(mediaType =>
            string.Equals(mediaType.MediaType.Value, "application/hal+json", StringComparison.OrdinalIgnoreCase))
    };
}