using NUnit.Framework;
using NSubstitute;
using System.Windows;
using System.Windows.Controls;
using CudaHelioCommanderLight.Config;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Helpers;

namespace CudaHelioCommanderLight.Tests
{
    [TestFixture]
    [Apartment(System.Threading.ApartmentState.STA)]
    public class ConfigWindowTests
    {
        private IMetricsConfig _mockMetricsConfig;
        private IMainHelper _mockMainHelper;
        private ConfigWindow _configWindow;

        [SetUp]
        public void Setup()
        {
            _mockMetricsConfig = Substitute.For<IMetricsConfig>();
            _mockMetricsConfig.K0Metric.Returns(MetricsConfig.K0Metrics.cm2ps);
            _mockMetricsConfig.VMetric.Returns(MetricsConfig.VMetrics.kmps);
            _mockMetricsConfig.DtMetric.Returns(MetricsConfig.DtMetrics.s);
            _mockMetricsConfig.IntensityMetric.Returns(MetricsConfig.IntensityMetrics.npm2ssrGeV);
            _mockMetricsConfig.ErrorFromGev.Returns(1.0);
            _mockMetricsConfig.ErrorToGev.Returns(100.0);

            _mockMainHelper = Substitute.For<IMainHelper>();

            _configWindow = new ConfigWindow(_mockMetricsConfig, _mockMainHelper);
            _configWindow.Show();
        }

        [TearDown]
        public void Teardown()
        {
            _configWindow.Close();
        }

        [Test]
        public void Constructor_InitializesControlsCorrectly()
        {
            Assert.Multiple(() =>
            {
                // Verify combo box selections
                Assert.That(_configWindow.K0UnitsComboBox.SelectedItem,
                    Is.EqualTo(MetricsConfig.K0Metrics.cm2ps));
                Assert.That(_configWindow.VUnitsComboBox.SelectedItem,
                    Is.EqualTo(MetricsConfig.VMetrics.kmps));

                // Verify text boxes
                Assert.That(_configWindow.ErrorFromGevTb.Text, Is.EqualTo("1"));
                Assert.That(_configWindow.ErrorToGevTb.Text, Is.EqualTo("100"));
            });
        }

        [Test]
        public void OkButton_Click_UpdatesMetricsConfig()
        {
            // Arrange
            _configWindow.K0UnitsComboBox.SelectedItem = MetricsConfig.K0Metrics.cm2ps;
            _configWindow.VUnitsComboBox.SelectedItem = MetricsConfig.VMetrics.kmps;
            _configWindow.ErrorFromGevTb.Text = "5";
            _configWindow.ErrorToGevTb.Text = "150";

            _mockMainHelper.TryConvertToDouble("5", out Arg.Any<double>())
                .Returns(x => { x[1] = 5.0; return true; });
            _mockMainHelper.TryConvertToDouble("150", out Arg.Any<double>())
                .Returns(x => { x[1] = 150.0; return true; });

            // Act
            _configWindow.OkButton_Click(null, null);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(_mockMetricsConfig.K0Metric,
                    Is.EqualTo(MetricsConfig.K0Metrics.cm2ps));
                Assert.That(_mockMetricsConfig.VMetric,
                    Is.EqualTo(MetricsConfig.VMetrics.kmps));
                Assert.That(_mockMetricsConfig.ErrorFromGev, Is.EqualTo(5.0));
                Assert.That(_mockMetricsConfig.ErrorToGev, Is.EqualTo(150.0));
                Assert.IsTrue(_configWindow.HasChanged);
            });
        }
    }
}
