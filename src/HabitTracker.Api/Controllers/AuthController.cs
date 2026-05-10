namespace HabitTracker.Api.Controllers;

using HabitTracker.Api.Data;
using HabitTracker.Api.Dtos.Auth;
using HabitTracker.Api.Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    ApplicationDbContext applicationDbContext) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto registerUserDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registerUserDto);

        var identityUser = new IdentityUser
        {
            UserName = registerUserDto.Email,
            Email = registerUserDto.Email,
        };

        var result = await userManager.CreateAsync(identityUser, registerUserDto.Password);

        if (!result.Succeeded)
        {
            var errors = new Dictionary<string, object?> { { "errors", result.Errors.ToDictionary(e => e.Code, e => e.Description) } };
            return Problem(detail: "User registration failed.", statusCode: StatusCodes.Status400BadRequest, title: "Registration Failed", extensions: errors);
        }

        var appUser = new User(Guid.NewGuid(), identityUser.Id, registerUserDto.Name, registerUserDto.Email);
        applicationDbContext.Users.Add(appUser);

        try
        {
            await applicationDbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await userManager.DeleteAsync(identityUser);
            throw;
        }

        return CreatedAtAction(null, new { id = appUser.Id }, new { appUser.Id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loginDto);

        var result = await signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password, isPersistent: false, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        var identityUser = await userManager.FindByEmailAsync(loginDto.Email);

        if (identityUser == null)
        {
            await signInManager.SignOutAsync();
            return Unauthorized();
        }

        var appUser = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.IdentityId == identityUser.Id, cancellationToken);

        if (appUser == null)
        {
            await signInManager.SignOutAsync();
            return Unauthorized();
        }

        return Ok(new { appUser.Id });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }
}