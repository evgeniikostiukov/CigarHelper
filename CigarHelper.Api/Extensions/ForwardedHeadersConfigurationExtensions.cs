using System.Net;
using CigarHelper.Api.Options;
using Microsoft.AspNetCore.HttpOverrides;

namespace CigarHelper.Api.Extensions;

public static class ForwardedHeadersConfigurationExtensions
{
    public static IServiceCollection AddCigarForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(ReverseProxyForwardedHeadersOptions.SectionName);
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            var cfg = section.Get<ReverseProxyForwardedHeadersOptions>() ?? new ReverseProxyForwardedHeadersOptions();
            if (!cfg.Enabled)
            {
                options.ForwardedHeaders = ForwardedHeaders.None;
                return;
            }

            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto
                | ForwardedHeaders.XForwardedHost;

            var limit = cfg.ForwardLimit < 1 ? 1 : cfg.ForwardLimit;
            options.ForwardLimit = limit;

            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();

            foreach (var s in cfg.KnownProxyAddresses)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;
                if (IPAddress.TryParse(s.AsSpan().Trim(), out var ip))
                    options.KnownProxies.Add(ip);
            }

            if (cfg.TrustPrivateNetworks)
            {
                options.KnownIPNetworks.Add(new IPNetwork(IPAddress.Parse("10.0.0.0"), 8));
                options.KnownIPNetworks.Add(new IPNetwork(IPAddress.Parse("172.16.0.0"), 12));
                options.KnownIPNetworks.Add(new IPNetwork(IPAddress.Parse("192.168.0.0"), 16));
            }

            if (options.KnownProxies.Count == 0)
            {
                options.KnownProxies.Add(IPAddress.Loopback);
                options.KnownProxies.Add(IPAddress.IPv6Loopback);
            }
        });

        return services;
    }
}
