using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HabitTracker.Api.IntegrationTests;

public sealed class AuthAndHabitsIntegrationTests(IntegrationTestWebApplicationFactory factory)
    : IClassFixture<IntegrationTestWebApplicationFactory>
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

        using var loginJson = await ReadJsonAsync(loginResponse);
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
        using var refreshJson = await ReadJsonAsync(refreshResponse);
        Assert.False(string.IsNullOrWhiteSpace(refreshJson.RootElement.GetProperty("accessToken").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(refreshJson.RootElement.GetProperty("refreshToken").GetString()));
    }

    [Fact]
    public async Task GetHabits_WhenAuthenticatedAndFiltered_ThenReturnsExpectedItems()
    {
        // Arrange
        using var client = factory.CreateClient();
        var token = await RegisterAndLoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createReadResponse = await client.PostAsJsonAsync("/api/habits", new
        {
            Name = "Read docs",
            Description = "Read architecture docs",
            Type = "Binary",
            Frequency = new
            {
                Type = "Daily",
                TimesPerPeriod = 1
            }
        });

        var createRunResponse = await client.PostAsJsonAsync("/api/habits", new
        {
            Name = "Run",
            Description = "Run 5km",
            Type = "Binary",
            Frequency = new
            {
                Type = "Daily",
                TimesPerPeriod = 1
            }
        });

        // Act
        var listResponse = await client.GetAsync(new Uri("/api/habits?search=Read&pageNumber=1&pageSize=10", UriKind.Relative));

        // Assert setup
        Assert.Equal(HttpStatusCode.Created, createReadResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, createRunResponse.StatusCode);

        // Assert list
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        using var json = await ReadJsonAsync(listResponse);
        var items = json.RootElement.GetProperty("items");

        Assert.Equal(JsonValueKind.Array, items.ValueKind);
        Assert.Single(items.EnumerateArray());
        Assert.Equal("Read docs", items[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetHabits_WhenUnauthenticated_ThenReturnsUnauthorized()
    {
        // Arrange
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync(new Uri("/api/habits", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<string> RegisterAndLoginAsync(HttpClient client)
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
        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        return accessToken!;
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content);
    }
}
