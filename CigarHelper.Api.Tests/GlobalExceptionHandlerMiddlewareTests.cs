using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CigarHelper.Api.Exceptions;
using CigarHelper.Api.Middleware;
using Xunit;

namespace CigarHelper.Api.Tests;

/// <summary>
/// GlobalExceptionHandlerMiddleware:
/// - NotFoundException       → 404
/// - ForbiddenException      → 403
/// - ConflictException       → 409
/// - ArgumentException       → 422
/// - UnauthorizedAccessException → 403
/// - прочие                  → 500
/// - ответ всегда application/problem+json
/// - тело содержит correlationId из HttpContext.Items
/// </summary>
public class GlobalExceptionHandlerMiddlewareTests
{
    private static IHost BuildHost(Exception exceptionToThrow, string environment = "Production")
    {
        return new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.UseEnvironment(environment);
                webBuilder.ConfigureServices(services =>
                    services.AddProblemDetails());
                webBuilder.Configure(app =>
                {
                    app.UseMiddleware<CorrelationIdMiddleware>();
                    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
                    app.Run(_ => throw exceptionToThrow);
                });
            })
            .Build();
    }

    private static async Task<(HttpStatusCode Status, JsonElement Body)> SendGetAsync(IHost host, string? correlationId = null)
    {
        var client = host.GetTestClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        if (correlationId != null)
            request.Headers.Add("X-Correlation-ID", correlationId);
        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (response.StatusCode, body);
    }

    [Fact]
    public async Task NotFoundException_Returns404WithProblemDetails()
    {
        using var host = BuildHost(new NotFoundException("Cigar not found"));
        await host.StartAsync();

        var (status, body) = await SendGetAsync(host);

        Assert.Equal(HttpStatusCode.NotFound, status);
        Assert.Equal(404, body.GetProperty("status").GetInt32());
        Assert.Contains("not found", body.GetProperty("detail").GetString()!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ForbiddenException_Returns403WithProblemDetails()
    {
        using var host = BuildHost(new ForbiddenException("Access denied"));
        await host.StartAsync();

        var (status, body) = await SendGetAsync(host);

        Assert.Equal(HttpStatusCode.Forbidden, status);
        Assert.Equal(403, body.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task ConflictException_Returns409WithProblemDetails()
    {
        using var host = BuildHost(new ConflictException("Already exists"));
        await host.StartAsync();

        var (status, body) = await SendGetAsync(host);

        Assert.Equal(HttpStatusCode.Conflict, status);
        Assert.Equal(409, body.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task ArgumentException_Returns422WithProblemDetails()
    {
        using var host = BuildHost(new ArgumentException("Invalid param"));
        await host.StartAsync();

        var (status, body) = await SendGetAsync(host);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Equal(422, body.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task UnauthorizedAccessException_Returns403WithProblemDetails()
    {
        using var host = BuildHost(new UnauthorizedAccessException("Forbidden"));
        await host.StartAsync();

        var (status, body) = await SendGetAsync(host);

        Assert.Equal(HttpStatusCode.Forbidden, status);
        Assert.Equal(403, body.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task UnhandledException_Returns500WithProblemDetails()
    {
        using var host = BuildHost(new InvalidOperationException("Something broke"));
        await host.StartAsync();

        var (status, body) = await SendGetAsync(host);

        Assert.Equal(HttpStatusCode.InternalServerError, status);
        Assert.Equal(500, body.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task ErrorResponse_ContainsCorrelationId()
    {
        using var host = BuildHost(new NotFoundException("Missing"));
        await host.StartAsync();
        var expectedCid = "test-correlation-id";

        var (_, body) = await SendGetAsync(host, correlationId: expectedCid);

        // ProblemDetails.Extensions помечен [JsonExtensionData] — поля выводятся на корневой уровень JSON
        Assert.Equal(expectedCid, body.GetProperty("correlationId").GetString());
    }

    [Fact]
    public async Task InProduction_UnhandledExceptionDoesNotLeakDetails()
    {
        using var host = BuildHost(new InvalidOperationException("internal secret"), "Production");
        await host.StartAsync();

        var (_, body) = await SendGetAsync(host);

        var detail = body.TryGetProperty("detail", out var d) ? d.GetString() : null;
        Assert.DoesNotContain("internal secret", detail ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ContentType_IsProblemJson()
    {
        using var host = BuildHost(new NotFoundException("x"));
        await host.StartAsync();
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
