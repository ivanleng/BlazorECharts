# Quick Start Guide

This guide walks you through adding BlazorECharts to your Blazor application and rendering your first chart.

## Prerequisites

- .NET 8.0 SDK or .NET 9.0 SDK
- A Blazor WebAssembly or Blazor Server project

## Step 1: Install the NuGet Package

Add the BlazorECharts package to your Blazor project:

```bash
dotnet add package BlazorECharts
```

Or add it directly to your `.csproj`:

```xml
<PackageReference Include="BlazorECharts" Version="1.0.0" />
```

## Step 2: Add the ECharts Script Tag

BlazorECharts does not bundle the Apache ECharts JavaScript library. You must load it yourself via a CDN or local file.

### Blazor WebAssembly

Add the script tag to `wwwroot/index.html`, before the closing `</body>` tag:

```html
<body>
    <div id="app">Loading...</div>

    <script src="https://cdn.jsdelivr.net/npm/echarts@5/dist/echarts.min.js"></script>
    <script src="_framework/blazor.webassembly.js"></script>
</body>
```

### Blazor Server

Add the script tag to `Pages/_Host.cshtml` or `Pages/_Layout.cshtml` (depending on your template), before the closing `</body>` tag:

```html
<body>
    <component type="typeof(App)" render-mode="ServerPrerendered" />

    <script src="https://cdn.jsdelivr.net/npm/echarts@5/dist/echarts.min.js"></script>
    <script src="_framework/blazor.server.js"></script>
</body>
```

**Important:** The ECharts script must be loaded before any chart components render. Place it before the Blazor framework script.

## Step 3: Add the Using Directive

Add the BlazorECharts namespace to your `_Imports.razor` file so the component is available in all pages:

```razor
@using BlazorECharts.Components
@using BlazorECharts.Models
```

Or add it to a single page:

```razor
@using BlazorECharts.Components
@using BlazorECharts.Models
```

## Step 4: Render Your First Chart

Create a new Razor page or add the following to an existing page:

```razor
@page "/my-chart"
@using BlazorECharts.Components
@using BlazorECharts.Models

<EChart Option="@_option" Height="400px" />

@code {
    private EChartOption _option = new()
    {
        Title = new Title { Text = "My First Chart" },
        Tooltip = new Tooltip { Trigger = "axis" },
        XAxis = new XAxis
        {
            Type = "category",
            Data = new List<string> { "Mon", "Tue", "Wed", "Thu", "Fri" }
        },
        YAxis = new YAxis { Type = "value" },
        Series = new List<Series>
        {
            new LineSeries
            {
                Name = "Sales",
                Data = new object[] { 150, 230, 224, 218, 335 }
            }
        }
    };
}
```

Run your application and navigate to `/my-chart`. You should see an interactive line chart.

## Minimal Example (5 Lines)

If you already have the package installed and ECharts loaded, the minimum code to render a chart is:

```razor
<EChart Option="@_option" />

@code {
    private object _option = new
    {
        xAxis = new { type = "category", data = new[] { "A", "B", "C" } },
        yAxis = new { type = "value" },
        series = new[] { new { type = "line", data = new[] { 10, 20, 30 } } }
    };
}
```

Anonymous objects work because the `Option` parameter accepts `object` and serializes it to JSON with camelCase naming.

## Using Raw JSON

You can also pass a raw JSON string directly:

```razor
<EChart Option="@_json" />

@code {
    private string _json = """
    {
        "xAxis": { "type": "category", "data": ["A", "B", "C"] },
        "yAxis": { "type": "value" },
        "series": [{ "type": "bar", "data": [10, 20, 30] }]
    }
    """;
}
```

This is useful when copying options from the [ECharts Examples Gallery](https://echarts.apache.org/examples/).

## Adding Event Handling

Handle click events on data points:

```razor
<EChart Option="@_option" OnClick="HandleClick" />
<p>Clicked: @_clicked</p>

@code {
    private string _clicked = "nothing yet";

    private void HandleClick(EChartClickEvent e)
    {
        _clicked = $"{e.SeriesName}: {e.Name} = {e.Value}";
    }

    private EChartOption _option = new()
    {
        XAxis = new XAxis { Type = "category", Data = new List<string> { "A", "B", "C" } },
        YAxis = new YAxis { Type = "value" },
        Series = new List<Series>
        {
            new BarSeries { Name = "Data", Data = new object[] { 10, 20, 30 } }
        }
    };
}
```

## Applying a Theme

ECharts supports built-in themes like `"dark"`. Pass the theme name to the `Theme` parameter:

```razor
<EChart Option="@_option" Theme="dark" Height="400px" />
```

You can switch themes at runtime. Changing the `Theme` parameter triggers automatic chart re-initialization with the new theme applied.

## Dynamic Updates

Update the chart by changing the `Option` parameter. BlazorECharts detects the change and calls `setOption` to merge the new data:

```razor
<EChart Option="@_option" />
<button @onclick="AddDataPoint">Add Point</button>

@code {
    private List<int> _data = new() { 10, 20, 30 };

    private EChartOption _option => new()
    {
        XAxis = new XAxis { Type = "category", Data = _data.Select((_, i) => $"P{i}").ToList() },
        YAxis = new YAxis { Type = "value" },
        Series = new List<Series>
        {
            new LineSeries { Name = "Values", Data = _data.Cast<object>().ToArray() }
        }
    };

    private void AddDataPoint()
    {
        _data.Add(Random.Shared.Next(5, 50));
    }
}
```

## Next Steps

- [API Reference](api-reference.md) -- full list of parameters, models, and events
- [Interop & Lifecycle](interop-lifecycle.md) -- understand how the JS bridge works
- [Overview](overview.md) -- feature summary and architecture
