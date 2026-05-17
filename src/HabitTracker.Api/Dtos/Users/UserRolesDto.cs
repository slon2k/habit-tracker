namespace HabitTracker.Api.Dtos.Users;

/// <summary>
/// DTO representing a user's assigned roles.
/// </summary>
/// <param name="UserId">User unique identifier.</param>
/// <param name="Roles">List of assigned roles.</param>
public sealed record UserRolesDto(
    Guid UserId,
    IReadOnlyCollection<string> Roles);
