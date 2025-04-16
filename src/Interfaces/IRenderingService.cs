using CudaHelioCommanderLight.Models;
using System.Windows.Controls;

namespace CudaHelioCommanderLight.Interfaces
{
    public interface IRenderingService
    {
        ErrorStructure? AmsErrorsListBox_SelectionChanged(
            ErrorStructure errorStructure,
            IWpfPlotWrapper amsGraphWpfPlot,
            IWpfPlotWrapper amsGraphRatioWpfPlot,
            AmsExecution amsExecution);

        void RenderAmsGraph(
            AmsExecution amsExecution,
            IWpfPlotWrapper amsGraphWpfPlot,
            ErrorStructure? errorStructure = null);

        void RenderAmsRatioGraph(
            AmsExecution amsExecution,
            IWpfPlotWrapper amsGraphRatioWpfPlot,
            ErrorStructure? errorStructure = null);

        void CreateErrorGraph(DataGrid activeCalculationsDataGrid);
    }
}
