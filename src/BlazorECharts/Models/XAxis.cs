using System.Text.Json.Serialization;

namespace BlazorECharts.Models;

/// <summary>
/// X-axis configuration.
/// </summary>
public class XAxis
{
    /// <summary>
    /// Axis type: "category", "value", "time", or "log".
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Category data for the axis.
    /// </summary>
    [JsonPropertyName("data")]
    public List<string>? Data { get; set; }

    /// <summary>
    /// Axis name displayed alongside the axis.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Whether to leave a gap at the boundary of the axis.
    /// </summary>
    [JsonPropertyName("boundaryGap")]
    public object? BoundaryGap { get; set; }
}
