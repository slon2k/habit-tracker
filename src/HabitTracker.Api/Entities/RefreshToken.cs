namespace HabitTracker.Api.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; private set; }

    public string UserId { get; private set; } = string.Empty;

    public required string Token { get; set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    // Constructor for EF
    private RefreshToken()
    {
    }

    /// <summary>
    /// Creates a new refresh token.
    /// </summary>
    public RefreshToken(string token, DateTime expires, string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        Id = Guid.NewGuid();
        Token = token;
        ExpiresAtUtc = expires;
        CreatedAtUtc = DateTime.UtcNow;
        UserId = userId.Trim();
    }
}