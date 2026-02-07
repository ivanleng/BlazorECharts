using System.Text.Json.Serialization;

namespace BlazorECharts.Models.Events;

/// <summary>
/// Event data for chart click events.
/// </summary>
public class EChartClickEvent
{
    /// <summary>
    /// Data name of the clicked item.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Name of the series the clicked item belongs to.
    /// </summary>
    [JsonPropertyName("seriesName")]
    public string SeriesName { get; set; } = "";

    /// <summary>
    /// Index of the data item in the series.
    /// </summary>
    [JsonPropertyName("dataIndex")]
    public int DataIndex { get; set; }

    /// <summary>
    /// Value of the clicked data item (serialized as JSON string).
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = "";

    /// <summary>
    /// Component type: "series", "markPoint", etc.
    /// </summary>
    [JsonPropertyName("componentType")]
    public string ComponentType { get; set; } = "";
}
