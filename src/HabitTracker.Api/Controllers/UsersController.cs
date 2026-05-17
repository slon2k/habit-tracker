using HabitTracker.Api.Data;
using HabitTracker.Api.Dtos.Users;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Api.Controllers;

[Authorize]
[Route("api/users")]
public sealed class UsersController(ApplicationDbContext applicationDbContext) : BaseApiController(applicationDbContext)
{
    /// <summary>
    /// Gets a user by their unique ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 OK with user DTO, or 404 if not found.</returns>
    [HttpGet("{id:guid}", Name = "GetUserById")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await ApplicationDbContext.Users.FindAsync([id], cancellationToken);

        return user == null ? NotFound() : Ok(UserDto.FromEntity(user));
    }

    /// <summary>
    /// Gets the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 OK with user DTO, or 401 if not authenticated.</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentUserAsync(cancellationToken);

        return user == null ? Unauthorized() : Ok(UserDto.FromEntity(user));
    }
}

