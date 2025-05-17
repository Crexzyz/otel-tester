using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OtelTester.Api.Metrics;

namespace OtelTester.Api.Controllers;

/// <summary>
/// Controller to simulate telemetry data.
/// </summary>
/// <param name="logger">Logger instance.</param>
/// <param name="jsonOptions">JSON deserializer options.</param>
[ApiController]
public class TelemetryController(ILogger<TelemetryController> logger, IOptions<JsonSerializerOptions> jsonOptions) : ControllerBase
{
    /// <summary>
    /// Default message to return when the simulation finishes.
    /// </summary>
    private const string s_defaultSuccessMessage = "Simulation completed successfully.";

    /// <summary>
    /// Route for the simulation endpoint.
    /// </summary>
    private const string s_route = "simulate";

    /// <summary>
    /// The global JSON deserializer options.
    /// </summary>
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value;

    /// <summary>
    /// Logger instance for the controller.
    /// </summary>
    private readonly ILogger<TelemetryController> _logger = logger;

    /// <summary>
    /// Validates if the status code can have a response body.
    /// </summary>
    /// <param name="statusCode">The status code to check.</param>
    /// <returns>True if approved by the HTTP spec, false otherwise.</returns>
    private static bool CanHaveResponseBody(int statusCode)
    {
        // Per HTTP spec, these codes must not include a response body
        return statusCode != StatusCodes.Status204NoContent
            && statusCode != StatusCodes.Status304NotModified
            && (statusCode < 100 || statusCode >= 200);
    }

    /// <summary>
    /// Sends a request to the next telemetry system in the chain.
    /// </summary>
    /// <param name="next">Parameters for the next simulation.</param>
    /// <returns>The result of the simulation.</returns>
    private async Task<HopResult> RunNextSimulationAsync(SimulationParams next)
    {
        string protocol = HttpContext.Request.IsHttps
            ? "https"
            : "http";

        string nextUri = $"{protocol}://{next.Host}/{s_route}";
        _logger.LogInternal(LogLevel.Information, $"Chaining request to {nextUri}");

        using HttpClient client = new();
        HttpResponseMessage response = await client.PostAsync(
            nextUri,
            new StringContent(
                JsonSerializer.Serialize(next),
                Encoding.UTF8,
                MediaTypeNames.Application.Json
            )
        );

        HopResult result = await response.Content.ReadFromJsonAsync<HopResult>() ?? new();

        return new()
        {
            Host = next.Host,
            StatusCode = response.StatusCode.ToString(),
            Message = result.Message,
            Hops = result.Hops
        };
    }

    /// <summary>
    /// Executes a telemetry simulation run.
    /// </summary>
    /// <param name="simulation">Object with simulation instructions.</param>
    [HttpPost]
    [Route(s_route)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SimulateAsync([FromBody] SimulationParams simulation)
    {
        DefaultMetrics.RequestCounter.Add(1);
        _logger.LogInternal(LogLevel.Information, "Simulation request received");

        if (simulation.Delay > 0)
        {
            Thread.Sleep(simulation.Delay);
        }

        if (!string.IsNullOrEmpty(simulation.LogParams.Message))
        {
            _logger.Log(
                simulation.LogParams.LogLevel,
                "{message}",
                simulation.LogParams.Message
            );
        }

        if (simulation.NextHops.Count == 0)
        {
            int statusCode = (int)simulation.StatusCode;
            return CanHaveResponseBody(statusCode)
                ? StatusCode(statusCode, new HopResult()
                {
                    Message = s_defaultSuccessMessage,
                    StatusCode = simulation.StatusCode.ToString(),
                    Host = simulation.Host,
                    Hops = []
                })
                : StatusCode(statusCode);
        }

        List<HopResult> responses = [];

        foreach (SimulationParams next in simulation.NextHops)
        {
            responses.Add(await RunNextSimulationAsync(next));
        }

        return Ok(new HopResult()
        {
            Message = s_defaultSuccessMessage,
            StatusCode = simulation.StatusCode.ToString(),
            Host = simulation.Host,
            Hops = responses
        });
    }
}
