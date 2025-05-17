namespace OtelTester.Api.Controllers;

/// <summary>
/// Class representing the result of a hop.
/// </summary>
public class HopResult
{
    /// <summary>
    /// The host where the hop was executed.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// The status code returned by the hop.
    /// </summary>
    public string StatusCode { get; set; } = string.Empty;

    /// <summary>
    /// The message returned by the hop.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The nested hops executed by this hop.
    /// </summary>
    public List<HopResult> Hops { get; set; } = [];
}
