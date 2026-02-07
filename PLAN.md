# PLAN.md — BlazorECharts Implementation Plan

This document defines the phased implementation plan for the BlazorECharts library. Each phase builds on the previous one and produces a testable, working increment.

---

## Phase 1: Project Scaffolding & Solution Structure

**Goal:** Establish the solution, project files, folder structure, and build pipeline so that `dotnet build` and `dotnet test` succeed (even with no real logic yet).

### Tasks

1. **Create solution file** — `BlazorECharts.sln` at repository root.
2. **Create library project** — `src/BlazorECharts/BlazorECharts.csproj`
   - Razor Class Library (`Sdk="Microsoft.NET.Sdk.Razor"`).
   - Multi-target: `net8.0;net9.0`.
   - Enable nullable reference types.
   - Set `<IsPackable>true</IsPackable>`, `<GenerateDocumentationFile>true</GenerateDocumentationFile>`.
   - Add `<PackageReadmeFile>README.md</PackageReadmeFile>` and include README in pack.
3. **Create test project** — `tests/BlazorECharts.Tests/BlazorECharts.Tests.csproj`
   - Target `net8.0` (or `net9.0`).
   - References: `bunit`, `xunit`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`.
   - Project reference to library.
4. **Create sample project** — `samples/BlazorECharts.Samples/BlazorECharts.Samples.csproj`
   - Blazor WebAssembly (standalone) targeting `net8.0`.
   - Project reference to library.
5. **Create folder structure** in library:
   - `Components/`
   - `Interop/`
   - `Models/`
   - `Models/Events/`
   - `wwwroot/`
6. **Create `.gitignore`** for .NET (bin, obj, artifacts, .vs, etc.).
7. **Add all projects to solution** and verify `dotnet build BlazorECharts.sln` succeeds.

### Acceptance

- `dotnet restore` succeeds.
- `dotnet build` succeeds with zero errors.
- `dotnet test` runs (0 tests is fine).
- Solution structure matches CLAUDE.md layout.

---

## Phase 2: JavaScript Interop Module

**Goal:** Implement the JS module that manages ECharts instances, auto-resize, and event forwarding.

### Tasks

1. **Create `wwwroot/echarts-interop.js`** as an ES module with the following exports:
   - `initChart(elementId, dotNetRef, optionJson, theme)` — creates an ECharts instance on the given DOM element, applies the option, registers event listeners, attaches a `ResizeObserver`.
   - `updateOption(elementId, optionJson)` — calls `chart.setOption(JSON.parse(optionJson), true)` for merge update.
   - `resize(elementId)` — calls `chart.resize()`.
   - `dispose(elementId)` — disconnects `ResizeObserver`, disposes chart, removes from internal map.
2. **Internal chart registry** — `Map<string, { chart, observer, dotNetRef }>`.
3. **Event forwarding:**
   - `chart.on("click", ...)` → `dotNetRef.invokeMethodAsync("OnChartClick", { name, seriesName, dataIndex, value, ... })`.
   - `chart.on("legendselectchanged", ...)` → `dotNetRef.invokeMethodAsync("OnLegendSelectChanged", legendName)`.
4. **ResizeObserver** — attached per chart to its container `<div>`, calls `chart.resize()` on size change with a debounce (~100ms).
5. **Error handling** — wrap all operations in try/catch, log to `console.error`.

### Design Notes

- ECharts is NOT bundled. The consuming app loads it via a `<script>` tag (CDN or local). The JS module accesses `echarts` from the global scope (`window.echarts`) or via import map.
- The module is loaded by C# via `IJSRuntime.InvokeAsync<IJSObjectReference>("import", "/_content/BlazorECharts/echarts-interop.js")`.

### Acceptance

- JS file loads without syntax errors.
- Manual test: calling `initChart` with a DOM element and valid option JSON renders a chart.

---

## Phase 3: C# Interop Layer

**Goal:** Create the C# class that wraps JS interop calls with proper async patterns and error handling.

### Tasks

1. **Create `Interop/EChartsInterop.cs`:**
   ```csharp
   public class EChartsInterop : IAsyncDisposable
   {
       Task InitAsync(string elementId, DotNetObjectReference<T> dotNetRef, string optionJson, string? theme);
       Task UpdateAsync(string elementId, string optionJson);
       Task ResizeAsync(string elementId);
       Task DisposeChartAsync(string elementId);
       ValueTask DisposeAsync(); // disposes the JS module reference
   }
   ```
2. The class holds an `IJSObjectReference` (the imported module) and a `Lazy<Task<IJSObjectReference>>` for deferred module import.
3. All methods must guard against:
   - Module not yet loaded (await the lazy import).
   - `JSDisconnectedException` (Blazor Server circuit lost) — catch and swallow.
   - `ObjectDisposedException` — catch and swallow.
4. Serialization helper: a private method that takes `object option` and returns JSON string:
   - If `option` is `string` → assume raw JSON, pass through.
   - Otherwise → serialize with cached `JsonSerializerOptions` (camelCase, ignore nulls).

### Acceptance

- Class compiles with no warnings.
- Unit test verifies serialization helper outputs correct JSON for a typed model and for a raw string.

---

## Phase 4: Strongly-Typed Option Models

**Goal:** Provide C# classes that map to the ECharts JSON `option` structure for Line, Bar, Pie, and Sankey charts.

### Tasks

1. **`Models/EChartOption.cs`** — root option:
   - `Title` (object with `Text`, `Subtext`, `Left`, etc.)
   - `Tooltip` (object with `Trigger`, `Formatter`, etc.)
   - `Legend` (object with `Data`, `Orient`, etc.)
   - `XAxis` / `YAxis` (list or single)
   - `Series` (list of `Series` base)
   - `Grid`, `Color`, `BackgroundColor` — optional common properties.
2. **`Models/XAxis.cs`** — `Type` ("category"/"value"), `Data`, `Name`, `BoundaryGap`, etc.
3. **`Models/YAxis.cs`** — `Type`, `Name`, `Min`, `Max`, etc.
4. **`Models/Series.cs`** — abstract base: `Name`, `Type` (discriminator), `Data`.
5. **`Models/LineSeries.cs`** — extends Series: `Smooth`, `AreaStyle`, `Stack`, `Symbol`, `LineStyle`.
6. **`Models/BarSeries.cs`** — extends Series: `Stack`, `BarWidth`, `ItemStyle`.
7. **`Models/PieSeries.cs`** — extends Series: `Radius`, `Center`, `RoseType`, `Label`, `Emphasis`.
8. **`Models/SankeySeries.cs`** — extends Series: `Links` (list of `{ source, target, value }`), `Nodes`/`Data`, `Orient`, `Label`.
9. **`Models/Events/EChartClickEvent.cs`** — `Name`, `SeriesName`, `DataIndex`, `Value`, `ComponentType`.
10. **`Models/Events/EChartLegendEvent.cs`** — `Name`, `Selected` (dictionary).
11. **Polymorphic serialization** — Series list must serialize the `Type` discriminator. Use `[JsonDerivedType]` (net8+) or a custom converter.

### Design Notes

- Keep models simple — only include commonly used properties. Users can always fall back to anonymous objects for advanced options.
- Every property should be nullable so users only set what they need.
- Property names must match ECharts JSON keys exactly via `[JsonPropertyName]`.

### Acceptance

- Serialization tests: create a typed `EChartOption` with `LineSeries`, serialize to JSON, verify output matches expected ECharts JSON structure.
- Repeat for Bar, Pie, Sankey.
- Verify null properties are omitted from JSON output.

---

## Phase 5: EChart Razor Component

**Goal:** Implement the `<EChart />` Blazor component that ties everything together.

### Tasks

1. **`Components/EChart.razor`** — markup:
   ```html
   <div @ref="_element" id="@_chartId" style="width: @Width; height: @Height;"></div>
   ```
2. **`Components/EChart.razor.cs`** — code-behind (partial class):
   - **Parameters:**
     - `[Parameter] public object Option { get; set; }` — required.
     - `[Parameter] public string Height { get; set; } = "400px";`
     - `[Parameter] public string Width { get; set; } = "100%";`
     - `[Parameter] public string? Theme { get; set; }`
     - `[Parameter] public EventCallback<EChartClickEvent> OnClick { get; set; }`
     - `[Parameter] public EventCallback<string> OnLegendSelect { get; set; }`
     - `[Parameter] public EventCallback OnInitialized { get; set; }` (renamed to avoid conflict with lifecycle — use `OnChartInitialized`).
   - **Fields:**
     - `ElementReference _element`
     - `string _chartId = Guid.NewGuid().ToString("N")`
     - `DotNetObjectReference<EChart> _dotNetRef`
     - `EChartsInterop _interop` (injected or created)
     - `bool _disposed`
     - `string? _currentTheme` (track for re-init on change)
     - `object? _previousOption` (track for change detection)
   - **Lifecycle:**
     - `OnAfterRenderAsync(firstRender)`:
       - If `firstRender` → create `_dotNetRef`, call `_interop.InitAsync(...)`, invoke `OnChartInitialized`.
       - If not `firstRender` and option changed → call `_interop.UpdateAsync(...)`.
       - If theme changed → dispose and re-init.
     - `OnParametersSetAsync` → detect option/theme changes, set flags for `OnAfterRenderAsync`.
     - `DisposeAsync` → guard with `_disposed`, call `_interop.DisposeChartAsync(...)`, dispose `_dotNetRef`.
   - **JS-invokable callbacks:**
     - `[JSInvokable] public async Task OnChartClick(EChartClickEvent evt)` → invoke `OnClick.InvokeAsync(evt)`.
     - `[JSInvokable] public async Task OnLegendSelectChanged(string name)` → invoke `OnLegendSelect.InvokeAsync(name)`.
3. **Option change detection:** Compare serialized JSON of current vs previous option (simple string equality on the JSON). Cache the last-sent JSON to avoid unnecessary updates.

### Acceptance

- bUnit test: render `<EChart Option="@opt" />`, verify the container `<div>` is rendered with correct `style`.
- bUnit test: verify JS interop `initChart` is called on first render.
- bUnit test: verify `updateOption` is called when `Option` parameter changes.
- bUnit test: verify `dispose` is called on component disposal.

---

## Phase 6: Sample Application

**Goal:** Build a working Blazor WebAssembly sample app demonstrating all features.

### Tasks

1. **Set up sample project** with:
   - `index.html` including ECharts CDN `<script>` tag.
   - Navigation sidebar linking to all example pages.
   - Project reference to `BlazorECharts`.
2. **Create example pages:**
   - `LineChartExample.razor` — basic line chart with multiple series.
   - `BarChartExample.razor` — grouped/stacked bar chart.
   - `PieChartExample.razor` — pie chart with labels.
   - `SankeyChartExample.razor` — Sankey diagram with nodes and links.
   - `DynamicUpdateExample.razor` — timer-based data update showing live chart changes.
   - `ClickEventExample.razor` — display clicked data point info.
   - `TypedModelExample.razor` — build chart option using strongly-typed C# models.
   - `DarkThemeExample.razor` — toggle between light and dark themes.
   - `ResizableExample.razor` — chart in a resizable flexbox container.
3. Each page includes a brief description and the relevant code snippet.

### Acceptance

- `dotnet run` on sample project starts without errors.
- All 9 example pages render correctly in a browser.
- Charts respond to resize, clicks, and option updates.

---

## Phase 7: Testing

**Goal:** Comprehensive test coverage for the library.

### Tasks

1. **Component rendering tests** (bUnit):
   - Renders container div with correct dimensions.
   - Calls `initChart` JS interop on first render.
   - Calls `updateOption` when option changes.
   - Calls `dispose` on component disposal.
   - Handles multiple charts on same page (different IDs).
2. **Event callback tests** (bUnit):
   - Simulate JS-invokable `OnChartClick` — verify `EventCallback` fires.
   - Simulate JS-invokable `OnLegendSelectChanged` — verify callback fires.
3. **Serialization tests** (xUnit):
   - `EChartOption` with `LineSeries` → correct JSON.
   - `EChartOption` with `BarSeries` → correct JSON.
   - `EChartOption` with `PieSeries` → correct JSON.
   - `EChartOption` with `SankeySeries` → correct JSON.
   - Null properties are omitted.
   - `[JsonPropertyName]` casing is correct.
   - Polymorphic Series list serializes `type` discriminator.
4. **Interop layer tests:**
   - Raw JSON string passes through without re-serialization.
   - Typed object is serialized to JSON.
   - Graceful handling of disposed interop (no throw).

### Acceptance

- `dotnet test` passes with all tests green.
- Coverage includes: rendering, updates, disposal, events, serialization, multi-chart.

---

## Phase 8: Documentation

**Goal:** Complete documentation for library consumers.

### Tasks

1. **Update `README.md`** with:
   - Project description and badges.
   - Quick start: install, add script tag, first chart in 5 lines.
   - Parameter reference table.
   - Code examples for each chart type.
   - Links to detailed docs.
2. **`docs/overview.md`** — what ECharts is, what this wrapper does, supported features.
3. **`docs/quickstart.md`** — step-by-step: install NuGet, add `<script>`, add `@using`, render `<EChart>`.
4. **`docs/api-reference.md`** — full parameter list, event models, option models.
5. **`docs/interop-lifecycle.md`** — how JS interop works, lifecycle diagram, update vs re-init, disposal.
6. **XML doc comments** on all public types and members in the library.

### Acceptance

- README contains working copy-paste examples.
- All public APIs have XML doc comments.
- `dotnet build` produces no missing-XML-comment warnings for public members.

---

## Phase 9: NuGet Packaging & Final Polish

**Goal:** Produce a shippable `.nupkg` and verify end-to-end.

### Tasks

1. **Finalize `.csproj` metadata:**
   - `PackageId`, `Version`, `Authors`, `Description`, `PackageTags`, `RepositoryUrl`, `License`.
   - `<PackageReadmeFile>README.md</PackageReadmeFile>`.
   - Include README in package: `<None Include="../../README.md" Pack="true" PackagePath="/" />`.
2. **Run `dotnet pack -c Release -o ./artifacts`** — verify `.nupkg` is created.
3. **Inspect package** — verify it contains:
   - Compiled DLLs for net8.0 and net9.0.
   - `staticwebassets` with `echarts-interop.js`.
   - README.md.
   - XML documentation.
4. **End-to-end test:** Create a throwaway Blazor app, install the local `.nupkg`, render all chart types.
5. **Clean up:** Remove any TODO comments, unused code, debug logging.

### Acceptance

- `dotnet pack` succeeds.
- `.nupkg` contains all expected files.
- A fresh Blazor app can install and use the package.

---

## Phase Summary

| Phase | Name                        | Depends On | Key Deliverable                        |
| ----- | --------------------------- | ---------- | -------------------------------------- |
| 1     | Project Scaffolding         | —          | Building solution with empty projects  |
| 2     | JavaScript Interop Module   | 1          | `echarts-interop.js`                   |
| 3     | C# Interop Layer            | 1          | `EChartsInterop.cs`                    |
| 4     | Strongly-Typed Models       | 1          | Option model classes                   |
| 5     | EChart Razor Component      | 2, 3, 4   | `<EChart />` component                 |
| 6     | Sample Application          | 5          | Working demo app                       |
| 7     | Testing                     | 5          | Full test suite                        |
| 8     | Documentation               | 5, 6       | README, docs, XML comments             |
| 9     | NuGet Packaging             | All        | Shippable `.nupkg`                     |

Phases 2, 3, and 4 can be developed in parallel since they have no cross-dependencies (only Phase 1).
Phases 6, 7, and 8 can also be partially parallelized after Phase 5 is complete.
