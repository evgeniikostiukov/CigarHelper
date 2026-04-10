namespace CigarHelper.Api.Observability;

/// <summary>Снимок состояния метрик на момент запроса.</summary>
public sealed record MetricsSnapshot(
    long TotalRequests,
    long TotalErrors,
    double TotalDurationMs);

/// <summary>Интерфейс сборщика базовых HTTP-метрик (запросы, ошибки, длительность).</summary>
public interface IMetricsCollector
{
    /// <summary>Записывает итог одного HTTP-запроса.</summary>
    void Record(int statusCode, double durationMs);

    /// <summary>Возвращает текущий снимок накопленных метрик.</summary>
    MetricsSnapshot Snapshot();
}
