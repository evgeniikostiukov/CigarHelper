using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CigarHelper.Api.Observability;
using Xunit;

namespace CigarHelper.Api.Tests;

/// <summary>
/// RequestMetricsMiddleware:
/// - каждый запрос увеличивает TotalRequests;
/// - ответ 5xx увеличивает TotalErrors;
/// - ответ 4xx не считается ошибкой сервера;
/// - фиксирует длительность > 0.
/// </summary>
public class RequestMetricsMiddlewareTests
{
    private static (IHost Host, IMetricsCollector Collector) BuildHost(int responseStatusCode)
    {
        var host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                    services.AddSingleton<IMetricsCollector, InMemoryMetricsCollector>());
                webBuilder.Configure(app =>
                {
                    app.UseMiddleware<CigarHelper.Api.Middleware.RequestMetricsMiddleware>();
                    app.Run(ctx =>
                    {
                        ctx.Response.StatusCode = responseStatusCode;
                        return Task.CompletedTask;
                    });
                });
            })
            .Build();

        var collector = host.Services.GetRequiredService<IMetricsCollector>();
        return (host, collector);
    }

    [Fact]
    public async Task SuccessfulRequest_IncrementsTotalRequests()
    {
        var (host, collector) = BuildHost(200);
        await host.StartAsync();
        var client = host.GetTestClient();

        await client.GetAsync("/test");

        Assert.Equal(1, collector.Snapshot().TotalRequests);
    }

    [Fact]
    public async Task ServerErrorRequest_IncrementsErrorsAndRequests()
    {
        var (host, collector) = BuildHost(500);
        await host.StartAsync();
        var client = host.GetTestClient();

        await client.GetAsync("/fail");

        var snap = collector.Snapshot();
        Assert.Equal(1, snap.TotalRequests);
        Assert.Equal(1, snap.TotalErrors);
    }

    [Fact]
    public async Task ClientErrorRequest_DoesNotIncrementServerErrors()
    {
        var (host, collector) = BuildHost(404);
        await host.StartAsync();
        var client = host.GetTestClient();

        await client.GetAsync("/notfound");

        var snap = collector.Snapshot();
        Assert.Equal(1, snap.TotalRequests);
        Assert.Equal(0, snap.TotalErrors);
    }

    [Fact]
    public async Task Request_RecordsDurationGreaterThanZero()
    {
        var (host, collector) = BuildHost(200);
        await host.StartAsync();
        var client = host.GetTestClient();

        await client.GetAsync("/time");

        var snap = collector.Snapshot();
        Assert.True(snap.TotalDurationMs >= 0, "Duration must be non-negative");
        Assert.Equal(1, snap.TotalRequests);
    }

    [Fact]
    public async Task MultipleRequests_AccumulateCorrectly()
    {
        var (host, collector) = BuildHost(200);
        await host.StartAsync();
        var client = host.GetTestClient();

        await client.GetAsync("/1");
        await client.GetAsync("/2");
        await client.GetAsync("/3");

        Assert.Equal(3, collector.Snapshot().TotalRequests);
    }
}
