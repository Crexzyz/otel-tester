using Microsoft.Extensions.Primitives;

namespace OtelTester.Api.Middleware;

/// <summary>
/// Middleware to handle correlation IDs in HTTP requests.
/// </summary>
/// <param name="next">The next middleware in the pipeline.</param>
/// <param name="logger">Logger instance for logging the ID.</param>
public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    /// <summary>
    /// HTTP header name for the correlation ID.
    /// </summary>
    public const string CorrelationIdHeader = "X-Correlation-ID";

    /// <summary>
    /// Next middleware in the pipeline.
    /// </summary>
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// Logger instance for logging the correlation ID.
    /// </summary>
    private readonly ILogger<CorrelationIdMiddleware> _logger = logger;

    /// <summary>
    /// Invokes the middleware to handle correlation IDs in HTTP requests.
    /// </summary>
    /// <param name="context">HTTP context of the request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId;

        // Check if the request has a correlation ID or create a new one
        // if not.
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues id) || string.IsNullOrWhiteSpace(id))
        {
            correlationId = Guid.NewGuid().ToString();
        }
        else
        {
            correlationId = id.ToString();
        }

        context.Response.Headers[CorrelationIdHeader] = correlationId;
        context.Items[CorrelationIdHeader] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationIdHeader] = correlationId
        }))
        {
            await _next(context);
        }
    }
}
