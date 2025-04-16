using CudaHelioCommanderLight.Interfaces;
using ScottPlot;

namespace CudaHelioCommanderLight.Models
{
    public class AmsExecutionPltErrorModel
    {
        public AmsExecution? AmsExecution { get; set; }
        public ErrorStructure? ErrorStructure { get; set; }
        public Plot? Plt { get; set; }
        public IPlotWrapper PltWrapper { get; set; }
    }
}
