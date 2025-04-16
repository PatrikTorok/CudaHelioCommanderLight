using CudaHelioCommanderLight.Config;


namespace CudaHelioCommanderLight.Interfaces
{
    public interface IMetricsConfig
    {
        MetricsConfig.K0Metrics K0Metric { get; set; }
        MetricsConfig.VMetrics VMetric { get; set; }
        MetricsConfig.DtMetrics DtMetric { get; set; }
        MetricsConfig.IntensityMetrics IntensityMetric { get; set; }
        double ErrorFromGev { get; set; }
        double ErrorToGev { get; set; }
        bool WasInitialized { get; set; }

        void RegisterObserver(IMetricsConfigObserver observer);
        void UnregisterObserver(IMetricsConfigObserver observer);

        void LoadConfigurationinfo();
        void SaveConfigurationInfo();


        string ToString();
    }
}
