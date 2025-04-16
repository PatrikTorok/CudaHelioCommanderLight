using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace CudaHelioCommanderLight.Config
{
    public class MetricsConfig : IMetricsConfig
    {

        public enum K0Metrics
        {
            cm2ps,
            m2ps
        }

        public enum VMetrics
        {
            kmps,
            mps
        }

        public enum DtMetrics
        {
            s,
            m
        }

        public enum IntensityMetrics
        {
            npm2ssrGeV
        }

        public K0Metrics K0Metric { get; set; }
        public VMetrics VMetric { get; set; }
        public DtMetrics DtMetric { get; set; }
        public IntensityMetrics IntensityMetric { get; set; }

        private readonly IMainHelper _mainHelper;

        public double ErrorFromGev
        {
            get { return _errorFromGev; }
            set
            {
                if (_errorFromGev != value)
                {
                    _errorFromGev = value;
                    NotifyObservers();
                }
            }
        }

        public double ErrorToGev
        {
            get { return _errorToGev; }
            set
            {
                if (_errorToGev != value)
                {
                    _errorToGev = value;
                    NotifyObservers();
                }
            }
        }

        public bool WasInitialized { get; set; }
        private double _errorFromGev;
        private double _errorToGev;
        private readonly List<IMetricsConfigObserver> _observers = new List<IMetricsConfigObserver>();

        private static MetricsConfig _instance;
        private static readonly object _lock = new object();

        private MetricsConfig(IMainHelper mainHelper)
        {
            _mainHelper = mainHelper ?? throw new ArgumentNullException(nameof(mainHelper));

            K0Metric = K0Metrics.cm2ps;
            VMetric = VMetrics.kmps;
            DtMetric = DtMetrics.s;
            IntensityMetric = IntensityMetrics.npm2ssrGeV;

            // Default Gevs
            ErrorFromGev = 0.5;
            ErrorToGev = 100;

            if (!WasInitialized)
            {
                LoadConfigurationinfo();
            }
        }

        public static MetricsConfig GetInstance(IMainHelper mainHelper)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new MetricsConfig(mainHelper);
                    }
                }
            }
            return _instance;
        }

        public void RegisterObserver(IMetricsConfigObserver observer)
        {
            _observers.Add(observer);
            NotifyObservers();
        }

        public void UnregisterObserver(IMetricsConfigObserver observer)
        {
            _observers.Remove(observer);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            string kStr, vstr, dtstr, wstr;

            switch (K0Metric)
            {
                case K0Metrics.cm2ps:
                    kStr = "cm^2/s";
                    break;
                case K0Metrics.m2ps:
                    kStr = "m^2/s";
                    break;
                default:
                    kStr = "unknown";
                    break;
            }

            switch (VMetric)
            {
                case VMetrics.kmps:
                    vstr = "km/s";
                    break;
                case VMetrics.mps:
                    vstr = "m/s";
                    break;
                default:
                    vstr = "unknown";
                    break;
            }

            switch (DtMetric)
            {
                case DtMetrics.s:
                    dtstr = "sec";
                    break;
                case DtMetrics.m:
                    dtstr = "min";
                    break;
                default:
                    dtstr = "unknown";
                    break;
            }

            switch (IntensityMetric)
            {
                case IntensityMetrics.npm2ssrGeV:
                    wstr = "n/m^2*s*sr*GeV";
                    break;
                default:
                    wstr = "unknown";
                    break;
            }

            sb.Append(string.Format("Metrics used: K0 ({0}) | dt ({1}) | V ({2}) | w ({3})", kStr, dtstr, vstr, wstr));

            return sb.ToString();
        }

        public static string CACHE_DIR = "cache";
        private readonly string CONFIGURATION_CACHE = Path.Combine(CACHE_DIR, "configInfo.xml");

        public void LoadConfigurationinfo()
        {
            Directory.CreateDirectory(CACHE_DIR);

            if (File.Exists(CONFIGURATION_CACHE))
            {
                try
                {
                    XDocument configInfo = XDocument.Load(CONFIGURATION_CACHE);

                    XElement el = configInfo.Element("configInfo");

                    var errorFromGev = el.Element("errorFromGev").Value;
                    var errorToGev = el.Element("errorToGev").Value;

                    _mainHelper.TryConvertToDouble(errorFromGev, out double gevFromError);
                    _mainHelper.TryConvertToDouble(errorToGev, out double gevToError);

                    this.ErrorFromGev = gevFromError;
                    this.ErrorToGev = gevToError;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cannot parse config info xml");
                }
            }
            else
            {
                SaveConfigurationInfo();
            }

            WasInitialized = true;
        }

        public void SaveConfigurationInfo()
        {
            XElement configInfo = new XElement(
                "configInfo",
                    new XElement("errorFromGev", this.ErrorFromGev),
                    new XElement("errorToGev", this.ErrorToGev)
             );

            configInfo.Save(CONFIGURATION_CACHE);
        }

        private void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                observer.NotifyMetricsConfigChanged(this);
            }
        }
        public static void SetTestInstance(MetricsConfig testInstance)
        {
            lock (_lock)
            {
                _instance = testInstance;
            }
        }
        public static void ResetInstance()
        {
            _instance = null;
        }
    }
}
