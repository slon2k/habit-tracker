using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HabitTracker.Api.Middleware;

/// <summary>
/// Middleware for handling exceptions globally and converting them to ProblemDetails responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private static readonly Action<ILogger, string, Exception?> LogUnhandledException =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, nameof(ExceptionHandlingMiddleware)),
            "An unhandled exception has occurred: {ExceptionMessage}");

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            LogUnhandledException(_logger, exception.Message, exception);
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var problemDetails = exception switch
        {
            ArgumentException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Request",
                Detail = exception.Message,
                Type = "https://api.habittracker.com/errors/invalid-request"
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later.",
                Type = "https://api.habittracker.com/errors/internal-server-error"
            }
        };

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        return context.Response.WriteAsJsonAsync(problemDetails, options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    /// <summary>
    /// Adds exception handling middleware to the request pipeline.
    /// Must be called early in the middleware pipeline, ideally after logging setup.
    /// </summary>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
