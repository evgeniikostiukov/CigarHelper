using System.Text.Json;
using CigarHelper.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CigarHelper.Api.Middleware;

/// <summary>
/// Перехватывает все необработанные исключения, логирует с CorrelationId
/// и возвращает RFC 7807 Problem Details (application/problem+json).
///
/// Порядок в pipeline: после CorrelationIdMiddleware, до UseSerilogRequestLogging.
/// </summary>
public sealed class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger,
    IHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var correlationId = context.Items[CorrelationIdMiddleware.ItemsKey] as string ?? "n/a";
        var (status, title, detail) = MapException(ex, correlationId);

        logger.LogError(ex,
            "Unhandled exception. CorrelationId={CorrelationId} Status={Status} Detail={Detail}",
            correlationId, status, detail);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
        };
        problem.Extensions["correlationId"] = correlationId;

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problem, JsonOptions));
    }

    private (int Status, string Title, string Detail) MapException(Exception ex, string correlationId)
    {
        return ex switch
        {
            NotFoundException e => (404, "Not Found", e.Message),
            ForbiddenException e => (403, "Forbidden", e.Message),
            UnauthorizedAccessException e => (403, "Forbidden", e.Message),
            ConflictException e => (409, "Conflict", e.Message),
            ArgumentException e => (422, "Unprocessable Entity", e.Message),
            _ => (500, "Internal Server Error", environment.IsDevelopment()
                ? ex.Message
                : $"An unexpected error occurred. Reference: {correlationId}")
        };
    }
}
