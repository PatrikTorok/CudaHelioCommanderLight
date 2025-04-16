using NUnit.Framework;
using NSubstitute;
using CudaHelioCommanderLight.Models;
using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Operations;
using System.Collections.Generic;
using System.Windows.Automation;

namespace CudaHelioCommanderLight.Tests
{
    [TestFixture]
    public class DisplayAmsHeatmapWindowOperationTests
    {
        private IMainHelper _mockMainHelper;
        private List<ErrorStructure> _validErrors;
        private DisplayAmsHeatmapModel _validModel;

        [SetUp]
        public void Setup()
        {
            _mockMainHelper = Substitute.For<IMainHelper>();

            _mockMainHelper.TryConvertToDouble(Arg.Any<string>(), out Arg.Any<double>())
                .Returns(x =>
                {
                    x[1] = 0.0;
                    return true;
                });

            _validErrors = new List<ErrorStructure>
            {
                new ErrorStructure(_mockMainHelper) { K0 = 1.0, V = 100, Error = 5.0, MaxError = 10.0 },
                new ErrorStructure(_mockMainHelper) { K0 = 1.0, V = 200, Error = 6.0, MaxError = 12.0 },
                new ErrorStructure(_mockMainHelper) { K0 = 2.0, V = 100, Error = 7.0, MaxError = 14.0 },
                new ErrorStructure(_mockMainHelper) { K0 = 2.0, V = 200, Error = 8.0, MaxError = 16.0 }
            };

            _validModel = new DisplayAmsHeatmapModel
            {
                Errors = _validErrors,
                Tag = "RMS",
                GraphName = "Test Heatmap"
            };
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Operate_ValidInput_GeneratesHeatmapWithoutErrors()
        {
            Assert.DoesNotThrow(() => DisplayAmsHeatmapWindowOperation.Operate(_validModel));
        }
/*
        [Test]
        [Apartment(ApartmentState.STA)]
        public void Operate_InvalidInputXSizeTooSmall_ShowsErrorMessage()
        {
            var invalidErrors = new List<ErrorStructure>
            {
                new ErrorStructure(_mockMainHelper) { K0 = 1.0, V = 100 },
                new ErrorStructure(_mockMainHelper) { K0 = 1.0, V = 200 }
            };
            _validModel.Errors = invalidErrors;

            Assert.DoesNotThrow(() => DisplayAmsHeatmapWindowOperation.Operate(_validModel));
            // Automate clicking "OK"
            AutomationElement dialog = AutomationElement.RootElement.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.NameProperty, "Cannot make map, size too small"));

            if (dialog != null)
            {
                var okButton = dialog.FindFirst(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "OK"));
                InvokePattern clickPattern = okButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                clickPattern.Invoke();
            }
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void Operate_InvalidInputYSizeTooSmall_ShowsErrorMessage()
        {
            var invalidErrors = new List<ErrorStructure>
    {
        new ErrorStructure(_mockMainHelper) { K0 = 1.0, V = 100 },
        new ErrorStructure(_mockMainHelper) { K0 = 2.0, V = 100 }
    };
            _validModel.Errors = invalidErrors;

            Assert.DoesNotThrow(() => DisplayAmsHeatmapWindowOperation.Operate(_validModel));

            // Automate clicking "OK"
            AutomationElement dialog = AutomationElement.RootElement.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.NameProperty, "Cannot make map, size too small"));

            if (dialog != null)
            {
                var okButton = dialog.FindFirst(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "OK"));
                InvokePattern clickPattern = okButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                clickPattern.Invoke();
            }
        }*/
    }

}
