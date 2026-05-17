namespace HabitTracker.Api.Dtos.Users;

/// <summary>
/// User DTO for API responses.
/// </summary>
/// <param name="Id">User unique identifier.</param>
/// <param name="Email">User email address.</param>
/// <param name="Name">User display name.</param>
public sealed record UserDto(
    Guid Id,
    string Email,
    string Name)
{
    public static UserDto FromEntity(Entities.User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return new UserDto(user.Id, user.Email, user.Name);
    }
}
