using NUnit.Framework;
using NSubstitute;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Models;
using CudaHelioCommanderLight.MainWindowServices;
using ScottPlot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Enums;
using CudaHelioCommanderLight.Wrappers;
using System.Windows;

namespace CudaHelioCommanderLight.Tests
{

    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class RenderingServiceTests
    {
        private RenderingService _renderingService;
        private IMainHelper _mainHelper;
        private IDialogService _dialogService;

        [SetUp]
        public void Setup()
        {
            _mainHelper = Substitute.For<IMainHelper>();
            _dialogService = Substitute.For<IDialogService>();
            _renderingService = new RenderingService(_mainHelper,_dialogService);
        }

        [Test]
        public void AmsErrorsListBox_SelectionChanged_RendersGraphsAndReturnsError()
        {
            // Arrange
            var amsExecution = new AmsExecution
            {
                TKin = new List<double> { 1.0, 2.0, 3.0 },
                Spe1e3 = new List<double> { 10, 20, 30 }
            };
            var errorStructure = new ErrorStructure(_mainHelper)
            {
                TKinList = new List<double> { 1.0 },
                Spe1e3binList = new List<double> { 50.0 },
                FilePath = "error_data/errors.txt"
            };
            var amsGraphWpfPlot = new WpfPlot();
            var amsGraphRatioWpfPlot = new WpfPlot();

            // Act
            var result = _renderingService.AmsErrorsListBox_SelectionChanged(
                errorStructure, new WpfPlotWrapper(amsGraphWpfPlot), new WpfPlotWrapper(amsGraphRatioWpfPlot), amsExecution);

            // Assert
            Assert.That(result, Is.EqualTo(errorStructure));
            Assert.DoesNotThrow(() => amsGraphWpfPlot.Render());
            Assert.DoesNotThrow(() => amsGraphRatioWpfPlot.Render());
        }

        [Test]
        public void RenderAmsGraph_CallsRenderAndResetsPlot()
        {
            // Arrange
            var amsExecution = new AmsExecution
            {
                TKin = new List<double> { 1.0, 2.0, 3.0 },
                Spe1e3 = new List<double> { 10, 20, 30 }
            };
            var errorStructure = new ErrorStructure(_mainHelper)
            {
                TKinList = new List<double> { 1.0 },
                Spe1e3binList = new List<double> { 50.0 },
                FilePath = "error_data/errors.txt"
            };
            var mockWpfPlot = Substitute.For<IWpfPlotWrapper>();

            // Act
            _renderingService.RenderAmsGraph(amsExecution, mockWpfPlot, errorStructure);

            // Assert
            mockWpfPlot.Received(1).Reset();
            mockWpfPlot.Received(1).Render();
        }

        [Test]
        public void RenderAmsRatioGraph_CallsRenderAndResetsPlot()
        {
            var amsExecution = new AmsExecution
            {
                TKin = new List<double> { 1.0, 2.0, 3.0 },
                Spe1e3 = new List<double> { 10, 20, 30 }
            };
            var errorStructure = new ErrorStructure(_mainHelper)
            {
                TKinList = new List<double> { 1.0 },
                Spe1e3binList = new List<double> { 50.0 },
                FilePath = "error_data/errors.txt"
            };
            var amsGraphRatioWpfPlot = Substitute.For<IWpfPlotWrapper>();

            // Act
            _renderingService.RenderAmsRatioGraph(amsExecution, amsGraphRatioWpfPlot, errorStructure);

            // Assert
            amsGraphRatioWpfPlot.Received(1).Reset();
            amsGraphRatioWpfPlot.Received(1).Render();
        }
        [Test]
        public void RenderGraphOfErrors_CreatesScatterPlotsForExecutions()
        {
            // Arrange
            var executionDetails = new List<ExecutionDetail>
            {
                new ExecutionDetail
                {
                    FolderName = "Folder1",
                    Executions = new List<Execution>
                    {
                        new Execution(2.0, 1.0, 10, 0.1, MethodType.FP_1D),
                    }
                }
            };

            // Act & Assert
            Assert.DoesNotThrow(() => _renderingService.RenderGraphOfErrors(executionDetails));
        }

        [Test]
        public void AmsErrorsListBox_SelectionChanged_NullErrorStructure_ReturnsNull()
        {
            // Arrange
            var amsExecution = new AmsExecution();
            var amsGraph = new WpfPlotWrapper(new WpfPlot());
            var amsRatioGraph = new WpfPlotWrapper(new WpfPlot());

            // Act
            var result = _renderingService.AmsErrorsListBox_SelectionChanged(
                null, amsGraph, amsRatioGraph, amsExecution);

            // Assert
            Assert.That(result, Is.Null);
        }


        [Test]
        public void RenderAmsRatioGraph_WithNullErrorStructure_HandlesGracefully()
        {
            // Arrange
            var execution = new AmsExecution { TKin = new List<double> { 1.0 }, Spe1e3 = new List<double> { 2.0 } };
            var mockPlot = Substitute.For<IWpfPlotWrapper>();

            // Act & Assert
            Assert.DoesNotThrow(() =>
                _renderingService.RenderAmsRatioGraph(execution, mockPlot, null));
        }
        [Test]
        public void RenderAmsGraph_HandlesPlotResetException()
        {
            // Arrange
            var mockPlot = Substitute.For<IWpfPlotWrapper>();
            mockPlot.When(x => x.Reset()).Throw<Exception>();
            var execution = new AmsExecution { TKin = new List<double> { 1.0 }, Spe1e3 = new List<double> { 2.0 } };
            var errorStructure = new ErrorStructure(_mainHelper)
            {
                TKinList = new List<double> { 1.0 },
                Spe1e3binList = new List<double> { 50.0 },
                FilePath = "error_data/errors.txt"
            };
            // Act & Assert
            Assert.DoesNotThrow(() =>
                _renderingService.RenderAmsGraph(execution, mockPlot, errorStructure));
        }
        [Test]
        public void CreateErrorGraph_CanceledFileDialog_AbortsProcess()
        {
            // Arrange
            var mockMainHelper = Substitute.For<IMainHelper>();
            var mockFileDialogService = Substitute.For<IDialogService>();
            mockFileDialogService.ShowOpenFileDialog(out Arg.Any<string>()).Returns(false);

            var service = new RenderingService(mockMainHelper, mockFileDialogService);
            var dataGrid = new DataGrid();

            // Act
            service.CreateErrorGraph(dataGrid);

            // Assert
            mockMainHelper.DidNotReceive().ExtractOutputDataFile(Arg.Any<string>(), out _);
        }
        [Test]
        public void CreateErrorGraph_FailedDataExtract_ShowsErrorMessage()
        {
            // Arrange
            _dialogService.ShowOpenFileDialog(out Arg.Any<string>()).Returns(true);

            string dummyFilePath = "dummy/path.txt";
            _dialogService.When(x => x.ShowOpenFileDialog(out dummyFilePath)).Do(x => x[0] = dummyFilePath);

            _mainHelper.ExtractOutputDataFile(dummyFilePath, out _).Returns(false);

            var service = new RenderingService(_mainHelper, _dialogService);
            var dataGrid = new DataGrid();

            // Act
            service.CreateErrorGraph(dataGrid);

            // Assert
            _dialogService.Received(1).ShowMessage(
            "Cannot read data values from the input file.",
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        }

    }
}
