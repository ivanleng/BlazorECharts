using System.Text.Json.Serialization;

namespace BlazorECharts.Models;

/// <summary>
/// Bar series configuration.
/// </summary>
public class BarSeries : Series
{
    /// <summary>
    /// Creates a new BarSeries with type set to "bar".
    /// </summary>
    public BarSeries()
    {
        Type = "bar";
    }

    /// <summary>
    /// Stack group name for stacked bar charts.
    /// </summary>
    [JsonPropertyName("stack")]
    public string? Stack { get; set; }

    /// <summary>
    /// Width of the bar.
    /// </summary>
    [JsonPropertyName("barWidth")]
    public string? BarWidth { get; set; }

    /// <summary>
    /// Item style configuration.
    /// </summary>
    [JsonPropertyName("itemStyle")]
    public object? ItemStyle { get; set; }
}
