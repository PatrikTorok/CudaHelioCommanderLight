using NSubstitute;
using CudaHelioCommanderLight.Exceptions;
using CudaHelioCommanderLight.Models;
using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Operations;
using CudaHelioCommanderLight.Interfaces;

namespace CudaHelioCommanderLight.Tests
{
    [TestFixture]
    public class ComputeErrorOperationTests
    {
        private IMainHelper _mockMainHelper;
        private IMetricsConfig _mockMetricsConfig;

        [SetUp]
        public void Setup()
        {
            _mockMainHelper = Substitute.For<IMainHelper>();
            _mockMetricsConfig = Substitute.For<IMetricsConfig>();

            _mockMetricsConfig.ErrorFromGev.Returns(0.5);
            _mockMetricsConfig.ErrorToGev.Returns(100);
        }

        [Test]
        public void Operate_ValidInput_ReturnsCorrectComputedErrorModel()
        {
            // Arrange
            var amsExecution = new AmsExecution
            {
                TKin = new List<double> { 0.5, 0.6, 0.7, 0.8, 0.9, 1.0, 1.1, 1.2, 1.3, 1.4 },
                Spe1e3 = new List<double> { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }
            };

            var libraryItem = new OutputFileContent
            {
                TKinList = new List<double> { 0.5, 0.6, 0.7, 0.8, 0.9, 1.0, 1.1, 1.2, 1.3, 1.4 },
                Spe1e3List = new List<double> { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }
            };

            var model = new ErrorComputeModel
            {
                AmsExecution = amsExecution,
                LibraryItem = libraryItem
            };

            _mockMetricsConfig.ErrorFromGev.Returns(0.5);
            _mockMetricsConfig.ErrorToGev.Returns(1.4);

            // Act
            var result = ComputeErrorOperation.Operate(model, _mockMainHelper, _mockMetricsConfig);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Error, Is.Not.NaN);
            Assert.That(result.MaxError, Is.Not.NaN);
        }

        [Test]
        public void Operate_EmptyReferenceTi_ReturnsModelWithNaNValues()
        {
            // Arrange
            var model = new ErrorComputeModel
            {
                AmsExecution = new AmsExecution(),
                LibraryItem = new OutputFileContent { Spe1e3List = new List<double>() }
            };

            // Act
            var result = ComputeErrorOperation.Operate(model, _mockMainHelper, _mockMetricsConfig);

            // Assert
            Assert.That(result.Error, Is.NaN);
            Assert.That(result.MaxError, Is.NaN);
        }

        [Test]
        public void Operate_NullAmsExecutionSpe1e3_ThrowsArgumentNullException()
        {
            // Arrange
            var model = new ErrorComputeModel
            {
                AmsExecution = new AmsExecution { Spe1e3 = null },
                LibraryItem = new OutputFileContent { Spe1e3List = new List<double> { 1, 2, 3 } }
            };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => ComputeErrorOperation.Operate(model, _mockMainHelper, _mockMetricsConfig)
            );
        }

        [Test]
        public void Operate_MissingRequiredGeVValue_ThrowsWrongConfigurationException()
        {
            // Arrange
            var amsExecution = new AmsExecution
            {
                TKin = new List<double> { 1.0, 2.0, 3.0 },
                Spe1e3 = new List<double> { 10, 20, 30 }
            };

            var libraryItem = new OutputFileContent
            {
                TKinList = new List<double> { 1.0, 2.0, 3.0 },
                Spe1e3List = new List<double> { 11, 22, 33 }
            };

            var model = new ErrorComputeModel
            {
                AmsExecution = amsExecution,
                LibraryItem = libraryItem
            };

            _mockMetricsConfig.ErrorFromGev.Returns(0.5);
            _mockMetricsConfig.ErrorToGev.Returns(3.0);

            // Act & Assert
            Assert.Throws<WrongConfigurationException>(
                () => ComputeErrorOperation.Operate(model, _mockMainHelper, _mockMetricsConfig)
            );
        }
    }
}
