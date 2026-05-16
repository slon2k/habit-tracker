namespace HabitTracker.Api.Dtos.Users;

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
