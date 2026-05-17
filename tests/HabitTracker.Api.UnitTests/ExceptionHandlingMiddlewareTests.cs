using System.Text.Json;
using HabitTracker.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace HabitTracker.Api.UnitTests;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenArgumentExceptionThrown_ReturnsBadRequestProblemDetails()
    {
        // Arrange
        RequestDelegate next = _ => throw new ArgumentException("Bad input");
        var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);

        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType, StringComparison.OrdinalIgnoreCase);

        var json = await ReadResponseJsonAsync(context);
        Assert.Equal("Invalid Request", json.RootElement.GetProperty("title").GetString());
        Assert.Equal("Bad input", json.RootElement.GetProperty("detail").GetString());
        Assert.Equal("https://api.habittracker.com/errors/invalid-request", json.RootElement.GetProperty("type").GetString());
        Assert.Equal(StatusCodes.Status400BadRequest, json.RootElement.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task InvokeAsync_WhenUnexpectedExceptionThrown_ReturnsInternalServerErrorProblemDetails()
    {
        // Arrange
        RequestDelegate next = _ => throw new InvalidOperationException("Sensitive details");
        var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);

        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType, StringComparison.OrdinalIgnoreCase);

        var json = await ReadResponseJsonAsync(context);
        Assert.Equal("Internal Server Error", json.RootElement.GetProperty("title").GetString());
        Assert.Equal("An unexpected error occurred. Please try again later.", json.RootElement.GetProperty("detail").GetString());
        Assert.Equal("https://api.habittracker.com/errors/internal-server-error", json.RootElement.GetProperty("type").GetString());
        Assert.Equal(StatusCodes.Status500InternalServerError, json.RootElement.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_PassesThroughWithoutWritingProblemDetails()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = context =>
        {
            nextCalled = true;
            context.Response.StatusCode = StatusCodes.Status204NoContent;
            return Task.CompletedTask;
        };

        var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    [Fact]
    public async Task InvokeAsync_WhenContextIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => middleware.InvokeAsync(null!));
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonDocument> ReadResponseJsonAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        return JsonDocument.Parse(body);
    }
}
