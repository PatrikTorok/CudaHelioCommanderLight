using CudaHelioCommanderLight.Config;
using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Interfaces;

namespace CudaHelioCommanderLight.Operations
{
    public class OpenConfigurationWindowOperation : IOpenConfigurationWindowOperation
    {
        public IConfigWindow Operate(IMetricsConfig metricsConfig, IMainHelper mainHelper)
        {
            var configWindow = new ConfigWindow(metricsConfig, mainHelper);
            configWindow.ShowDialog();
            return configWindow;
        }
    }

}
