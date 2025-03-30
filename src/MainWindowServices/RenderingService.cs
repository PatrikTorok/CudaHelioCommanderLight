using CudaHelioCommanderLight.Config;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Models;
using CudaHelioCommanderLight.Operations;
using Microsoft.Win32;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Wrappers;

namespace CudaHelioCommanderLight.MainWindowServices
{
    public class RenderingService : IRenderingService
    {
        private readonly IDialogService _dialogService;
        private readonly IMainHelper _mainHelper;
        public RenderingService(IMainHelper mainHelper, IDialogService dialogService)
        {
            _mainHelper = mainHelper;
            _dialogService = dialogService; 
        }
        public ErrorStructure? AmsErrorsListBox_SelectionChanged(ErrorStructure errorStructure, IWpfPlotWrapper amsGraphWpfPlot,
            IWpfPlotWrapper amsGraphRatioWpfPlot, AmsExecution amsExecution)
        {
            ErrorStructure error = errorStructure;
            if (error == null)
            {
                return null;
            }

            AmsExecution exD = amsExecution;

            RenderAmsGraph(exD, amsGraphWpfPlot, error);
            RenderAmsRatioGraph(exD, amsGraphRatioWpfPlot, error);

            return error;
        }

        public void RenderAmsGraph(AmsExecution amsExecution, IWpfPlotWrapper amsGraphWpfPlot,
            ErrorStructure? errorStructure = null)
        {
            try
            {
                amsGraphWpfPlot.Reset();
            }
            catch { amsGraphWpfPlot = new WpfPlotWrapper(new WpfPlot()); }
            var amsExecutionErrorModel = new AmsExecutionPltErrorModel()
            {
                AmsExecution = amsExecution,
                ErrorStructure = errorStructure,
                PltWrapper = amsGraphWpfPlot.PlotWrapper,
            };

            RenderAmsErrorGraphOperation.Operate(amsExecutionErrorModel, _mainHelper);
            amsGraphWpfPlot.Render();
        }

        public void RenderAmsRatioGraph(AmsExecution amsExecution, IWpfPlotWrapper amsGraphRatioWpfPlot,
            ErrorStructure? errorStructure = null)
        {
            try
            {
                amsGraphRatioWpfPlot.Reset();
            }
            catch { amsGraphRatioWpfPlot = new WpfPlotWrapper(new WpfPlot()); }

            var plotWrapper = amsGraphRatioWpfPlot.PlotWrapper;

            var amsExecutionErrorModel = new AmsExecutionPltErrorModel()
            {
                AmsExecution = amsExecution,
                ErrorStructure = errorStructure,
                PltWrapper = plotWrapper
            };

            RenderAmsErrorRatioGraphOperation.Operate(amsExecutionErrorModel, _mainHelper);
            amsGraphRatioWpfPlot.Render();
        }


        public void CreateErrorGraph(DataGrid activeCalculationsDataGrid)
        {

            List<ExecutionDetail> selectedExecutionDetails = new List<ExecutionDetail>();


            if (!_dialogService.ShowOpenFileDialog(out string filePath))
            {
                return;
            }

            bool dataExtractSuccess = _mainHelper.ExtractOutputDataFile(filePath, out OutputFileContent outputFileContent);

            if (!dataExtractSuccess)
            {
                _dialogService.ShowMessage("Cannot read data values from the input file.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            foreach (ExecutionDetail executionDetail in (ObservableCollection<ExecutionDetail>)activeCalculationsDataGrid.ItemsSource)
            {
                if (executionDetail.IsSelected)
                {
                    selectedExecutionDetails.Add(executionDetail);

                    foreach (Execution execution in executionDetail.Executions)
                    {
                        ExecutionHelper.InitializeOutput1e3BinDataFromOnlineDir(execution, _mainHelper);
                        if (execution.StandardDeviatons != null)
                        {
                            try { 
                                execution.ComputeError(outputFileContent, MetricsConfig.GetInstance(_mainHelper));
                            }
                            catch (ArgumentOutOfRangeException ex)
                            {
                                MessageBox.Show(ex.ToString());
                                return;
                            }
                        }
                    }
                }
            }

            RenderGraphOfErrors(selectedExecutionDetails);
        }

        internal void RenderGraphOfErrors(List<ExecutionDetail> selectedExecutionDetails)
        {
            List<System.Drawing.Color> colorList = new List<System.Drawing.Color>()
            {
                System.Drawing.Color.Red,
                System.Drawing.Color.Green,
                System.Drawing.Color.Blue,
                System.Drawing.Color.Aqua,
                System.Drawing.Color.Orange
            };
            var plt = new ScottPlot.Plot(600, 400);

            List<string> vAndKToIdx = new List<string>();

            //foreach (GraphInfo graphInfo in loadedGraphs)
            for (int idx = 0; idx < selectedExecutionDetails.Count; idx++)
            {
                List<Execution> nLowestExecutions = selectedExecutionDetails[idx].GetLowestExecutions(10);

                foreach (Execution e in nLowestExecutions)
                {
                    string stringify = "V=" + e.V + "K0=" + e.K0;

                    if (!vAndKToIdx.Contains(stringify))
                    {
                        vAndKToIdx.Add(stringify);
                    }
                }

                double[] x = nLowestExecutions.Select(e => (double)vAndKToIdx.IndexOf("V=" + e.V + "K0=" + e.K0)).ToArray();
                double[] y = nLowestExecutions.Select(e => e.ErrorValue).ToArray();

                plt.PlotScatter(x, y, markerSize: 5, lineWidth: 0, color: colorList[idx], label: selectedExecutionDetails[idx].FolderName);
            }

            plt.XTicks(Enumerable.Range(0, vAndKToIdx.Count).Select(a => (double)a).ToArray(), vAndKToIdx.ToArray());

            plt.Title("Plotted errors");
            plt.YLabel("Error");
            plt.XLabel("Index");
            plt.Legend();
            var plotViewer = new ScottPlot.WpfPlotViewer(plt);
            plotViewer.Show();
        }
    }
}
