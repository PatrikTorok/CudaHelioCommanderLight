using NUnit.Framework;
using System.Windows.Controls;
using System.Windows;
using CudaHelioCommanderLight.Enums;
using CudaHelioCommanderLight.MainWindowServices;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Services;
using NSubstitute;

namespace CudaHelioCommanderLight.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class CompareServiceTests
    {
        private CompareService _compareService;
        private IDialogService _dialogService;

        [SetUp]
        public void Setup()
        {
            _dialogService = Substitute.For<IDialogService>();
            _compareService = new CompareService(_dialogService);
        }

        [Test]
        public void CompareWithLib_LibHelium_ReturnsCorrectPathAndType()
        {
            // Act
            var result = _compareService.CompareWithLib("libHelium", null, null);

            // Assert
            Assert.AreEqual(@"libFiles\lib-helium", result.Item1);
            Assert.AreEqual(LibStructureType.DIRECTORY_SEPARATED, result.Item2);
        }

        [Test]
        public void CompareWithLib_LibProton_ReturnsCorrectPathAndType()
        {
            // Act
            var result = _compareService.CompareWithLib("libProton", null, null);

            // Assert
            Assert.AreEqual(@"libFiles\lib-proton", result.Item1);
            Assert.AreEqual(LibStructureType.DIRECTORY_SEPARATED, result.Item2);
        }

        [Test]
        public void CompareWithLib_ForceField_ReturnsCorrectPathAndType()
        {
            // Act
            var result = _compareService.CompareWithLib("forceField", null, null);

            // Assert
            Assert.AreEqual(@"libFiles\lib-forcefield2023", result.Item1);
            Assert.AreEqual(LibStructureType.FILES_FORCEFIELD2023_COMPARISION, result.Item2);
        }

        [Test]
        public void CompareWithLib_Geliosphere_NoRatioSelected_ShowsErrorMessage()
        {
            // Arrange
            var geliosphereLibRatio = new ComboBox(); // No selection made
            var geliosphereLibType = new ComboBox();

            // Act
            var result = _compareService.CompareWithLib("geliosphere", geliosphereLibRatio, geliosphereLibType);

            // Assert
            Assert.IsNull(result.Item1);
            Assert.AreEqual(LibStructureType.DIRECTORY_SEPARATED, result.Item2);
        }

        [Test]
        public void CompareWithLib_Geliosphere_ValidSelections_ReturnsCorrectPathAndType()
        {
            // Arrange
            var geliosphereLibRatio = new ComboBox();
            geliosphereLibRatio.Items.Add("0.5");
            geliosphereLibRatio.SelectedItem = "0.5";

            var geliosphereLibType = new ComboBox();
            geliosphereLibType.Items.Add("helium");
            geliosphereLibType.SelectedItem = "helium";

            // Act
            var result = _compareService.CompareWithLib("geliosphere", geliosphereLibRatio, geliosphereLibType);

            // Assert
            Assert.AreEqual(@"libFiles\lib-geliosphere-helium-0.5", result.Item1);
            Assert.AreEqual(LibStructureType.FILES_SOLARPROP_LIB, result.Item2);
        }
    }
}
