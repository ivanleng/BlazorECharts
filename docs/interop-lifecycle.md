# JavaScript Interop & Component Lifecycle

This document explains how BlazorECharts manages the JavaScript bridge between Blazor and Apache ECharts, including module loading, chart initialization, option updates, theme changes, and cleanup.

---

## Architecture Overview

```
Blazor Component (C#)            JavaScript (Browser)
=======================          =====================

EChart.razor.cs                  echarts-interop.js
    |                                |
    |-- EChartsInterop.cs            |-- chartRegistry (Map)
    |       |                        |       |
    |       |--- InitAsync() ------->|--- initChart()
    |       |--- UpdateAsync() ----->|--- updateOption()
    |       |--- ResizeAsync() ----->|--- resize()
    |       |--- DisposeChartAsync()->|--- dispose()
    |       |                        |
    |       |<-- OnChartClick() -----|<-- chart.on("click")
    |       |<-- OnLegendSelect() --|<-- chart.on("legendselectchanged")
```

Communication flows in both directions:
- **C# to JS:** The `EChartsInterop` class calls exported JS module functions
- **JS to C#:** ECharts events trigger `dotNetRef.invokeMethodAsync()` to call `[JSInvokable]` methods on the Blazor component

---

## Module Loading

The JavaScript interop module is loaded lazily on first use.

### How It Works

1. `EChartsInterop` is created with a reference to `IJSRuntime`
2. The JS module path is `/_content/BlazorECharts/echarts-interop.js` (Razor Class Library static asset convention)
3. The module import is wrapped in a `Lazy<Task<IJSObjectReference>>` so it only loads once, regardless of how many charts are on the page
4. The first interop call (typically `InitAsync`) triggers the import

```csharp
// Inside EChartsInterop constructor
_moduleTask = new Lazy<Task<IJSObjectReference>>(() =>
    _jsRuntime.InvokeAsync<IJSObjectReference>(
        "import", "/_content/BlazorECharts/echarts-interop.js").AsTask());
```

### Prerequisite

Apache ECharts must already be loaded in the page (via a `<script>` tag) before the interop module runs. The module accesses `echarts` from the global scope (`window.echarts`).

---

## Chart Initialization

Chart initialization happens in `OnAfterRenderAsync` when `firstRender` is `true`.

### Sequence

```
1. OnAfterRenderAsync(firstRender: true)
2.   Create DotNetObjectReference<EChart>
3.   Create EChartsInterop instance
4.   Call _interop.InitAsync(chartId, dotNetRef, option, theme)
5.     JS module loads (if not already loaded)
6.     initChart() in JS:
7.       Find DOM element by ID
8.       Call echarts.init(element, theme)
9.       Parse option JSON and call chart.setOption(option)
10.      Register "click" event listener -> dotNetRef.invokeMethodAsync("OnChartClick")
11.      Register "legendselectchanged" listener -> dotNetRef.invokeMethodAsync("OnLegendSelectChanged")
12.      Create ResizeObserver on the container element
13.      Store { chart, observer, dotNetRef } in chartRegistry Map
14.  Fire OnChartInitialized callback (if delegate is set)
```

### Why OnAfterRenderAsync?

JavaScript interop is not available during server-side prerendering or before the component's DOM elements exist. `OnAfterRenderAsync` is the earliest safe point to interact with the DOM.

### Chart ID

Each `<EChart>` component generates a unique ID using `Guid.NewGuid().ToString("N")`. This ID is:
- Set as the `id` attribute on the container `<div>`
- Used as the key in the JS `chartRegistry` Map
- Passed to all interop calls to identify which chart to operate on

This allows multiple `<EChart>` components on the same page without conflicts.

---

## Option Updates via setOption Merge

When the `Option` parameter changes after initial render, the component updates the existing chart rather than recreating it.

### Change Detection

Change detection happens in `OnParametersSet`:

1. The current `Option` is serialized to JSON using `EChartsInterop.SerializeOption()`
2. The resulting JSON string is compared to the previously stored `_lastOptionJson`
3. If different, a `_needsUpdate` flag is set

```csharp
protected override void OnParametersSet()
{
    var optionJson = EChartsInterop.SerializeOption(Option);
    if (optionJson != _lastOptionJson)
    {
        _needsUpdate = true;
    }
    _lastOptionJson = optionJson;
}
```

### Update Execution

In the next `OnAfterRenderAsync` call, if `_needsUpdate` is true:

1. `_interop.UpdateAsync(chartId, option)` is called
2. The JS `updateOption()` function parses the JSON and calls `chart.setOption(option, true)`
3. The second argument `true` enables merge mode, meaning only changed properties are updated

### Why Merge Mode?

ECharts `setOption(option, true)` merges the new configuration with the existing one. This is more efficient than full replacement because:
- Animations play smoothly between states
- Only the changed properties are processed
- Internal chart state (zoom level, selected legend items) is preserved

---

## Theme Changes Require Re-initialization

ECharts does not support changing the theme of an existing chart instance. When the `Theme` parameter changes, the component must destroy and recreate the chart.

### Detection

In `OnParametersSet`, the current theme is compared to `_currentTheme`:

```csharp
if (_currentTheme != Theme)
{
    _needsReinit = true;
}
```

### Re-initialization Sequence

```
1. OnAfterRenderAsync (not firstRender, _needsReinit == true)
2.   Call _interop.DisposeChartAsync(chartId)
3.     JS dispose():
4.       Disconnect ResizeObserver
5.       Call chart.dispose()
6.       Remove from chartRegistry
7.   Update _currentTheme = Theme
8.   Call _interop.InitAsync(chartId, dotNetRef, option, theme)
9.     JS initChart() with new theme (full initialization sequence)
```

---

## Disposal and Cleanup

The `<EChart>` component implements `IAsyncDisposable` to clean up all resources when the component is removed from the render tree.

### What Gets Cleaned Up

| Resource | Cleanup Action | Where |
|----------|---------------|-------|
| ECharts instance | `chart.dispose()` | JS `dispose()` |
| ResizeObserver | `observer.disconnect()` | JS `dispose()` |
| Debounce timers | `clearTimeout()` | JS `dispose()` |
| Chart registry entry | `chartRegistry.delete(elementId)` | JS `dispose()` |
| JS module reference | `module.DisposeAsync()` | C# `EChartsInterop.DisposeAsync()` |
| DotNetObjectReference | `_dotNetRef.Dispose()` | C# `EChart.DisposeAsync()` |

### Disposal Sequence

```
1. EChart.DisposeAsync() called by Blazor
2.   Guard: if (_disposed) return; _disposed = true;
3.   Call _interop.DisposeChartAsync(chartId)
4.     JS dispose() cleans up chart, observer, timers, registry entry
5.   Call _interop.DisposeAsync()
6.     If module was loaded, call module.DisposeAsync()
7.   Call _dotNetRef.Dispose()
```

### Error Resilience

All disposal steps are wrapped in try/catch blocks. Specific exceptions that are caught and swallowed:

- `JSDisconnectedException` -- occurs in Blazor Server when the SignalR circuit is already lost
- `ObjectDisposedException` -- occurs if the JS runtime is already disposed

This prevents disposal errors from crashing the application during navigation or circuit disconnection.

### Double-Dispose Protection

The `_disposed` boolean flag ensures `DisposeAsync` is idempotent. Calling it multiple times has no effect after the first call.

---

## Auto-Resize

Each chart automatically resizes when its container changes size.

### How It Works

1. During `initChart()`, a `ResizeObserver` is created and attached to the chart's container `<div>`
2. When the observer fires (container resized), a debounced `chart.resize()` call is made
3. The debounce delay is 100ms to avoid excessive resize calls during fluid layout changes

```javascript
const observer = new ResizeObserver(() => {
    debounce(elementId, () => {
        chart.resize();
    }, 100);
});
observer.observe(element);
```

### Cleanup

The `ResizeObserver` is disconnected during chart disposal. Debounce timers are also cleared.

---

## JS-Invokable Callbacks

The component exposes two methods that JavaScript can call back into C#:

### OnChartClick

```csharp
[JSInvokable]
public async Task OnChartClick(EChartClickEvent evt)
```

Triggered by the ECharts `"click"` event. The JS handler extracts relevant properties from the ECharts params object and sends them as a serializable `EChartClickEvent`:

```javascript
chart.on("click", function (params) {
    dotNetRef.invokeMethodAsync("OnChartClick", {
        name: params.name || "",
        seriesName: params.seriesName || "",
        dataIndex: params.dataIndex ?? 0,
        value: params.value != null ? JSON.stringify(params.value) : "",
        componentType: params.componentType || ""
    });
});
```

### OnLegendSelectChanged

```csharp
[JSInvokable]
public async Task OnLegendSelectChanged(string name)
```

Triggered by the ECharts `"legendselectchanged"` event. Sends just the legend item name as a string.

---

## Lifecycle Diagram

```
Component Created
       |
       v
OnParametersSet()
  - Serialize option to JSON
  - Set _needsInit = true (first time)
       |
       v
OnAfterRenderAsync(firstRender: true)
  - Create DotNetObjectReference
  - Create EChartsInterop
  - Import JS module (lazy)
  - Call initChart() in JS
  - Fire OnChartInitialized
       |
       v
  [Chart is live and interactive]
       |
       |--- User changes Option parameter
       |       |
       |       v
       |    OnParametersSet()
       |      - Compare JSON, set _needsUpdate = true
       |       |
       |       v
       |    OnAfterRenderAsync(firstRender: false)
       |      - Call updateOption() in JS (merge mode)
       |       |
       |       v
       |    [Chart updated with animation]
       |
       |--- User changes Theme parameter
       |       |
       |       v
       |    OnParametersSet()
       |      - Detect theme change, set _needsReinit = true
       |       |
       |       v
       |    OnAfterRenderAsync(firstRender: false)
       |      - Call dispose() in JS
       |      - Call initChart() in JS with new theme
       |       |
       |       v
       |    [Chart re-created with new theme]
       |
       |--- User clicks data point (in browser)
       |       |
       |       v
       |    JS chart.on("click") fires
       |      - dotNetRef.invokeMethodAsync("OnChartClick", event)
       |       |
       |       v
       |    C# OnChartClick() -> OnClick.InvokeAsync(evt)
       |
       v
Component Removed from Render Tree
       |
       v
DisposeAsync()
  - JS dispose(): chart, observer, timers, registry
  - C# DisposeAsync(): JS module reference
  - C# Dispose(): DotNetObjectReference
       |
       v
  [All resources released]
```

---

## See Also

- [API Reference](api-reference.md) -- full parameter and model documentation
- [Quick Start](quickstart.md) -- get a chart running
- [Overview](overview.md) -- project summary
