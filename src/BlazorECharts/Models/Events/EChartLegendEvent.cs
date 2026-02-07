using System.Text.Json.Serialization;

namespace BlazorECharts.Models.Events;

/// <summary>
/// Event data for legend select changed events.
/// </summary>
public class EChartLegendEvent
{
    /// <summary>
    /// Name of the legend item that was toggled.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Map of legend item names to their selected state.
    /// </summary>
    [JsonPropertyName("selected")]
    public Dictionary<string, bool>? Selected { get; set; }
}
