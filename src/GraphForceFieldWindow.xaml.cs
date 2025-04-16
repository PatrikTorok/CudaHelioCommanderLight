using CudaHelioCommanderLight.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using ScottPlot;

namespace CudaHelioCommanderLight
{
    /// <summary>
    /// Interaction logic for GraphForceFieldWindow.xaml
    /// </summary>
    public partial class GraphForceFieldWindow : Window
    {
        private List<ErrorStructure> forceFieldErrors;
        public WpfPlot PlotViewTest => this.PlotView;
        public List<ErrorStructure> ForceFieldErrors => this.forceFieldErrors;
        public GraphForceFieldWindow(List<ErrorStructure> forceFieldErrors)
        {
            InitializeComponent();
            this.forceFieldErrors = forceFieldErrors;
            RenderGraph();
        }

        private void RenderGraph()
        {
            List<Tuple<double, double>> extractedData = new List<Tuple<double, double>>();

            // Loop through each item in the forceFieldErrors list
            foreach (var errorStructure in forceFieldErrors)
            {
                // Parse the integer value from the FilePath string
                string fileName = Path.GetFileNameWithoutExtension(errorStructure.FilePath);
                int xval = int.Parse(fileName);
                double yval = errorStructure.Error;

                // Add the extracted data as a tuple to the list
                extractedData.Add(new Tuple<double, double>(xval, yval));
            }

            // Sort the extracted data by x
            extractedData.Sort((a, b) => a.Item1.CompareTo(b.Item1));

            // Convert the extracted data to separate x and y arrays
            double[] x = extractedData.Select(t => t.Item1).ToArray();
            double[] y = extractedData.Select(t => t.Item2).ToArray();

            // Create a new plot object

            // Add a scatter plot of the x and y arrays
            var scatter = new PlottableScatter(x, y);
            scatter.color = Color.Red;
            PlotView.plt.Add(scatter);

            // Customize the plot appearance
            PlotView.plt.Title("Error vs. X");
            PlotView.plt.XLabel("Force field file number");
            PlotView.plt.YLabel("Error");

            // Render the plot to the plot view control
            PlotView.Render();
        }
    }
}
