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
    public IActionResult GetUserById(Guid id)
    {
        var user = ApplicationDbContext.Users.FirstOrDefault(u => u.Id == id);

        return user == null ? NotFound() : Ok(UserDto.FromEntity(user));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await GetCurrentUserAsync();
        
        return user == null ? Unauthorized() : Ok(UserDto.FromEntity(user));
    }
}

