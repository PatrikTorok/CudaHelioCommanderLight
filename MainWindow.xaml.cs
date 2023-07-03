﻿using CudaHelioCommanderLight.Config;
using CudaHelioCommanderLight.Enums;
using CudaHelioCommanderLight.Exceptions;
using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Models;
using CudaHelioCommanderLight.Operations;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CudaHelioCommanderLight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ExecutionDetail> ExecutionDetailList { get; set; }
        private string versionStr = "Version: 1.1.0l";
        private MetricsConfig metricsConfig;
        private PanelType currentlyDisplayedPanelType;
        private int executionDetailSelectedIdx = -1;
        private List<string> GeliosphereLibTypes = new List<string>() { "Burger", "JGR" };
        private List<decimal> GeliosphereLibBurgerRatios = new List<decimal>();// { 0.05m, 0.1m, 0.15m, 0.2m, 0.3m};
        private List<decimal> GeliosphereLibJGRRatios = new List<decimal>();// { 0.05m, 0.1m, 0.15m, 0.3m };
        public MainWindow()
        {
            InitializeComponent();

            metricsConfig = new MetricsConfig();
            MetricsUsedTB.Text = metricsConfig.ToString();

            SwitchPanels(PanelType.NONE);
            versionTb.Text = versionStr;

            InitializeAvailableGeliosphereLibs();
        }

        private void InitializeAvailableGeliosphereLibs()
        {
            GeliosphereLibBurgerRatios = new List<decimal>();
            GeliosphereLibJGRRatios = new List<decimal>();

            var libFilesPath = @"libFiles\";

            List<string> directories = Directory.GetDirectories(libFilesPath)
            .Select(Path.GetFileName)
            .ToList();

            foreach (string directoryName in directories)
            {
                if (directoryName.Contains("Burger"))
                {
                    var numberStr = directoryName.Split('-').Last();
                    if (MainHelper.TryConvertToDecimal(numberStr, out decimal number))
                    {
                        GeliosphereLibBurgerRatios.Add(number);
                    }

                }
                else if (directoryName.Contains("JGR"))
                {
                    var numberStr = directoryName.Split('-').Last();
                    if (MainHelper.TryConvertToDecimal(numberStr, out decimal number))
                    {
                        GeliosphereLibJGRRatios.Add(number);
                    }
                }
            }

            geliosphereLibType.ItemsSource = GeliosphereLibTypes;
            geliosphereLibType.SelectedIndex = 0;

            geliosphereLibRatio.ItemsSource = GeliosphereLibBurgerRatios;
            geliosphereLibRatio.SelectedIndex = 0;
        }


        private void AboutUsButton_Click(object sender, RoutedEventArgs e)
        {
            string message = "Slovak Academy of Sciences\n\nDeveloped by: Martin Nguyen, Pavol Bobik\n\nCopyright 2023";

            MessageBox.Show(message, "About Us", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region AMS
        // TODO: move
        private void RenderAmsGraph(AmsExecution amsExecution, ErrorStructure errorStructure = null)
        {
            AmsGraphWpfPlot.Reset();
            var plt = AmsGraphWpfPlot.plt;

            bool firstRun = true;
            double minY = 0.0;
            double maxY = 0.0;


            double[] x = amsExecution.TKin.ToArray();
            double[] y = amsExecution.Spe1e3.ToArray();

            double[] xLog = ScottPlot.Tools.Log10(x);
            double[] yLog = ScottPlot.Tools.Log10(y);

            var amsLegend = Path.GetFileNameWithoutExtension(amsExecution.FileName) + ": reference spectrum";
            plt.PlotScatter(xLog, yLog, markerSize: 1, color: System.Drawing.Color.Orange, label: amsLegend);

            // Temporary X ticks
            double min = x[0];
            double max = x[x.Length - 1];

            List<double> tickPositionsList = new List<double>();
            List<string> tickNamesList = new List<string>();

            for (double z = min; z <= max; z *= 10)
            {
                tickPositionsList.Add(z);
                tickNamesList.Add(z.ToString());
            }

            tickPositionsList.Add(max);
            tickNamesList.Add(max.ToString());

            double[] tickPositions = ScottPlot.Tools.Log10(tickPositionsList.ToArray());
            string[] tickLabels = tickNamesList.ToArray();
            plt.XTicks(tickPositions, tickLabels);


            // Temporary Y ticks
            if (firstRun)
            {
                minY = y.Min();
                maxY = y.Max();
            }
            else
            {
                minY = minY < y.Min() ? minY : y.Min();
                maxY = maxY > y.Max() ? maxY : y.Max();
            }

            firstRun = false;

            //
            if (errorStructure != null)
            {
                double[] x2 = errorStructure.TKinList.ToArray();
                double[] y2 = errorStructure.Spe1e3binList.ToArray();

                double[] xLog2 = ScottPlot.Tools.Log10(x2);
                double[] yLog2 = ScottPlot.Tools.Log10(y2);

                var legend = Path.GetFileName(Path.GetDirectoryName(errorStructure.FilePath)) + ": library spectrum";
                plt.PlotScatter(xLog2, yLog2, markerSize: 1, color: System.Drawing.Color.Green, label: legend);

                // Temporary X ticks
                double min2 = x2[0];
                double max2 = x2[x2.Length - 1];

                List<double> tickPositionsList2 = new List<double>();
                List<string> tickNamesList2 = new List<string>();

                for (double z = min2; z <= max2; z *= 10)
                {
                    tickPositionsList2.Add(z);
                    tickNamesList2.Add(z.ToString());
                }

                tickPositionsList2.Add(max2);
                tickNamesList2.Add(max2.ToString());

                double[] tickPositions2 = ScottPlot.Tools.Log10(tickPositionsList2.ToArray());
                string[] tickLabels2 = tickNamesList2.ToArray();
                plt.XTicks(tickPositions2, tickLabels2);


                minY = minY < y2.Min() ? minY : y2.Min();
                maxY = maxY > y2.Max() ? maxY : y2.Max();

                firstRun = false;
            }
            //


            List<double> tickPositionsListY = new List<double>();
            List<string> tickNamesListY = new List<string>();

            for (double z = minY; z <= maxY; z *= 10)
            {
                if (Math.Abs(z) < 1)
                {
                    z = 1;
                }

                tickPositionsListY.Add(z);
                tickNamesListY.Add(z.ToString("E2"));
            }

            tickPositionsListY.Add(maxY);
            tickNamesListY.Add(maxY.ToString("E2"));

            double[] tickPositionsY = ScottPlot.Tools.Log10(tickPositionsListY.ToArray());
            string[] tickLabelsY = tickNamesListY.ToArray();
            plt.YTicks(tickPositionsY, tickLabelsY);

            plt.PlotHSpan(
                x1: ScottPlot.Tools.Log10(new double[] { metricsConfig.ErrorFromGev }).First(),
                x2: ScottPlot.Tools.Log10(new double[] { metricsConfig.ErrorToGev }).First(),
                draggable: false,
                color: System.Drawing.Color.FromArgb(0, 255, 0, 0),
                alpha: 0.1
                );


            plt.Ticks(useExponentialNotation: true);
            plt.Ticks(logScaleX: true);
            plt.Ticks(logScaleY: true);
            plt.Title("Spectra");
            plt.YLabel("Spe1e3 [proton_flux m^-2sr^-1s^-1GeV^-1]");
            plt.XLabel("Kinetic Energy [GeV]");
            plt.Legend();

            AmsGraphWpfPlot.Render();
        }

        private void RenderAmsRatioGraph(AmsExecution amsExecution, ErrorStructure errorStructure = null)
        {
            AmsGraphRatioWpfPlot.Reset();
            var plt = AmsGraphRatioWpfPlot.plt;

            if (errorStructure == null)
            {
                return;
            }

            double[] x = amsExecution.TKin.ToArray();
            double[] y = amsExecution.Spe1e3.ToArray();
            double[] x2 = errorStructure.TKinList.ToArray();
            double[] y2 = errorStructure.Spe1e3binList.ToArray();

            var ratioX = x.Length < x2.Length ? x : x2;
            double[] ratioY = y.Length < y2.Length ? y : y2;

            var biggerX = x.Length > x2.Length ? x.Length : x2.Length;

            int x1Idx = 0;
            int x2Idx = 0;
            int ratioIdx = 0;

            for (int idx = 0; idx < biggerX; idx++)
            {
                if (ratioIdx >= ratioX.Length)
                {
                    break;
                }


                if ((x.Length > x1Idx && x[x1Idx] == ratioX[ratioIdx]) && (x2.Length > x2Idx && x2[x2Idx] == ratioX[ratioIdx]))
                {
                    ratioY[ratioIdx] = y[x1Idx] / y2[x2Idx];
                    x1Idx++;
                    x2Idx++;
                    ratioIdx++;
                    continue;
                }

                if ((x.Length > x1Idx && x[x1Idx] != ratioX[ratioIdx]))
                {
                    x1Idx++;
                    continue;
                }

                if ((x2.Length > x2Idx && x2[x2Idx] != ratioX[ratioIdx]))
                {
                    x2Idx++;
                    continue;
                }
            }

            //var errorName = Path.GetDirectoryName(Path.GetFileName(errorStructure.FilePath));
            var amsLegend = Path.GetFileNameWithoutExtension(amsExecution.FileName);
            var errorName = Path.GetFileName(Path.GetDirectoryName(errorStructure.FilePath));
            plt.PlotScatter(ratioX, ratioY, markerSize: 1, color: System.Drawing.Color.Red, label: $"Ratio {amsLegend}/ {errorName}");


            plt.PlotHSpan(
                x1: metricsConfig.ErrorFromGev,
                x2: metricsConfig.ErrorToGev,
                draggable: false,
                color: System.Drawing.Color.FromArgb(0, 255, 0, 0),
                alpha: 0.1
                );

            plt.Title("Ratio graph");
            plt.YLabel("Ratio");
            plt.XLabel("Kinetic Energy [GeV]");
            plt.Legend();

            AmsGraphRatioWpfPlot.Render();
        }

        private void DrawAmsHeatmapBtn_Click(object sender, RoutedEventArgs e)
        {
            if (amsComputedErrors.Count == 0)
            {
                MessageBox.Show("Empty errors!");
                return;
            }

            HeatMapGraph heatMap = new HeatMapGraph();
            heatMap.Show();

            var sortedErrors = amsComputedErrors.OrderBy(err => err.K0).ThenBy(err => err.V).ToList();

            var k0List = amsComputedErrors.Select(err => err.K0).Distinct().OrderBy(k => k);
            var vList = amsComputedErrors.Select(err => err.V).Distinct().OrderBy(v => v);
            int xSize = amsComputedErrors.Select(err => err.K0).Distinct().Count();
            int ySize = amsComputedErrors.Select(err => err.V).Distinct().Count();

            if (xSize < 2 || ySize < 2)
            {
                MessageBox.Show("Cannot make map");
                return;
            }

            HeatMapGraph.HeatPoint[,] heatPoints = new HeatMapGraph.HeatPoint[xSize, ySize];

            for (int i = 0; i < xSize; i++)
            {
                var currK0 = k0List.ElementAt(i);
                for (int j = 0; j < ySize; j++)
                {
                    var currV = vList.ElementAt(j);
                    var computedError = sortedErrors.Where(err => err.K0 == currK0 && err.V == currV).FirstOrDefault();

                    //if (computedError == null)
                    //{
                    //    Console.WriteLine("Error NotFound on K0:" + currK0 + " V:" + currV);
                    //    heatPoints[i, j] = new HeatMapGraph.HeatPoint(currK0, currV, 0);
                    //    continue;
                    //}

                    var tag = ((Button)sender).Tag;

                    if (tag.Equals("RMS"))
                    {
                        heatPoints[i, j] = new HeatMapGraph.HeatPoint(computedError.K0, computedError.V, computedError.Error);
                    }
                    else if ((tag.Equals("LowMaxError")))
                    {
                        heatPoints[i, j] = new HeatMapGraph.HeatPoint(computedError.K0, computedError.V, computedError.MaxError);
                    }
                }
            }

            heatMap.SetPoints(heatPoints, xSize, ySize);
            heatMap.Render();

            return;
        }

        private List<ErrorStructure> amsComputedErrors;

        private void CompareWithLibrary(string libPath, LibStructureType libStructureType)
        {
            try
            {
                if (!Directory.Exists(libPath))
                {
                    MessageBox.Show("Library not found", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                //AmsExecution exD = (AmsExecution)dataGridAmsInner.SelectedItem;
                AmsExecution exD = currentDisplayedAmsInvestigation;
                var loadedLib = new List<OutputFileContent>();

                amsComputedErrors = new List<ErrorStructure>();

                if (libStructureType == LibStructureType.DIRECTORY_SEPARATED)
                {

                    foreach (var a in Directory.GetDirectories(libPath))
                    {
                        //var libFile = Path.Combine(libPath, a, "output_1e3bin.dat");
                        var libFile = Path.Combine(a, "output_1e3bin.dat");

                        bool dataExtractSuccess = MainHelper.ExtractOutputDataFile(libFile, out OutputFileContent outputFileContent);

                        if (!dataExtractSuccess)
                        {
                            MessageBox.Show("Cannot read data values from the input file.");
                            return;
                        }

                        // ...
                        var dividedList = new List<double>();
                        for (int idx = 0; idx < outputFileContent.TKinList.Count(); idx++)
                        {
                            dividedList.Add(outputFileContent.Spe1e3List[idx] / outputFileContent.Spe1e3NList[idx]);
                        }
                        outputFileContent.Spe1e3List = dividedList;

                        //var error = ComputeError(exD, outputFileContent);

                        var error = new ErrorStructure();
                        ComputeError(exD, outputFileContent, out double errVal, out double maxErrVal);
                        error.Error = errVal;
                        error.MaxError = maxErrVal;
                        error.FilePath = outputFileContent.FilePath;
                        error.DirName = Path.GetFileName(Path.GetDirectoryName(outputFileContent.FilePath));
                        error.TKinList = outputFileContent.TKinList;
                        error.Spe1e3binList = outputFileContent.Spe1e3List;
                        error.TrySetVAndK0(error.DirName, libStructureType);

                        amsComputedErrors.Add(error);
                    }
                }
                else if (libStructureType == LibStructureType.FILES_SOLARPROP_LIB)
                {
                    foreach (var libFile in Directory.GetFiles(libPath))
                    {
                        bool dataExtractSuccess = MainHelper.ExtractOutputDataFile(libFile, out OutputFileContent outputFileContent);

                        if (!dataExtractSuccess)
                        {
                            MessageBox.Show("Cannot read data values from the input file.");
                            return;
                        }

                        var dividedList = new List<double>();
                        for (int idx = 0; idx < outputFileContent.TKinList.Count(); idx++)
                        {
                            dividedList.Add(outputFileContent.Spe1e3List[idx]);
                        }
                        outputFileContent.Spe1e3List = dividedList;

                        //var error = ComputeError(exD, outputFileContent);

                        var error = new ErrorStructure();
                        ComputeError(exD, outputFileContent, out double errVal, out double maxErrVal);
                        error.Error = errVal;
                        error.MaxError = maxErrVal;
                        error.FilePath = outputFileContent.FilePath;
                        error.DirName = Path.GetFileName(outputFileContent.FilePath);
                        error.TKinList = outputFileContent.TKinList;
                        error.Spe1e3binList = outputFileContent.Spe1e3List;
                        error.TrySetVAndK0(Path.GetFileName(outputFileContent.FilePath), libStructureType);

                        amsComputedErrors.Add(error);
                    }
                }
                else if (libStructureType == LibStructureType.FILES_FORCEFIELD2023 || libStructureType == LibStructureType.FILES_FORCEFIELD2023_COMPARISION)
                {
                    // TODO: Violation, temp, prerobit nejak
                    var forceFieldErrors = new List<ErrorStructure>();

                    foreach (var libFile in Directory.GetFiles(libPath))
                    {
                        bool dataExtractSuccess = MainHelper.ExtractForceFieldOutputDataFile(libFile, out OutputFileContent outputFileContent);

                        if (!dataExtractSuccess)
                        {
                            MessageBox.Show("Cannot read data values from the input file.");
                            return;
                        }

                        var dividedList = new List<double>();
                        for (int idx = 0; idx < outputFileContent.TKinList.Count(); idx++)
                        {
                            dividedList.Add(outputFileContent.Spe1e3List[idx]);
                        }
                        outputFileContent.Spe1e3List = dividedList;

                        //var error = ComputeError(exD, outputFileContent);

                        var error = new ErrorStructure();
                        ComputeError(exD, outputFileContent, out double errVal, out double maxErrVal);
                        error.Error = errVal;
                        error.MaxError = maxErrVal;
                        error.FilePath = outputFileContent.FilePath;
                        error.DirName = Path.GetFileName(outputFileContent.FilePath);
                        error.TKinList = outputFileContent.TKinList;
                        error.Spe1e3binList = outputFileContent.Spe1e3List;

                        // tmp - FILES_FORCEFIELD2023_COMPARISION
                        string fileName = Path.GetFileNameWithoutExtension(error.FilePath);
                        int xval = int.Parse(fileName);
                        error.V = xval;
                        error.K0 = 0;
                        // end tmp

                        forceFieldErrors.Add(error);
                    }


                    if (libStructureType == LibStructureType.FILES_FORCEFIELD2023_COMPARISION)
                    {
                        amsComputedErrors.AddRange(forceFieldErrors);
                    }
                    else
                    {
                        GraphForceFieldWindow graphForceFieldWindow = new GraphForceFieldWindow(forceFieldErrors);
                        graphForceFieldWindow.Show();
                        return;
                    }
                }

                //amsErrorsListBox.Items.Clear();
                if (sortByError)
                {
                    amsErrorsListBox.ItemsSource = amsComputedErrors.OrderBy(er => er.Error).ToList();
                }
                else
                {
                    amsErrorsListBox.ItemsSource = amsComputedErrors.OrderBy(er => er.MaxError).ToList();
                }

                // Assign lowest values
                currentDisplayedAmsInvestigation.LowRMSError = amsComputedErrors.OrderBy(er => er.Error).FirstOrDefault().Error.ToString("0.##") + "%";
                currentDisplayedAmsInvestigation.MinError = amsComputedErrors.OrderBy(er => er.MaxError).FirstOrDefault().MaxError.ToString("0.##") + "%";
                dataGridAmsInner.Items.Refresh();
            }
            catch(WrongConfigurationException e)
            {
                MessageBox.Show(e.Message);
                OpenConfigurationWindow();
            }
        }

        private void CompareAllLoadedWithLib(string libPath, LibStructureType libStructureType)
        {
            try {
            if (!Directory.Exists(libPath))
            {
                MessageBox.Show("Library not found", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (libStructureType == LibStructureType.DIRECTORY_SEPARATED)
            {
                foreach (var exD in AmsExecutionList)
                {
                    var loadedLib = new List<OutputFileContent>();

                    amsComputedErrors = new List<ErrorStructure>();

                    foreach (var a in Directory.GetDirectories(libPath))
                    {
                        //var libFile = Path.Combine(libPath, a, "output_1e3bin.dat");
                        var libFile = Path.Combine(a, "output_1e3bin.dat");

                        bool dataExtractSuccess = MainHelper.ExtractOutputDataFile(libFile, out OutputFileContent outputFileContent);

                        if (!dataExtractSuccess)
                        {
                            MessageBox.Show("Cannot read data values from the input file.");
                            return;
                        }

                        // ...
                        var dividedList = new List<double>();
                        for (int idx = 0; idx < outputFileContent.TKinList.Count(); idx++)
                        {
                            dividedList.Add(outputFileContent.Spe1e3List[idx] / outputFileContent.Spe1e3NList[idx]);
                        }
                        outputFileContent.Spe1e3List = dividedList;

                        //var error = ComputeError(exD, outputFileContent);

                        var error = new ErrorStructure();
                        ComputeError(exD, outputFileContent, out double errVal, out double maxErrVal);
                        error.Error = errVal;
                        error.MaxError = maxErrVal;
                        error.FilePath = outputFileContent.FilePath;
                        error.DirName = Path.GetFileName(Path.GetDirectoryName(outputFileContent.FilePath));
                        error.TKinList = outputFileContent.TKinList;
                        error.Spe1e3binList = outputFileContent.Spe1e3List;
                        error.TrySetVAndK0(error.DirName, libStructureType);

                        amsComputedErrors.Add(error);
                    }

                    // Assign lowest values
                    exD.LowRMSError = amsComputedErrors.OrderBy(er => er.Error).FirstOrDefault().Error.ToString("0.##") + "%";
                    exD.MinError = amsComputedErrors.OrderBy(er => er.MaxError).FirstOrDefault().MaxError.ToString("0.##") + "%";
                    dataGridAmsInner.Items.Refresh();
                }
            }
            else if (libStructureType == LibStructureType.FILES_SOLARPROP_LIB)
            {
                foreach (var exD in AmsExecutionList)
                {
                    var loadedLib = new List<OutputFileContent>();

                    amsComputedErrors = new List<ErrorStructure>();

                    foreach (var libFile in Directory.GetFiles(libPath))
                    {
                        bool dataExtractSuccess = MainHelper.ExtractOutputDataFile(libFile, out OutputFileContent outputFileContent);

                        if (!dataExtractSuccess)
                        {
                            MessageBox.Show("Cannot read data values from the input file.");
                            return;
                        }

                        var dividedList = new List<double>();
                        for (int idx = 0; idx < outputFileContent.TKinList.Count(); idx++)
                        {
                            dividedList.Add(outputFileContent.Spe1e3List[idx]);
                        }
                        outputFileContent.Spe1e3List = dividedList;

                        var error = new ErrorStructure();
                        ComputeError(exD, outputFileContent, out double errVal, out double maxErrVal);
                        error.Error = errVal;
                        error.MaxError = maxErrVal;
                        error.FilePath = outputFileContent.FilePath;
                        error.DirName = Path.GetFileName(outputFileContent.FilePath);
                        error.TKinList = outputFileContent.TKinList;
                        error.Spe1e3binList = outputFileContent.Spe1e3List;
                        error.TrySetVAndK0(Path.GetFileName(outputFileContent.FilePath), libStructureType);

                        amsComputedErrors.Add(error);
                    }

                    // Assign lowest values
                    exD.LowRMSError = amsComputedErrors.OrderBy(er => er.Error).FirstOrDefault().Error.ToString("0.##") + "%";
                    exD.MinError = amsComputedErrors.OrderBy(er => er.MaxError).FirstOrDefault().MaxError.ToString("0.##") + "%";
                    dataGridAmsInner.Items.Refresh();
                }
            }
            }
            catch (WrongConfigurationException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void CompareWithHeliumLibBtn_Click(object sender, RoutedEventArgs e)
        {
            var libPath = @"libFiles\lib-helium";
            CompareWithLibrary(libPath, LibStructureType.DIRECTORY_SEPARATED);
        }

        private void CompareWithLibBtn_Click(object sender, RoutedEventArgs e)
        {
            var libPath = @"libFiles\lib-proton";
            //var libPath = @"C:\Users\marti\Desktop\temp\Sat_Jul_24_09_01_02_2021\dt=50.000000K0=1.5e+22V=300.000000\output_1e3bin.dat";
            CompareWithLibrary(libPath, LibStructureType.DIRECTORY_SEPARATED);
        }

        private void CompareWithGeliosphereLibBtn_Click(object sender, RoutedEventArgs e)
        {
            var libtype = geliosphereLibType.SelectedItem.ToString();
            var libRatio = geliosphereLibRatio.SelectedItem.ToString().Replace(',', '.');
            var libPath = $"libFiles\\lib-geliosphere-{libtype}-{libRatio}";

            Console.WriteLine(libPath);
            CompareWithLibrary(libPath, LibStructureType.FILES_SOLARPROP_LIB);
        }

        private void CompareAllLoadedWithHeLibBtn_Click(object sender, RoutedEventArgs e)
        {
            var libPath = @"libFiles\lib-helium";
            CompareAllLoadedWithLib(libPath, LibStructureType.DIRECTORY_SEPARATED);
        }

        private async void CompareAllLoadedWithLibBtn_Click(object sender, RoutedEventArgs e)
        {
            var libPath = @"libFiles\lib-proton";
            //var libPath = @"C:\Users\marti\Desktop\temp\Sat_Jul_24_09_01_02_2021\dt=50.000000K0=1.5e+22V=300.000000\output_1e3bin.dat";
            CompareAllLoadedWithLib(libPath, LibStructureType.DIRECTORY_SEPARATED);
        }

        private async void CompareAllLoadedWithSolarpropLibBtn_Click(object sender, RoutedEventArgs e)
        {
            var libPath = @"libFiles\lib-solarprop";
            //var libPath = @"C:\Users\marti\Desktop\temp\Sat_Jul_24_09_01_02_2021\dt=50.000000K0=1.5e+22V=300.000000\output_1e3bin.dat";
            CompareAllLoadedWithLib(libPath, LibStructureType.FILES_SOLARPROP_LIB);
        }


        private void ComputeError(AmsExecution amsExecution, OutputFileContent libraryItem, out double error, out double maxError)
        {
            List<double> referenceTi = libraryItem.Spe1e3List;
            List<double> eta = new List<double>();

            error = double.NaN;
            maxError = double.NaN;

            if (referenceTi == null || referenceTi.Count == 0)
            {
                return;
            }

            if (referenceTi == null || referenceTi.Count == 0)
            {
                return;
            }

            List<double> computedTi = new List<double>();

            computedTi = amsExecution.Spe1e3.ToList();

            for (int i = 0; i < amsExecution.Spe1e3.Count; i++)
            {
                eta.Add(0);
            }

            maxError = 0;

            double sumUp = 0;
            double sumDown = 0;

            int etaIdx = 0;

            var firstRequiredValue = (double)((int)(metricsConfig.ErrorFromGev * 10) / 10.0);
            var lastRequiredValue = (double)((int)(metricsConfig.ErrorFromGev * 10) / 10.0);
            if (amsExecution.TKin.IndexOf(firstRequiredValue) == -1 || libraryItem.TKinList.IndexOf(firstRequiredValue) == -1)
            {
                throw new WrongConfigurationException($"Starting GeV value {firstRequiredValue} from configuration file is not found in library. Adjust configuration file first.");
            }
            else if (amsExecution.TKin.IndexOf(lastRequiredValue) == -1 || libraryItem.TKinList.IndexOf(lastRequiredValue) == -1)
            {
                throw new WrongConfigurationException($"Ending GeV value {lastRequiredValue} from configuration file is not found in library. Adjust configuration file first.");
            }

            for (double T = metricsConfig.ErrorFromGev; T <= metricsConfig.ErrorToGev; T += 0.1)
            //for (double T = 0.5; T <= 2; T += 0.1)
            {
                double TValue = (double)((int)(T * 10) / 10.0);
                int idx1 = amsExecution.TKin.IndexOf(TValue);
                int idx2 = libraryItem.TKinList.IndexOf(TValue);
                eta[etaIdx] = (computedTi[idx1] - referenceTi[idx2]) / referenceTi[idx2];

                sumUp += eta[etaIdx] * eta[etaIdx];
                sumDown++;

                etaIdx++;



                var ratio = Math.Abs(100 - ((computedTi[idx1] / referenceTi[idx2]) * 100));
                if (ratio > maxError)
                {
                    maxError = ratio;
                }
            }

            error = Math.Sqrt(sumUp / sumDown);
            error *= 100.0;
        }

        private void AmsErrorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var error = (ErrorStructure)amsErrorsListBox.SelectedItem;
            AmsExecution exD = (AmsExecution)dataGridAmsInner.SelectedItem;

            RenderAmsGraph(exD, error);
            RenderAmsRatioGraph(exD, error);
            openedLibraryNameTb.Text = error.DirName;
        }

        private bool sortByError = true;

        private void AmsApplyFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(((Button)sender).Tag?.ToString()))
            {
                var str = ((Button)sender).Tag.ToString();

                if (str == "error")
                {
                    sortByError = true;
                }
                else
                {
                    sortByError = false;
                }
            }

            var filterV = amsFilterVTb.Text;
            var filterK0 = amsFilterK0Tb.Text;

            var filteredList = amsComputedErrors.ToList();

            if (!string.IsNullOrEmpty(filterV))
            {
                bool success = MainHelper.TryConvertToDouble(filterV, out double V);
                filteredList = filteredList.Where(er => er.V == (int)V).ToList();
            }

            if (!string.IsNullOrEmpty(filterK0))
            {
                bool success = MainHelper.TryConvertToDouble(filterK0, out double K0);
                filteredList = filteredList.Where(er => AreDoubleValuesEqual(er.K0, K0)).ToList();
            }

            //amsErrorsListBox.Items.Clear();
            if (sortByError)
            {
                amsErrorsListBox.ItemsSource = filteredList.OrderBy(er => er.Error).ToList();
            }
            else
            {
                amsErrorsListBox.ItemsSource = filteredList.OrderBy(er => er.MaxError).ToList();
            }
        }

        private bool AreDoubleValuesEqual(double d1, double d2)
        {
            return Math.Abs(d1 - d2) < 0.0000001;
        }

        private void geliosphereLibType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (geliosphereLibType.SelectedIndex)
            {
                case 0: // burger
                    geliosphereLibRatio.ItemsSource = GeliosphereLibBurgerRatios;
                    geliosphereLibRatio.SelectedIndex = 0;
                    break;
                case 1: // JGR
                    geliosphereLibRatio.ItemsSource = GeliosphereLibJGRRatios;
                    geliosphereLibRatio.SelectedIndex = 0;
                    break;
            }
        }

        private void CompareWithForceFieldLibBtn_Copy_Click(object sender, RoutedEventArgs e)
        {
            var libPath = @"libFiles\lib-forcefield2023";
            //var libPath = @"C:\Users\marti\Desktop\temp\Sat_Jul_24_09_01_02_2021\dt=50.000000K0=1.5e+22V=300.000000\output_1e3bin.dat";
            CompareWithLibrary(libPath, LibStructureType.FILES_FORCEFIELD2023_COMPARISION);
        }

        private void ExportAsCsvBtn_Click(object sender, RoutedEventArgs e)
        {
            ExportAsCsvOperation.Operate((IEnumerable<ErrorStructure>)amsErrorsListBox.ItemsSource);
        }
        #endregion AMS

        private void OpenExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();

            System.Windows.Forms.DialogResult dialogResult = folderDialog.ShowDialog();

            if (dialogResult != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            string selectedFolderPath = folderDialog.SelectedPath;

            if (string.IsNullOrEmpty(selectedFolderPath))
            {
                return;
            }

            SwitchPanels(PanelType.STATUS_CHECKER);

            ExecutionStatus executionStatus = MainHelper.ExtractOfflineExecStatus(selectedFolderPath);

            ExecutionDetailList = new ObservableCollection<ExecutionDetail>(executionStatus.activeExecutions);
            ActiveCalculationsDataGrid.ItemsSource = ExecutionDetailList;
        }

        private void SwitchPanels(PanelType panelType)
        {
            this.ExplorerMainpanel.Visibility = Visibility.Hidden;
            this.ExplorerLeftPanel.Visibility = Visibility.Hidden;
            this.ExplorerRightPanel.Visibility = Visibility.Hidden;
            this.StatusCheckerMainPanel.Visibility = Visibility.Hidden;
            this.StatusCheckerGridPanelDetail.Visibility = Visibility.Hidden;
            this.AmsInvestigationPanel.Visibility = Visibility.Hidden;
            this.AmsInvestigationDetailPanel.Visibility = Visibility.Hidden;

            switch (panelType)
            {
                case PanelType.STATUS_CHECKER_DETAIL:
                    this.StatusCheckerGridPanelDetail.Visibility = Visibility.Visible;
                    break;
                case PanelType.STATUS_CHECKER:
                    this.StatusCheckerMainPanel.Visibility = Visibility.Visible;
                    break;
                case PanelType.EXPLORER:
                    this.ExplorerMainpanel.Visibility = Visibility.Visible;
                    this.ExplorerRightPanel.Visibility = Visibility.Visible;
                    this.ExplorerLeftPanel.Visibility = Visibility.Visible;
                    break;
                case PanelType.AMS_INVESTIGATION:
                    this.AmsInvestigationPanel.Visibility = Visibility.Visible;
                    break;
                case PanelType.AMS_INVESTIGATION_DETAIL:
                    this.AmsInvestigationDetailPanel.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }

            currentlyDisplayedPanelType = panelType;

        }

        private ObservableCollection<AmsExecution> AmsExecutionList { get; set; }
        private AmsExecution currentDisplayedAmsInvestigation;

        private void RmsComputeModeBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = true;
            open.Title = "Select AMS files";

            if (open.ShowDialog() == false)
            {
                return;
            }

            SwitchPanels(PanelType.AMS_INVESTIGATION);
            AmsExecutionDetail executionStatus = MainHelper.ExtractMultipleOfflineStatus(open.FileNames.ToList());

            AmsExecutionList = new ObservableCollection<AmsExecution>(executionStatus.AmsExecutions);
            dataGridAmsInner.ItemsSource = AmsExecutionList;
        }

        private void CloseMainPanelButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentlyDisplayedPanelType == PanelType.EXPLORER)
            {
                SwitchPanels(PanelType.STATUS_CHECKER_DETAIL);
            }
            else if (currentlyDisplayedPanelType == PanelType.STATUS_CHECKER_DETAIL)
            {
                SwitchPanels(PanelType.STATUS_CHECKER);
            }
            else if (currentlyDisplayedPanelType == PanelType.STATUS_CHECKER || currentlyDisplayedPanelType == PanelType.AMS_INVESTIGATION)
            {
                SwitchPanels(PanelType.NONE);
            }
            else if (currentlyDisplayedPanelType == PanelType.AMS_INVESTIGATION_DETAIL)
            {
                SwitchPanels(PanelType.AMS_INVESTIGATION);
            }
        }

        //private void DgItemCheckboxClicked(object sender, RoutedEventArgs e)
        //{
        //    if (ActiveCalculationsDataGrid.SelectedIndex == -1)
        //    {
        //        return;
        //    }

        //    CheckBox clickedCb = (CheckBox)sender;
        //    ExecutionDetail selectedExecutionDetail = (ExecutionDetail)ActiveCalculationsDataGrid.SelectedItem;

        //    selectedExecutionDetail.IsSelected = clickedCb.IsChecked == true;
        //}

        //private void CalculationDetailButton_Click(object sender, RoutedEventArgs e)
        //{
        //    ExecutionDetail exD = (ExecutionDetail)ActiveCalculationsDataGrid.SelectedItem;
        //    executionDetailSelectedIdx = ExecutionDetailList.IndexOf(exD);
        //    SwitchPanels(PanelType.STATUS_CHECKER_DETAIL);

        //    dgInner.ItemsSource = exD.Executions;
        //}

        private void CreateErrorGraphBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                List<ExecutionDetail> selectedExecutionDetails = new List<ExecutionDetail>();


                if (fileDialog.ShowDialog() == false)
                {
                    return;
                }
                string filePath = fileDialog.FileName;
                bool dataExtractSuccess = MainHelper.ExtractOutputDataFile(filePath, out OutputFileContent outputFileContent);

                if (!dataExtractSuccess)
                {
                    MessageBox.Show("Cannot read data values from the input file.");
                    return;
                }

                foreach (ExecutionDetail executionDetail in ActiveCalculationsDataGrid.Items)
                {
                    if (executionDetail.IsSelected)
                    {
                        selectedExecutionDetails.Add(executionDetail);

                        foreach (Execution execution in executionDetail.Executions)
                        {
                            ExecutionHelper.InitializeOutput1e3BinDataFromOnlineDir(execution);

                            if (execution.StandardDeviatons != null)
                            {
                                try
                                {
                                    execution.ComputeError(outputFileContent, metricsConfig); // If error in computeError, this was changed recently
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
            catch(WrongConfigurationException ex)
            {
                MessageBox.Show(ex.Message);
                OpenConfigurationWindow();
            }
        }

        private void RenderGraphOfErrors(List<ExecutionDetail> selectedExecutionDetails)
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

            bool firstRun = true;
            double minY = 0.0;
            double maxY = 0.0;

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
                firstRun = false;
            }

            plt.XTicks(Enumerable.Range(0, vAndKToIdx.Count).Select(a => (double)a).ToArray(), vAndKToIdx.ToArray());

            plt.Title("Plotted errors");
            plt.YLabel("Error");
            plt.XLabel("Index");
            plt.Legend();
            var plotViewer = new ScottPlot.WpfPlotViewer(plt);

            plotViewer.Show();
        }

        private void ActiveCalculationsDataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Button b = sender as Button;

            DataGrid dataGrid = sender as DataGrid;
            DataGridRow row = findParentOfType<DataGridRow>(e.OriginalSource as DependencyObject);

            if (dataGrid != null && row != null)
            {
                //the row DataContext is the selected item
                if (dataGrid.SelectedItems.Contains(row.DataContext))
                {
                    dataGrid.SelectedItems.Remove(row.DataContext);
                    //mark event as handled so that datagrid does not
                    //just select it again on the current click.
                    e.Handled = true;
                }
            }
        }

        //This helper is used to find your target, which in your case is a DataGridRow.
        //In general, if you continue with WPF, I would suggest adding
        //this very handy method to your extensions library.
        private T findParentOfType<T>(DependencyObject source) where T : DependencyObject
        {
            T ret = default(T);
            UIElement parent = VisualTreeHelper.GetParent(source) as UIElement;

            if (parent != null)
            {
                ret = parent as T ?? findParentOfType<T>(parent) as T;
            }
            return ret;
        }

        private void ImageTeplateRightMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            ImageViewer imageViewer = new ImageViewer((string)((Image)sender).Tag);
            imageViewer.ShowDialog();
        }

        private void AmsSelectDetailButton_Click(object sender, RoutedEventArgs e)
        {
            AmsExecution exD = (AmsExecution)dataGridAmsInner.SelectedItem;
            var executionDetailSelectedIdx = AmsExecutionList.IndexOf(exD);
            currentDisplayedAmsInvestigation = exD;
            SwitchPanels(PanelType.AMS_INVESTIGATION_DETAIL);

            RenderAmsGraph(exD);
            amsComputedErrors = new List<ErrorStructure>();
            amsErrorsListBox.ItemsSource = new List<ErrorStructure>();
        }

        private void ConfigureMetricsBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenConfigurationWindow();
        }

        private void OpenConfigurationWindow()
        {
            var configWindowResult = OpenConfigurationWindowOperation.Operate(metricsConfig);

            this.metricsConfig = configWindowResult.MetricsConfig;
            this.MetricsUsedTB.Text = metricsConfig.ToString();

            if (currentlyDisplayedPanelType == PanelType.AMS_INVESTIGATION_DETAIL && configWindowResult.HasChanged)
            {
                CompareWithLibBtn_Click(null, null);
            }
        }

        private void DrawForceField2023RmsBtn_Click(object sender, RoutedEventArgs e)
        {
            var libPath = @"libFiles\lib-forcefield2023";
            CompareWithLibrary(libPath, LibStructureType.FILES_FORCEFIELD2023);
        }

        private void DgItemCheckboxClicked(object sender, RoutedEventArgs e)
        {
            if (ActiveCalculationsDataGrid.SelectedIndex == -1)
            {
                return;
            }

            CheckBox clickedCb = (CheckBox)sender;
            ExecutionDetail selectedExecutionDetail = (ExecutionDetail)ActiveCalculationsDataGrid.SelectedItem;

            selectedExecutionDetail.IsSelected = clickedCb.IsChecked == true;
        }

        private void CalculationDetailButton_Click(object sender, RoutedEventArgs e)
        {
            ExecutionDetail exD = (ExecutionDetail)ActiveCalculationsDataGrid.SelectedItem;
            executionDetailSelectedIdx = ExecutionDetailList.IndexOf(exD);
            SwitchPanels(PanelType.STATUS_CHECKER_DETAIL);

            dgInner.ItemsSource = exD.Executions;
        }

        private void DetailBtn_Click(object sender, RoutedEventArgs e)
        {
            Execution openedExecution = GetCurrentlyOpenedExecution();

            if (string.IsNullOrEmpty(openedExecution.LocalDirPath))
            {
                MessageBox.Show("Sorry, detail view for this execution is not available");
                return;
            }

            var windowExplorer = new WindowExplorer(this, openedExecution);

            windowExplorer.LoadDataToUI();

            SwitchPanels(PanelType.EXPLORER);
        }

        private Execution GetCurrentlyOpenedExecution()
        {
            if (dgInner.SelectedIndex != -1)
            {
                return (Execution)dgInner.SelectedItem;
            }

            return null;
        }

        private void ExportJsonBtn_Click(object sender, RoutedEventArgs e)
        {
            ExecutionDetail executionDetail = ExecutionDetailList[executionDetailSelectedIdx];

            if (executionDetail == null)
            {
                return;
            }

            SaveFileDialog fileDialog = new SaveFileDialog();

            fileDialog.Filter = "JSON File|*.json";
            fileDialog.Title = "Save JSON File";
            fileDialog.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (fileDialog.FileName != "")
            {
                ExportAsJsonOperation.Operate(executionDetail.Executions, fileDialog.FileName);
            }
        }

        private void ComputeErrorBtn_Click(object sender, RoutedEventArgs e)
        {
            ExecutionDetail executionDetail = ExecutionDetailList[executionDetailSelectedIdx];

            OpenFileDialog fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == true)
            {
                string filePath = fileDialog.FileName;

                // extracted input reference file
                //bool dataExtractSuccess = MainHelper.ExtractOutputDataFile(filePath, out List<double> tKinList, out List<double> spe1e3NList, out List<double> spe1e3List, out List<double> stdDeviationList);
                // TMP!!
                bool dataExtractSuccess = MainHelper.ExtractOutputDataFile(filePath, out OutputFileContent outputFileContent);

                if (!dataExtractSuccess)
                {
                    MessageBox.Show("Cannot read data values from the input file.");
                    return;
                }

                LoadedOutputFileChecker dataChecker = new LoadedOutputFileChecker(outputFileContent);

                dataChecker.ShowDialog();


                // TODO: temp!
                //StreamWriter sw = new StreamWriter(@"C:\Users\marti\Desktop\computed-error.txt");

                foreach (Execution execution in executionDetail.Executions)
                {
                    ExecutionHelper.InitializeOutput1e3BinDataFromOnlineDir(execution);

                    if (execution.StandardDeviatons != null)
                    {
                        //execution.ComputeError(outputFileContent.Spe1e3List); // If error in computeError, this was changed recently
                        try
                        {
                            execution.ComputeError(outputFileContent, metricsConfig); // If error in computeError, this was changed recently
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            MessageBox.Show(ex.ToString());
                            return;
                        }
                    }

                    // TODO: temp!
                    //sw.WriteLine($"{execution.V}\t{execution.K0}\t{execution.Error}\t");
                }

                // TODO: temp!
                //sw.Close();

                dgInner.Items.Refresh();
            }
        }

        private void Spe1e3CanvasFit30gevCanvas_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DisplayImageOperation.Operate(spe1e3Fit30gevCanvas.Background);
        }

        private void Spe1e3CanvasFitCanvas_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DisplayImageOperation.Operate(spe1e3FitCanvas.Background);
        }

        private void Spe1e3Canvas_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DisplayImageOperation.Operate(spe1e3Canvas.Background);
        }

        private void Spe1e3nCanvas_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DisplayImageOperation.Operate(spe1e3nCanvas.Background);
        }

        private void Spe1e3Canvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mainCanvas.Background = spe1e3Canvas.Background;
        }

        private void Spe1e3nCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mainCanvas.Background = spe1e3nCanvas.Background;
        }

        private void Spe1e3Fit30gevCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mainCanvas.Background = spe1e3Fit30gevCanvas.Background;
        }

        private void Spe1e3FitCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mainCanvas.Background = spe1e3FitCanvas.Background;
        }

        private void DrawHeatmapBtn_Click(object sender, RoutedEventArgs e)
        {
            HeatMapGraph heatMap = new HeatMapGraph();
            heatMap.Show();

            ExecutionDetail executionDetail = ExecutionDetailList[executionDetailSelectedIdx];

            int xSize = executionDetail.paramK0.Count;
            int ySize = executionDetail.paramV.Count;

            if (xSize < 2 || ySize < 2)
            {
                MessageBox.Show("Cannot make map");
                return;
            }

            HeatMapGraph.HeatPoint[,] heatPoints = new HeatMapGraph.HeatPoint[xSize, ySize];

            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    double k0 = executionDetail.paramK0[i];
                    double V = executionDetail.paramV[j];
                    Execution ex = executionDetail.GetExecutionByParam(V, k0);

                    if (ex == null)
                    {
                        heatPoints[i, j] = new HeatMapGraph.HeatPoint(k0, V, double.NaN);
                        continue;
                    }

                    double error = ex.ErrorValue;

                    heatPoints[i, j] = new HeatMapGraph.HeatPoint(k0, V, error);
                }
            }

            heatMap.SetPoints(heatPoints, xSize, ySize);
            heatMap.Render();

            return;
        }
    }
}