using BlazorECharts.Interop;
using BlazorECharts.Models.Events;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorECharts.Components;

/// <summary>
/// Blazor component that wraps an Apache ECharts instance.
/// </summary>
public partial class EChart : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// The chart option object. Accepts a strongly-typed <see cref="Models.EChartOption"/>,
    /// an anonymous object, or a raw JSON string.
    /// </summary>
    [Parameter]
    public object Option { get; set; } = default!;

    /// <summary>
    /// Height of the chart container. Default is "400px".
    /// </summary>
    [Parameter]
    public string Height { get; set; } = "400px";

    /// <summary>
    /// Width of the chart container. Default is "100%".
    /// </summary>
    [Parameter]
    public string Width { get; set; } = "100%";

    /// <summary>
    /// ECharts theme name. Changing the theme triggers chart re-initialization.
    /// </summary>
    [Parameter]
    public string? Theme { get; set; }

    /// <summary>
    /// Callback invoked when a data point is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<EChartClickEvent> OnClick { get; set; }

    /// <summary>
    /// Callback invoked when a legend item selection changes.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnLegendSelect { get; set; }

    /// <summary>
    /// Callback invoked after the chart is initialized.
    /// </summary>
    [Parameter]
    public EventCallback OnChartInitialized { get; set; }

    private ElementReference _element;
    private readonly string _chartId = Guid.NewGuid().ToString("N");
    private DotNetObjectReference<EChart>? _dotNetRef;
    private EChartsInterop? _interop;
    private bool _disposed;
    private string? _currentTheme;
    private string? _lastOptionJson;
    private bool _needsInit;
    private bool _needsUpdate;
    private bool _needsReinit;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (Option is null) return;

        var optionJson = EChartsInterop.SerializeOption(Option);

        if (_lastOptionJson is null)
        {
            _needsInit = true;
        }
        else if (_currentTheme != Theme)
        {
            _needsReinit = true;
        }
        else if (optionJson != _lastOptionJson)
        {
            _needsUpdate = true;
        }

        _lastOptionJson = optionJson;
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _interop = new EChartsInterop(JSRuntime);
            _dotNetRef = DotNetObjectReference.Create(this);
            _currentTheme = Theme;

            try
            {
                await _interop.InitAsync(_chartId, _dotNetRef, Option, Theme);
                _needsInit = false;

                if (OnChartInitialized.HasDelegate)
                {
                    await OnChartInitialized.InvokeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BlazorECharts] Error initializing chart: {ex.Message}");
            }
        }
        else if (_needsReinit && _interop is not null)
        {
            try
            {
                await _interop.DisposeChartAsync(_chartId);
                _currentTheme = Theme;
                await _interop.InitAsync(_chartId, _dotNetRef!, Option, Theme);
                _needsReinit = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BlazorECharts] Error re-initializing chart: {ex.Message}");
            }
        }
        else if (_needsUpdate && _interop is not null)
        {
            try
            {
                await _interop.UpdateAsync(_chartId, Option);
                _needsUpdate = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BlazorECharts] Error updating chart: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// JS-invokable callback for chart click events.
    /// </summary>
    [JSInvokable]
    public async Task OnChartClick(EChartClickEvent evt)
    {
        if (OnClick.HasDelegate)
        {
            await OnClick.InvokeAsync(evt);
        }
    }

    /// <summary>
    /// JS-invokable callback for legend selection change events.
    /// </summary>
    [JSInvokable]
    public async Task OnLegendSelectChanged(string name)
    {
        if (OnLegendSelect.HasDelegate)
        {
            await OnLegendSelect.InvokeAsync(name);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_interop is not null)
        {
            try
            {
                await _interop.DisposeChartAsync(_chartId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BlazorECharts] Error disposing chart: {ex.Message}");
            }

            await _interop.DisposeAsync();
        }

        _dotNetRef?.Dispose();
    }
}
