# BlazorECharts

A Blazor component library wrapping [Apache ECharts](https://echarts.apache.org/) v6 for .NET 8, .NET 9, and .NET 10. Build interactive Line, Bar, Pie, and Sankey charts entirely from C# and Razor markup, with full support for both Blazor WebAssembly and Blazor Server.

## Features

- Single `<EChart>` component for all chart types
- Strongly-typed C# option models with camelCase JSON serialization
- Three option modes: typed models, anonymous objects, or raw JSON strings
- Click and legend selection event callbacks
- Automatic chart resizing via `ResizeObserver`
- Theme support with runtime switching
- Efficient option merging (not full re-init) on data changes
- Clean disposal of JS resources, observers, and .NET references

## Quick Start

### 1. Install the NuGet package

```bash
dotnet add package BlazorECharts
```

### 2. Add the ECharts script to your HTML

**Blazor WebAssembly** (`wwwroot/index.html`) or **Blazor Server** (`Pages/_Host.cshtml`):

```html
<script src="https://cdn.jsdelivr.net/npm/echarts@5/dist/echarts.min.js"></script>
```

### 3. Add the using directive

In your `_Imports.razor`:

```razor
@using BlazorECharts.Components
@using BlazorECharts.Models
```

### 4. Render a chart

```razor
<EChart Option="@_option" Height="400px" />

@code {
    private EChartOption _option = new()
    {
        Title = new Title { Text = "Weekly Sales" },
        XAxis = new XAxis
        {
            Type = "category",
            Data = new List<string> { "Mon", "Tue", "Wed", "Thu", "Fri" }
        },
        YAxis = new YAxis { Type = "value" },
        Series = new List<Series>
        {
            new LineSeries { Name = "Sales", Data = new object[] { 150, 230, 224, 218, 335 } }
        }
    };
}
```

## Parameter Reference

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Option` | `object` | (required) | Chart configuration: `EChartOption`, anonymous object, or JSON string |
| `Height` | `string` | `"400px"` | CSS height of the chart container |
| `Width` | `string` | `"100%"` | CSS width of the chart container |
| `Theme` | `string?` | `null` | ECharts theme name (e.g., `"dark"`) |
| `OnClick` | `EventCallback<EChartClickEvent>` | -- | Data point click callback |
| `OnLegendSelect` | `EventCallback<string>` | -- | Legend item toggle callback |
| `OnChartInitialized` | `EventCallback` | -- | Fires after chart is created |

## Chart Examples

### Line Chart

```razor
<EChart Option="@_lineOption" />

@code {
    private EChartOption _lineOption = new()
    {
        Title = new Title { Text = "Temperature Trend" },
        Tooltip = new Tooltip { Trigger = "axis" },
        XAxis = new XAxis
        {
            Type = "category",
            Data = new List<string> { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" }
        },
        YAxis = new YAxis { Type = "value", Name = "°C" },
        Series = new List<Series>
        {
            new LineSeries
            {
                Name = "High",
                Data = new object[] { 10, 11, 13, 11, 12, 12, 9 },
                Smooth = true
            },
            new LineSeries
            {
                Name = "Low",
                Data = new object[] { 1, -2, 2, 5, 3, 2, 0 },
                Smooth = true,
                AreaStyle = new { }
            }
        }
    };
}
```

### Bar Chart

```razor
<EChart Option="@_barOption" />

@code {
    private EChartOption _barOption = new()
    {
        Title = new Title { Text = "Quarterly Revenue" },
        Tooltip = new Tooltip { Trigger = "axis" },
        Legend = new Legend { Data = new List<string> { "Product A", "Product B" } },
        XAxis = new XAxis
        {
            Type = "category",
            Data = new List<string> { "Q1", "Q2", "Q3", "Q4" }
        },
        YAxis = new YAxis { Type = "value" },
        Series = new List<Series>
        {
            new BarSeries
            {
                Name = "Product A",
                Data = new object[] { 320, 302, 341, 374 },
                BarWidth = "40%"
            },
            new BarSeries
            {
                Name = "Product B",
                Data = new object[] { 220, 182, 191, 234 },
                BarWidth = "40%"
            }
        }
    };
}
```

### Pie Chart

```razor
<EChart Option="@_pieOption" />

@code {
    private EChartOption _pieOption = new()
    {
        Title = new Title { Text = "Traffic Sources", Left = "center" },
        Tooltip = new Tooltip { Trigger = "item" },
        Legend = new Legend { Orient = "vertical", Left = "left" },
        Series = new List<Series>
        {
            new PieSeries
            {
                Name = "Traffic",
                Radius = "50%",
                Data = new object[]
                {
                    new { value = 1048, name = "Search Engine" },
                    new { value = 735, name = "Direct" },
                    new { value = 580, name = "Email" },
                    new { value = 484, name = "Social" },
                    new { value = 300, name = "Video" }
                },
                Label = new { show = true, formatter = "{b}: {d}%" },
                Emphasis = new { itemStyle = new { shadowBlur = 10, shadowColor = "rgba(0,0,0,0.3)" } }
            }
        }
    };
}
```

### Sankey Diagram

```razor
<EChart Option="@_sankeyOption" Height="500px" />

@code {
    private EChartOption _sankeyOption = new()
    {
        Title = new Title { Text = "Energy Flow" },
        Tooltip = new Tooltip { Trigger = "item" },
        Series = new List<Series>
        {
            new SankeySeries
            {
                Data = new object[]
                {
                    new SankeyNode { Name = "Coal" },
                    new SankeyNode { Name = "Gas" },
                    new SankeyNode { Name = "Electricity" },
                    new SankeyNode { Name = "Heat" },
                    new SankeyNode { Name = "Industry" },
                    new SankeyNode { Name = "Residential" }
                },
                Links = new List<SankeyLink>
                {
                    new() { Source = "Coal", Target = "Electricity", Value = 60 },
                    new() { Source = "Coal", Target = "Heat", Value = 25 },
                    new() { Source = "Gas", Target = "Electricity", Value = 40 },
                    new() { Source = "Gas", Target = "Heat", Value = 35 },
                    new() { Source = "Electricity", Target = "Industry", Value = 70 },
                    new() { Source = "Electricity", Target = "Residential", Value = 30 },
                    new() { Source = "Heat", Target = "Industry", Value = 35 },
                    new() { Source = "Heat", Target = "Residential", Value = 25 }
                }
            }
        }
    };
}
```

## Event Handling

```razor
<EChart Option="@_option" OnClick="HandleClick" OnLegendSelect="HandleLegend" />
<p>Last click: @_lastClick</p>

@code {
    private string _lastClick = "";

    private void HandleClick(EChartClickEvent e)
    {
        _lastClick = $"{e.SeriesName} / {e.Name}: {e.Value}";
    }

    private void HandleLegend(string name)
    {
        Console.WriteLine($"Legend toggled: {name}");
    }
}
```

## Documentation

| Document | Description |
|----------|-------------|
| [docs/overview.md](docs/overview.md) | Project overview, architecture, and supported features |
| [docs/quickstart.md](docs/quickstart.md) | Step-by-step installation and first chart guide |
| [docs/api-reference.md](docs/api-reference.md) | Full parameter, event, and model reference |
| [docs/interop-lifecycle.md](docs/interop-lifecycle.md) | JS interop details, lifecycle diagram, disposal |

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).
