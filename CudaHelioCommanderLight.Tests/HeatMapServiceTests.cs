using NUnit.Framework;
using NSubstitute;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using CudaHelioCommanderLight.Models;
using CudaHelioCommanderLight.MainWindowServices;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Enums;
using CudaHelioCommanderLight.Operations;

namespace CudaHelioCommanderLight.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class HeatMapServiceTests
    {
        private HeatMapService _heatMapService;
        private IDialogService _mockDialogService;
        private IHeatMapGraphFactory _mockGraphFactory;
        private IHeatMapGraph _mockHeatMapGraph;

        [SetUp]
        public void Setup()
        {
            _mockDialogService = Substitute.For<IDialogService>();
            _mockGraphFactory = Substitute.For<IHeatMapGraphFactory>();
            _mockHeatMapGraph = Substitute.For<IHeatMapGraph>();

            _mockGraphFactory.Create().Returns(_mockHeatMapGraph);

            _heatMapService = new HeatMapService(_mockDialogService, _mockGraphFactory);
        }

        [Test]
        public void DrawHeatmapBtn_ValidData_CreatesHeatMap()
        {
            // Arrange
            var executionDetail = new ExecutionDetail
            {
                FolderName = "TestFolder",
                MethodType = MethodType.FP_1D
            };

            // Add parameters FIRST
            executionDetail.AddK0(1.0);
            executionDetail.AddK0(1.5);
            executionDetail.AddV(2.0);
            executionDetail.AddV(2.5);

            // Then add executions matching these parameters
            executionDetail.Executions.AddRange(new[]
            {
                new Execution(V: 2.0, K0: 1.0, N: 10, dt: 0.1, MethodType.FP_1D) { ErrorValue = 5 },
                new Execution(V: 2.0, K0: 1.5, N: 10, dt: 0.1, MethodType.FP_1D) { ErrorValue = 4 },
                new Execution(V: 2.5, K0: 1.0, N: 10, dt: 0.1, MethodType.FP_1D) { ErrorValue = 3 },
                new Execution(V: 2.5, K0: 1.5, N: 10, dt: 0.1, MethodType.FP_1D) { ErrorValue = 2 }
            });

            var executionDetailList = new ObservableCollection<ExecutionDetail> { executionDetail };

            // Act
            _heatMapService.DrawHeatmapBtn(executionDetailList, 0);

            // Assert
            _mockGraphFactory.Received(1).Create();
            _mockHeatMapGraph.Received(1).SetPoints(
                Arg.Is<HeatMapGraph.HeatPoint[,]>(points =>
                    points.GetLength(0) == 2 && // xSize (K0 params)
                    points.GetLength(1) == 2 && // ySize (V params)
                    points[0, 0].Intensity == 5 &&   // Verify data mapping
                    points[1, 1].Intensity == 2      // Use correct property from HeatPoint
                ),
                Arg.Any<int>(),
                Arg.Any<int>()
            );
        }

        [Test]
        public void DrawHeatmapBtn_InsufficientData_ShowsErrorMessage()
        {
            // Arrange
            var executionDetail = new ExecutionDetail();

            var executionDetailList = new ObservableCollection<ExecutionDetail> { executionDetail };

            // Act
            _heatMapService.DrawHeatmapBtn(executionDetailList, 0);

            // Assert
            _mockDialogService.Received().ShowMessage(
                "Cannot make map",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }

        [Test]
        public void DrawAmsHeatmapBtn_EmptyErrors_ShowsErrorMessage()
        {
            // Arrange
            var emptyErrors = new List<ErrorStructure>();

            // Act
            _heatMapService.DrawAmsHeatmapBtn("Investigation", emptyErrors, "Tag");

            // Assert
            _mockDialogService.Received().ShowMessage(
                "Empty errors!",
                "Warning",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }

    }
}
