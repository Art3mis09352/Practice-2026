using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Practice.Infrastructure;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = MapException(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception with status code {StatusCode}", statusCode);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private static (int statusCode, string title, string detail) MapException(Exception exception)
    {
        return exception switch
        {
            InvalidOperationException ex => (
                StatusCodes.Status400BadRequest,
                "Business rule violation",
                ex.Message),

            ArgumentException ex => (
                StatusCodes.Status400BadRequest,
                "Invalid argument",
                ex.Message),

            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                ex.Message),

            KeyNotFoundException ex => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                ex.Message),

            UnauthorizedAccessException ex => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                ex.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Server error",
                "Произошла непредвиденная ошибка.")
        };
    }
}
