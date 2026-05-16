namespace HabitTracker.Api.Dtos.Users;

public sealed record UserRolesDto(
    Guid UserId,
    IReadOnlyCollection<string> Roles);
