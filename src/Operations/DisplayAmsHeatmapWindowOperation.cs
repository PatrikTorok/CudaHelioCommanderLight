using CudaHelioCommanderLight.Models;
using System.Linq;
using System.Windows;

namespace CudaHelioCommanderLight.Operations
{
    public class DisplayAmsHeatmapWindowOperation : Operation<DisplayAmsHeatmapModel>
    {
        public static new void Operate(DisplayAmsHeatmapModel model)
        {
            var amsComputedErrors = model.Errors;
            HeatMapGraph heatMap = new HeatMapGraph();
            heatMap.Show();

            var sortedErrors = amsComputedErrors.OrderBy(err => err.K0).ThenBy(err => err.V).ToList();

            var k0List = amsComputedErrors.Select(err => err.K0).Distinct().OrderBy(k => k);
            var vList = amsComputedErrors.Select(err => err.V).Distinct().OrderBy(v => v);
            int xSize = amsComputedErrors.Select(err => err.K0).Distinct().Count();
            int ySize = amsComputedErrors.Select(err => err.V).Distinct().Count();

            if (xSize < 2 || ySize < 2)
            {
                MessageBox.Show("Cannot make map, size too small");
                return;
            }

            HeatMapGraph.HeatPoint[,] heatPoints = new HeatMapGraph.HeatPoint[xSize, ySize];

            for (int i = 0; i < xSize; i++)
            {
                var currK0 = k0List.ElementAt(i);
                for (int j = 0; j < ySize; j++)
                {
                    var currV = vList.ElementAt(j);
                    var computedError = sortedErrors.Where(err => err.K0 == currK0 && err.V == currV).FirstOrDefault();

                    if (model.Tag.Equals("RMS"))
                    {
                        heatPoints[i, j] = new HeatMapGraph.HeatPoint(computedError.K0, computedError.V, computedError.Error);
                    }
                    else if ((model.Tag.Equals("LowMaxError")))
                    {
                        heatPoints[i, j] = new HeatMapGraph.HeatPoint(computedError.K0, computedError.V, computedError.MaxError);
                    }
                }
            }

            heatMap.SetPoints(heatPoints, xSize, ySize);
            heatMap.GraphTitle = model.GraphName ?? "";
            heatMap.XLabel = "V [km/s]";
            heatMap.YLabel = "K0 [cm^2/s]";
            heatMap.ColorbarLabel = "Deviation [%]";
            heatMap.Render();
        }
    }
}
