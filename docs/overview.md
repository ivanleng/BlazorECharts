# Overview

## What is Apache ECharts?

[Apache ECharts](https://echarts.apache.org/) is an open-source JavaScript visualization library originally developed by Baidu and now an Apache Software Foundation top-level project. It provides interactive, highly customizable charts that run in the browser using Canvas or SVG rendering. ECharts supports dozens of chart types, animations, large dataset handling, and theming.

## What is BlazorECharts?

BlazorECharts is a Blazor component library that wraps Apache ECharts v6 so you can build rich, interactive charts entirely from C# and Razor markup. Instead of writing JavaScript to configure and manage chart instances, you use:

- A single `<EChart>` Razor component
- Strongly-typed C# option models that map to the ECharts JSON API
- Automatic JavaScript interop for chart lifecycle management
- Event callbacks wired back into your Blazor component code

The library ships as a NuGet package (`BlazorECharts`) targeting both .NET 8 and .NET 9.

## Hosting Model Support

| Hosting Model       | Supported |
|---------------------|-----------|
| Blazor WebAssembly  | Yes       |
| Blazor Server       | Yes       |

Both models are fully supported. The library uses `IJSRuntime` and lazy module loading, which works transparently across WebAssembly and Server hosting.

**Note for Blazor Server:** Chart options are serialized to JSON and sent over SignalR. Very large datasets may hit the default SignalR message size limit. For large data, consider chunking or increasing the limit in your server configuration.

## Supported Chart Types

BlazorECharts provides strongly-typed series models for the following chart types:

### Line Charts

Standard line charts with support for smooth curves, area fills, stacked series, and custom symbols.

```csharp
new LineSeries { Name = "Sales", Data = new object[] { 120, 200, 150, 80 }, Smooth = true }
```

### Bar Charts

Vertical and horizontal bar charts with grouping, stacking, and custom bar widths.

```csharp
new BarSeries { Name = "Revenue", Data = new object[] { 300, 450, 280, 390 }, Stack = "total" }
```

### Pie Charts

Standard pie charts, donut charts (inner/outer radius), and nightingale rose charts.

```csharp
new PieSeries
{
    Data = new object[]
    {
        new { value = 1048, name = "Search" },
        new { value = 735, name = "Direct" }
    },
    Radius = "50%"
}
```

### Sankey Diagrams

Flow diagrams showing weighted relationships between nodes, with configurable orientation and labels.

```csharp
new SankeySeries
{
    Data = new object[] { new SankeyNode { Name = "A" }, new SankeyNode { Name = "B" } },
    Links = new List<SankeyLink>
    {
        new SankeyLink { Source = "A", Target = "B", Value = 100 }
    }
}
```

## Beyond Built-in Types

The `Option` parameter on the `<EChart>` component accepts `object`, which means you are not limited to the provided models. You can pass:

1. **Strongly-typed models** (`EChartOption`) for compile-time safety
2. **Anonymous objects** for quick prototyping or chart types without dedicated models
3. **Raw JSON strings** for full control or pasting options from the ECharts examples gallery

This means every chart type that ECharts supports can be rendered through BlazorECharts, even if there is no dedicated C# model for it.

## Key Features

- **Single component API** -- one `<EChart>` component handles all chart types
- **Automatic resizing** -- charts respond to container size changes via `ResizeObserver`
- **Theming** -- pass any ECharts theme name; theme changes trigger automatic chart re-initialization
- **Click events** -- receive `EChartClickEvent` data when users click data points
- **Legend events** -- receive callbacks when users toggle legend items
- **Efficient updates** -- option changes use ECharts merge mode (`setOption`), not full re-initialization
- **Clean disposal** -- `ResizeObserver`, `DotNetObjectReference`, and JS module are all properly cleaned up

## Architecture

```
Your Blazor App
    |
    v
<EChart Option="@myOption" />       (Razor component)
    |
    v
EChart.razor.cs                      (C# code-behind, lifecycle management)
    |
    v
EChartsInterop.cs                    (C# JS interop wrapper)
    |
    v
echarts-interop.js                   (ES module, chart registry, events)
    |
    v
Apache ECharts v6                    (loaded via CDN <script> tag)
```

## Next Steps

- [Quick Start Guide](quickstart.md) -- get a chart rendering in minutes
- [API Reference](api-reference.md) -- full parameter and model documentation
- [Interop & Lifecycle](interop-lifecycle.md) -- how the JS bridge works under the hood
