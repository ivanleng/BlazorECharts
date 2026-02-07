# API Reference

Complete reference for the BlazorECharts component, parameters, events, and option models.

---

## EChart Component

**Namespace:** `BlazorECharts.Components`

The `<EChart>` component renders an Apache ECharts instance inside a `<div>` container. It implements `IAsyncDisposable` for proper cleanup.

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Option` | `object` | (required) | Chart configuration. Accepts an `EChartOption` instance, an anonymous object, or a raw JSON string. |
| `Height` | `string` | `"400px"` | CSS height of the chart container. |
| `Width` | `string` | `"100%"` | CSS width of the chart container. |
| `Theme` | `string?` | `null` | ECharts theme name (e.g., `"dark"`). Changing this triggers chart re-initialization. |
| `OnClick` | `EventCallback<EChartClickEvent>` | -- | Callback fired when a data point is clicked. |
| `OnLegendSelect` | `EventCallback<string>` | -- | Callback fired when a legend item selection changes. Receives the legend item name. |
| `OnChartInitialized` | `EventCallback` | -- | Callback fired after the chart instance is created on first render. |

### Usage

```razor
<EChart Option="@option"
        Height="500px"
        Width="80%"
        Theme="dark"
        OnClick="HandleClick"
        OnLegendSelect="HandleLegend"
        OnChartInitialized="HandleInit" />
```

### Option Parameter Modes

The `Option` parameter accepts three types:

**1. Strongly-typed model:**
```csharp
private EChartOption _option = new()
{
    Title = new Title { Text = "Sales" },
    XAxis = new XAxis { Type = "category", Data = new List<string> { "Q1", "Q2" } },
    YAxis = new YAxis { Type = "value" },
    Series = new List<Series> { new LineSeries { Data = new object[] { 100, 200 } } }
};
```

**2. Anonymous object:**
```csharp
private object _option = new
{
    title = new { text = "Sales" },
    xAxis = new { type = "category", data = new[] { "Q1", "Q2" } },
    yAxis = new { type = "value" },
    series = new[] { new { type = "line", data = new[] { 100, 200 } } }
};
```

**3. Raw JSON string:**
```csharp
private string _option = """
{
    "title": { "text": "Sales" },
    "xAxis": { "type": "category", "data": ["Q1", "Q2"] },
    "yAxis": { "type": "value" },
    "series": [{ "type": "line", "data": [100, 200] }]
}
""";
```

---

## Event Models

### EChartClickEvent

**Namespace:** `BlazorECharts.Models.Events`

Passed to the `OnClick` callback when a user clicks a data point.

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Data name of the clicked item (e.g., category label). |
| `SeriesName` | `string` | Name of the series the clicked item belongs to. |
| `DataIndex` | `int` | Zero-based index of the data item within its series. |
| `Value` | `string` | Value of the clicked data item, serialized as a JSON string. |
| `ComponentType` | `string` | Component type: `"series"`, `"markPoint"`, etc. |

```csharp
private void HandleClick(EChartClickEvent e)
{
    Console.WriteLine($"Clicked {e.Name} in {e.SeriesName}, value: {e.Value}");
}
```

### EChartLegendEvent

**Namespace:** `BlazorECharts.Models.Events`

Represents legend selection change data.

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Name of the legend item that was toggled. |
| `Selected` | `Dictionary<string, bool>?` | Map of all legend item names to their current selected state. |

**Note:** The `OnLegendSelect` event callback receives `string` (the legend item name), not the full `EChartLegendEvent` object. The `EChartLegendEvent` model is available for advanced use cases.

---

## Option Models

All option models are in the `BlazorECharts.Models` namespace. Properties use `[JsonPropertyName]` attributes to match ECharts camelCase JSON keys. All properties are nullable -- only set the ones you need; null properties are omitted from serialized JSON.

### EChartOption

Root configuration object.

| Property | Type | Description |
|----------|------|-------------|
| `Title` | `Title?` | Chart title configuration. |
| `Tooltip` | `Tooltip?` | Tooltip configuration. |
| `Legend` | `Legend?` | Legend configuration. |
| `XAxis` | `XAxis?` | X-axis configuration. |
| `YAxis` | `YAxis?` | Y-axis configuration. |
| `Grid` | `Grid?` | Grid layout configuration. |
| `Series` | `List<Series>?` | List of data series. |
| `Color` | `List<string>?` | Custom color palette (e.g., `["#5470c6", "#91cc75"]`). |
| `BackgroundColor` | `string?` | Background color of the chart. |

### Title

| Property | Type | Description |
|----------|------|-------------|
| `Text` | `string?` | Main title text. |
| `Subtext` | `string?` | Subtitle text. |
| `Left` | `string?` | Horizontal position: `"left"`, `"center"`, `"right"`, or a pixel/percent value. |
| `Top` | `string?` | Vertical position: `"top"`, `"middle"`, `"bottom"`, or a pixel/percent value. |

### Tooltip

| Property | Type | Description |
|----------|------|-------------|
| `Trigger` | `string?` | Trigger type: `"item"`, `"axis"`, or `"none"`. |
| `Formatter` | `string?` | Tooltip formatter string or template. |

### Legend

| Property | Type | Description |
|----------|------|-------------|
| `Data` | `List<string>?` | Legend item names. If omitted, ECharts auto-collects from series names. |
| `Orient` | `string?` | Orientation: `"horizontal"` or `"vertical"`. |
| `Left` | `string?` | Horizontal position. |
| `Top` | `string?` | Vertical position. |

### XAxis

| Property | Type | Description |
|----------|------|-------------|
| `Type` | `string?` | Axis type: `"category"`, `"value"`, `"time"`, or `"log"`. |
| `Data` | `List<string>?` | Category labels (required when type is `"category"`). |
| `Name` | `string?` | Axis name displayed alongside the axis. |
| `BoundaryGap` | `object?` | Whether to leave a gap at axis boundaries. `true`/`false` or an array like `["20%", "20%"]`. |

### YAxis

| Property | Type | Description |
|----------|------|-------------|
| `Type` | `string?` | Axis type: `"category"`, `"value"`, `"time"`, or `"log"`. |
| `Name` | `string?` | Axis name displayed alongside the axis. |
| `Min` | `object?` | Minimum axis value. Number or `"dataMin"`. |
| `Max` | `object?` | Maximum axis value. Number or `"dataMax"`. |

### Grid

| Property | Type | Description |
|----------|------|-------------|
| `Left` | `string?` | Distance from the left side of the container. |
| `Right` | `string?` | Distance from the right side. |
| `Top` | `string?` | Distance from the top. |
| `Bottom` | `string?` | Distance from the bottom. |
| `ContainLabel` | `bool?` | Whether the grid area contains axis tick labels. |

---

## Series Models

All series classes inherit from the `Series` base class. The `Type` property is set automatically in each subclass constructor.

### Series (Base Class)

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string?` | Series name, shown in legend and tooltip. |
| `Type` | `string?` | Series type discriminator (`"line"`, `"bar"`, `"pie"`, `"sankey"`). Set automatically by subclasses. |
| `Data` | `object?` | Data points. Format varies by series type. |

Polymorphic serialization is supported via `[JsonDerivedType]` attributes. A `List<Series>` containing mixed types (e.g., `LineSeries` and `BarSeries`) serializes correctly.

### LineSeries

Inherits from `Series`. Type is set to `"line"`.

| Property | Type | Description |
|----------|------|-------------|
| `Smooth` | `bool?` | Whether to render the line as a smooth curve. |
| `AreaStyle` | `object?` | Area fill style. Set to `new { }` to enable area fill with defaults. |
| `Stack` | `string?` | Stack group name. Series with the same stack name are stacked. |
| `Symbol` | `string?` | Data point symbol shape: `"circle"`, `"rect"`, `"triangle"`, `"none"`, etc. |
| `LineStyle` | `object?` | Line style object (e.g., `new { width = 3, type = "dashed" }`). |

```csharp
new LineSeries
{
    Name = "Temperature",
    Data = new object[] { 11, 11, 15, 13, 12, 13, 10 },
    Smooth = true,
    AreaStyle = new { },
    LineStyle = new { width = 3 }
}
```

### BarSeries

Inherits from `Series`. Type is set to `"bar"`.

| Property | Type | Description |
|----------|------|-------------|
| `Stack` | `string?` | Stack group name. Series with the same stack name are stacked. |
| `BarWidth` | `string?` | Bar width as a pixel value or percentage (e.g., `"60%"`). |
| `ItemStyle` | `object?` | Item style object (e.g., `new { borderRadius = new[] { 4, 4, 0, 0 } }`). |

```csharp
new BarSeries
{
    Name = "Revenue",
    Data = new object[] { 320, 302, 341, 374 },
    Stack = "total",
    BarWidth = "60%"
}
```

### PieSeries

Inherits from `Series`. Type is set to `"pie"`.

| Property | Type | Description |
|----------|------|-------------|
| `Radius` | `object?` | Radius as a string (`"50%"`) or array (`new[] { "40%", "70%" }` for donut). |
| `Center` | `object?` | Center position as an array (e.g., `new[] { "50%", "50%" }`). |
| `RoseType` | `string?` | Nightingale rose mode: `"radius"` or `"area"`. |
| `Label` | `object?` | Label configuration (e.g., `new { show = true, formatter = "{b}: {d}%" }`). |
| `Emphasis` | `object?` | Hover emphasis style (e.g., `new { itemStyle = new { shadowBlur = 10 } }`). |

Pie series data uses name/value objects:

```csharp
new PieSeries
{
    Name = "Traffic Source",
    Radius = "50%",
    Data = new object[]
    {
        new { value = 1048, name = "Search Engine" },
        new { value = 735, name = "Direct" },
        new { value = 580, name = "Email" },
        new { value = 484, name = "Social" },
        new { value = 300, name = "Video" }
    },
    Label = new { show = true, formatter = "{b}: {d}%" }
}
```

### SankeySeries

Inherits from `Series`. Type is set to `"sankey"`.

| Property | Type | Description |
|----------|------|-------------|
| `Links` | `List<SankeyLink>?` | Links (edges) between nodes. |
| `Orient` | `string?` | Diagram orientation: `"horizontal"` (default) or `"vertical"`. |
| `Label` | `object?` | Node label configuration. |

Sankey data is a list of `SankeyNode` objects (passed as `Data`) combined with `Links`:

```csharp
new SankeySeries
{
    Data = new object[]
    {
        new SankeyNode { Name = "Source A" },
        new SankeyNode { Name = "Source B" },
        new SankeyNode { Name = "Target X" },
        new SankeyNode { Name = "Target Y" }
    },
    Links = new List<SankeyLink>
    {
        new() { Source = "Source A", Target = "Target X", Value = 50 },
        new() { Source = "Source A", Target = "Target Y", Value = 30 },
        new() { Source = "Source B", Target = "Target X", Value = 40 },
        new() { Source = "Source B", Target = "Target Y", Value = 20 }
    }
}
```

### SankeyLink

| Property | Type | Description |
|----------|------|-------------|
| `Source` | `string?` | Source node name. |
| `Target` | `string?` | Target node name. |
| `Value` | `double?` | Weight/value of the link. |

### SankeyNode

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string?` | Node name (must match names referenced in `SankeyLink.Source` and `SankeyLink.Target`). |

---

## Serialization

All models are serialized with `System.Text.Json` using these settings:

- **Naming policy:** `JsonNamingPolicy.CamelCase`
- **Null handling:** `JsonIgnoreCondition.WhenWritingNull` -- null properties are omitted from output
- **Polymorphism:** `[JsonDerivedType]` on the `Series` base class enables correct serialization of `LineSeries`, `BarSeries`, `PieSeries`, and `SankeySeries` in a `List<Series>`

If `Option` is passed as a `string`, it is sent to JavaScript as-is (no serialization step).

---

## See Also

- [Overview](overview.md) -- project summary and architecture
- [Quick Start](quickstart.md) -- get running in minutes
- [Interop & Lifecycle](interop-lifecycle.md) -- how the JS bridge works
