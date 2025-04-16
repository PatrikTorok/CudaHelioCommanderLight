using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Models;
using NUnit.Framework;
using NSubstitute;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CudaHelioCommanderLight.Operations;
using System.Windows;
using CudaHelioCommanderLight.Helpers;
using System.Globalization;

namespace CudaHelioCommanderLight.Tests
{
    [TestFixture]
    public class ExportListAsCsvOperationTests
    {
        private IMainHelper _mainHelper;
        private IDialogService _dialogService;
        private IFileWriter _fileWriter;
        private List<ErrorStructure> _testData;
        private const string TestFilePath = "test_export.csv";

        [SetUp]
        public void SetUp()
        {
            _dialogService = Substitute.For<IDialogService>();
            _fileWriter = Substitute.For<IFileWriter>();
            _mainHelper = Substitute.For<IMainHelper>();

            _testData = new List<ErrorStructure>
            {
                new ErrorStructure(_mainHelper)
                {
                    DisplayName = "Test 1",
                    Error = 0.1,
                    MaxError = 0.5,
                    FilePath = "path/to/file1.txt"
                },
                new ErrorStructure(_mainHelper)
                {
                    DisplayName = "Test 2",
                    Error = 0.2,
                    MaxError = 0.6,
                    FilePath = "path/to/file2.txt"
                }
            };
        }

        [Test]
        public void Operate_WhenUserCancels_DoesNotExport()
        {
            // Arrange
            _dialogService.SaveFileDialog(out Arg.Any<string>(), Arg.Any<string>()).Returns(false);

            // Act
            ExportListAsCsvOperation.Operate(_testData, _fileWriter, _dialogService);

            // Assert
            _fileWriter.DidNotReceive().WriteToFile(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void Operate_WhenUserConfirms_WritesCorrectCsv()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");  // Use comma decimal separator
            // Arrange
            var expectedCsv = new StringBuilder()
                .AppendLine("Test 1,0,1,0,5,path/to/file1.txt")
                .AppendLine("Test 2,0,2,0,6,path/to/file2.txt")
                .ToString();

            _dialogService.SaveFileDialog(out Arg.Any<string>(), Arg.Any<string>())
                .Returns(x =>
                {
                    x[0] = TestFilePath;
                    return true;
                });

            // Act
            ExportListAsCsvOperation.Operate(_testData, _fileWriter, _dialogService);

            // Assert
            _fileWriter.Received(1).WriteToFile(TestFilePath, expectedCsv);
            _dialogService.Received(1).ShowMessage(
                "Export successful!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
            CultureInfo.CurrentCulture = originalCulture;
        }

        [Test]
        public void Operate_WhenIOExceptionOccurs_ShowsError()
        {
            // Arrange
            var exception = new IOException("Disk error");
            _dialogService.SaveFileDialog(out Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            _fileWriter.When(x => x.WriteToFile(Arg.Any<string>(), Arg.Any<string>()))
                .Do(x => throw exception);

            // Act
            ExportListAsCsvOperation.Operate(_testData, _fileWriter, _dialogService);

            // Assert
            _dialogService.Received(1).ShowMessage(
                $"An error occurred while exporting the CSV file: {exception.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

        [Test]
        public void Operate_WithEmptyList_WritesEmptyFile()
        {
            // Arrange
            var emptyList = new List<ErrorStructure>();
            _dialogService.SaveFileDialog(out Arg.Any<string>(), Arg.Any<string>())
                .Returns(x =>
                {
                    x[0] = TestFilePath;
                    return true;
                });

            // Act
            ExportListAsCsvOperation.Operate(emptyList, _fileWriter, _dialogService);

            // Assert
            _fileWriter.Received(1).WriteToFile(TestFilePath, string.Empty);
        }
    }
}
