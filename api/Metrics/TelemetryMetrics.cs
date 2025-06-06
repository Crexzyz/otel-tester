using System.Diagnostics.Metrics;

namespace OtelTester.Api.Metrics;

/// <summary>
/// Static class to define values for default metrics captured
/// by the API.
/// </summary>
public static class TelemetryMetrics
{
    /// <summary>
    /// Name of the meter used to capture internal metrics.
    /// </summary>
    public const string ApiMeterName = "OtelTester.ApiMetrics";

    /// <summary>
    /// Meter for dynamically tagged metrics.
    /// </summary>
    public const string TagMeterName = "OtelTester.TagMetrics";

    /// <summary>
    /// Version of the meter used to capture metrics.
    /// </summary>
    public const string MeterVersion = "1.0.0";

    /// <summary>
    /// Meter instance used to capture internal metrics.
    /// </summary>
    public static readonly Meter ApiMeter = new(ApiMeterName, MeterVersion);

    /// <summary>
    /// Meter for dynamically tagged metrics.
    /// </summary>
    public static readonly Meter TagsMeter = new(TagMeterName, MeterVersion);

    /// <summary>
    /// Counter to track the number of requests to the simulate API.
    /// </summary>
    public static readonly Counter<long> RequestCounter = ApiMeter.CreateCounter<long>(
        "oteltester.apimetrics.simulationrequests",
        description: "Total number of requests to the simulate API"
    );

    public static readonly Counter<long> ChainCounter = ApiMeter.CreateCounter<long>(
        "oteltester.apimetrics.chainrequests",
        description: "Total number of chained requests done by an instance"
    );

    public static readonly Counter<long> TagCounter = TagsMeter.CreateCounter<long>(
        "oteltester.tagmetrics.taggedcounter",
        description: "Total number of dynamically tagged counts"
    );
}
