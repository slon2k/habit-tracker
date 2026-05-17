using HabitTracker.Api.Data;
using HabitTracker.Api.Dtos.Users;
using HabitTracker.Api.Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Api.Controllers;

[Authorize(Roles = nameof(AppRole.Admin))]
[Route("api/users/{userId:guid}/roles")]
public sealed class UserRolesController(
    ApplicationDbContext applicationDbContext,
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager) : BaseApiController(applicationDbContext)
{
    /// <summary>
    /// Lists all roles assigned to a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 OK with roles, or 404 if user not found.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(UserRolesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListUserRoles(Guid userId, CancellationToken cancellationToken)
    {
        var identityUser = await FindIdentityUserByAppUserIdAsync(userId, cancellationToken);
        if (identityUser == null)
        {
            return NotFound();
        }

        var roles = await userManager.GetRolesAsync(identityUser);
        var dto = new UserRolesDto(userId, [.. roles]);

        return Ok(dto);
    }

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="role">The role to assign.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 OK if assigned, or 404 if user not found.</returns>
    [HttpPut("{role}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, AppRole role, CancellationToken cancellationToken)
    {
        var identityUser = await FindIdentityUserByAppUserIdAsync(userId, cancellationToken);
        if (identityUser == null)
        {
            return NotFound();
        }

        var roleName = role.ToString();
        var identityRole = await FindRoleByNameAsync(roleName, cancellationToken);
        if (identityRole == null)
        {
            return NotFound(new { error = $"Role '{roleName}' does not exist." });
        }

        if (await userManager.IsInRoleAsync(identityUser, identityRole.Name!))
        {
            return NoContent();
        }

        var result = await userManager.AddToRoleAsync(identityUser, identityRole.Name!);
        
        return !result.Succeeded
            ? Problem(
                detail: "Could not assign role.",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?>
                {
                    ["errors"] = result.Errors.Select(e => e.Description).ToArray(),
                })
            : NoContent();
    }

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="role">The role to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 OK if removed, or 404 if user not found.</returns>
    [HttpDelete("{role}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveRoleFromUser(Guid userId, AppRole role, CancellationToken cancellationToken)
    {
        var identityUser = await FindIdentityUserByAppUserIdAsync(userId, cancellationToken);
        if (identityUser == null)
        {
            return NotFound();
        }

        var roleName = role.ToString();
        var identityRole = await FindRoleByNameAsync(roleName, cancellationToken);
        if (identityRole is null)
        {
            return NotFound(new { error = $"Role '{roleName}' does not exist." });
        }

        if (!await userManager.IsInRoleAsync(identityUser, identityRole.Name!))
        {
            return NoContent();
        }

        var result = await userManager.RemoveFromRoleAsync(identityUser, identityRole.Name!);

        return !result.Succeeded
            ? Problem(
                detail: "Could not remove role.",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?>
                {
                    ["errors"] = result.Errors.Select(e => e.Description).ToArray(),
                })
            : NoContent();
    }

    private async Task<IdentityUser?> FindIdentityUserByAppUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var appUser = await ApplicationDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return appUser == null ? null : await userManager.FindByIdAsync(appUser.IdentityId);
    }

    private async Task<IdentityRole?> FindRoleByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        var normalizedRoleName = roleName.Trim().ToUpperInvariant();

        return await roleManager.Roles.FirstOrDefaultAsync(
            r => r.NormalizedName == normalizedRoleName,
            cancellationToken);
    }
}
