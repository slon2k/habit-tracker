using HabitTracker.Api.Data;
using HabitTracker.Api.Dtos;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HabitTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public sealed class UserController(ApplicationDbContext applicationDbContext) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public IActionResult GetUserById(Guid id)
    {
        var user = applicationDbContext.Users.FirstOrDefault(u => u.Id == id);

        if (user == null)        
        {
            return NotFound();
        }

        return Ok(UserDto.FromEntity(user));
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var identityId = HttpContext.User?.FindFirst("sub")?.Value;

        if (identityId == null)
        {
            return Unauthorized();
        }

        var user = applicationDbContext.Users.FirstOrDefault(u => u.Id.ToString() == identityId);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(UserDto.FromEntity(user));
    }
}

