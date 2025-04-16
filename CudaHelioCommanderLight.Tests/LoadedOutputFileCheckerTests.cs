using NUnit.Framework;
using NSubstitute;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Models;
using CudaHelioCommanderLight.Enums;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using CudaHelioCommanderLight.Helpers;
using System.Windows;

namespace CudaHelioCommanderLight.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class LoadedOutputFileCheckerTests
    {
        private LoadedOutputFileChecker _checker;
        private IMainHelper _mockMainHelper;
        private OutputFileContent _outputFileContent;

        [SetUp]
        public void Setup()
        {
            _mockMainHelper = Substitute.For<IMainHelper>();
            _outputFileContent = new OutputFileContent
            {
                FirstLine = "TKin Spectra Count StdDev",
                NumberOfColumns = 4,
                TKinList = new List<double> { 1.0, 2.0, 3.0 },
                Spe1e3List = new List<double> { 10.0, 20.0, 30.0 },
                Spe1e3NList = new List<double> { 100, 200, 300 },
                StdDevList = new List<double> { 0.1, 0.2, 0.3 }
            };
            _checker = new LoadedOutputFileChecker(_outputFileContent, _mockMainHelper);
        }

        [Test]
        public void Constructor_InitializesCorrectly()
        {
            Assert.That(_checker.outputFileContent, Is.EqualTo(_outputFileContent));
            Assert.That(_checker.ExecutionCheckDataGrid.Items.Count, Is.EqualTo(4));
        }

        [Test]
        public void UpdateList_PopulatesExecutionRowsCorrectly()
        {
            var rows = _checker.ExecutionCheckDataGrid.ItemsSource.Cast<LoadedOutputFileChecker.ExecutionRow>().ToList();
            Assert.That(rows.Count, Is.EqualTo(3));
            Assert.That(rows[0].TKin, Is.EqualTo(1.0));
            Assert.That(rows[0].Spectra, Is.EqualTo(10.0));
            Assert.That(rows[0].Count, Is.EqualTo(100));
            Assert.That(rows[0].StandardDeviation, Is.EqualTo(0.1));
        }

        [Test]
        public void DivideSpectraCb_Checked_DividesSpectra()
        {
            _checker.DivideSpectraCb.IsChecked = true;
            _checker.DivideSpectraCb_Checked(null, null);

            var rows = _checker.ExecutionCheckDataGrid.ItemsSource.Cast<LoadedOutputFileChecker.ExecutionRow>().ToList();
            Assert.That(rows[0].Spectra, Is.EqualTo(0.1).Within(0.001));
            Assert.That(rows[1].Spectra, Is.EqualTo(0.1).Within(0.001));
            Assert.That(rows[2].Spectra, Is.EqualTo(0.1).Within(0.001));
        }
        [Test]
        public void UpdateColumnSelectorSections_HandlesDifferentColumnCounts()
        {
            _outputFileContent.NumberOfColumns = 3;
            _checker = new LoadedOutputFileChecker(_outputFileContent, _mockMainHelper);

            Assert.Multiple(() =>
            {
                Assert.That(_checker.Col1Grid.Visibility, Is.EqualTo(Visibility.Visible));
                Assert.That(_checker.Col4Grid.Visibility, Is.EqualTo(Visibility.Hidden));
            });
        }

        [Test]
        public void ColumnSelectorComboBox_SelectionChanged_HandlesInvalidTag()
        {
            // Arrange
            var invalidComboBox = new ComboBox { Tag = "invalid" };

            // Act/Assert
            Assert.DoesNotThrow(() =>
                _checker.ColumnSelectorComboBox_SelectionChanged(invalidComboBox, null));
        }



        [Test]
        public void UpdateWHLISList_AddsNewRowsWhenNeeded()
        {
            // Arrange
            _outputFileContent.WHLISList = new List<double> { 5.0, 6.0 };

            // Act
            _checker.UpdateWHLISList();
            var rows = _checker.ExecutionCheckDataGrid.ItemsSource.Cast<LoadedOutputFileChecker.ExecutionRow>().ToList();

            // Assert
            Assert.That(rows.Count, Is.EqualTo(3));
            Assert.That(rows[0].WHLIS, Is.EqualTo(5.0));
            Assert.That(rows[1].WHLIS, Is.EqualTo(6.0));
        }

        [Test]
        public void SetupDivideSpectraCb_DisablesWhenMissingData()
        {
            // Arrange
            _outputFileContent.Spe1e3List = new List<double>();

            // Recreate checker with modified data
            _checker = new LoadedOutputFileChecker(_outputFileContent, _mockMainHelper);

            // Assert
            Assert.That(_checker.DivideSpectraCb.IsEnabled, Is.False);
        }



        [Test]
        public void UpdateOtherList_MergesExistingRows()
        {
            // Arrange
            _outputFileContent.OtherList = new List<double> { 99.0 };

            // Act
            _checker.UpdateOtherList();
            var rows = _checker.ExecutionCheckDataGrid.ItemsSource.Cast<LoadedOutputFileChecker.ExecutionRow>().ToList();

            // Assert
            Assert.That(rows[0].Other, Is.EqualTo(99.0));
            Assert.That(rows.Count, Is.EqualTo(3));
        }

        [Test]
        public void DivideSpectraCb_Uncheck_RevertsValues()
        {
            // Arrange
            _checker.DivideSpectraCb.IsChecked = true;
            _checker.DivideSpectraCb_Checked(null, null);

            // Act
            _checker.DivideSpectraCb.IsChecked = false;
            _checker.DivideSpectraCb_Checked(null, null);
            var rows = _checker.ExecutionCheckDataGrid.ItemsSource.Cast<LoadedOutputFileChecker.ExecutionRow>().ToList();

            // Assert
            Assert.That(rows[0].Spectra, Is.EqualTo(10.0));
        }

        [Test]
        public void UpdateList_HandlesEmptyData()
        {
            // Arrange
            var emptyContent = new OutputFileContent
            {
                FirstLine = "",
                NumberOfColumns = 0,
                TKinList = new List<double>(),
                Spe1e3List = new List<double>()
            };
            var checker = new LoadedOutputFileChecker(emptyContent, _mockMainHelper);

            // Assert
            Assert.That(checker.ExecutionCheckDataGrid.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void InitializeSections_CreatesCorrectGridReferences()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_checker.columnSelectorSections.Count, Is.EqualTo(6));
                Assert.That(_checker.columnSelectorTextBlocks.Count, Is.EqualTo(6));
                Assert.That(_checker.columnSelectorComboBoxes.Count, Is.EqualTo(6));
            });
        }
    }
}
