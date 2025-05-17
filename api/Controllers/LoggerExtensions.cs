namespace OtelTester.Api.Controllers;

/// <summary>
/// For custom logging logic.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// For logging with an internal source tag to differentiate
    /// between custom logs and system logs.
    /// </summary>
    /// <param name="logger">The instance used to log.</param>
    /// <param name="level">The log level to use.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="tags">Extra tags to add to the log.</param>
    public static void LogInternal(
        this ILogger logger,
        LogLevel level,
        string message,
        params (string key, object value)[] tags)
    {
        Dictionary<string, object> props = tags.ToDictionary(t => t.key, t => t.value);
        props["logSource"] = "oteltester";
        logger.Log(level, new EventId(), props, null, (state, ex) => message);
    }
}
