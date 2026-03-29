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
}
