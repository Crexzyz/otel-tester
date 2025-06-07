namespace OtelTester.Api.Middleware;

/// <summary>
/// Delegating handler for adding correlation IDs to outgoing
/// HTTP requests.
/// </summary>
/// <param name="httpContextAccessor">For accessing the current HTTP context.</param>
public class CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    /// <summary>
    /// Name of the HTTP client used for correlation ID handling.
    /// </summary>
    public const string HttpClientName = "CorrelationIdHttpClient";

    /// <summary>
    /// For modifying outgoing HTTP requests.
    /// </summary>
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Adds the correlation ID to the outgoing request headers if it exists
    /// and is not empty.
    /// </summary>
    /// <param name="request">The request to add the correlation ID.</param>
    private void AddCorrelationId(HttpRequestMessage request)
    {
        HttpContext? context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return;
        }

        if (!context.Items.TryGetValue(CorrelationIdMiddleware.CorrelationIdHeader, out object? correlationIdObj))
        {
            return;
        }

        if (correlationIdObj is null)
        {
            return;
        }

        string correlationId = correlationIdObj.ToString()!;
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            return;
        }

        request.Headers.Add(CorrelationIdMiddleware.CorrelationIdHeader, correlationId);
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        AddCorrelationId(request);
        return base.SendAsync(request, cancellationToken);
    }
}
