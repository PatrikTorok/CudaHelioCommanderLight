using CudaHelioCommanderLight.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CudaHelioCommanderLight.Wrappers
{
    public class HeatMapGraphWrapper : IHeatMapGraph
    {
        private readonly HeatMapGraph _heatMapGraph;

        public HeatMapGraphWrapper()
        {
            _heatMapGraph = new HeatMapGraph();
        }

        public void Show() => _heatMapGraph.Show();
        public void SetPoints(HeatMapGraph.HeatPoint[,] heatPoints, int xSize, int ySize)
            => _heatMapGraph.SetPoints(heatPoints, xSize, ySize);
        public void Render() => _heatMapGraph.Render();
    }
    public class HeatMapGraphFactory : IHeatMapGraphFactory
    {
        public IHeatMapGraph Create() => new HeatMapGraphWrapper();
    }
}
