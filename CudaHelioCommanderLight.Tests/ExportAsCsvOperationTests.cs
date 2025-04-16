using NUnit.Framework;
using NSubstitute;
using System;
using System.Windows;
using CudaHelioCommanderLight.Operations;
using CudaHelioCommanderLight.Interfaces;
using static CudaHelioCommanderLight.HeatMapGraph;
using CudaHelioCommanderLight.Models;
using CudaHelioCommanderLight.Helpers;

[TestFixture]
public class ExportAsCsvOperationTests
{
    private IFileWriter _mockFileWriter;
    private IDialogService _mockDialogService;
    private HeatPoint[,] _sampleHeatPoints;
    private IMainHelper _mainHelper;

    [SetUp]
    public void SetUp()
    {
        _mockFileWriter = Substitute.For<IFileWriter>();
        _mockDialogService = Substitute.For<IDialogService>();
        _mainHelper = Substitute.For<MainHelper>();

        _sampleHeatPoints = new HeatPoint[,]
        {
            { new HeatPoint (0,0,1.0 ), new HeatPoint (1, 1, 2.0) }
        };


        _mockDialogService.SaveFileDialog(out Arg.Any<string>(), Arg.Any<string>())
            .Returns(x =>
            {
                x[0] = "test.csv";
                return true;
            });
    }

    [Test]
    public void Operate_WhenSaveFileDialogIsCancelled_ShouldNotWriteToFile()
    {
        // Arrange
        _mockDialogService.SaveFileDialog(out Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act
        ExportAsCsvOperation.Operate(_sampleHeatPoints, _mockFileWriter, _mockDialogService);

        // Assert
        _mockFileWriter.DidNotReceive().WriteToFile(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public void Operate_WhenSaveFileDialogIsConfirmed_ShouldWriteCorrectContentToFile()
    {
        // Arrange
        string expectedCsvContent = "0,0,1\r\n1,1,2\r\n";

        // Act
        ExportAsCsvOperation.Operate(_sampleHeatPoints, _mockFileWriter, _mockDialogService);

        // Assert
        _mockFileWriter.Received(1).WriteToFile("test.csv", Arg.Is<string>(s => s == expectedCsvContent));
    }

    [Test]
    public void Operate_WhenIOExceptionOccurs_ShouldShowErrorMessage()
    {
        // Arrange
        _mockFileWriter.When(x => x.WriteToFile(Arg.Any<string>(), Arg.Any<string>()))
                       .Do(x => throw new IOException("Simulated failure"));

        // Act
        ExportAsCsvOperation.Operate(_sampleHeatPoints, _mockFileWriter, _mockDialogService);

        // Assert
        _mockDialogService.Received(1).ShowMessage(Arg.Is<string>(m => m.Contains("error")),
                                                   Arg.Any<string>(), MessageBoxButton.OK, MessageBoxImage.Error);
    }
    [Test]
    public void Operate_WithEmptyHeatPoints_ShouldWriteEmptyContentToFile()
    {
        // Arrange
        HeatPoint[,] emptyHeatPoints = new HeatPoint[0, 0];

        // Act
        ExportAsCsvOperation.Operate(emptyHeatPoints, _mockFileWriter, _mockDialogService);

        // Assert
        _mockFileWriter.Received(1).WriteToFile("test.csv", Arg.Is<string>(s => s == string.Empty));
    }

    [Test]
    public void Operate_WithErrorStructures_ShouldWriteCorrectContentToFile()
    {
        // Arrange
        var errors = new List<ErrorStructure>
        {
            new ErrorStructure(_mainHelper) { K0 = 1.23, V = 45, Error = 0.5 },
            new ErrorStructure(_mainHelper) { K0 = 2.34, V = 67, Error = 1.2 }
        };
        string expectedCsvContent = "K0,V,Error\r\n1.23,45,0.5\r\n2.34,67,1.2\r\n";

        // Act
        ExportAsCsvOperation.Operate(errors, _mockFileWriter, _mockDialogService);

        // Assert
        _mockFileWriter.Received(1).WriteToFile("test.csv", Arg.Is<string>(s => s == expectedCsvContent));
    }

    [Test]
    public void Operate_WhenErrorStructuresSaveFileDialogCancelled_ShouldNotWriteToFile()
    {
        // Arrange
        var errors = new List<ErrorStructure>
        {
            new ErrorStructure(_mainHelper) { K0 = 1.23, V = 45, Error = 0.5 }
        };
        _mockDialogService.SaveFileDialog(out Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act
        ExportAsCsvOperation.Operate(errors, _mockFileWriter, _mockDialogService);

        // Assert
        _mockFileWriter.DidNotReceive().WriteToFile(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public void Operate_WhenErrorStructuresIOExceptionOccurs_ShouldShowErrorMessage()
    {
        // Arrange
        var errors = new List<ErrorStructure>
        {
            new ErrorStructure(_mainHelper) { K0 = 1.23, V = 45, Error = 0.5 }
        };

        _mockFileWriter.When(x => x.WriteToFile(Arg.Any<string>(), Arg.Any<string>()))
                       .Do(x => throw new IOException("Simulated failure"));

        // Act
        ExportAsCsvOperation.Operate(errors, _mockFileWriter, _mockDialogService);

        // Assert
        _mockDialogService.Received(1).ShowMessage(Arg.Is<string>(m => m.Contains("error")),
                                                   Arg.Any<string>(), MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

