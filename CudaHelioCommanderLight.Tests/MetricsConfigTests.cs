using CudaHelioCommanderLight.Config;
using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Interfaces;
using NSubstitute;
using System.Xml.Linq;

[TestFixture]
public class MetricsConfigTests
{
    private IMainHelper _mainHelper;
    private string _tempCacheDir;
    private string _originalCacheDir;

    [SetUp]
    public void Setup()
    {
        MetricsConfig.ResetInstance();
        _mainHelper = Substitute.For<IMainHelper>();
        _originalCacheDir = MetricsConfig.CACHE_DIR;
        _tempCacheDir = Path.Combine(Path.GetTempPath(), "test_cache");
        MetricsConfig.CACHE_DIR = _tempCacheDir;
        Directory.CreateDirectory(_tempCacheDir);


        var configFilePath = Path.Combine(_tempCacheDir, "configInfo.xml");
        if (File.Exists(configFilePath))
        {
            File.Delete(configFilePath);
        }
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_tempCacheDir, true);
        MetricsConfig.CACHE_DIR = _originalCacheDir;
    }

    [Test]
    public void GetInstance_ReturnsSingletonInstance()
    {
        // Arrange


        // Act
        var instance1 = MetricsConfig.GetInstance(_mainHelper);
        var instance2 = MetricsConfig.GetInstance(_mainHelper);

        // Assert
        Assert.That(instance2, Is.SameAs(instance1));
    }

    [Test]
    public void Constructor_InitializesDefaultValues()
    {
        // Arrange & Act
        var config = MetricsConfig.GetInstance(_mainHelper);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(config.K0Metric, Is.EqualTo(MetricsConfig.K0Metrics.cm2ps));
            Assert.That(config.VMetric, Is.EqualTo(MetricsConfig.VMetrics.kmps));
            Assert.That(config.DtMetric, Is.EqualTo(MetricsConfig.DtMetrics.s));
            Assert.That(config.IntensityMetric, Is.EqualTo(MetricsConfig.IntensityMetrics.npm2ssrGeV));
            Assert.That(config.ErrorFromGev, Is.EqualTo(0.5));
            Assert.That(config.ErrorToGev, Is.EqualTo(100));
        });
    }

    [Test]
    public void Properties_NotifyObserversOnChange()
    {
        // Arrange
        var config = MetricsConfig.GetInstance(_mainHelper);
        var observer = Substitute.For<IMetricsConfigObserver>();

        // Act
        config.RegisterObserver(observer);
        config.ErrorFromGev = 1.0;
        config.ErrorToGev = 50.0;

        // Assert
        Received.InOrder(() =>
        {
            observer.NotifyMetricsConfigChanged(config); // Initial registration
            observer.NotifyMetricsConfigChanged(config); // ErrorFromGev change
            observer.NotifyMetricsConfigChanged(config); // ErrorToGev change
        });
    }

    [Test]
    public void LoadConfigurationInfo_LoadsValuesFromXml()
    {
        // Arrange
        const double testFrom = 1.5;
        const double testTo = 50.0;
        File.WriteAllText(Path.Combine(_tempCacheDir, "configInfo.xml"),
            $@"<configInfo>
                <errorFromGev>{testFrom}</errorFromGev>
                <errorToGev>{testTo}</errorToGev>
            </configInfo>");

        _mainHelper.TryConvertToDouble(testFrom.ToString(), out _).Returns(x =>
        {
            x[1] = testFrom;
            return true;
        });

        _mainHelper.TryConvertToDouble(testTo.ToString(), out _).Returns(x =>
        {
            x[1] = testTo;
            return true;
        });

        var config = MetricsConfig.GetInstance(_mainHelper);

        // Act
        config.LoadConfigurationinfo();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(config.ErrorFromGev, Is.EqualTo(testFrom));
            Assert.That(config.ErrorToGev, Is.EqualTo(testTo));
            Assert.That(config.WasInitialized, Is.True);
        });
    }

    [Test]
    public void LoadConfigurationInfo_HandlesInvalidXml()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempCacheDir, "configInfo.xml"), "INVALID XML CONTENT");

        // Act & Assert
        Assert.DoesNotThrow(() => MetricsConfig.GetInstance(_mainHelper).LoadConfigurationinfo());
    }

    [Test]
    public void SaveConfigurationInfo_WritesToXml()
    {
        // Arrange
        const double testFrom = 2.0;
        const double testTo = 75.0;
        var config = MetricsConfig.GetInstance(_mainHelper);
        config.ErrorFromGev = testFrom;
        config.ErrorToGev = testTo;

        // Act
        config.SaveConfigurationInfo();

        // Assert
        var doc = XDocument.Load(Path.Combine(_tempCacheDir, "configInfo.xml"));
        Assert.Multiple(() =>
        {
            Assert.That(double.Parse(doc.Element("configInfo").Element("errorFromGev").Value), Is.EqualTo(testFrom));
            Assert.That(double.Parse(doc.Element("configInfo").Element("errorToGev").Value), Is.EqualTo(testTo));
        });
    }

    [Test]
    public void ToString_ReturnsCorrectFormat()
    {
        // Arrange
        var config = MetricsConfig.GetInstance(_mainHelper);
        config.K0Metric = MetricsConfig.K0Metrics.m2ps;
        config.VMetric = MetricsConfig.VMetrics.mps;
        config.DtMetric = MetricsConfig.DtMetrics.m;

        // Act
        var result = config.ToString();

        // Assert
        Assert.That(result, Is.EqualTo(
            "Metrics used: K0 (m^2/s) | dt (min) | V (m/s) | w (n/m^2*s*sr*GeV)"));
    }

    [Test]
    public void GetInstance_NullMainHelper_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => MetricsConfig.GetInstance(null));
    }
}
