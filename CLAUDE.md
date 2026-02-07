# CLAUDE.md — BlazorECharts

## Project Overview

**BlazorECharts** is a Blazor component library that wraps [Apache ECharts](https://echarts.apache.org/) v6 for use in both Blazor WebAssembly and Blazor Server applications. It ships as a NuGet package (`BlazorECharts`) exposing Razor components, strongly-typed C# option models, and JavaScript interop for chart lifecycle management.

### Supported Chart Types

Line, Bar, Pie, Sankey — with an extensible architecture for adding more.

### Target Frameworks

- `net8.0`
- `net9.0`

---

## Repository Structure

```
BlazorECharts/
├── CLAUDE.md                          # This file — AI assistant guide
├── PLAN.md                            # Phased implementation plan
├── README.md                          # User-facing documentation
├── BlazorECharts.sln                  # Solution file
│
├── src/
│   └── BlazorECharts/                 # Main library project
│       ├── BlazorECharts.csproj
│       ├── Components/
│       │   └── EChart.razor           # Primary <EChart /> component
│       │   └── EChart.razor.cs        # Component code-behind
│       ├── Interop/
│       │   └── EChartsInterop.cs      # C# JS interop wrapper
│       ├── Models/
│       │   ├── EChartOption.cs         # Root option model
│       │   ├── XAxis.cs
│       │   ├── YAxis.cs
│       │   ├── Series.cs              # Base series class
│       │   ├── LineSeries.cs
│       │   ├── BarSeries.cs
│       │   ├── PieSeries.cs
│       │   ├── SankeySeries.cs
│       │   └── Events/
│       │       ├── EChartClickEvent.cs
│       │       └── EChartLegendEvent.cs
│       └── wwwroot/
│           └── echarts-interop.js     # JS module for ECharts interop
│
├── samples/
│   └── BlazorECharts.Samples/        # Sample Blazor project
│       ├── BlazorECharts.Samples.csproj
│       └── Pages/
│           ├── LineChartExample.razor
│           ├── BarChartExample.razor
│           ├── PieChartExample.razor
│           ├── SankeyChartExample.razor
│           ├── DynamicUpdateExample.razor
│           ├── ClickEventExample.razor
│           ├── TypedModelExample.razor
│           ├── DarkThemeExample.razor
│           └── ResizableExample.razor
│
├── tests/
│   └── BlazorECharts.Tests/          # Unit/integration tests
│       ├── BlazorECharts.Tests.csproj
│       ├── Components/
│       │   └── EChartTests.cs
│       └── Models/
│           └── OptionSerializationTests.cs
│
└── docs/                              # Extended documentation
    ├── overview.md
    ├── quickstart.md
    ├── api-reference.md
    └── interop-lifecycle.md
```

---

## Build & Run Commands

```bash
# Restore all packages
dotnet restore BlazorECharts.sln

# Build entire solution (library + samples + tests)
dotnet build BlazorECharts.sln

# Build library only
dotnet build src/BlazorECharts/BlazorECharts.csproj

# Run tests
dotnet test tests/BlazorECharts.Tests/BlazorECharts.Tests.csproj

# Run sample app
dotnet run --project samples/BlazorECharts.Samples/BlazorECharts.Samples.csproj

# Create NuGet package
dotnet pack src/BlazorECharts/BlazorECharts.csproj -c Release -o ./artifacts
```

---

## Code Conventions

### C# Style

- **Namespace root:** `BlazorECharts` — sub-namespaces mirror folder structure (`BlazorECharts.Models`, `BlazorECharts.Interop`, `BlazorECharts.Components`).
- **Naming:** PascalCase for public members, `_camelCase` for private fields. No Hungarian notation.
- **Nullable reference types:** Enabled project-wide. All public API parameters must have explicit nullability annotations.
- **File-scoped namespaces:** Use `namespace BlazorECharts.Models;` (not block-scoped).
- **One type per file** — file name matches type name.
- **Blazor components** use code-behind pattern: `EChart.razor` for markup, `EChart.razor.cs` for logic.
- **Access modifiers:** Always explicit (`public`, `private`, `internal`). No implicit access.

### JSON Serialization

- Use `System.Text.Json` exclusively — no Newtonsoft.
- All option model properties use `[JsonPropertyName("camelCase")]` to match ECharts JSON schema.
- Null properties must be excluded from serialization via `JsonIgnoreCondition.WhenWritingNull` (set at the serializer options level).
- The `JsonSerializerOptions` instance should be shared/cached, never created per-call.

### JavaScript Interop

- JS is in a single ES module: `wwwroot/echarts-interop.js`.
- Import ECharts as an ES module (`import * as echarts from ...`).
- All chart instances are tracked in a `Map<string, { chart, observer }>` keyed by a unique chart ID.
- .NET calls JS via `IJSRuntime.InvokeAsync` / `InvokeVoidAsync`.
- JS calls .NET via `dotNetRef.invokeMethodAsync("MethodName", payload)`.
- All JS-invocable .NET methods must be decorated with `[JSInvokable]`.

### Component Lifecycle

1. `OnAfterRenderAsync(firstRender: true)` — import JS module, call `initChart`.
2. `OnParametersSetAsync` — detect option changes, call `updateOption` (not re-init).
3. `IAsyncDisposable.DisposeAsync` — call `dispose` in JS, release `DotNetObjectReference`, dispose JS module reference.

### Error Handling

- Never throw from component lifecycle methods — catch, log to console, and surface meaningful messages.
- JS interop failures must be caught and logged; the component should degrade gracefully.
- Parameter validation: check required parameters in `OnParametersSet`, log warnings via `Console.WriteLine` or `ILogger` if available.

---

## Key Design Decisions

### Option Parameter Accepts `object`

The `Option` parameter is typed as `object` to support three usage modes:
1. **Strongly-typed model** (`EChartOption`) — serialized to JSON automatically.
2. **Anonymous object** — serialized to JSON automatically.
3. **Raw JSON string** — passed directly to JS without serialization.

The interop layer must detect the type and serialize accordingly.

### Update Merging (Not Recreating)

When `Option` changes, the JS side calls `chart.setOption(newOption, true)` to merge rather than replace. The chart is never re-initialized unless the theme changes.

### Auto-Resize

Each chart attaches a `ResizeObserver` to its container `<div>` in JS. On resize, it calls `chart.resize()`. The observer is cleaned up in `dispose`.

### Unique Chart IDs

Each `<EChart />` instance generates a unique ID (`Guid.NewGuid().ToString("N")`) used to key the JS instance map. This allows multiple charts per page without conflicts.

---

## Testing Approach

- **Unit tests** use [bUnit](https://bunit.dev/) for Blazor component testing.
- **Serialization tests** verify that C# option models produce correct ECharts-compatible JSON.
- **Test framework:** xUnit.
- **Mocking:** JS interop is mocked in bUnit tests via `JSInterop.Setup*`.
- Tests must cover: rendering, option updates, disposal, event callbacks, multi-chart scenarios.

---

## Dependency Policy

- The library itself has **zero external NuGet dependencies** beyond the default Blazor/ASP.NET Core framework references.
- Test project may reference: `bunit`, `xunit`, `Microsoft.NET.Test.Sdk`.
- Sample project references the library via project reference.
- Apache ECharts JS is loaded via CDN (`<script>` tag) by the consuming app — the library does NOT bundle the ECharts JS file.

---

## NuGet Packaging

- Package ID: `BlazorECharts`
- Static web assets in `wwwroot/` are automatically included via the Razor Class Library SDK.
- The `.csproj` must set:
  - `<IsPackable>true</IsPackable>`
  - `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
  - `<PackageReadmeFile>README.md</PackageReadmeFile>`
- Pack command: `dotnet pack -c Release`

---

## Common Pitfalls

1. **JS interop before render:** Never call JS interop in `OnInitializedAsync` — only in `OnAfterRenderAsync`.
2. **Disposing twice:** Guard `DisposeAsync` with a `_disposed` flag to prevent double-dispose errors.
3. **Serialization casing:** ECharts expects camelCase JSON keys. Always verify `[JsonPropertyName]` attributes match ECharts docs.
4. **Server-side prerendering:** JS interop is unavailable during prerendering. Guard calls with `firstRender` checks.
5. **Large option objects:** Avoid sending huge datasets via JS interop on Blazor Server (SignalR message size limits). Document this limitation.
6. **Theme changes:** Changing theme requires chart disposal and re-initialization (ECharts limitation). Handle this in the component.

---

## Git Workflow

- Development happens on feature branches prefixed with `claude/`.
- Commits should be atomic and descriptive.
- Push to the assigned branch; never force-push without explicit permission.
- PR descriptions should reference the relevant PLAN.md phase.
