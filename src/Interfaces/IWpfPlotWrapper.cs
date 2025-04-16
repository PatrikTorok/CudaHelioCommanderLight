using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CudaHelioCommanderLight.Interfaces
{
    public interface IWpfPlotWrapper
    {
        void Reset();
        void Render();
        IPlotWrapper PlotWrapper { get; }
    }

}
