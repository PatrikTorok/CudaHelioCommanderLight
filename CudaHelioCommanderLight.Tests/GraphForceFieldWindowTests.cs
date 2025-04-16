using NUnit.Framework;
using NSubstitute;
using CudaHelioCommanderLight.Models;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using CudaHelioCommanderLight.Helpers;

namespace CudaHelioCommanderLight.Tests
{
    [TestFixture]
    [Apartment(System.Threading.ApartmentState.STA)]
    public class GraphForceFieldWindowTests
    {
        private List<ErrorStructure> _testErrors;
        private GraphForceFieldWindow _window;
        private IMainHelper _mockMainHelper;

        [SetUp]
        public void Setup()
        {
            _mockMainHelper = Substitute.For<IMainHelper>();
            _testErrors = new List<ErrorStructure>
            {
                new ErrorStructure(_mockMainHelper) { K0 = 1.0, V = 100, Error = 5.0, MaxError = 10.0, FilePath = "100.txt" },
                new ErrorStructure(_mockMainHelper) { K0 = 1.0, V = 200, Error = 6.0, MaxError = 12.0,FilePath = "200.txt" },
                new ErrorStructure(_mockMainHelper) { K0 = 2.0, V = 100, Error = 7.0, MaxError = 14.0 ,FilePath = "100.txt"},
                new ErrorStructure(_mockMainHelper) { K0 = 2.0, V = 200, Error = 8.0, MaxError = 16.0,FilePath = "200.txt" }
            };

            _window = new GraphForceFieldWindow(_testErrors);
            _window.Show();
        }

        [TearDown]
        public void Teardown()
        {
            _window.Close();
        }

        [Test]
        public void Constructor_WithValidData_RendersGraph()
        {

            Assert.That(_window.PlotViewTest.plt.GetPlottables().Count, Is.EqualTo(1));
            var scatterPlot = _window.PlotViewTest.plt.GetPlottables()[0] as PlottableScatter;

            Assert.That(scatterPlot.ys, Is.EqualTo(new[] { 5.0, 7.0, 6.0,8.0 }));
        }



        [Test]
        public void RenderGraph_HandlesInvalidFileNameFormat()
        {
            // Arrange
            var invalidErrors = new List<ErrorStructure>
            {
                new ErrorStructure(_mockMainHelper) { FilePath = "invalid.txt", Error = 0.1 }
            };

            // Act/Assert
            Assert.Throws<FormatException>(() =>
                new GraphForceFieldWindow(invalidErrors));
        }

        [Test]
        public void RenderGraph_HandlesSingleDataPoint()
        {
            // Arrange
            var singleError = new List<ErrorStructure>
            {
                new ErrorStructure(_mockMainHelper) { FilePath = "500.txt", Error = 0.4 }
            };
            var window = new GraphForceFieldWindow(singleError);

            // Act
            var scatterPlot = window.PlotViewTest.plt.GetPlottables()[0] as PlottableScatter;

            // Assert
            Assert.That(scatterPlot.xs, Is.EqualTo(new[] { 500.0 }));
            Assert.That(scatterPlot.ys, Is.EqualTo(new[] { 0.4 }));
        }
        [Test]
        public void RenderGraph_SetsCorrectAxisLabels()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_window.PlotViewTest.plt.GetSettings().title.text, Is.EqualTo("Error vs. X"));
                Assert.That(_window.PlotViewTest.plt.GetSettings().xLabel.text, Is.EqualTo("Force field file number"));
                Assert.That(_window.PlotViewTest.plt.GetSettings().yLabel.text, Is.EqualTo("Error"));
            });
        }

    }
}
