const chartRegistry = new Map();

let resizeDebounceTimers = new Map();

function debounce(elementId, fn, delay) {
    if (resizeDebounceTimers.has(elementId)) {
        clearTimeout(resizeDebounceTimers.get(elementId));
    }
    const timer = setTimeout(() => {
        fn();
        resizeDebounceTimers.delete(elementId);
    }, delay);
    resizeDebounceTimers.set(elementId, timer);
}

export function initChart(elementId, dotNetRef, optionJson, theme) {
    try {
        const element = document.getElementById(elementId);
        if (!element) {
            console.error(`[BlazorECharts] Element with ID '${elementId}' not found.`);
            return;
        }

        if (chartRegistry.has(elementId)) {
            dispose(elementId);
        }

        const chart = echarts.init(element, theme || undefined);

        const option = JSON.parse(optionJson);
        chart.setOption(option);

        chart.on("click", function (params) {
            try {
                dotNetRef.invokeMethodAsync("OnChartClick", {
                    name: params.name || "",
                    seriesName: params.seriesName || "",
                    dataIndex: params.dataIndex ?? 0,
                    value: params.value != null ? JSON.stringify(params.value) : "",
                    componentType: params.componentType || ""
                });
            } catch (e) {
                console.error("[BlazorECharts] Error invoking OnChartClick:", e);
            }
        });

        chart.on("legendselectchanged", function (params) {
            try {
                dotNetRef.invokeMethodAsync("OnLegendSelectChanged", params.name || "");
            } catch (e) {
                console.error("[BlazorECharts] Error invoking OnLegendSelectChanged:", e);
            }
        });

        const observer = new ResizeObserver(() => {
            debounce(elementId, () => {
                try {
                    if (chartRegistry.has(elementId)) {
                        chart.resize();
                    }
                } catch (e) {
                    console.error("[BlazorECharts] Error during resize:", e);
                }
            }, 100);
        });
        observer.observe(element);

        chartRegistry.set(elementId, { chart, observer, dotNetRef });
    } catch (e) {
        console.error("[BlazorECharts] Error initializing chart:", e);
    }
}

export function updateOption(elementId, optionJson) {
    try {
        const entry = chartRegistry.get(elementId);
        if (!entry) {
            console.error(`[BlazorECharts] Chart '${elementId}' not found for update.`);
            return;
        }
        const option = JSON.parse(optionJson);
        entry.chart.setOption(option, true);
    } catch (e) {
        console.error("[BlazorECharts] Error updating chart option:", e);
    }
}

export function resize(elementId) {
    try {
        const entry = chartRegistry.get(elementId);
        if (entry) {
            entry.chart.resize();
        }
    } catch (e) {
        console.error("[BlazorECharts] Error resizing chart:", e);
    }
}

export function dispose(elementId) {
    try {
        const entry = chartRegistry.get(elementId);
        if (entry) {
            if (entry.observer) {
                entry.observer.disconnect();
            }
            if (entry.chart) {
                entry.chart.dispose();
            }
            chartRegistry.delete(elementId);
        }
        if (resizeDebounceTimers.has(elementId)) {
            clearTimeout(resizeDebounceTimers.get(elementId));
            resizeDebounceTimers.delete(elementId);
        }
    } catch (e) {
        console.error("[BlazorECharts] Error disposing chart:", e);
    }
}
