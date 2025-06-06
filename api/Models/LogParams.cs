using System.ComponentModel;

namespace OtelTester.Api.Models;

/// <summary>
/// Parameters for the logging simulation.
/// </summary>
public class LogParams
{
    /// <summary>
    /// The message to log.
    /// </summary>
    [DefaultValue("")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The level to use for the log.
    /// </summary>
    [DefaultValue(nameof(LogLevel.Information))]
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}
