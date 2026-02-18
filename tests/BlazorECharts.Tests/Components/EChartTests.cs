using BlazorECharts.Components;
using BlazorECharts.Models;
using BlazorECharts.Models.Events;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace BlazorECharts.Tests.Components;

/// <summary>
/// bUnit tests for the <see cref="EChart"/> Blazor component.
/// </summary>
public class EChartTests : TestContext
{
    private const string ModulePath = "/_content/Vora.BlazorECharts/echarts-interop.js";

    /// <summary>
    /// Helper to set up a loose JS module mock that accepts any calls.
    /// </summary>
    private BunitJSModuleInterop SetupModule()
    {
        var module = JSInterop.SetupModule(ModulePath);
        module.Mode = JSRuntimeMode.Loose;
        return module;
    }

    /// <summary>
    /// Helper to build a minimal EChartOption for testing.
    /// </summary>
    private static EChartOption CreateSimpleOption()
    {
        return new EChartOption
        {
            XAxis = new XAxis { Type = "category", Data = new List<string> { "A", "B", "C" } },
            YAxis = new YAxis { Type = "value" },
            Series = new List<Series>
            {
                new LineSeries { Name = "Test", Data = new object[] { 1, 2, 3 } }
            }
        };
    }

    [Fact]
    public void RendersDiv_WithDefaultWidthAndHeight()
    {
        // Arrange
        SetupModule();
        var option = CreateSimpleOption();

        // Act
        var cut = RenderComponent<EChart>(parameters => parameters
            .Add(p => p.Option, option));

        // Assert
        var div = cut.Find("div");
        Assert.Contains("width: 100%", div.GetAttribute("style"));
        Assert.Contains("height: 400px", div.GetAttribute("style"));
    }

    [Fact]
    public void RendersDiv_WithCustomWidthAndHeight()
    {
        // Arrange
        SetupModule();
        var option = CreateSimpleOption();

        // Act
        var cut = RenderComponent<EChart>(parameters => parameters
            .Add(p => p.Option, option)
            .Add(p => p.Width, "800px")
            .Add(p => p.Height, "600px"));

        // Assert
        var div = cut.Find("div");
        Assert.Contains("width: 800px", div.GetAttribute("style"));
        Assert.Contains("height: 600px", div.GetAttribute("style"));
    }

    [Fact]
    public void InitChart_IsCalledOnFirstRender()
    {
        // Arrange
        var module = SetupModule();
        var initInvocation = module.SetupVoid("initChart", _ => true);
        var option = CreateSimpleOption();

        // Act
        RenderComponent<EChart>(parameters => parameters
            .Add(p => p.Option, option));

        // Assert
        Assert.Single(initInvocation.Invocations);
        // Verify the first argument is the chart element ID (a 32-char hex string)
        var elementId = initInvocation.Invocations[0].Arguments[0]?.ToString();
        Assert.NotNull(elementId);
        Assert.Equal(32, elementId!.Length);
    }

    [Fact]
    public void UpdateOption_IsCalledWhenOptionParameterChanges()
    {
        // Arrange
        var module = SetupModule();
        module.SetupVoid("initChart", _ => true);
        var updateInvocation = module.SetupVoid("updateOption", _ => true);
        var option = CreateSimpleOption();

        var cut = RenderComponent<EChart>(parameters => parameters
            .Add(p => p.Option, option));

        // Act - change the option to a different object
        var newOption = new EChartOption
        {
            XAxis = new XAxis { Type = "category", Data = new List<string> { "X", "Y", "Z" } },
            YAxis = new YAxis { Type = "value" },
            Series = new List<Series>
            {
                new BarSeries { Name = "Updated", Data = new object[] { 10, 20, 30 } }
            }
        };

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Option, newOption));

        // Assert
        Assert.Single(updateInvocation.Invocations);
    }

    [Fact]
    public async Task Dispose_IsCalledOnComponentDisposal()
    {
        // Arrange
        var module = SetupModule();
        module.SetupVoid("initChart", _ => true);
        var disposeInvocation = module.SetupVoid("dispose", _ => true);
        var option = CreateSimpleOption();

        var cut = RenderComponent<EChart>(parameters => parameters
            .Add(p => p.Option, option));

        // Act
        await cut.Instance.DisposeAsync();

        // Assert
        Assert.Single(disposeInvocation.Invocations);
    }

    [Fact]
    public void MultipleCharts_HaveDifferentIds()
    {
        // Arrange
        SetupModule();
        var option = CreateSimpleOption();

        // Act
        var cut1 = RenderComponent<EChart>(parameters => parameters
            .Add(p => p.Option, option));
        var cut2 = RenderComponent<EChart>(parameters => parameters
            .Add(p => p.Option, option));

        // Assert
        var div1 = cut1.Find("div");
        var div2 = cut2.Find("div");
        var id1 = div1.GetAttribute("id");
        var id2 = div2.GetAttribute("id");

        Assert.NotNull(id1);
        Assert.NotNull(id2);
        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public async Task OnChartClick_FiresEventCallback()
    {
        // Arrange
        SetupModule();
        var option = CreateSimpleOption();
        EChartClickEvent? receivedEvent = null;

        var cut = RenderComponent<EChart>(parameters => parameters
            .Add(p => p.Option, option)
            .Add(p => p.OnClick, EventCallback.Factory.Create<EChartClickEvent>(
                this, evt => receivedEvent = evt)));

        var clickEvent = new EChartClickEvent
        {
            Name = "A",
            SeriesName = "Test",
            DataIndex = 0,
            Value = "1",
            ComponentType = "series"
        };

        // Act - invoke the JS-invokable method directly
        await cut.Instance.OnChartClick(clickEvent);

        // Assert
        Assert.NotNull(receivedEvent);
        Assert.Equal("A", receivedEvent!.Name);
        Assert.Equal("Test", receivedEvent.SeriesName);
        Assert.Equal(0, receivedEvent.DataIndex);
        Assert.Equal("1", receivedEvent.Value);
        Assert.Equal("series", receivedEvent.ComponentType);
    }

    [Fact]
    public async Task OnLegendSelectChanged_FiresEventCallback()
    {
        // Arrange
        SetupModule();
        var option = CreateSimpleOption();
        string? receivedName = null;

        var cut = RenderComponent<EChart>(parameters => parameters
            .Add(p => p.Option, option)
            .Add(p => p.OnLegendSelect, EventCallback.Factory.Create<string>(
                this, name => receivedName = name)));

        // Act - invoke the JS-invokable method directly
        await cut.Instance.OnLegendSelectChanged("Sales");

        // Assert
        Assert.NotNull(receivedName);
        Assert.Equal("Sales", receivedName);
    }

    [Fact]
    public async Task OnChartClick_DoesNotThrow_WhenNoCallback()
    {
        // Arrange
        SetupModule();
        var option = CreateSimpleOption();

        var cut = RenderComponent<EChart>(parameters => parameters
            .Add(p => p.Option, option));

        var clickEvent = new EChartClickEvent
        {
            Name = "A",
            SeriesName = "Test",
            DataIndex = 0,
            Value = "1",
            ComponentType = "series"
        };

        // Act & Assert - should not throw when no callback is registered
        var exception = await Record.ExceptionAsync(() => cut.Instance.OnChartClick(clickEvent));
        Assert.Null(exception);
    }

    [Fact]
    public async Task OnLegendSelectChanged_DoesNotThrow_WhenNoCallback()
    {
        // Arrange
        SetupModule();
        var option = CreateSimpleOption();

        var cut = RenderComponent<EChart>(parameters => parameters
            .Add(p => p.Option, option));

        // Act & Assert - should not throw when no callback is registered
        var exception = await Record.ExceptionAsync(() => cut.Instance.OnLegendSelectChanged("Sales"));
        Assert.Null(exception);
    }
}
