using System.Text.Json.Serialization;

namespace BlazorECharts.Models;

/// <summary>
/// Sankey diagram series configuration.
/// </summary>
public class SankeySeries : Series
{
    /// <summary>
    /// Creates a new SankeySeries with type set to "sankey".
    /// </summary>
    public SankeySeries()
    {
        Type = "sankey";
    }

    /// <summary>
    /// Links between nodes.
    /// </summary>
    [JsonPropertyName("links")]
    public List<SankeyLink>? Links { get; set; }

    /// <summary>
    /// Orientation of the sankey diagram: "horizontal" or "vertical".
    /// </summary>
    [JsonPropertyName("orient")]
    public string? Orient { get; set; }

    /// <summary>
    /// Label configuration.
    /// </summary>
    [JsonPropertyName("label")]
    public object? Label { get; set; }
}

/// <summary>
/// Represents a link between two nodes in a Sankey diagram.
/// </summary>
public class SankeyLink
{
    /// <summary>
    /// Source node name.
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// Target node name.
    /// </summary>
    [JsonPropertyName("target")]
    public string? Target { get; set; }

    /// <summary>
    /// Value/weight of the link.
    /// </summary>
    [JsonPropertyName("value")]
    public double? Value { get; set; }
}

/// <summary>
/// Represents a node in a Sankey diagram.
/// </summary>
public class SankeyNode
{
    /// <summary>
    /// Node name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
