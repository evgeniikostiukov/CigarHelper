using System.Diagnostics;
using CigarHelper.Api.Observability;

namespace CigarHelper.Api.Middleware;

/// <summary>
/// Замеряет длительность каждого HTTP-запроса и передаёт результат в IMetricsCollector.
/// Должен располагаться после CorrelationIdMiddleware.
/// </summary>
public sealed class RequestMetricsMiddleware(RequestDelegate next, IMetricsCollector metrics)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            sw.Stop();
            metrics.Record(context.Response.StatusCode, sw.Elapsed.TotalMilliseconds);
        }
    }
}
