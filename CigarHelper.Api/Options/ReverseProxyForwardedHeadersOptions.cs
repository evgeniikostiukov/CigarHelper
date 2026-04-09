namespace CigarHelper.Api.Options;

/// <summary>
/// Доверенный reverse proxy: X-Forwarded-For / Proto / Host (см. <see cref="Microsoft.AspNetCore.Builder.ForwardedHeadersExtensions"/>).
/// Без явных <see cref="KnownProxyAddresses"/> после включения доверяются только loopback (безопасный дефолт для локальных сценариев).
/// </summary>
public sealed class ReverseProxyForwardedHeadersOptions
{
    public const string SectionName = "ForwardedHeaders";

    public bool Enabled { get; set; }

    /// <summary>IPv4/IPv6 доверенного прокси (например внутренний IP ingress).</summary>
    public string[] KnownProxyAddresses { get; set; } = [];

    public int ForwardLimit { get; set; } = 1;

    /// <summary>
    /// Если true — в список доверенных сетей forwarded headers добавляются частные диапазоны (10/8, 172.16/12, 192.168/16),
    /// чтобы запросы от контейнера nginx в Docker bridge к API принимали X-Forwarded-*.
    /// Включайте только когда до API нет прямого доступа из интернета, только через edge TLS и внутреннюю сеть compose.
    /// </summary>
    public bool TrustPrivateNetworks { get; set; }
}
