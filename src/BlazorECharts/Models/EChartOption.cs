using System.Text.Json.Serialization;

namespace BlazorECharts.Models;

/// <summary>
/// Root option model for ECharts configuration.
/// </summary>
public class EChartOption
{
    /// <summary>
    /// Chart title configuration.
    /// </summary>
    [JsonPropertyName("title")]
    public Title? Title { get; set; }

    /// <summary>
    /// Tooltip configuration.
    /// </summary>
    [JsonPropertyName("tooltip")]
    public Tooltip? Tooltip { get; set; }

    /// <summary>
    /// Legend configuration.
    /// </summary>
    [JsonPropertyName("legend")]
    public Legend? Legend { get; set; }

    /// <summary>
    /// X-axis configuration. Can be a single axis or a list.
    /// </summary>
    [JsonPropertyName("xAxis")]
    public XAxis? XAxis { get; set; }

    /// <summary>
    /// Y-axis configuration. Can be a single axis or a list.
    /// </summary>
    [JsonPropertyName("yAxis")]
    public YAxis? YAxis { get; set; }

    /// <summary>
    /// Data series list.
    /// </summary>
    [JsonPropertyName("series")]
    public List<Series>? Series { get; set; }

    /// <summary>
    /// Grid configuration.
    /// </summary>
    [JsonPropertyName("grid")]
    public Grid? Grid { get; set; }

    /// <summary>
    /// Custom color palette.
    /// </summary>
    [JsonPropertyName("color")]
    public List<string>? Color { get; set; }

    /// <summary>
    /// Background color of the chart.
    /// </summary>
    [JsonPropertyName("backgroundColor")]
    public string? BackgroundColor { get; set; }
}

/// <summary>
/// Chart title configuration.
/// </summary>
public class Title
{
    /// <summary>
    /// Main title text.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// Subtitle text.
    /// </summary>
    [JsonPropertyName("subtext")]
    public string? Subtext { get; set; }

    /// <summary>
    /// Horizontal alignment of the title.
    /// </summary>
    [JsonPropertyName("left")]
    public string? Left { get; set; }

    /// <summary>
    /// Top position of the title.
    /// </summary>
    [JsonPropertyName("top")]
    public string? Top { get; set; }
}

/// <summary>
/// Tooltip configuration.
/// </summary>
public class Tooltip
{
    /// <summary>
    /// Trigger type: "item", "axis", or "none".
    /// </summary>
    [JsonPropertyName("trigger")]
    public string? Trigger { get; set; }

    /// <summary>
    /// Tooltip formatter string or template.
    /// </summary>
    [JsonPropertyName("formatter")]
    public string? Formatter { get; set; }
}

/// <summary>
/// Legend configuration.
/// </summary>
public class Legend
{
    /// <summary>
    /// Legend data items.
    /// </summary>
    [JsonPropertyName("data")]
    public List<string>? Data { get; set; }

    /// <summary>
    /// Orientation: "horizontal" or "vertical".
    /// </summary>
    [JsonPropertyName("orient")]
    public string? Orient { get; set; }

    /// <summary>
    /// Horizontal position.
    /// </summary>
    [JsonPropertyName("left")]
    public string? Left { get; set; }

    /// <summary>
    /// Top position.
    /// </summary>
    [JsonPropertyName("top")]
    public string? Top { get; set; }
}

/// <summary>
/// Grid configuration for chart layout.
/// </summary>
public class Grid
{
    /// <summary>
    /// Distance from left.
    /// </summary>
    [JsonPropertyName("left")]
    public string? Left { get; set; }

    /// <summary>
    /// Distance from right.
    /// </summary>
    [JsonPropertyName("right")]
    public string? Right { get; set; }

    /// <summary>
    /// Distance from bottom.
    /// </summary>
    [JsonPropertyName("bottom")]
    public string? Bottom { get; set; }

    /// <summary>
    /// Distance from top.
    /// </summary>
    [JsonPropertyName("top")]
    public string? Top { get; set; }

    /// <summary>
    /// Whether the grid region contains the axis tick labels.
    /// </summary>
    [JsonPropertyName("containLabel")]
    public bool? ContainLabel { get; set; }
}
