using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OtelTester.Api.Metrics;
using OtelTester.Api.Models;

namespace OtelTester.Api.Controllers;

/// <summary>
/// Controller to simulate telemetry data.
/// </summary>
/// <param name="logger">Logger instance.</param>
/// <param name="clientFactory">Factory to create HTTP clients.</param>
public class TelemetryController(ILogger<TelemetryController> logger, IHttpClientFactory clientFactory) : OtelTesterController(logger, clientFactory)
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
    /// Hostname set to the current instance.
    /// </summary>
    private readonly string _hostname = Environment.GetEnvironmentVariable("OTELTESTER_HOSTNAME")!;

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
        KeyValuePair<string, object> tag = new("host", _hostname);
        string nextUri = $"{next.Uri}/{s_route}";
        Logger.LogInternal(LogLevel.Information, $"Chaining request to {nextUri}", ("host", _hostname));

        HttpResponseMessage response = await HttpClient.PostAsync(
            nextUri,
            new StringContent(
                JsonSerializer.Serialize(next),
                Encoding.UTF8,
                MediaTypeNames.Application.Json
            )
        );

        TelemetryMetrics.ChainCounter.Add(1, [new("host", _hostname)]);

        HopResult result = await response.Content.ReadFromJsonAsync<HopResult>() ?? new();

        return new()
        {
            Host = next.Uri,
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
        TelemetryMetrics.RequestCounter.Add(1);
        Logger.LogInternal(LogLevel.Information, "Simulation request received", ("host", _hostname));

        if (simulation.Delay > 0)
        {
            await Task.Delay(simulation.Delay);
        }

        if (!string.IsNullOrEmpty(simulation.LogParams.Message))
        {
            Logger.Log(
                simulation.LogParams.LogLevel,
                "{message}",
                simulation.LogParams.Message
            );
        }

        if (!string.IsNullOrEmpty(simulation.MetricTag))
        {
            TelemetryMetrics.TagCounter.Add(1, [new("metricTag", simulation.MetricTag)]);
        }

        if (simulation.NextHops.Count == 0)
        {
            int statusCode = (int)simulation.StatusCode;
            return CanHaveResponseBody(statusCode)
                ? StatusCode(statusCode, new HopResult()
                {
                    Message = s_defaultSuccessMessage,
                    StatusCode = simulation.StatusCode.ToString(),
                    Host = simulation.Uri,
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
            Host = simulation.Uri,
            Hops = responses
        });
    }
}
