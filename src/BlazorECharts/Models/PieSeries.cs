using System.Text.Json.Serialization;

namespace BlazorECharts.Models;

/// <summary>
/// Pie series configuration.
/// </summary>
public class PieSeries : Series
{
    /// <summary>
    /// Creates a new PieSeries with type set to "pie".
    /// </summary>
    public PieSeries()
    {
        Type = "pie";
    }

    /// <summary>
    /// Radius of the pie. Can be a string like "50%" or an array like ["40%", "70%"] for donut chart.
    /// </summary>
    [JsonPropertyName("radius")]
    public object? Radius { get; set; }

    /// <summary>
    /// Center position of the pie chart.
    /// </summary>
    [JsonPropertyName("center")]
    public object? Center { get; set; }

    /// <summary>
    /// Rose type: "radius" or "area" for nightingale rose charts.
    /// </summary>
    [JsonPropertyName("roseType")]
    public string? RoseType { get; set; }

    /// <summary>
    /// Label configuration.
    /// </summary>
    [JsonPropertyName("label")]
    public object? Label { get; set; }

    /// <summary>
    /// Emphasis configuration for hover effects.
    /// </summary>
    [JsonPropertyName("emphasis")]
    public object? Emphasis { get; set; }
}
