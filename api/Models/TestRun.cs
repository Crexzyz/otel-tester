using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using OtelTester.Api.Controllers;

namespace OtelTester.Api.Models;

/// <summary>
/// Represents a test run configuration for telemetry simulations.
/// </summary>
public class TestRun
{
    /// <summary>
    /// Delay in milliseconds between each simulation.
    /// </summary>
    [Range(0, 100000, ErrorMessage = "Delay must be between 0 and 100000 milliseconds.")]
    [DefaultValue(0)]
    public int DelayBetweenSimulations { get; set; } = 0;

    /// <summary>
    /// Number of times to repeat the simulation.
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Repetitions must be between 1 and 1000.")]
    [DefaultValue(1)]
    public int Repetitions { get; set; } = 1;

    /// <summary>
    /// List of simulation scenarios to run.
    /// </summary>
    public List<SimulationParams> Simulations { get; set; } = [];
}
