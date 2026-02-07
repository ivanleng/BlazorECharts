using System.Text.Json.Serialization;

namespace BlazorECharts.Models;

/// <summary>
/// Line series configuration.
/// </summary>
public class LineSeries : Series
{
    /// <summary>
    /// Creates a new LineSeries with type set to "line".
    /// </summary>
    public LineSeries()
    {
        Type = "line";
    }

    /// <summary>
    /// Whether the line is smooth.
    /// </summary>
    [JsonPropertyName("smooth")]
    public bool? Smooth { get; set; }

    /// <summary>
    /// Area style configuration. Set to an empty object to enable area fill.
    /// </summary>
    [JsonPropertyName("areaStyle")]
    public object? AreaStyle { get; set; }

    /// <summary>
    /// Stack group name for stacked line charts.
    /// </summary>
    [JsonPropertyName("stack")]
    public string? Stack { get; set; }

    /// <summary>
    /// Symbol shape for data points.
    /// </summary>
    [JsonPropertyName("symbol")]
    public string? Symbol { get; set; }

    /// <summary>
    /// Line style configuration.
    /// </summary>
    [JsonPropertyName("lineStyle")]
    public object? LineStyle { get; set; }
}
