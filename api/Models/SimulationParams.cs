using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace OtelTester.Api.Models;

/// <summary>
/// Parameters passed to the API to simulate metrics.
/// </summary>
public class SimulationParams
{
    /// <summary>
    /// User-defined status code to be returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    /// <summary>
    /// URI of the telemetry system to send the request to. Ignored
    /// for the base request.
    /// </summary>
    [Required]
    [DefaultValue("localhost")]
    [RegularExpression(
        @"^(https?:\/\/)?((localhost)|((?=.{1,253}$)([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,})|((\d{1,3}\.){3}\d{1,3}))(:\d{1,5})?$",
        ErrorMessage = "The URI must be a valid domain or IP, with optional port and http or https scheme."
    )]
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// Artificial delay in milliseconds to simulate network latency.
    /// </summary>
    [Range(0, int.MaxValue)]
    [DefaultValue(0)]
    public int Delay { get; set; } = 0;

    /// <summary>
    /// If not null or empty, adds a new metric value to the tagged counter
    /// with this tag.
    /// </summary>
    public string? MetricTag { get; set; } = null;

    /// <summary>
    /// Logging parameters to be used for the custom log.
    /// </summary>
    public LogParams LogParams { get; set; } = new();

    /// <summary>
    /// Requests to be sent next other instances.
    /// </summary>
    public List<SimulationParams> NextHops { get; set; } = [];
}
