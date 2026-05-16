using HabitTracker.Api.Data;
using HabitTracker.Api.Dtos;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Api.Controllers;

[Authorize]
[Route("api/users")]
public sealed class UsersController(ApplicationDbContext applicationDbContext) : BaseApiController(applicationDbContext)
{
    [HttpGet("{id:guid}", Name = "GetUserById")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await ApplicationDbContext.Users.FindAsync([id], cancellationToken);

        return user == null ? NotFound() : Ok(UserDto.FromEntity(user));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentUserAsync(cancellationToken);

        return user == null ? Unauthorized() : Ok(UserDto.FromEntity(user));
    }
}

