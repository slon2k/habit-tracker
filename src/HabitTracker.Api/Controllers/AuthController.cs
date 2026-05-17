namespace HabitTracker.Api.Controllers;

using HabitTracker.Api.Data;
using HabitTracker.Api.Dtos.Auth;
using HabitTracker.Api.Entities;
using HabitTracker.Api.Services.Auth;

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
    ITokenService tokenService,
    ApplicationDbContext applicationDbContext) : ControllerBase
{
    /// <summary>
    /// Registers a new user with email, password, and name.
    /// </summary>
    /// <param name="registerUserDto">The registration request DTO.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 Created with user ID, or 400 if registration fails.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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


        return CreatedAtRoute("GetUserById", new { id = appUser.Id }, new { appUser.Id });
    }

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    /// <param name="loginDto">The login request DTO.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 OK with tokens, or 401 if authentication fails.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loginDto);

        var identityUser = await userManager.FindByEmailAsync(loginDto.Email);

        if (identityUser == null)
        {
            return Unauthorized();
        }
        var result = await signInManager.CheckPasswordSignInAsync(identityUser, loginDto.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        var appUser = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.IdentityId == identityUser.Id, cancellationToken);

        if (appUser == null)
        {
            return Unauthorized();
        }

        var tokenResult = await tokenService.CreateAccessToken(appUser.IdentityId, appUser.Id, appUser.Email);

        var dto = new LoginResultDto(
            tokenResult.AccessToken,
            tokenResult.AccessTokenExpiresAtUtc,
            tokenResult.RefreshToken,
            tokenResult.RefreshTokenExpiresAtUtc);
        return Ok(dto);
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshTokenDto">The refresh token request DTO.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 OK with new tokens, or 401 if refresh fails.</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(refreshTokenDto);

        var identityUserId = tokenService.ValidateRefreshToken(refreshTokenDto.RefreshToken, refreshTokenDto.AccessToken);

        if (identityUserId == null)
        {
            return Unauthorized();
        }

        var identityUser = await userManager.FindByIdAsync(identityUserId);

        if (identityUser == null)
        {
            return Unauthorized();
        }

        var appUser = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.IdentityId == identityUser.Id, cancellationToken);

        if (appUser == null)
        {
            return Unauthorized();
        }

        var tokenResult = await tokenService.CreateAccessToken(appUser.IdentityId, appUser.Id, appUser.Email);

        var dto = new LoginResultDto(
            tokenResult.AccessToken,
            tokenResult.AccessTokenExpiresAtUtc,
            tokenResult.RefreshToken,
            tokenResult.RefreshTokenExpiresAtUtc);
        
        return Ok(dto);
    }
}