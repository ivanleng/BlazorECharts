using System.Text.Json.Serialization;

namespace BlazorECharts.Models;

/// <summary>
/// Y-axis configuration.
/// </summary>
public class YAxis
{
    /// <summary>
    /// Axis type: "category", "value", "time", or "log".
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Axis name displayed alongside the axis.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Minimum value of the axis.
    /// </summary>
    [JsonPropertyName("min")]
    public object? Min { get; set; }

    /// <summary>
    /// Maximum value of the axis.
    /// </summary>
    [JsonPropertyName("max")]
    public object? Max { get; set; }
}
