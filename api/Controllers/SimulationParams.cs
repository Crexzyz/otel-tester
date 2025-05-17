using System.ComponentModel.DataAnnotations;

namespace OtelTester.Api.Controllers;

/// <summary>
/// Parameters passed to the API to simulate metrics.
/// </summary>
public class SimulationParams
{
    /// <summary>
    /// Artificial delay in milliseconds to simulate network latency.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int Delay { get; set; } = 0;

    /// <summary>
    /// Artificial flag to simulate an error.
    /// </summary>
    public bool Fail { get; set; } = false;

    /// <summary>
    /// Custom log message to be sent to the telemetry system.
    /// </summary>
    public string Log { get; set; } = string.Empty;

    /// <summary>
    /// URI of the telemetry system to send the request to. Ignored
    /// for the base request.
    /// </summary>
    [Required]
    [RegularExpression(@"^((localhost)|((?=.{1,253}$)([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,})|((\d{1,3}\.){3}\d{1,3}))(:\d{1,5})?$")]
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Requests to be sent next other instances.
    /// </summary>
    public List<SimulationParams> NextHops { get; set; } = [];
}
