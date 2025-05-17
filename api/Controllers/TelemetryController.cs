using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OtelTester.Api.Metrics;

namespace OtelTester.Api.Controllers;

/// <summary>
/// Controller to simulate telemetry data.
/// </summary>
/// <param name="logger">Logger instance.</param>
[ApiController]
public class TelemetryController(ILogger<TelemetryController> logger) : ControllerBase
{
    /// <summary>
    /// Route for the simulation endpoint.
    /// </summary>
    private const string s_route = "simulate";

    /// <summary>
    /// Logger instance for the controller.
    /// </summary>
    private readonly ILogger<TelemetryController> _logger = logger;

    /// <summary>
    /// Sends a request to the next telemetry system in the chain.
    /// </summary>
    /// <param name="next">Parameters for the next simulation.</param>
    /// <returns>The result of the simulation.</returns>
    private async Task<string> RunNextSimulationAsync(SimulationParams next)
    {
        string protocol = HttpContext.Request.IsHttps
            ? "https"
            : "http";

        string nextUri = $"{protocol}://{next.Host}/{s_route}";
        _logger.LogInternal(LogLevel.Information, $"Chaining request to {nextUri}");

        using HttpClient client = new();
        HttpResponseMessage response = await client.PostAsync(
            nextUri,
            new StringContent(JsonSerializer.Serialize(next),
            Encoding.UTF8,
            "application/json")
        );

        return $"{next.Host}: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
    }

    /// <summary>
    /// Executes a telemetry simulation run.
    /// </summary>
    /// <param name="simulation">Object with simulation instructions.</param>
    [HttpPost]
    [Route(s_route)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SimulateAsync([FromBody] SimulationParams simulation)
    {
        DefaultMetrics.RequestCounter.Add(1);
        _logger.LogInternal(LogLevel.Information, "Simulation request received");

        if (simulation.Delay > 0)
        {
            Thread.Sleep(simulation.Delay);
        }

        if (!string.IsNullOrEmpty(simulation.Log))
        {
            _logger.LogInformation("Custom log: {log}", simulation.Log);
        }

        if (simulation.Fail)
        {
            return BadRequest("Artificial error triggered.");
        }

        if (simulation.NextHops.Count == 0)
        {
            return Ok("Simulation completed successfully.");
        }

        List<string> responses = [];

        foreach (SimulationParams next in simulation.NextHops)
        {
            string response = await RunNextSimulationAsync(next);
            responses.Add(response);
        }

        return Ok(responses);
    }
}
