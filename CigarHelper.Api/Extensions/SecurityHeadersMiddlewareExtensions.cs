namespace CigarHelper.Api.Extensions;

public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Базовые заголовки для снижения риска MIME-sniffing и встраивания в iframe (OWASP baseline для HTTP API).
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            await next();
        });
    }
}
