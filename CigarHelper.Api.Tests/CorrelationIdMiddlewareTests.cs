using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace CigarHelper.Api.Tests;

/// <summary>
/// CorrelationIdMiddleware:
/// - если X-Correlation-ID отсутствует → генерирует GUID и кладёт в ответ;
/// - если X-Correlation-ID передан в запросе → отражает его без изменений;
/// - записывает CorrelationId в HttpContext.Items["CorrelationId"].
/// </summary>
public class CorrelationIdMiddlewareTests
{
    private static IHost BuildHost(RequestDelegate? next = null)
    {
        return new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.Configure(app =>
                {
                    app.UseMiddleware<CigarHelper.Api.Middleware.CorrelationIdMiddleware>();

                    app.Run(async ctx =>
                    {
                        if (next != null)
                            await next(ctx);
                        else
                            ctx.Response.StatusCode = 200;
                    });
                });
            })
            .Build();
    }

    [Fact]
    public async Task WhenNoCorrelationIdHeader_ResponseContainsGeneratedGuid()
    {
        using var host = BuildHost();
        await host.StartAsync();
        var client = host.GetTestClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.True(Guid.TryParse(correlationId, out _), $"Expected GUID but got: {correlationId}");
    }

    [Fact]
    public async Task WhenCorrelationIdHeaderProvided_ResponseEchoesTheSameValue()
    {
        using var host = BuildHost();
        await host.StartAsync();
        var client = host.GetTestClient();
        var expectedId = "my-trace-id-123";

        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-Correlation-ID", expectedId);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.Equal(expectedId, correlationId);
    }

    [Fact]
    public async Task CorrelationId_IsStoredInHttpContextItems()
    {
        string? capturedId = null;

        using var host = BuildHost(ctx =>
        {
            capturedId = ctx.Items["CorrelationId"] as string;
            return Task.CompletedTask;
        });
        await host.StartAsync();
        var client = host.GetTestClient();
        var expectedId = "context-check-id";

        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-Correlation-ID", expectedId);
        await client.SendAsync(request);

        Assert.Equal(expectedId, capturedId);
    }
}
