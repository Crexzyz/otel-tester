using Microsoft.AspNetCore.Mvc;
using OtelTester.Api.Middleware;

namespace OtelTester.Api.Controllers;

[ApiController]
public abstract class OtelTesterController(ILogger logger, IHttpClientFactory httpClientFactory) : ControllerBase
{
    /// <summary>
    /// Logger instance for the controller.
    /// </summary>
    protected readonly ILogger Logger = logger;

    /// <summary>
    /// HTTP client that injects correlation IDs (if any) into outgoing requests.
    /// </summary>
    protected readonly HttpClient HttpClient = httpClientFactory.CreateClient(CorrelationIdDelegatingHandler.HttpClientName);
}
