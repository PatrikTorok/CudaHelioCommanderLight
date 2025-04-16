using CudaHelioCommanderLight.Interfaces;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CudaHelioCommanderLight.Wrappers
{
    public class WpfPlotWrapper : IWpfPlotWrapper
    {
        private readonly WpfPlot _wpfPlot;

        public WpfPlotWrapper(WpfPlot wpfPlot)
        {
            _wpfPlot = wpfPlot;
        }

        public void Reset() => _wpfPlot.Reset();
        public void Render() => _wpfPlot.Render();
        public IPlotWrapper PlotWrapper => new PlotWrapper(_wpfPlot.plt);
    }
}
