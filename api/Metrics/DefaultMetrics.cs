using System.Diagnostics.Metrics;

namespace OtelTester.Api.Metrics;

/// <summary>
/// Static class to define values for default metrics captured
/// by the API.
/// </summary>
public static class DefaultMetrics
{
    /// <summary>
    /// Name of the meter used to capture metrics.
    /// </summary>
    public const string MeterName = "OtelTester.ApiMetrics";

    /// <summary>
    /// Version of the meter used to capture metrics.
    /// </summary>
    public const string MeterVersion = "1.0.0";

    /// <summary>
    /// Meter instance used to capture metrics.
    /// </summary>
    public static readonly Meter ApiMeter = new(MeterName, MeterVersion);

    /// <summary>
    /// Counter to track the number of requests to the simulate API.
    /// </summary>
    public static readonly Counter<long> RequestCounter = ApiMeter.CreateCounter<long>(
        "oteltester.apimetrics.simulationrequests",
        description: "Total number of requests to the simulate API"
    );
}
