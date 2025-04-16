using CudaHelioCommanderLight.Interfaces;
using ScottPlot;
using System.Drawing;

public class PlotWrapper : IPlotWrapper
{
    private readonly Plot _plot;

    public PlotWrapper(Plot plot)
    {
        _plot = plot;
    }

    public void PlotScatter(double[] x, double[] y, int markerSize, Color color, string label)
    {
        _plot.PlotScatter(x, y, markerSize: markerSize, color: color, label: label);
    }

    public void XTicks(double[] positions, string[] labels)
    {
        _plot.XTicks(positions, labels);
    }

    public void YTicks(double[] positions, string[] labels)
    {
        _plot.YTicks(positions, labels);
    }

    public void PlotHSpan(double x1, double x2, bool draggable, Color color, double alpha)
    {
        _plot.PlotHSpan(x1: x1, x2: x2, draggable: draggable, color: color, alpha: alpha);
    }

    public void Ticks(bool useExponentialNotation = false, bool logScaleX = false, bool logScaleY = false)
    {
        _plot.Ticks(useExponentialNotation: useExponentialNotation,
                    logScaleX: logScaleX,
                    logScaleY: logScaleY);
    }

    public void Title(string title) => _plot.Title(title);

    public void YLabel(string label) => _plot.YLabel(label);

    public void XLabel(string label) => _plot.XLabel(label);

    public void Legend() => _plot.Legend();
}
