namespace HabitTracker.Api.Dtos;

public sealed record UserRolesDto(
    Guid UserId,
    IReadOnlyCollection<string> Roles);
