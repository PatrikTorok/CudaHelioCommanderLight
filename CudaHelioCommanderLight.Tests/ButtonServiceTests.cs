using NUnit.Framework;
using NSubstitute;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Models;
using System.Collections.ObjectModel;
using System.Windows;
using CudaHelioCommanderLight.MainWindowServices;
using CudaHelioCommanderLight.Enums;

namespace CudaHelioCommanderLight.Tests
{
    [TestFixture]
    public class ButtonServiceTests
    {
        private IDialogService _mockDialogService;
        private ButtonService _buttonService;
        private ObservableCollection<ExecutionDetail> _testExecutionDetails;

        [SetUp]
        public void Setup()
        {
            _mockDialogService = Substitute.For<IDialogService>();
            _buttonService = new ButtonService(_mockDialogService);
            _testExecutionDetails = new ObservableCollection<ExecutionDetail>(new List<ExecutionDetail>
            {
                new ExecutionDetail
                {
                    FolderName = "Folder1",
                    Executions = new List<Execution>
                    {
                        new Execution(2.0, 1.0, 10, 0.1, MethodType.FP_1D),
                    }
                }
            });

        }

        [Test]
        public void AboutUsButton_CallsShowMessageWithCorrectParameters()
        {
            // Act
            _buttonService.AboutUsButton();

            // Assert
            _mockDialogService.Received(1).ShowMessage(
                "Slovak Academy of Sciences\n\nDeveloped by: Martin Nguyen, Pavol Bobik\n\nCopyright 2023",
                "About Us",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        [Test]
        public void ExportJsonBtn_WithNullExecutionDetail_DoesNotCallDialog()
        {
            // Arrange
            _testExecutionDetails[0] = null;

            // Act
            _buttonService.ExportJsonBtn(_testExecutionDetails, 0);

            // Assert
            _mockDialogService.DidNotReceive().SaveFileDialogWithTitle(
                out Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>()
            );
        }

        [Test]
        public void ExportJsonBtn_WhenUserCancelsDialog_DoesNotExport()
        {
            // Arrange
            _mockDialogService.SaveFileDialogWithTitle(
                out Arg.Any<string>(),
                "JSON File|*.json",
                "Save JSON File"
            ).Returns(false);

            // Act
            _buttonService.ExportJsonBtn(_testExecutionDetails, 0);

            // Assert
            _mockDialogService.Received(1).SaveFileDialogWithTitle(
                out Arg.Any<string>(),
                "JSON File|*.json",
                "Save JSON File"
            );
        }

        [Test]
        public void ExportJsonBtn_WithValidParameters_ExportsCorrectly()
        {
            // Arrange
            const string testPath = "test.json";
            var testExecutions = new List<Execution> { new Execution(2.0, 1.0, 10, 0.1, MethodType.FP_1D) };
            _testExecutionDetails[0].Executions = testExecutions;

            _mockDialogService.SaveFileDialogWithTitle(
                out Arg.Any<string>(),
                "JSON File|*.json",
                "Save JSON File"
            ).Returns(x => {
                x[0] = testPath;
                return true;
            });

            // Act
            _buttonService.ExportJsonBtn(_testExecutionDetails, 0);

            // Assert
            _mockDialogService.Received(1).SaveFileDialogWithTitle(
                out Arg.Any<string>(),
                "JSON File|*.json",
                "Save JSON File"
            );

        }
    }
}
