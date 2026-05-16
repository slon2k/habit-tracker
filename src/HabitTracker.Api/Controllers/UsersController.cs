using HabitTracker.Api.Data;
using HabitTracker.Api.Dtos;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Api.Controllers;

[Authorize]
[Route("api/users")]
public sealed class UsersController(ApplicationDbContext applicationDbContext) : BaseApiController(applicationDbContext)
{
    [HttpGet("{id:guid}")]
    public IActionResult GetUserById(Guid id)
    {
        var user = ApplicationDbContext.Users.FirstOrDefault(u => u.Id == id);

        if (user == null)        
        {
            return NotFound();
        }

        return Ok(UserDto.FromEntity(user));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(UserDto.FromEntity(user));
    }
}

