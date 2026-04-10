namespace CigarHelper.Api.Middleware;

/// <summary>
/// Читает X-Correlation-ID из входящего запроса (или генерирует новый GUID),
/// сохраняет в HttpContext.Items["CorrelationId"] и возвращает в заголовке ответа.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-ID";
    public const string ItemsKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        context.Items[ItemsKey] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await next(context);
    }
}
