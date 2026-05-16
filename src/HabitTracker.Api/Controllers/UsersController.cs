using HabitTracker.Api.Data;
using HabitTracker.Api.Dtos;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HabitTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public sealed class UsersController(ApplicationDbContext applicationDbContext) : ControllerBase
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
        if (HttpContext.User is not ClaimsPrincipal principal)
        {
            return Unauthorized();
        }

        var appUserIdClaim = principal.FindFirstValue("app_user_id");

        if (Guid.TryParse(appUserIdClaim, out var appUserId))
        {
            var userByAppId = applicationDbContext.Users.FirstOrDefault(u => u.Id == appUserId);

            if (userByAppId != null)
            {
                return Ok(UserDto.FromEntity(userByAppId));
            }
        }

        var identityId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(identityId))
        {
            return Unauthorized();
        }

        var user = applicationDbContext.Users.FirstOrDefault(u => u.IdentityId == identityId);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(UserDto.FromEntity(user));
    }
}

