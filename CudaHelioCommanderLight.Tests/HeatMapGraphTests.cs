using NUnit.Framework;
using NSubstitute;
using System.Windows.Media;
using System.Windows;
using System.Collections;
using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight;
using System.Globalization;
using CudaHelioCommanderLight.Services;

[TestFixture]
[Apartment(ApartmentState.STA)]
public class HeatMapGraphTests
{
    private HeatMapGraph _heatMapGraph;
    private IMainHelper _mockMainHelper;
    private HeatMapGraph.HeatPoint[,] _testHeatPoints;
    private IFileWriter _mockFileWriter;
    private IDialogService _mockDialogService;


    [SetUp]
    public void Setup()
    {
        _mockMainHelper = Substitute.For<IMainHelper>();
        _mockFileWriter = Substitute.For<IFileWriter>();
        _mockDialogService = Substitute.For<IDialogService>();
        _mockMainHelper.TryConvertToDouble(Arg.Any<string>(), out Arg.Any<double>())
            .Returns(x =>
            {
                string input = x.ArgAt<string>(0);
                bool success = double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double result);

                x[1] = result;
                return success;
            });
        _heatMapGraph = new HeatMapGraph(_mockMainHelper);

        // Setup test data: 2x2 grid
        _testHeatPoints = new HeatMapGraph.HeatPoint[2, 2]
        {
            { new HeatMapGraph.HeatPoint(1, 10, 5), new HeatMapGraph.HeatPoint(1, 20, 10) },
            { new HeatMapGraph.HeatPoint(2, 10, 15), new HeatMapGraph.HeatPoint(2, 20, 20) }
        };

        _heatMapGraph.SetPoints(_testHeatPoints, 2, 2);
        _mockDialogService.SaveFileDialog(out Arg.Any<string>(), Arg.Any<string>())
    .Returns(x => {
        x[0] = "test.csv";
        return true;
    });
    }

    [Test]
    public void SetPoints_InitializesDataCorrectly()
    {
        // Assert
        Assert.That(_heatMapGraph.xCount, Is.EqualTo(2));
        Assert.That(_heatMapGraph.yCount, Is.EqualTo(2));
        Assert.That(_heatMapGraph.HeatPoints, Is.EqualTo(_testHeatPoints));
    }

    [Test]
    public void ComputeColor_ReturnsExpectedColors()
    {
        // Test minimum value
        var minColor = _heatMapGraph.ComputeColor(0, 100, 0);
        Assert.That(minColor, Is.EqualTo(Color.FromArgb(0xFF, 0, 0, 0xFF))); // Blue

        // Test middle value
        var midColor = _heatMapGraph.ComputeColor(0, 100, 50);
        Assert.That(midColor, Is.EqualTo(Color.FromArgb(0xFF, 0, 0xFF, 0))); // Green

        // Test maximum value
        var maxColor = _heatMapGraph.ComputeColor(0, 100, 100);
        Assert.That(maxColor, Is.EqualTo(Color.FromArgb(0xFF, 0xFF, 0, 0))); // Red
    }

    [Test]
    public void Render_SetsCorrectMinMaxValues()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");  // Use comma decimal separator
        // Act
        _heatMapGraph.Render();

        // Assert
        Assert.That(_heatMapGraph.MinTb.Text, Is.EqualTo("5,00%"));
        Assert.That(_heatMapGraph.MaxTb.Text, Is.EqualTo("20,00%"));
        CultureInfo.CurrentCulture = originalCulture;

    }

    [Test]
    public void RerenderBtn_Click_UpdatesColorRange()
    {
        // Arrange
        _heatMapGraph.MinColorValueTb.Text = "10";
        _heatMapGraph.MaxColorValueTb.Text = "50";
        _mockMainHelper.TryConvertToDouble("10", out double _).Returns(x => { x[1] = 10.0; return true; });
        _mockMainHelper.TryConvertToDouble("50", out double _).Returns(x => { x[1] = 50.0; return true; });

        // Act
        _heatMapGraph.RerenderBtn_Click(null, null);

        // Assert
        Assert.That(_heatMapGraph.minColorValue, Is.EqualTo(10.0));
        Assert.That(_heatMapGraph.maxColorValue, Is.EqualTo(50.0));
    }

    [Test]
    public void ExportAsCsvBtn_Click_CallsFileWriter()
    {
        // Arrange
        _heatMapGraph.fileWriter = _mockFileWriter;
        _heatMapGraph.dialogService = _mockDialogService;

        // Act
        _heatMapGraph.ExportAsCsvBtn_Click(null, null);

        // Assert
        _mockFileWriter.Received(1).WriteToFile(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public void SetMinToMinBtn_Click_SetsCorrectMinimum()
    {
        // Arrange
        _heatMapGraph.MinColorValueTb.Text = "5.00";
        _heatMapGraph.MaxColorValueTb.Text = "50.00";
        _heatMapGraph.SetPoints(
            new HeatMapGraph.HeatPoint[,]
            {
            { new HeatMapGraph.HeatPoint(1, 10, 5), new HeatMapGraph.HeatPoint(1, 20, 10) },
            { new HeatMapGraph.HeatPoint(2, 10, 15), new HeatMapGraph.HeatPoint(2, 20, 20) }
            },
            xCount: 2,
            yCount: 2
        );

        // Act
        _heatMapGraph.SetMinToMinBtn_Click(null, null);

        // Assert
        Assert.That(_heatMapGraph.MinColorValueTb.Text, Is.EqualTo("5"));
    }


    [Test]
    public void SetMaxToMaxBtn_Click_SetsCorrectMaximum()
    {
        // Act
        _heatMapGraph.SetMaxToMaxBtn_Click(null, null);

        // Assert
        Assert.That(_heatMapGraph.MaxColorValueTb.Text, Is.EqualTo("20"));
    }

    [Test]
    public void Mark5LowestHighestCb_Checked_UpdatesRenderingFlags()
    {
        // Arrange
        _heatMapGraph.Mark5LowestCb.IsChecked = true;
        _heatMapGraph.Mark5HighestCb.IsChecked = true;

        // Act
        _heatMapGraph.Mark5LowestHighestCb_Checked(null, null);

        // Assert
        Assert.That(_heatMapGraph.mark5Lowest, Is.True);
        Assert.That(_heatMapGraph.mark5Highest, Is.True);
    }
}
