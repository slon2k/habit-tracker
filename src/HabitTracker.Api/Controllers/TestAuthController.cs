namespace HabitTracker.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/test")]
public sealed class TestAuthController : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult PublicEndpoint()
    {
        return Ok(new { message = "This is public" });
    }

    [HttpGet("protected")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public IActionResult ProtectedEndpoint()
    {
        var principal = HttpContext.User;
        var isAuthenticated = principal?.Identity?.IsAuthenticated ?? false;
        var authType = principal?.Identity?.AuthenticationType ?? "none";
        var claims = principal?.Claims?.Select(c => new { c.Type, c.Value }).ToList() ?? new();
        var subClaim = principal?.FindFirst("sub")?.Value;

        return Ok(new
        {
            isAuthenticated,
            authType,
            claimCount = claims.Count,
            claims,
            subClaim
        });
    }
}
