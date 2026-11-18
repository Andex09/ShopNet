using Microsoft.AspNetCore.Diagnostics;
using ShopNet.Application.Common;
using ShopNet.Domain.Common;

namespace ShopNet.Api.Infrastructure;

/// <summary>
/// Translates exceptions into problem details. Expected errors keep their (safe) message; anything else becomes a
/// generic 500 and is logged with the full exception, never returned to the client.
/// </summary>
internal sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            NotFoundException e => (StatusCodes.Status404NotFound, "Resource not found", e.Message),
            ConflictException e => (StatusCodes.Status409Conflict, "Conflict", e.Message),
            DomainException e => (StatusCodes.Status400BadRequest, "Business rule violated", e.Message),
            BadHttpRequestException e => (e.StatusCode, "Bad request", "The request could not be read."),
            OperationCanceledException when httpContext.RequestAborted.IsCancellationRequested =>
                (499, "Request cancelled", (string?)null),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", null),
        };

        if (status >= StatusCodes.Status500InternalServerError)
        {
            LogUnhandledException(logger, exception);
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = { Status = status, Title = title, Detail = detail },
        });
    }

    [LoggerMessage(EventId = 5000, Level = LogLevel.Error, Message = "Unhandled exception")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception);
}
