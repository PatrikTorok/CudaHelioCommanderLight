using CudaHelioCommanderLight.Config;
using CudaHelioCommanderLight.Helpers;

namespace CudaHelioCommanderLight.Interfaces
{
    public interface IOpenConfigurationWindowOperation
    {
        IConfigWindow Operate(IMetricsConfig metricsConfig, IMainHelper mainHelper);
    }
}
