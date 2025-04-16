using CudaHelioCommanderLight.Constants;
using CudaHelioCommanderLight.Enums;
using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

[TestFixture]
public class MainHelperTests
{
    private MainHelper _mainHelper;
    private string _testDir;

    [SetUp]
    public void Setup()
    {
        _mainHelper = new MainHelper();
        _testDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_testDir);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_testDir, true);
    }

    [Test]
    public void ExtractMultipleOfflineStatus_ValidFiles_ReturnsAmsExecutionDetail()
    {
        // Arrange
        CreateTestFiles(new[] { "file1.txt", "file2.txt" });
        File.WriteAllText(Path.Combine(_testDir, "file1.txt"), "1.0 2.0");
        File.WriteAllText(Path.Combine(_testDir, "file2.txt"), "");

        // Act
        var result = _mainHelper.ExtractMultipleOfflineStatus(new[]
        {
            Path.Combine(_testDir, "file1.txt"),
            Path.Combine(_testDir, "file2.txt")
        });

        // Assert
        Assert.That(result.AmsExecutions.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExtractOutputDataFile_ValidFile_ReturnsTrue()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "output.txt");
        File.WriteAllText(filePath, "1.0 2.0 3.0 4.0");

        // Act
        var success = _mainHelper.ExtractOutputDataFile(filePath, out OutputFileContent content);

        // Assert
        Assert.IsTrue(success);
        Assert.That(content.TKinList.Count, Is.EqualTo(1));
    }

    [Test]
    public void ExtractForceFieldOutputDataFile_InvalidFile_ReturnsFalse()
    {
        // Arrange
        var invalidPath = "invalid_path.txt";

        // Act
        var result = _mainHelper.ExtractForceFieldOutputDataFile(invalidPath, out _);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void TryConvertToDouble_ValidString_ReturnsTrue()
    {
        // Arrange
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");  // Use comma decimal separator
        const string testValue = "123,45";

        try
        {
            // Act
            bool success = _mainHelper.TryConvertToDouble(testValue, out double result);

            // Assert
            Assert.IsTrue(success, "Conversion should succeed");
            Assert.That(result, Is.EqualTo(123.45).Within(0.001), "Result should be 123.45");
        }
        finally
        {
            // Cleanup
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Test]
    public void TryConvertToDecimal_InvalidString_ReturnsFalse()
    {
        // Arrange
        const string invalidValue = "invalid";

        // Act
        var success = _mainHelper.TryConvertToDecimal(invalidValue, out _);

        // Assert
        Assert.IsFalse(success);
    }

    [Test]
    public void ExtractOfflineExecStatus_ValidDirectory_ReturnsExecutionStatus()
    {
        // Arrange
        var offlineResultDirPath = Path.Combine(_testDir, "offlineResults1");
        Directory.CreateDirectory(offlineResultDirPath);
        var mainFolderPath = Path.Combine(offlineResultDirPath, "mainFolder");
        Directory.CreateDirectory(mainFolderPath);
        File.WriteAllText(Path.Combine(mainFolderPath, GlobalFilesToDownload.RunningInfoFile),
            "Grid-run 0\nType [FWMethod]\nV-size [1]\nK0-size [1]\nN-size [1]\ndt-size [1]\n" +
            "V-params [1.0]\nK0-params [1.0]\nN-params [1.0]\ndt-params [1.0]\n" +
            "Algorithm-started [1.0] [1.0] [1.0] [1.0]\nProgress |");

        // Act
        var result = _mainHelper.ExtractOfflineExecStatus(offlineResultDirPath);

        // Assert
        Assert.That(result.GetActiveExecutions().Count, Is.EqualTo(1));
        Assert.That(result.GetActiveExecutions()[0].MethodType, Is.EqualTo(MethodType.FP_1D));
    }


    [Test]
    public void ExtractMultipleOfflineStatus_InvalidFile_ProcessesValidFileAndThrowsException()
    {
        // Arrange
        CreateTestFiles(new[] { "valid.txt", "invalid.txt" });
        File.WriteAllText(Path.Combine(_testDir, "valid.txt"), "1.0 2.0");
        File.WriteAllText(Path.Combine(_testDir, "invalid.txt"), "invalid data");

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => _mainHelper.ExtractMultipleOfflineStatus(new[]
        {
        Path.Combine(_testDir, "valid.txt"),
        Path.Combine(_testDir, "invalid.txt")
    }));

        Assert.That(ex.Message, Is.EqualTo("Exception during parsing tKin and spe1e3 of ams spectra"));

        // Check that the valid file was processed before the exception
        var result = _mainHelper.ExtractMultipleOfflineStatus(new[] { Path.Combine(_testDir, "valid.txt") });
        Assert.That(result.AmsExecutions.Count, Is.EqualTo(1));
    }


    [Test]
    public void ExtractForceFieldOutputDataFile_ValidFile_ReturnsTrue()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "forceField.txt");
        File.WriteAllText(filePath, "1.0 2.0 3.0 4.0\n5.0 6.0 7.0 8.0");

        // Act
        var result = _mainHelper.ExtractForceFieldOutputDataFile(filePath, out var content);

        // Assert
        Assert.IsTrue(result);
        Assert.That(content.TKinList.Count, Is.EqualTo(2));
        Assert.That(content.Spe1e3List.Count, Is.EqualTo(2));
    }

    [Test]
    public void ExtractOutputDataFile_DifferentFormats_ParsesCorrectly()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "output.txt");
        File.WriteAllLines(filePath, new[]
        {
            "1.0 2.0",
            "3.0 4.0 5.0",
            "6.0 7.0 8.0 9.0"
        });

        // Act
        var result = _mainHelper.ExtractOutputDataFile(filePath, out var content);

        // Assert
        Assert.IsTrue(result);
        Assert.That(content.TKinList.Count, Is.EqualTo(3));
        Assert.That(content.Spe1e3List.Count, Is.EqualTo(3));
    }

    [Test]
    public void TryConvertToDouble_DifferentFormats_ConvertSuccessfully()
    {
        // Arrange
        var testCases = new[] { "123.45", "123,45", "123" };

        foreach (var testCase in testCases)
        {
            // Act
            var success = _mainHelper.TryConvertToDouble(testCase, out var result);

            // Assert
            Assert.IsTrue(success, $"Failed to convert {testCase}");
            Assert.That(result, Is.GreaterThan(0), $"Incorrect conversion for {testCase}");
        }
    }

    private void CreateTestFiles(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            File.Create(Path.Combine(_testDir, file)).Close();
        }
    }
    [Test]
    public void ExtractMultipleOfflineStatus_ValidAmsFile_ParsesCorrectly()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "ams.txt");
        File.WriteAllLines(filePath, new[]
        {
            "# Comment line",
            "1.0 2.0",
            "3.0 4.0 5.0 6.0"
        });

        // Act
        var result = _mainHelper.ExtractMultipleOfflineStatus(new[] { filePath });

        // Assert
        Assert.That(result.AmsExecutions.Count, Is.EqualTo(1));
        Assert.That(result.AmsExecutions[0].TKin.Count, Is.EqualTo(2));
        Assert.That(result.AmsExecutions[0].Spe1e3.Count, Is.EqualTo(2));
    }

    [Test]
    public void ExtractForceFieldOutputDataFile_ValidFile_ParsesCorrectly()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "forcefield.txt");
        File.WriteAllLines(filePath, new[]
        {
            "1.0 2.0 3.0 4.0",
            "5.0 6.0 7.0 8.0"
        });

        // Act
        var result = _mainHelper.ExtractForceFieldOutputDataFile(filePath, out var content);

        // Assert
        Assert.IsTrue(result);
        Assert.That(content.TKinList.Count, Is.EqualTo(2));
        Assert.That(content.Spe1e3List.Count, Is.EqualTo(2));
        Assert.That(content.Spe1e3NList.Count, Is.EqualTo(2));
        Assert.That(content.StdDevList.Count, Is.EqualTo(2));
    }

    [Test]
    public void ParseDetailFromRunningFile_ValidFile_ParsesCorrectly()
    {
        // Arrange
        var offlineResultDirPath = Path.Combine(_testDir, "offlineResults2"); // Unique directory
        Directory.CreateDirectory(offlineResultDirPath);

        var localMainFolderPath = Path.Combine(offlineResultDirPath, "mainFolder");
        Directory.CreateDirectory(localMainFolderPath);

        var runningInfoFilePath = Path.Combine(localMainFolderPath, GlobalFilesToDownload.RunningInfoFile);
        File.WriteAllLines(runningInfoFilePath, new[]
        {
        "Grid-run 1",
        "Type [FWMethod]",
        "V-size [2]",
        "K0-size [2]",
        "V-params [1.0] [2.0]",
        "K0-params [3.0] [4.0]",
        "Algorithm-started [1.0] [3.0] [5.0] [0.1]",
        "Algorithm-started [2.0] [4.0] [6.0] [0.2]",
        "Progress |||"
    });

        // Act
        var result = _mainHelper.ExtractOfflineExecStatus(offlineResultDirPath);

        // Assert
        Assert.That(result.GetActiveExecutions().Count, Is.EqualTo(1));
        var executionDetail = result.GetActiveExecutions()[0];
        Assert.IsTrue(executionDetail.IsGrid);
        Assert.That(executionDetail.MethodType, Is.EqualTo(MethodType.FP_1D));
        Assert.That(executionDetail.VSize, Is.EqualTo(2));
        Assert.That(executionDetail.K0Size, Is.EqualTo(2));
    }


}
