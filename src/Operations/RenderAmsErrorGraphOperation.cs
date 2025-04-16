using CudaHelioCommanderLight.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CudaHelioCommanderLight.Config;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Helpers;

namespace CudaHelioCommanderLight.Operations
{
    public class RenderAmsErrorGraphOperation : Operation<AmsExecutionPltErrorModel>
    {
        private readonly IMainHelper _mainHelper;

        public RenderAmsErrorGraphOperation(IMainHelper mainHelper)
        {
            _mainHelper = mainHelper ?? throw new ArgumentNullException(nameof(mainHelper));
        }

        public static new void Operate(AmsExecutionPltErrorModel amsExecutionErrorModel, IMainHelper mainHelper)
        {
            var amsExecution = amsExecutionErrorModel.AmsExecution;
            var errorStructure = amsExecutionErrorModel.ErrorStructure;

            var metricsConfig = MetricsConfig.GetInstance(mainHelper);
            var pltwrapper = amsExecutionErrorModel.PltWrapper;
            var tickPositionsListY = new List<double>();
            var tickNamesListY = new List<string>();
            var tickPositionsList = new List<double>();
            var tickNamesList = new List<string>();
            var x = amsExecution.TKin.ToArray();
            var y = amsExecution.Spe1e3.ToArray();
            var xLog = ScottPlot.Tools.Log10(x);
            var yLog = ScottPlot.Tools.Log10(y);
            var minY = y.Min();
            var maxY = y.Max();
            var min = x[0];
            var max = x[x.Length - 1];

            var amsLegend = Path.GetFileNameWithoutExtension(amsExecution.FileName) + ": reference spectrum";
            pltwrapper.PlotScatter(xLog, yLog, markerSize: 1, color: System.Drawing.Color.Orange, label: amsLegend);
            for (double z = min; z <= max; z *= 10)
            {
                tickPositionsList.Add(z);
                tickNamesList.Add(z.ToString());
            }

            tickPositionsList.Add(max);
            tickNamesList.Add(max.ToString());

            var tickPositions = ScottPlot.Tools.Log10(tickPositionsList.ToArray());
            var tickLabels = tickNamesList.ToArray();
            pltwrapper.XTicks(tickPositions, tickLabels);

            if (errorStructure != null)
            {
                var x2 = errorStructure.TKinList.ToArray();
                var y2 = errorStructure.Spe1e3binList.ToArray();

                var xLog2 = ScottPlot.Tools.Log10(x2);
                var yLog2 = ScottPlot.Tools.Log10(y2);

                var legend = Path.GetFileName(Path.GetDirectoryName(errorStructure.FilePath)) + ": library spectrum";
                pltwrapper.PlotScatter(xLog2, yLog2, markerSize: 1, color: System.Drawing.Color.Green, label: legend);

                double min2 = x2[0];
                double max2 = x2[x2.Length - 1];

                var tickPositionsList2 = new List<double>();
                var tickNamesList2 = new List<string>();

                for (double z = min2; z <= max2; z *= 10)
                {
                    tickPositionsList2.Add(z);
                    tickNamesList2.Add(z.ToString());
                }

                tickPositionsList2.Add(max2);
                tickNamesList2.Add(max2.ToString());

                var tickPositions2 = ScottPlot.Tools.Log10(tickPositionsList2.ToArray());
                var tickLabels2 = tickNamesList2.ToArray();
                pltwrapper.XTicks(tickPositions2, tickLabels2);

                minY = minY < y2.Min() ? minY : y2.Min();
                maxY = maxY > y2.Max() ? maxY : y2.Max();
            }

            for (var z = minY; z <= maxY; z *= 10)
            {
                if (Math.Abs(z) < 1)
                {
                    z = 1;
                }

                tickPositionsListY.Add(z);
                tickNamesListY.Add(z.ToString("E2"));
            }

            tickPositionsListY.Add(maxY);
            tickNamesListY.Add(maxY.ToString("E2"));

            var tickPositionsY = ScottPlot.Tools.Log10(tickPositionsListY.ToArray());
            var tickLabelsY = tickNamesListY.ToArray();

            pltwrapper.YTicks(tickPositionsY, tickLabelsY);

            pltwrapper.PlotHSpan(
                x1: ScottPlot.Tools.Log10(new double[] { metricsConfig.ErrorFromGev }).First(),
                x2: ScottPlot.Tools.Log10(new double[] { metricsConfig.ErrorToGev }).First(),
                draggable: false,
                color: System.Drawing.Color.FromArgb(0, 255, 0, 0),
                alpha: 0.1
             );


            pltwrapper.Ticks(useExponentialNotation: true);
            pltwrapper.Ticks(logScaleX: true);
            pltwrapper.Ticks(logScaleY: true);
            pltwrapper.Title("Spectra");
            pltwrapper.YLabel("Spe1e3 [proton_flux m^-2sr^-1s^-1GeV^-1]");
            pltwrapper.XLabel("Kinetic Energy [GeV]");
            pltwrapper.Legend();
            
        }
    }
}
