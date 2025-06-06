using System.Text.Json.Serialization;

namespace OtelTester.Api.Models;

/// <summary>
/// Information about a test run.
/// </summary>
/// <param name="guid">Unique identifier for the test run.</param>
/// <param name="testRun">Test run information associated with this test.</param>
public class TestInfo(Guid guid, TestRun testRun)
{
    public const string CreatedStatus = "Created";
    public const string RunningStatus = "Running";
    public const string CanceledStatus = "Canceled";
    public const string CompletedStatus = "Completed";
    public const string FailedStatus = "Failed";

    /// <summary>
    /// Unique identifier for the test run.
    /// </summary>
    public Guid Id { get; } = guid;

    /// <summary>
    /// Current status of the test run.
    /// </summary>
    public string Status { get; set; } = CreatedStatus;

    /// <summary>
    /// Date and time when the test run was started.
    /// </summary>
    public DateTime? StartedAt { get; private set; } = null;

    /// <summary>
    /// Date and time when the test run was completed.
    /// </summary>
    public DateTime? CompletedAt { get; private set; } = null;

    /// <summary>
    /// Current repetition being executed in the test run.
    /// </summary>
    public int? CurrentRepetition { get; set; } = null;

    /// <summary>
    /// Current simulation parameters being executed.
    /// </summary>
    public SimulationParams? CurrentSimulation { get; set; } = null;

    /// <summary>
    /// Error message if the test failed or was canceled.
    /// </summary>
    public string? ErrorMessage { get; set; } = null;

    /// <summary>
    /// Test run information associated with this test.
    /// </summary>
    public TestRun TestRun { get; } = testRun;

    /// <summary>
    /// Allows the test to be canceled by the user.
    /// </summary>
    [JsonIgnore]
    public CancellationTokenSource CancellationTokenSource { get; } = new();

    /// <summary>
    /// Starts the test run and sets status and date variables.
    /// </summary>
    public void Run()
    {
        Status = RunningStatus;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the status to canceled and records the completion time.
    /// </summary>
    public void OnCanceled()
    {
        Status = CanceledStatus;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = "Test was canceled by user.";
    }

    /// <summary>
    /// Sets the status to completed and records the completion time.
    /// </summary>
    public void Complete()
    {
        Status = CompletedStatus;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the status to failed, records the completion time,
    /// </summary>
    /// <param name="errorMessage">The error messaeg that caused the failure.</param>
    public void OnFailed(string errorMessage)
    {
        Status = FailedStatus;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }
}
