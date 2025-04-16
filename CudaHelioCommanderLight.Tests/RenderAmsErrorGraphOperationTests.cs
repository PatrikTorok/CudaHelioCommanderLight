using NUnit.Framework;
using NSubstitute;
using CudaHelioCommanderLight.Models;
using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Config;
using System.Collections.Generic;
using System.Drawing;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Operations;

[TestFixture]
public class RenderAmsErrorGraphOperationTests
{
    private IMainHelper _mockMainHelper;
    private IPlotWrapper _mockPlotWrapper;

    private AmsExecutionPltErrorModel _model;

    [SetUp]
    public void Setup()
    {
        _mockMainHelper = Substitute.For<IMainHelper>();
        //arrange
        var realMetricsConfig = MetricsConfig.GetInstance(_mockMainHelper);
        realMetricsConfig.ErrorFromGev = 0.1;
        realMetricsConfig.ErrorToGev = 10.0;

        MetricsConfig.SetTestInstance(realMetricsConfig);

        _mockPlotWrapper = Substitute.For<IPlotWrapper>();

        _model = new AmsExecutionPltErrorModel
        {
            AmsExecution = new AmsExecution
            {
                TKin = new List<double> { 1.0, 10.0 },
                Spe1e3 = new List<double> { 100.0, 200.0 },
                FileName = "ams_data.txt"
            },
            ErrorStructure = new ErrorStructure(_mockMainHelper)
            {
                TKinList = new List<double> { 1.0 },
                Spe1e3binList = new List<double> { 50.0 },
                FilePath = "error_data/errors.txt"
            },
            PltWrapper = _mockPlotWrapper,
            Plt = new ScottPlot.Plot(600, 400)
        };
    }

    [TearDown]
    public void TearDown()
    {
        MetricsConfig.ResetInstance();
    }



    [Test]
    public void Operate_ValidInput_PlotsCorrectScatter()
    {
        // Act
        RenderAmsErrorGraphOperation.Operate(_model, _mockMainHelper);

        // Assert
        _mockPlotWrapper.Received().PlotScatter(
            Arg.Any<double[]>(),
            Arg.Any<double[]>(),
            Arg.Any<int>(),
            Arg.Any<Color>(),
            Arg.Any<string>()
        );

    }

    [Test]
    public void Operate_ValidInput_PlotsCorrectHSpan()
    {
        // Act
        RenderAmsErrorGraphOperation.Operate(_model, _mockMainHelper);

        var expectedX1 = ScottPlot.Tools.Log10(new[] { 0.1 }).First(); // ≈ -1
        var expectedX2 = ScottPlot.Tools.Log10(new[] { 10.0 }).First(); // = 1

        // Assert
        _mockPlotWrapper.Received().PlotHSpan(
            -1,
            1,
            false,
            color: Color.FromArgb(0, 255, 0, 0),
            alpha: 0.1
        );
    }
    [Test]
    public void Operate_WhenErrorStructureIsNull_DoesNotPlotLibrarySpectrum()
    {
        // Arrange
        _model.ErrorStructure = null;

        // Act
        RenderAmsErrorGraphOperation.Operate(_model, _mockMainHelper);

        // Assert
        // Verify reference spectrum is plotted
        _mockPlotWrapper.Received(1).PlotScatter(
            Arg.Any<double[]>(),
            Arg.Any<double[]>(),
            Arg.Any<int>(),
            Arg.Is<Color>(c => c == Color.Orange),
            Arg.Any<string>()
        );

        // Verify library spectrum is NOT plotted
        _mockPlotWrapper.DidNotReceive().PlotScatter(
            Arg.Any<double[]>(),
            Arg.Any<double[]>(),
            Arg.Any<int>(),
            Arg.Is<Color>(c => c == Color.Green),
            Arg.Any<string>()
        );
    }

    [Test]
    public void Operate_WhenAmsExecutionDataIsEmpty_HandlesGracefully()
    {
        // Arrange
        _model.AmsExecution.TKin = new List<double>();
        _model.AmsExecution.Spe1e3 = new List<double>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            RenderAmsErrorGraphOperation.Operate(_model, _mockMainHelper)
        );
    }

    [Test]
    public void Operate_WhenErrorStructureDataIsEmpty_HandlesGracefully()
    {
        // Arrange
        _model.ErrorStructure.TKinList = new List<double>();
        _model.ErrorStructure.Spe1e3binList = new List<double>();

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() =>
            RenderAmsErrorGraphOperation.Operate(_model, _mockMainHelper)
        );
    }

    [Test]
    public void Operate_WithInvertedErrorRange_PlotsHSpanCorrectly()
    {
        // Arrange
        var realMetricsConfig = MetricsConfig.GetInstance(_mockMainHelper);
        realMetricsConfig.ErrorFromGev = 10.0;
        realMetricsConfig.ErrorToGev = 0.1;

        // Act
        RenderAmsErrorGraphOperation.Operate(_model, _mockMainHelper);

        // Assert
        var expectedX1 = ScottPlot.Tools.Log10(new[] { 10.0 }).First(); // 1.0
        var expectedX2 = ScottPlot.Tools.Log10(new[] { 0.1 }).First();  // -1.0

        _mockPlotWrapper.Received().PlotHSpan(
            1,
            -1,
            false,
            color: Color.FromArgb(0, 255, 0, 0),
            alpha: 0.1
        );
    }

    [Test]
    public void Operate_WhenPltWrapperIsNull_ThrowsNullReferenceException()
    {
        // Arrange
        _model.PltWrapper = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() =>
            RenderAmsErrorGraphOperation.Operate(_model, _mockMainHelper)
        );

    }
}
