using System.Text.Json;
using Microsoft.JSInterop;

namespace BlazorECharts.Interop;

/// <summary>
/// Provides C# interop with the ECharts JavaScript module.
/// </summary>
public class EChartsInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new instance of <see cref="EChartsInterop"/>.
    /// </summary>
    /// <param name="jsRuntime">The Blazor JS runtime.</param>
    public EChartsInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _moduleTask = new Lazy<Task<IJSObjectReference>>(() =>
            _jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "/_content/BlazorECharts/echarts-interop.js").AsTask());
    }

    /// <summary>
    /// Initializes a chart on the specified element.
    /// </summary>
    public async Task InitAsync<T>(string elementId, DotNetObjectReference<T> dotNetRef, object option, string? theme) where T : class
    {
        if (_disposed) return;
        try
        {
            var module = await _moduleTask.Value;
            var optionJson = SerializeOption(option);
            await module.InvokeVoidAsync("initChart", elementId, dotNetRef, optionJson, theme);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
    }

    /// <summary>
    /// Updates the chart option using merge mode.
    /// </summary>
    public async Task UpdateAsync(string elementId, object option)
    {
        if (_disposed) return;
        try
        {
            var module = await _moduleTask.Value;
            var optionJson = SerializeOption(option);
            await module.InvokeVoidAsync("updateOption", elementId, optionJson);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
    }

    /// <summary>
    /// Triggers a chart resize.
    /// </summary>
    public async Task ResizeAsync(string elementId)
    {
        if (_disposed) return;
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("resize", elementId);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
    }

    /// <summary>
    /// Disposes a specific chart instance.
    /// </summary>
    public async Task DisposeChartAsync(string elementId)
    {
        if (_disposed) return;
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("dispose", elementId);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
    }

    /// <summary>
    /// Serializes the option object to JSON. If the option is already a string, it is returned as-is.
    /// </summary>
    internal static string SerializeOption(object option)
    {
        if (option is string raw)
        {
            return raw;
        }

        return JsonSerializer.Serialize(option, JsonOptions);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_moduleTask.IsValueCreated)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException) { }
            catch (ObjectDisposedException) { }
        }
    }
}
