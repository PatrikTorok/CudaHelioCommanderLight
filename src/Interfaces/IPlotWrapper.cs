using System.Drawing;

namespace CudaHelioCommanderLight.Interfaces
{
    public interface IPlotWrapper
    {
        void PlotScatter(double[] x, double[] y, int markerSize, Color color, string label);
        void XTicks(double[] positions, string[] labels);
        void YTicks(double[] positions, string[] labels);
        void PlotHSpan(double x1, double x2, bool draggable, Color color, double alpha);
        void Ticks(bool useExponentialNotation = false, bool logScaleX = false, bool logScaleY = false);
        void Title(string title);
        void YLabel(string label);
        void XLabel(string label);
        void Legend();
    }
}
