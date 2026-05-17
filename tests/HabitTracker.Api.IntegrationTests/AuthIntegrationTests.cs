using System.Net;
using System.Net.Http.Json;

namespace HabitTracker.Api.IntegrationTests;

[Collection("IntegrationTests")]
public sealed class AuthIntegrationTests(IntegrationTestWebApplicationFactory factory)
{
    [Fact]
    public async Task AuthFlow_WhenRegisterLoginRefresh_ThenReturnsTokens()
    {
        // Arrange
        using var client = factory.CreateClient();
        var email = $"{Guid.NewGuid():N}@example.com";
        const string password = "StrongPassword123!";

        // Act
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Name = "Integration User",
            Password = password,
            ConfirmPassword = password
        });

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        });

        // Assert register/login
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        using var loginJson = await TestAuthHelpers.ReadJsonAsync(loginResponse);
        var accessToken = loginJson.RootElement.GetProperty("accessToken").GetString();
        var refreshToken = loginJson.RootElement.GetProperty("refreshToken").GetString();

        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshToken));

        // Act refresh
        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });

        // Assert refresh
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        using var refreshJson = await TestAuthHelpers.ReadJsonAsync(refreshResponse);
        Assert.False(string.IsNullOrWhiteSpace(refreshJson.RootElement.GetProperty("accessToken").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(refreshJson.RootElement.GetProperty("refreshToken").GetString()));
    }

    [Fact]
    public async Task Login_WhenPasswordIsWrong_ReturnsUnauthorized()
    {
        // Arrange
        using var client = factory.CreateClient();
        var email = $"{Guid.NewGuid():N}@example.com";
        const string password = "StrongPassword123!";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = email,
            Name = "Integration User",
            Password = password,
            ConfirmPassword = password
        });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        // Act
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = "WrongPassword123!"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_WhenRefreshTokenIsInvalid_ReturnsUnauthorized()
    {
        // Arrange
        using var client = factory.CreateClient();
        var login = await TestAuthHelpers.RegisterAndLoginWithTokensAsync(client);

        // Act
        var refreshResponse = await client.PostAsJsonAsync("/api/auth/refresh", new
        {
            AccessToken = login.AccessToken,
            RefreshToken = "invalid-refresh-token"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }
}
