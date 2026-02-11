using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace eShopServer.Middleware;

/// <summary>
/// Standardised error response returned by the API for all error cases.
/// </summary>
public record ApiErrorResponse(
    int StatusCode,
    string Error,
    string Message,
    string? Details = null,
    string? TraceId = null,
    DateTime? Timestamp = null
);

/// <summary>
/// Global exception-handling middleware that catches all unhandled exceptions
/// and converts them into a consistent JSON error response.
///
/// Mapping:
///   ArgumentException / ArgumentNullException      → 400 Bad Request
///   KeyNotFoundException                           → 404 Not Found
///   UnauthorizedAccessException                    → 401 Unauthorized
///   InvalidOperationException                      → 409 Conflict
///   NotImplementedException                        → 501 Not Implemented
///   DbUpdateConcurrencyException                   → 409 Conflict
///   DbUpdateException (with MySQL error mapping)   → 400 / 409 / 422 / 500
///   Everything else                                → 500 Internal Server Error
/// </summary>
public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(
        RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorType, userMessage) = MapException(exception);

        // ── Logging ──
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}",
                context.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(exception, "{ErrorType}: {Message}. TraceId: {TraceId}",
                errorType, exception.Message, context.TraceIdentifier);
        }

        // ── Build client-safe error response ──
        var response = new ApiErrorResponse(
            StatusCode: (int)statusCode,
            Error: errorType,
            Message: userMessage,
            Details: _env.IsDevelopment() ? exception.ToString() : null,
            TraceId: context.TraceIdentifier,
            Timestamp: DateTime.UtcNow
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    // ── Central exception → (StatusCode, ErrorType, FriendlyMessage) mapper ──
    private static (HttpStatusCode StatusCode, string ErrorType, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            // ── Application-level exceptions ──
            ArgumentNullException ex =>
                (HttpStatusCode.BadRequest, "BAD_REQUEST", ex.Message),

            ArgumentException ex =>
                (HttpStatusCode.BadRequest, "BAD_REQUEST", ex.Message),

            KeyNotFoundException ex =>
                (HttpStatusCode.NotFound, "NOT_FOUND", ex.Message),

            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "UNAUTHORIZED", "You are not authorized to perform this action."),

            InvalidOperationException ex =>
                (HttpStatusCode.Conflict, "CONFLICT", ex.Message),

            NotImplementedException =>
                (HttpStatusCode.NotImplemented, "NOT_IMPLEMENTED", "This feature is not yet available."),

            // ── EF Core concurrency conflict ──
            DbUpdateConcurrencyException =>
                (HttpStatusCode.Conflict, "CONFLICT",
                 "The record was modified by another user. Please refresh and try again."),

            // ── EF Core database update errors (constraint violations, etc.) ──
            DbUpdateException dbEx => MapDbUpdateException(dbEx),

            // ── Catch-all ──
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_SERVER_ERROR",
                  "An unexpected error occurred. Please try again later.")
        };
    }

    /// <summary>
    /// Inspects the inner exception of a DbUpdateException to extract MySQL-specific
    /// error codes and return a user-friendly message.
    /// </summary>
    private static (HttpStatusCode, string, string) MapDbUpdateException(DbUpdateException dbEx)
    {
        // Pomelo / MySqlConnector surfaces MySQL errors as MySqlException
        if (dbEx.InnerException is MySqlException mysqlEx)
        {
            return mysqlEx.ErrorCode switch
            {
                // 1062 – Duplicate entry (unique constraint violation)
                MySqlErrorCode.DuplicateKeyEntry =>
                    (HttpStatusCode.Conflict, "DUPLICATE_ENTRY",
                     "A record with the same unique value already exists. Please use a different value."),

                // 1451 – Cannot delete or update a parent row (FK constraint)
                MySqlErrorCode.RowIsReferenced2 =>
                    (HttpStatusCode.Conflict, "REFERENCE_CONFLICT",
                     "This item cannot be deleted or modified because it is referenced by other records."),

                // 1452 – Cannot add or update a child row (FK constraint – missing parent)
                MySqlErrorCode.NoReferencedRow2 =>
                    (HttpStatusCode.UnprocessableEntity, "INVALID_REFERENCE",
                     "The referenced item does not exist. Please check the related data and try again."),

                // 1406 – Data too long for column
                MySqlErrorCode.DataTooLong =>
                    (HttpStatusCode.BadRequest, "DATA_TOO_LONG",
                     "One or more fields exceed the maximum allowed length. Please shorten the input."),

                // 1048 – Column cannot be null
                MySqlErrorCode.ColumnCannotBeNull =>
                    (HttpStatusCode.BadRequest, "MISSING_REQUIRED_FIELD",
                     "A required field is missing. Please fill in all required fields and try again."),

                // 1364 – Field doesn't have a default value
                MySqlErrorCode.NoDefaultForField =>
                    (HttpStatusCode.BadRequest, "MISSING_REQUIRED_FIELD",
                     "A required field was not provided. Please fill in all required fields and try again."),

                // All other MySQL errors
                _ => (HttpStatusCode.InternalServerError, "DATABASE_ERROR",
                      "A database error occurred while processing your request. Please try again later.")
            };
        }

        // Non-MySQL DbUpdateException (shouldn't happen with Pomelo, but just in case)
        return (HttpStatusCode.InternalServerError, "DATABASE_ERROR",
                "A database error occurred while processing your request. Please try again later.");
    }
}

/// <summary>
/// Extension method to cleanly register the middleware in Program.cs.
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandler>();
    }
}

