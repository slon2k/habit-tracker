using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HabitTracker.Api.IntegrationTests;

[Collection("IntegrationTests")]
public sealed class HabitsIntegrationTests(IntegrationTestWebApplicationFactory factory)
{
    [Fact]
    public async Task CreateHabit_WhenAuthenticated_ReturnsCreated()
    {
        // Arrange
        using var client = factory.CreateClient();
        var token = await TestAuthHelpers.RegisterAndLoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var createResponse = await client.PostAsJsonAsync("/api/habits", new
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

        // Assert
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
    }

    [Fact]
    public async Task GetHabits_WhenAuthenticated_ReturnsCreatedItem()
    {
        // Arrange
        using var client = factory.CreateClient();
        var token = await TestAuthHelpers.RegisterAndLoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await client.PostAsJsonAsync("/api/habits", new
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

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // Act
        var listResponse = await client.GetAsync(new Uri("/api/habits?pageNumber=1&pageSize=10", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        using var json = await TestAuthHelpers.ReadJsonAsync(listResponse);
        var items = json.RootElement.GetProperty("items");

        Assert.NotEmpty(items.EnumerateArray());
        Assert.Contains(items.EnumerateArray(), h => h.GetProperty("name").GetString() == "Read docs");
    }

    [Fact]
    public async Task GetHabits_WhenAuthenticatedAndFiltered_ThenReturnsExpectedItems()
    {
        // Arrange
        using var client = factory.CreateClient();
        var token = await TestAuthHelpers.RegisterAndLoginAsync(client);
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
        using var json = await TestAuthHelpers.ReadJsonAsync(listResponse);
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

    [Fact]
    public async Task GetHabits_WhenSortedByNameAsc_ThenReturnsAscendingOrder()
    {
        // Arrange
        using var client = factory.CreateClient();
        var token = await TestAuthHelpers.RegisterAndLoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createZulu = await client.PostAsJsonAsync("/api/habits", new
        {
            Name = "Zulu",
            Description = "Z item",
            Type = "Binary",
            Frequency = new
            {
                Type = "Daily",
                TimesPerPeriod = 1
            }
        });

        var createAlpha = await client.PostAsJsonAsync("/api/habits", new
        {
            Name = "Alpha",
            Description = "A item",
            Type = "Binary",
            Frequency = new
            {
                Type = "Daily",
                TimesPerPeriod = 1
            }
        });

        Assert.Equal(HttpStatusCode.Created, createZulu.StatusCode);
        Assert.Equal(HttpStatusCode.Created, createAlpha.StatusCode);

        // Act
        var listResponse = await client.GetAsync(new Uri("/api/habits?sort=name:asc&pageNumber=1&pageSize=10", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        using var json = await TestAuthHelpers.ReadJsonAsync(listResponse);
        var names = json.RootElement
            .GetProperty("items")
            .EnumerateArray()
            .Select(x => x.GetProperty("name").GetString())
            .Where(x => x is not null)
            .Cast<string>()
            .ToList();

        Assert.True(names.Count >= 2);
        Assert.Equal("Alpha", names[0]);
    }

    [Fact]
    public async Task CreateHabit_WhenPayloadIsInvalid_ReturnsBadRequestValidationProblem()
    {
        // Arrange
        using var client = factory.CreateClient();
        var token = await TestAuthHelpers.RegisterAndLoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PostAsJsonAsync("/api/habits", new
        {
            Name = "",
            Description = "Invalid",
            Type = "WrongType",
            Frequency = new
            {
                Type = "Daily",
                TimesPerPeriod = 0
            }
        });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.StartsWith("application/json", response.Content.Headers.ContentType?.MediaType, StringComparison.OrdinalIgnoreCase);

        using var json = await TestAuthHelpers.ReadJsonAsync(response);
        Assert.Equal(400, json.RootElement.GetProperty("status").GetInt32());
        Assert.True(json.RootElement.TryGetProperty("errors", out var errors));
        Assert.Equal(JsonValueKind.Object, errors.ValueKind);
    }
}
