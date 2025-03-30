using NUnit.Framework;
using NSubstitute;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Models;
using CudaHelioCommanderLight.Services;
using CudaHelioCommanderLight.Operations;
using System.Collections.ObjectModel;
using System.Windows;
using CudaHelioCommanderLight.Enums;
using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.MainWindowServices;
using CudaHelioCommanderLight;
using System.Windows.Controls;
using ScottPlot;
using CudaHelioCommanderLight.Wrappers;
using CudaHelioCommanderLight.Config;

[TestFixture]
[Apartment(ApartmentState.STA)]
public class MainWindowTests
{
    private MainWindow _mainWindow;
    private IMainHelper _mainHelper;
    private IDialogService _dialogService;
    private IHeatMapGraphFactory _heatMapGraphFactory;
    private ButtonService _buttonService;
    private IRenderingService _renderingService;
    private HeatMapService _heatMapService;
    private ICompareService _compareService;
    private IFileWriter _fileWriter;
    private CompareLibraryOperation _compareLibraryOperation;
    private IMetricsConfig _metricsConfig;
    private IOpenConfigurationWindowOperation _openConfigurationWindowOperation;

    [SetUp]
    public void Setup()
    {
        _mainHelper = Substitute.For<IMainHelper>();
        _dialogService = Substitute.For<IDialogService>();
        _heatMapGraphFactory = Substitute.For<IHeatMapGraphFactory>();

        // Initialize services with correct dependencies
        _buttonService = new ButtonService(_dialogService);
        _renderingService = Substitute.For<IRenderingService>();
        _heatMapService = Substitute.For<HeatMapService>(_dialogService, _heatMapGraphFactory);
        _compareService = Substitute.For<ICompareService>();
        _fileWriter = Substitute.For<IFileWriter>();
        _metricsConfig = Substitute.For<IMetricsConfig>();
        _openConfigurationWindowOperation = Substitute.For<IOpenConfigurationWindowOperation>();

        _compareLibraryOperation = new CompareLibraryOperation(
            _dialogService,
            _mainHelper,
            _metricsConfig
        );



        _mainWindow = new MainWindow(
            mainHelper: _mainHelper,
            dialogService: _dialogService,
            buttonService: _buttonService,
            renderingService: _renderingService,
            heatMapService: _heatMapService,
            compareService: _compareService,
            fileWriter: _fileWriter,
            compareLibraryOperation: _compareLibraryOperation,
            metricsConfig: _metricsConfig,
            openConfigWindowOperation : _openConfigurationWindowOperation,
            true
        );

        _mainWindow.amsErrorsListBox = new ListBox();
        _dialogService.SaveFileDialog(out Arg.Any<string>(), Arg.Any<string>())
            .Returns(x => {
                x[0] = "test.csv";
                return true;
            });
    }

    [Test]
    public void AboutUsButton_Click_CallsDialogService()
    {
        // Act
        _mainWindow.AboutUsButton_Click(null, null);

        // Assert
        _dialogService.Received(1).ShowMessage(
            "Slovak Academy of Sciences\n\nDeveloped by: Martin Nguyen, Pavol Bobik\n\nCopyright 2023",
            "About Us",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }

    [Test]
    public void CompareWithLibrary_WhenLibraryNotFound_ShowsErrorMessage()
    {
        // Arrange
        string invalidPath = @"C:\InvalidPath";

        // Act
        _mainWindow.CompareWithLibrary(invalidPath, LibStructureType.DIRECTORY_SEPARATED);

        // Assert
        _dialogService.Received(1).ShowMessage("Library not found", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    [TearDown]
    public void TearDown()
    {
        // Clear received calls for all substitutes
        _heatMapService.ClearReceivedCalls();
        _dialogService.ClearReceivedCalls();
        _compareService.ClearReceivedCalls();
        _renderingService.ClearReceivedCalls();
    }
    [Test]
    public void ExportAsCsvBtn_Click_CallsExportAsCsvOperation()
    {
        // Arrange
        var testErrors = new List<ErrorStructure>
        {
            new ErrorStructure(_mainHelper) { K0 = 1.23, V = 45, Error = 0.5 },
            new ErrorStructure(_mainHelper) { K0 = 2.34, V = 67, Error = 1.2 }
        };
        _mainWindow.amsErrorsListBox.ItemsSource = testErrors;

        // Act
        _mainWindow.ExportAsCsvBtn_Click(null, null);

        // Assert
        _fileWriter.Received(1).WriteToFile(Arg.Any<string>(), Arg.Any<string>());
        _dialogService.Received(1).ShowMessage("Export successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [Test]
    public void RenderAmsGraph_CallsRenderingService()
    {
        // Arrange
        var amsExecution = new AmsExecution
        {
            TKin = new List<double> { 1.0, 2.0, 3.0 },
            Spe1e3 = new List<double> { 10, 20, 30 }
        };
        var errorStructure = new ErrorStructure(_mainHelper)
        {
            TKinList = new List<double> { 1.0 },
            Spe1e3binList = new List<double> { 50.0 },
            FilePath = "error_data/errors.txt"
        };

        // Act
        _mainWindow.RenderAmsGraph(amsExecution, errorStructure);

        // Assert
        _renderingService.Received(1).RenderAmsGraph(amsExecution, Arg.Any<IWpfPlotWrapper>(), errorStructure);
    }


    [Test]
    public void CompareAllLoadedWithLib_WhenLibraryNotFound_ShowsErrorMessage()
    {
        // Arrange
        string invalidPath = @"C:\InvalidPath";

        // Act
        _mainWindow.CompareAllLoadedWithLib(invalidPath, LibStructureType.DIRECTORY_SEPARATED);

        // Assert
        _dialogService.Received(1).ShowMessage("Library not found", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    [Test]
    public void DrawAmsHeatmapBtn_Click_CallsHeatMapService()
    {
        // Arrange
        var button = new Button { Tag = "RMS" };
        _mainWindow.amsComputedErrors = new List<ErrorStructure>();
        _mainWindow.currentDisplayedAmsInvestigation = new AmsExecution { FileName = "TestFile" };

        // Act
        _mainWindow.DrawAmsHeatmapBtn_Click(button, null);

        // Assert
        _heatMapService.Received(1).DrawAmsHeatmapBtn("TestFile", _mainWindow.amsComputedErrors, "RMS");
    }
    [Test]
    public void CompareWithLib_Click_CallsCompareService()
    {
        // Arrange
        var button = new Button { Tag = "libHelium" };
        _compareService.CompareWithLib(
            Arg.Is("libHelium"),
            Arg.Any<ComboBox>(),
            Arg.Any<ComboBox>()
        ).Returns(("libFiles\\lib-helium", LibStructureType.DIRECTORY_SEPARATED));

        // Act
        _mainWindow.CompareWithLib_Click(button, null);

        // Assert
        _compareService.Received(1).CompareWithLib(
            Arg.Is("libHelium"),
            Arg.Any<ComboBox>(),
            Arg.Any<ComboBox>()
        );
    }
    [Test]
    public void AmsErrorsListBox_SelectionChanged_CallsRenderingService()
    {
        // Arrange
        var errorStructure = new ErrorStructure(_mainHelper)
        {
            // Initialize required properties
            TKinList = new List<double> { 1.0, 2.0 },
            Spe1e3binList = new List<double> { 10.0, 20.0 },
            FilePath = "test/path.txt"
        };

        var amsExecution = new AmsExecution
        {
            // Initialize required properties
            TKin = new List<double> { 1.0, 2.0 },
            Spe1e3 = new List<double> { 10.0, 20.0 }
        };

        _mainWindow.amsErrorsListBox = new ListBox();
        _mainWindow.amsErrorsListBox.SelectedItem = errorStructure;

        _mainWindow.dataGridAmsInner = new DataGrid();
        _mainWindow.dataGridAmsInner.SelectedItem = amsExecution;

        // Act
        _mainWindow.AmsErrorsListBox_SelectionChanged(null, null);

        // Assert
        _renderingService.Received(1).AmsErrorsListBox_SelectionChanged(
            Arg.Any<ErrorStructure>(),
            Arg.Any<IWpfPlotWrapper>(),
            Arg.Any<IWpfPlotWrapper>(),
            Arg.Any<AmsExecution>()
        );
    }
    [Test]
    public void CreateErrorGraphBtn_Click_CallsRenderingService()
    {
        // Arrange
        _mainWindow.ActiveCalculationsDataGrid = new DataGrid();

        // Act
        _mainWindow.CreateErrorGraphBtn_Click(null, null);

        // Assert
        _renderingService.Received(1).CreateErrorGraph(_mainWindow.ActiveCalculationsDataGrid);
    }
    [Test]
    public void DrawHeatmapBtn_Click_CallsHeatMapService()
    {
        // Arrange
        var executionDetail = new ExecutionDetail
        {
            FolderName = "TestFolder",
            MethodType = MethodType.FP_1D
        };

        // Add parameters FIRST
        executionDetail.AddK0(1.0);
        executionDetail.AddK0(1.5);
        executionDetail.AddV(2.0);
        executionDetail.AddV(2.5);

        // Then add executions matching these parameters
        executionDetail.Executions.AddRange(new[]
        {
                new Execution(V: 2.0, K0: 1.0, N: 10, dt: 0.1, MethodType.FP_1D) { ErrorValue = 5 },
                new Execution(V: 2.0, K0: 1.5, N: 10, dt: 0.1, MethodType.FP_1D) { ErrorValue = 4 },
                new Execution(V: 2.5, K0: 1.0, N: 10, dt: 0.1, MethodType.FP_1D) { ErrorValue = 3 },
                new Execution(V: 2.5, K0: 1.5, N: 10, dt: 0.1, MethodType.FP_1D) { ErrorValue = 2 }
            });

        var executionDetailList = new ObservableCollection<ExecutionDetail> { executionDetail };
        _mainWindow.ExecutionDetailList = executionDetailList;
        _mainWindow.executionDetailSelectedIdx = 0;

        // Act
        _mainWindow.DrawHeatmapBtn_Click(null, null);

        // Assert
        _heatMapService.Received(1).DrawHeatmapBtn(_mainWindow.ExecutionDetailList, 0);
    }
    [Test]
    public void ExportListAsCsvBtn_Click_WithData_CallsFileWriter()
    {
        // Arrange
        var testErrors = new List<ErrorStructure>
    {
        new ErrorStructure(_mainHelper) { K0 = 1.0, V = 100, Error = 0.1 }
    };
        _mainWindow.amsErrorsListBox.ItemsSource = testErrors;

        // Act
        _mainWindow.ExportListAsCsvBtn_Click(null, null);

        // Assert
        _fileWriter.Received(1).WriteToFile(Arg.Any<string>(), Arg.Any<string>());
        _dialogService.Received(1).ShowMessage(
            "Export successful!",
            "Success",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }
    [Test]
    public void ExportListAsCsvBtn_Click_WithNoData_ShowsWarning()
    {
        // Arrange
        _mainWindow.amsErrorsListBox.ItemsSource = new List<ErrorStructure>();

        // Act
        _mainWindow.ExportListAsCsvBtn_Click(null, null);

        // Assert
        _dialogService.Received(1).ShowMessage(
            "No data available for export.",
            "Warning",
            MessageBoxButton.OK,
            MessageBoxImage.Warning
        );
        _fileWriter.DidNotReceive().WriteToFile(Arg.Any<string>(), Arg.Any<string>());
    }
    [Test]
    public void ComputeErrorBtn_Click_WithInvalidFile_ShowsErrorMessage()
    {
        // Arrange
        _mainWindow.ExecutionDetailList = new ObservableCollection<ExecutionDetail> { new ExecutionDetail() };
        _mainWindow.executionDetailSelectedIdx = 0;

        // Use the correct method with out parameter
        _dialogService.ShowOpenFileDialog(out Arg.Any<string>()).Returns(x => {
            x[0] = "test.csv";  // Set the out parameter
            return true;        // Return dialog result
        });

        _mainHelper.ExtractOutputDataFile(Arg.Any<string>(), out _).Returns(false);

        // Act
        _mainWindow.ComputeErrorBtn_Click(null, null);

        // Assert
        _dialogService.Received(1).ShowMessage(
            "Cannot read data values from the input file.",
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error
        );
    }
    [Test]
    public void ConfigureMetricsBtn_Click_ShowsConfigurationWindow()
    {
        // Arrange
        var mockConfigWindow = Substitute.For<IConfigWindow>();
        mockConfigWindow.HasChanged.Returns(false);

        _openConfigurationWindowOperation
            .Operate(Arg.Any<IMetricsConfig>(), Arg.Any<IMainHelper>())
            .Returns(mockConfigWindow);

        // Act
        _mainWindow.ConfigureMetricsBtn_Click(null, null);

        // Assert
        _openConfigurationWindowOperation.Received(1).Operate(
            Arg.Any<IMetricsConfig>(),
            Arg.Any<IMainHelper>()
        );
    }
}
