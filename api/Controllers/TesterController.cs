using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using OtelTester.Api.Models;

namespace OtelTester.Api.Controllers;

/// <summary>
/// Controller for triggering testing tasks.
/// </summary>
/// <param name="logger">Logger instance.</param>
[ApiController]
public class TesterController(ILogger<TesterController> logger) : ControllerBase
{
    /// <summary>
    /// Base route for the tester controller.
    /// </summary>
    private const string s_route = "tests";

    /// <summary>
    /// In memory store for simulation information.
    /// </summary>
    private static readonly ConcurrentDictionary<Guid, TestInfo> s_simulations = new();

    /// <summary>
    /// Tags for logging and telemetry purposes.
    /// </summary>
    private readonly (string key, object value)[] _testerTags = [
        ("internalType", "tester")
    ];

    /// <summary>
    /// Logger instance for the controller.
    /// </summary>
    private readonly ILogger<TesterController> _logger = logger;

    /// <summary>
    /// Max number of simulations allowed at a time.
    /// </summary>
    private const int s_maxSimulations = 3;

    /// <summary>
    /// Processes a test simulation asynchronously.
    /// </summary>
    /// <param name="testInfo">Information about the test to run.</param>
    /// <param name="token">Cancellation token for the operation.</param>
    private static async Task ProcessTestAsync(
        TestInfo testInfo, CancellationToken token = default
    )
    {
        TestRun testRun = testInfo.TestRun;
        HttpClient client = new();

        for (int repetition = 0; repetition < testRun.Repetitions; repetition++)
        {
            testInfo.CurrentRepetition = repetition + 1;
            foreach (SimulationParams simulation in testRun.Simulations)
            {
                testInfo.CurrentSimulation = simulation;
                await client.PostAsJsonAsync($"{simulation.Uri}/simulate", simulation, token);
                await Task.Delay(testRun.DelayBetweenSimulations, token);
            }
        }

        testInfo.CurrentRepetition = null;
        testInfo.CurrentSimulation = null;
    }

    /// <summary>
    /// Starts a new test simulation if the number of running tests
    /// is below the maximum.
    /// </summary>
    [HttpPost]
    [Route(s_route)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult NewTest([FromBody] TestRun testRun)
    {
        _logger.LogInternal(LogLevel.Information, "Processing new test request", _testerTags);
        int runningTests = s_simulations.Values
            .Where(s => s.Status == TestInfo.RunningStatus)
            .Count();

        if (runningTests >= s_maxSimulations)
        {
            _logger.LogInternal(LogLevel.Debug, $"Maximum number of tests reached: {s_maxSimulations}", _testerTags);
            return BadRequest("Maximum number of tests reached.");
        }

        Guid testId = Guid.NewGuid();
        TestInfo testInfo = new(testId, testRun);
        if (!s_simulations.TryAdd(testId, testInfo))
        {
            _logger.LogInternal(LogLevel.Error, $"Failed to add new test with ID {testId}", _testerTags);
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to start new test.");
        }

        CancellationToken token = testInfo.CancellationTokenSource.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                testInfo.Run();
                await ProcessTestAsync(testInfo, token);
                testInfo.Complete();
                _logger.LogInternal(LogLevel.Information, $"Test {testId} successfully completed", _testerTags);
            }
            catch (TaskCanceledException)
            {
                testInfo.OnCanceled();
            }
            catch (Exception ex)
            {
                _logger.LogInternal(LogLevel.Error, $"Unexpected error in test {testId}: {ex.Message}", _testerTags);
                testInfo.OnFailed(ex.Message);
            }
        });

        _logger.LogInternal(LogLevel.Information, $"Started new simulation with ID {testId}", _testerTags);
        _logger.LogInternal(LogLevel.Debug, $"Running simulations: {runningTests + 1}", _testerTags);
        return Created($"{s_route}/{testId}", testInfo);
    }

    /// <summary>
    /// Gets all tests that have been started.
    /// </summary>
    [HttpGet]
    [Route(s_route)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult GetTests()
    {
        _logger.LogInternal(LogLevel.Debug, "Retrieving all tests", _testerTags);
        if (s_simulations.IsEmpty)
        {
            return NoContent();
        }

        return Ok(s_simulations.Values.OrderByDescending(s => s.StartedAt));
    }

    /// <summary>
    /// Gets a specific test by its ID.
    /// </summary>
    /// <param name="id">The ID to search by.</param>
    [HttpGet]
    [Route("tests/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetTest(Guid id)
    {
        _logger.LogInternal(LogLevel.Debug, $"Getting test with ID {id}", _testerTags);
        if (!s_simulations.TryGetValue(id, out TestInfo? simulation))
        {
            return NotFound();
        }

        return Ok(simulation);
    }

    /// <summary>
    /// Cancels a running test by its ID.
    /// </summary>
    /// <param name="id">The ID of the test to cancel.</param>
    [HttpPost]
    [Route("tests/{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult CancelTest(Guid id)
    {
        _logger.LogInternal(LogLevel.Debug, $"Canceling test with ID {id}", _testerTags);
        if (!s_simulations.TryGetValue(id, out TestInfo? simulation))
        {
            _logger.LogInternal(LogLevel.Debug, "Test was not found");
            return NotFound();
        }

        if (simulation.Status != TestInfo.RunningStatus)
        {
            _logger.LogInternal(LogLevel.Debug, $"Test with ID {id} is not running, cannot cancel.", _testerTags);
            return BadRequest("Test is not running, cannot cancel.");
        }

        simulation.CancellationTokenSource.Cancel();
        _logger.LogInternal(LogLevel.Information, $"Cancellation requested for test with ID {id}", _testerTags);
        return NoContent();
    }

    /// <summary>
    /// Deletes a test by its ID. If the test is running,
    /// it will be canceled first.
    /// </summary>
    /// <param name="id">The ID of the test to delete.</param>
    [HttpDelete]
    [Route("tests/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteTest(Guid id)
    {
        _logger.LogInternal(LogLevel.Debug, $"Deleting test with ID {id}", _testerTags);
        if (!s_simulations.TryGetValue(id, out TestInfo? simulation))
        {
            _logger.LogInternal(LogLevel.Debug, "Test was not found", _testerTags);
            return NotFound();
        }

        if (simulation.Status == TestInfo.RunningStatus)
        {
            simulation.CancellationTokenSource.Cancel();
            _logger.LogInternal(LogLevel.Debug, "Cancellation requested for test with ID {id}", _testerTags);
        }

        if (!s_simulations.TryRemove(id, out _))
        {
            _logger.LogInternal(LogLevel.Error, $"Failed to remove test with ID {id}", _testerTags);
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete test.");
        }

        _logger.LogInternal(LogLevel.Information, $"Deleted test with ID {id}", _testerTags);
        return NoContent();
    }
}
