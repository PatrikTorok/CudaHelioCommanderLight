using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CudaHelioCommanderLight.Interfaces
{
    public interface IHeatMapGraph
    {
        void Show();
        void SetPoints(HeatMapGraph.HeatPoint[,] heatPoints, int xSize, int ySize);
        void Render();
    }
}
