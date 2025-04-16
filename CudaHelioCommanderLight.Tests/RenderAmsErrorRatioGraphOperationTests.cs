
using NSubstitute;
using CudaHelioCommanderLight.Models;
using CudaHelioCommanderLight.Operations;
using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Interfaces;
using System.Drawing;
using CudaHelioCommanderLight.Config;

namespace CudaHelioCommanderLight.Tests
{
    [TestFixture]
    public class RenderAmsErrorRatioGraphOperationTests
    {
        private IMainHelper _mockMainHelper;
        private IMetricsConfig _mockMetricsConfig;
        private IPlotWrapper _mockPlotWrapper;

        private AmsExecutionPltErrorModel _model;

        [SetUp]
        public void Setup()
        {
            _mockMainHelper = Substitute.For<IMainHelper>();
            _mockMetricsConfig = Substitute.For<IMetricsConfig>();
            _mockMetricsConfig.ErrorFromGev.Returns(0.5);
            _mockMetricsConfig.ErrorToGev.Returns(100);

            _mockPlotWrapper = Substitute.For<IPlotWrapper>();

            _model = new AmsExecutionPltErrorModel
            {
                AmsExecution = new AmsExecution
                {
                    TKin = new List<double> { 0.5, 0.6 },
                    Spe1e3 = new List<double> { 10.0, 20.0 },
                    FileName = "ams_data.txt"
                },
                ErrorStructure = new ErrorStructure(_mockMainHelper)
                {
                    TKinList = new List<double> { 0.5 },
                    Spe1e3binList = new List<double> { 5.0 },
                    FilePath = "error_data/errors.txt"
                },
                PltWrapper = _mockPlotWrapper // Use mocked wrapper
            };

        }

        [Test]
        public void Operate_ValidInput_PlotsCorrectHSpan()
        {
            // Arrange
            _mockMetricsConfig.ErrorFromGev.Returns(0);
            _mockMetricsConfig.ErrorToGev.Returns(3);

            // Act
            RenderAmsErrorRatioGraphOperation.Operate(_model, _mockMainHelper);

            // Assert
            _mockPlotWrapper.Received().PlotHSpan(
                0,
                3,
                draggable: false,
                color: Color.FromArgb(0, 255, 0, 0),
                alpha: 0.1
            );
        }

        [Test]
        public void Operate_MismatchedDataLengths_HandlesGracefully()
        {
            // Arrange
            _model.AmsExecution.TKin = new List<double> { 0.5, 0.6, 0.7, 0.8, 0.9, 1.0, 1.1, 1.2, 1.3 };
            _model.ErrorStructure.TKinList = new List<double> { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

            // Act & Assert
            Assert.DoesNotThrow(() =>
                RenderAmsErrorRatioGraphOperation.Operate(_model, _mockMainHelper)
            );
        }

        [Test]
        public void Operate_InvalidFilePath_GeneratesValidLabel()
        {
            // Arrange
            _model.ErrorStructure.FilePath = "invalid/path/";

            // Act
            RenderAmsErrorRatioGraphOperation.Operate(_model, _mockMainHelper);

            // Assert
            _mockPlotWrapper.Received().PlotScatter(
                Arg.Any<double[]>(),
                Arg.Any<double[]>(),
                markerSize: 1,
                color: Color.Red,
                label: "Ratio ams_data/ path"
            );
        }

        [Test]
        public void Operate_NullPlt_ThrowsArgumentNullException()
        {
            // Arrange
            _model.PltWrapper = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RenderAmsErrorRatioGraphOperation.Operate(_model, _mockMainHelper));
        }

    }
}
