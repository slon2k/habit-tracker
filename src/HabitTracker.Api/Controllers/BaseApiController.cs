using HabitTracker.Api.Data;
using HabitTracker.Api.Dtos.Common;
using HabitTracker.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

namespace HabitTracker.Api.Controllers;

/// <summary>
/// Base controller providing common authentication and authorization utilities for API controllers.
/// </summary>
[ApiController]
public abstract class BaseApiController(ApplicationDbContext applicationDbContext) : ControllerBase
{
    protected ApplicationDbContext ApplicationDbContext { get; } = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));

    /// <summary>
    /// Resolves the current authenticated user from JWT claims.
    /// Tries multiple claim types to handle different JWT claim mapping strategies.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The current user entity, or null if user claims are invalid or user not found in database.</returns>
    protected async Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (HttpContext.User is not ClaimsPrincipal principal)
        {
            return null;
        }

        // Try app_user_id claim first (directly maps to User.Id)
        var appUserIdClaim = principal.FindFirstValue("app_user_id");
        if (Guid.TryParse(appUserIdClaim, out var appUserId))
        {
            var userByAppId = await ApplicationDbContext.Users.FindAsync([appUserId], cancellationToken);
            if (userByAppId != null)
            {
                return userByAppId;
            }
        }

        // Fall back to identity claims (mapped to ClaimTypes.NameIdentifier or raw "sub")
        // JwtBearer may map "sub" to ClaimTypes.NameIdentifier depending on configuration
        var identityId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(identityId))
        {
            return null;
        }

        var userByIdentityId = await ApplicationDbContext.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId, cancellationToken);
        return userByIdentityId;
    }

    /// <summary>
    /// Resolves the current authenticated user and returns 401 Unauthorized if not found.
    /// Useful for quick inline authorization checks in controller actions.
    /// </summary>
    /// <returns>The current user entity, or an Unauthorized IActionResult if resolution fails.</returns>
    protected async Task<(User? user, IActionResult? error)> GetCurrentUserOrUnauthorizedAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        if (user == null)
        {
            return (null, Unauthorized());
        }
        return (user, null);
    }

    protected string CreateRouteLink(string actionName, object? routeValues) =>
        Url.Action(actionName, values: routeValues) 
            ?? throw new InvalidOperationException($"Unable to generate route link for action '{actionName}'.");

    protected bool AcceptsHalJson() => Request.GetTypedHeaders().Accept switch
    {
        null => false,
        { Count: 0 } => false,
        var acceptedMediaTypes => acceptedMediaTypes.Any(mediaType =>
            string.Equals(mediaType.MediaType.Value, "application/hal+json", StringComparison.OrdinalIgnoreCase))
    };
}
