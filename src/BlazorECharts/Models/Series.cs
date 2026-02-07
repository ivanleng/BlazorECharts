using System.Text.Json.Serialization;

namespace BlazorECharts.Models;

/// <summary>
/// Base class for all ECharts series types.
/// </summary>
[JsonDerivedType(typeof(LineSeries))]
[JsonDerivedType(typeof(BarSeries))]
[JsonDerivedType(typeof(PieSeries))]
[JsonDerivedType(typeof(SankeySeries))]
public class Series
{
    /// <summary>
    /// Series name, used in legend and tooltip.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Series type discriminator (e.g., "line", "bar", "pie", "sankey").
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Series data points.
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}
