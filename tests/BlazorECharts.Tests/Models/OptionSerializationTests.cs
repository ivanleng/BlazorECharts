using System.Text.Json;
using BlazorECharts.Interop;
using BlazorECharts.Models;
using Xunit;

namespace BlazorECharts.Tests.Models;

/// <summary>
/// Tests for JSON serialization of EChartOption and related model types.
/// </summary>
public class OptionSerializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void EChartOption_WithLineSeries_ProducesCorrectJson()
    {
        // Arrange
        var option = new EChartOption
        {
            Title = new Title { Text = "Line Chart" },
            XAxis = new XAxis { Type = "category", Data = new List<string> { "Mon", "Tue", "Wed" } },
            YAxis = new YAxis { Type = "value" },
            Series = new List<Series>
            {
                new LineSeries
                {
                    Name = "Sales",
                    Data = new object[] { 150, 230, 224 },
                    Smooth = true
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(option, JsonOptions);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Assert
        Assert.Equal("Line Chart", root.GetProperty("title").GetProperty("text").GetString());
        Assert.Equal("category", root.GetProperty("xAxis").GetProperty("type").GetString());
        Assert.Equal("value", root.GetProperty("yAxis").GetProperty("type").GetString());

        var series = root.GetProperty("series");
        Assert.Equal(1, series.GetArrayLength());

        var firstSeries = series[0];
        Assert.Equal("line", firstSeries.GetProperty("type").GetString());
        Assert.Equal("Sales", firstSeries.GetProperty("name").GetString());
        Assert.True(firstSeries.GetProperty("smooth").GetBoolean());
    }

    [Fact]
    public void EChartOption_WithBarSeries_ProducesCorrectJson()
    {
        // Arrange
        var option = new EChartOption
        {
            XAxis = new XAxis { Type = "category", Data = new List<string> { "A", "B", "C" } },
            YAxis = new YAxis { Type = "value" },
            Series = new List<Series>
            {
                new BarSeries
                {
                    Name = "Revenue",
                    Data = new object[] { 100, 200, 300 },
                    BarWidth = "60%"
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(option, JsonOptions);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Assert
        var series = root.GetProperty("series");
        Assert.Equal(1, series.GetArrayLength());

        var firstSeries = series[0];
        Assert.Equal("bar", firstSeries.GetProperty("type").GetString());
        Assert.Equal("Revenue", firstSeries.GetProperty("name").GetString());
        Assert.Equal("60%", firstSeries.GetProperty("barWidth").GetString());
    }

    [Fact]
    public void EChartOption_WithPieSeries_ProducesCorrectJson()
    {
        // Arrange
        var pieData = new[]
        {
            new { value = 1048, name = "Search" },
            new { value = 735, name = "Direct" }
        };

        var option = new EChartOption
        {
            Series = new List<Series>
            {
                new PieSeries
                {
                    Name = "Access",
                    Radius = "50%",
                    Data = pieData
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(option, JsonOptions);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Assert
        var series = root.GetProperty("series");
        Assert.Equal(1, series.GetArrayLength());

        var firstSeries = series[0];
        Assert.Equal("pie", firstSeries.GetProperty("type").GetString());
        Assert.Equal("Access", firstSeries.GetProperty("name").GetString());
        Assert.Equal("50%", firstSeries.GetProperty("radius").GetString());
    }

    [Fact]
    public void EChartOption_WithSankeySeries_ProducesCorrectJson()
    {
        // Arrange
        var option = new EChartOption
        {
            Series = new List<Series>
            {
                new SankeySeries
                {
                    Name = "Flow",
                    Data = new object[]
                    {
                        new { name = "Node1" },
                        new { name = "Node2" }
                    },
                    Links = new List<SankeyLink>
                    {
                        new SankeyLink { Source = "Node1", Target = "Node2", Value = 100 }
                    }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(option, JsonOptions);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Assert
        var series = root.GetProperty("series");
        Assert.Equal(1, series.GetArrayLength());

        var firstSeries = series[0];
        Assert.Equal("sankey", firstSeries.GetProperty("type").GetString());
        Assert.Equal("Flow", firstSeries.GetProperty("name").GetString());

        var links = firstSeries.GetProperty("links");
        Assert.Equal(1, links.GetArrayLength());
        Assert.Equal("Node1", links[0].GetProperty("source").GetString());
        Assert.Equal("Node2", links[0].GetProperty("target").GetString());
        Assert.Equal(100, links[0].GetProperty("value").GetDouble());
    }

    [Fact]
    public void NullProperties_AreOmittedFromJson()
    {
        // Arrange
        var option = new EChartOption
        {
            Title = new Title { Text = "Test" }
            // All other properties left null
        };

        // Act
        var json = JsonSerializer.Serialize(option, JsonOptions);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("title", out _));
        Assert.False(root.TryGetProperty("tooltip", out _));
        Assert.False(root.TryGetProperty("legend", out _));
        Assert.False(root.TryGetProperty("xAxis", out _));
        Assert.False(root.TryGetProperty("yAxis", out _));
        Assert.False(root.TryGetProperty("series", out _));
        Assert.False(root.TryGetProperty("grid", out _));
        Assert.False(root.TryGetProperty("color", out _));
        Assert.False(root.TryGetProperty("backgroundColor", out _));
    }

    [Fact]
    public void JsonPropertyName_UsesCamelCase()
    {
        // Arrange
        var option = new EChartOption
        {
            BackgroundColor = "#fff",
            XAxis = new XAxis { Type = "category", BoundaryGap = false },
            YAxis = new YAxis { Type = "value" },
            Grid = new Grid { ContainLabel = true }
        };

        // Act
        var json = JsonSerializer.Serialize(option, JsonOptions);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Assert - verify camelCase keys from JsonPropertyName attributes
        Assert.True(root.TryGetProperty("backgroundColor", out _));
        Assert.False(root.TryGetProperty("BackgroundColor", out _));

        Assert.True(root.TryGetProperty("xAxis", out _));
        Assert.False(root.TryGetProperty("XAxis", out _));

        Assert.True(root.TryGetProperty("yAxis", out _));
        Assert.False(root.TryGetProperty("YAxis", out _));

        var grid = root.GetProperty("grid");
        Assert.True(grid.TryGetProperty("containLabel", out _));
        Assert.False(grid.TryGetProperty("ContainLabel", out _));

        var xAxis = root.GetProperty("xAxis");
        Assert.True(xAxis.TryGetProperty("boundaryGap", out _));
        Assert.False(xAxis.TryGetProperty("BoundaryGap", out _));
    }

    [Fact]
    public void PolymorphicSeriesList_SerializesSubtypeProperties()
    {
        // Arrange
        var option = new EChartOption
        {
            Series = new List<Series>
            {
                new LineSeries { Name = "Line1", Data = new object[] { 1, 2 }, Smooth = true },
                new BarSeries { Name = "Bar1", Data = new object[] { 3, 4 }, BarWidth = "50%" },
                new PieSeries { Name = "Pie1", Radius = "60%" },
                new SankeySeries
                {
                    Name = "Sankey1",
                    Links = new List<SankeyLink>
                    {
                        new SankeyLink { Source = "A", Target = "B", Value = 10 }
                    }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(option, JsonOptions);
        using var doc = JsonDocument.Parse(json);
        var series = doc.RootElement.GetProperty("series");

        // Assert - each subtype's type discriminator and unique properties are present
        Assert.Equal(4, series.GetArrayLength());

        // LineSeries
        Assert.Equal("line", series[0].GetProperty("type").GetString());
        Assert.True(series[0].GetProperty("smooth").GetBoolean());

        // BarSeries
        Assert.Equal("bar", series[1].GetProperty("type").GetString());
        Assert.Equal("50%", series[1].GetProperty("barWidth").GetString());

        // PieSeries
        Assert.Equal("pie", series[2].GetProperty("type").GetString());
        Assert.Equal("60%", series[2].GetProperty("radius").GetString());

        // SankeySeries
        Assert.Equal("sankey", series[3].GetProperty("type").GetString());
        Assert.True(series[3].TryGetProperty("links", out _));
    }

    [Fact]
    public void SerializeOption_RawJsonString_PassesThroughWithoutReserialization()
    {
        // Arrange
        var rawJson = """{"title":{"text":"Raw"},"series":[{"type":"line"}]}""";

        // Act
        var result = EChartsInterop.SerializeOption(rawJson);

        // Assert - the exact same string is returned, not re-serialized
        Assert.Equal(rawJson, result);
    }

    [Fact]
    public void SerializeOption_TypedObject_SerializesToJson()
    {
        // Arrange
        var option = new EChartOption
        {
            Title = new Title { Text = "Typed" },
            Series = new List<Series>
            {
                new LineSeries { Name = "Data", Data = new object[] { 1, 2, 3 } }
            }
        };

        // Act
        var result = EChartsInterop.SerializeOption(option);

        // Assert - result is valid JSON with expected structure
        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        Assert.Equal("Typed", root.GetProperty("title").GetProperty("text").GetString());
        Assert.Equal(1, root.GetProperty("series").GetArrayLength());
        Assert.Equal("line", root.GetProperty("series")[0].GetProperty("type").GetString());
        Assert.Equal("Data", root.GetProperty("series")[0].GetProperty("name").GetString());
    }

    [Fact]
    public void SerializeOption_TypedObject_OmitsNullProperties()
    {
        // Arrange
        var option = new EChartOption
        {
            Title = new Title { Text = "Simple" }
        };

        // Act
        var result = EChartsInterop.SerializeOption(option);

        // Assert - null properties should not appear in the output
        using var doc = JsonDocument.Parse(result);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("title", out _));
        Assert.False(root.TryGetProperty("series", out _));
        Assert.False(root.TryGetProperty("xAxis", out _));
        Assert.False(root.TryGetProperty("yAxis", out _));
    }
}
