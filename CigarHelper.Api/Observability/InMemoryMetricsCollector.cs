using System.Threading;

namespace CigarHelper.Api.Observability;

/// <summary>
/// Потокобезопасный in-memory коллектор метрик на базе Interlocked-счётчиков.
/// Регистрируется как Singleton — хранит агрегат за время жизни процесса.
/// </summary>
public sealed class InMemoryMetricsCollector : IMetricsCollector
{
    private long _totalRequests;
    private long _totalErrors;
    private double _totalDurationMs;
    private readonly Lock _lock = new();

    public void Record(int statusCode, double durationMs)
    {
        Interlocked.Increment(ref _totalRequests);

        if (statusCode >= 500)
            Interlocked.Increment(ref _totalErrors);

        lock (_lock)
            _totalDurationMs += durationMs;
    }

    public MetricsSnapshot Snapshot()
    {
        var requests = Interlocked.Read(ref _totalRequests);
        var errors = Interlocked.Read(ref _totalErrors);
        double duration;
        lock (_lock)
            duration = _totalDurationMs;
        return new MetricsSnapshot(requests, errors, duration);
    }
}
