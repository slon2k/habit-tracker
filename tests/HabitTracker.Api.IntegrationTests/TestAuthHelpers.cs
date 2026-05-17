using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HabitTracker.Api.IntegrationTests;

internal static class TestAuthHelpers
{
    internal static async Task<string> RegisterAndLoginAsync(HttpClient client)
    {
        var login = await RegisterAndLoginWithTokensAsync(client);
        return login.AccessToken;
    }

    internal static async Task<LoginTokens> RegisterAndLoginWithTokensAsync(HttpClient client)
    {
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

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        using var json = await ReadJsonAsync(loginResponse);
        var accessToken = json.RootElement.GetProperty("accessToken").GetString();
        var refreshToken = json.RootElement.GetProperty("refreshToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshToken));

        return new LoginTokens(accessToken!, refreshToken!);
    }

    internal static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content);
    }
}

internal sealed record LoginTokens(string AccessToken, string RefreshToken);
