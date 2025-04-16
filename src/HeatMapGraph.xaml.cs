using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Operations;
using CudaHelioCommanderLight.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CudaHelioCommanderLight
{
    /// <summary>
    /// Interaction logic for HeatMapGraph.xaml
    /// </summary>
    public partial class HeatMapGraph : Window
    {
        public class HeatPoint
        {
            public double X;
            public double Y;
            public double Intensity;
            public HeatPoint(double iX, double iY, double bIntensity)
            {
                X = iX;
                Y = iY;
                Intensity = bIntensity;
            }
        }

        public string GraphTitle { get; set; }
        public string XLabel { get; set; }
        public string YLabel { get; set; }
        public string ColorbarLabel { get; set; }

        internal HeatPoint[,] HeatPoints;
        internal int xCount;
        internal int yCount;

        private ArrayList drawnObjects;
        internal bool mark5Lowest = false;
        internal bool mark5Highest = false;

        private List<Color> ColorsOfMap = new List<Color>();
        private byte Alpha = 0xff;

        private bool isMinMaxColorValueExternal;
        internal double minColorValue;
        internal double maxColorValue;

        internal IFileWriter fileWriter;
        internal IDialogService dialogService;
        private readonly IMainHelper _mainHelper;

        public HeatMapGraph(IMainHelper mainHelper)
        {
            InitializeComponent();
            drawnObjects = new ArrayList();
            GraphTitle = string.Empty;

            ColorsOfMap.AddRange(new Color[]{
                Color.FromArgb(Alpha, 0, 0, 0xFF) ,//Blue
                Color.FromArgb(Alpha, 0, 0xFF, 0xFF) ,//Cyan
                Color.FromArgb(Alpha, 0, 0xFF, 0) ,//Green
                Color.FromArgb(Alpha, 0xFF, 0xFF, 0) ,//Yellow
                Color.FromArgb(Alpha, 0xFF, 0, 0) ,//Red
            });

            isMinMaxColorValueExternal = false;
            _mainHelper = mainHelper ?? throw new ArgumentNullException(nameof(mainHelper));
        }

        public HeatMapGraph(double minColorValue, double maxColorValue, IMainHelper mainHelper) : this(mainHelper)
        {
            isMinMaxColorValueExternal = true;
            this.minColorValue = minColorValue;
            this.maxColorValue = maxColorValue;
        }

        public HeatMapGraph() : this(new MainHelper()) { }

        public Color ComputeColor(double minVal, double maxVal, double val)
        {
            if (val > maxVal)
            {
                val = maxVal;
            }

            double valPerc = (val - minVal) / (maxVal - minVal);
            double colorPerc = 1d / (ColorsOfMap.Count - 1);// % of each block of color. the last is the "100% Color"
            double blockOfColor = valPerc / colorPerc;// the integer part repersents how many block to skip
            int blockIdx = (int)Math.Truncate(blockOfColor);// Idx of 
            double valPercResidual = valPerc - (blockIdx * colorPerc);//remove the part represented of block 
            double percOfColor = valPercResidual / colorPerc;// % of color of this block that will be filled

            Color cTarget = ColorsOfMap[blockIdx];
            Color cNext = val == maxVal ? ColorsOfMap[blockIdx] : ColorsOfMap[blockIdx + 1];

            var deltaR = cNext.R - cTarget.R;
            var deltaG = cNext.G - cTarget.G;
            var deltaB = cNext.B - cTarget.B;

            var R = cTarget.R + (deltaR * percOfColor);
            var G = cTarget.G + (deltaG * percOfColor);
            var B = cTarget.B + (deltaB * percOfColor);

            Color c = ColorsOfMap[0];
            try
            {
                c = Color.FromArgb(Alpha, (byte)R, (byte)G, (byte)B);
            }
            catch (Exception ex)
            {
            }
            return c;
        }

        public void SetPoints(HeatPoint[,] heatPoints, int xCount, int yCount)
        {
            this.HeatPoints = heatPoints;
            this.xCount = xCount;
            this.yCount = yCount;
        }

        public void Render()
        {
            List<double> intensities = new List<double>();

            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    intensities.Add(HeatPoints[i, j].Intensity);
                }
            }

            List<double> first10Lowest = intensities.Where(x => !double.IsNaN(x)).OrderBy(x => x).Take(10).ToList();
            List<double> first10Highest = intensities.Where(x => !double.IsNaN(x)).OrderByDescending(x => x).Take(10).ToList();

            double min = 999;
            double max = -999;

            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    double intensity = HeatPoints[i, j].Intensity;

                    if (double.IsNaN(intensity))
                    {
                        continue;
                    }

                    if (intensity > max)
                    {
                        max = intensity;
                    }

                    if (intensity < min)
                    {
                        min = intensity;
                    }
                }
            }

            // Set UI of min/max values and validate
            if (!isMinMaxColorValueExternal)
            {
                MinColorValueTb.Text = min.ToString();
                MaxColorValueTb.Text = max.ToString();

                MaxTb.Text = string.Format("{0:F2}%", max);
                MinTb.Text = string.Format("{0:F2}%", min);
            }
            else
            {
                MaxTb.Text = string.Format("{0:F2}%", maxColorValue);
                MinTb.Text = string.Format("{0:F2}%", minColorValue);
            }

            GraphTitleLabelTb.Text = GraphTitle;
            ColorbarLabelTb.Text = ColorbarLabel;

            int tileWidth = 25;
            int tileHeight = 25;

            for (int i = 0; i < xCount; i++)
            {
                HeatPoint point = HeatPoints[i, 0];
                int top = 0 + i * tileWidth;
                int left = 0 + yCount * tileHeight;
                string text = point.X.ToString();

                ShapeDrawing.DrawText(GraphCanvas, text, top, left, Brushes.Black, drawnObjects);
            }

            int yLabelTop = 30 + xCount / 2 * tileWidth;
            int yLabelLeft = 20 + (yCount + 1) * tileHeight;
            ShapeDrawing.DrawText(GraphCanvas, YLabel, yLabelTop, yLabelLeft, Brushes.Black, drawnObjects, true, 270);

            int xLabelTop = 0 + (xCount + 1) * tileWidth;
            int xLabelLeft = 0 + yCount / 2 * tileHeight;
            ShapeDrawing.DrawText(GraphCanvas, XLabel, xLabelTop, xLabelLeft, Brushes.Black, drawnObjects, true);

            for (int i = 0; i < yCount; i++)
            {
                HeatPoint point = HeatPoints[0, i];
                int top = 0 + xCount * tileWidth;
                int left = 0 + i * tileHeight;
                string text = point.Y.ToString();

                ShapeDrawing.DrawText(GraphCanvas, text, top, left, Brushes.Black, drawnObjects);
            }

            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    HeatPoint point = HeatPoints[i, j];

                    int top = 0 + i * tileWidth;
                    int left = 0 + j * tileHeight;

                    Brush fillBrush;
                    if (double.IsNaN(point.Intensity))
                    {
                        fillBrush = new SolidColorBrush(Colors.White);
                    }
                    else
                    {
                        if (!isMinMaxColorValueExternal)
                        {
                            fillBrush = new SolidColorBrush(ComputeColor(min, max, point.Intensity));
                        }
                        else
                        {
                            fillBrush = new SolidColorBrush(ComputeColor(minColorValue, maxColorValue, point.Intensity));
                        }
                    }

                    ShapeDrawing.DrawSolidRectangle(GraphCanvas, top, left, tileWidth, tileHeight, fillBrush, drawnObjects);

                    if (mark5Lowest && first10Lowest.Contains(point.Intensity))
                    {
                        double centerX = left + (tileWidth / 2);
                        double centerY = top + (tileHeight / 2);
                        double radius = 5;

                        ShapeDrawing.DrawCircle(GraphCanvas, centerX, centerY, radius, Brushes.Yellow, drawnObjects);
                    }

                    if (mark5Highest && first10Highest.Contains(point.Intensity))
                    {
                        double centerX = left + (tileWidth / 2);
                        double centerY = top + (tileHeight / 2);
                        double radius = 5;
                        ShapeDrawing.DrawCircle(GraphCanvas, centerX, centerY, radius, Brushes.Cyan, drawnObjects);
                    }
                }
            }

            LinearGradientBrush brush = new LinearGradientBrush();

            for (int i = 0; i < ColorsOfMap.Count; i++)
            {
                brush.GradientStops.Add(new GradientStop(ColorsOfMap[i], 1.0 - (double)i / (ColorsOfMap.Count - 1)));
            }

            GradientRect.Fill = brush;
        }

        private void UndrawHistogram()
        {
            foreach (UIElement element in drawnObjects)
            {
                GraphCanvas.Children.Remove(element);
            }

            drawnObjects = new ArrayList();
        }

        internal void Mark5LowestHighestCb_Checked(object sender, RoutedEventArgs e)
        {
            mark5Lowest = Mark5LowestCb.IsChecked == true;
            mark5Highest = Mark5HighestCb.IsChecked == true;
            UndrawHistogram();
            Render();
        }
        
        internal void RerenderBtn_Click(object sender, RoutedEventArgs e)
        {
            isMinMaxColorValueExternal = true;
            _mainHelper.TryConvertToDouble(MinColorValueTb.Text, out double outMinColorValue);
            _mainHelper.TryConvertToDouble(MaxColorValueTb.Text, out double outMaxColorValue);

            minColorValue = outMinColorValue;
            maxColorValue = outMaxColorValue;
            Render();
        }

        private void SetMax100Btn_Click(object sender, RoutedEventArgs e)
        {
            MaxColorValueTb.Text = "100";
            RerenderBtn_Click(null, null);
        }

        private void SetMin0Btn_Click(object sender, RoutedEventArgs e)
        {
            MinColorValueTb.Text = "0";
            RerenderBtn_Click(null, null);
        }

        internal void SetMaxToMaxBtn_Click(object sender, RoutedEventArgs e)
        {
            List<double> intensities = new List<double>();

            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    intensities.Add(HeatPoints[i, j].Intensity);
                }
            }

            var max = intensities.Where(x => !double.IsNaN(x)).OrderByDescending(x => x).FirstOrDefault();
            MaxColorValueTb.Text = max.ToString();
            RerenderBtn_Click(null, null);
        }

        internal void SetMinToMinBtn_Click(object sender, RoutedEventArgs e)
        {
            List<double> intensities = new List<double>();

            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    intensities.Add(HeatPoints[i, j].Intensity);
                }
            }

            var min = intensities.Where(x => !double.IsNaN(x)).OrderBy(x => x).FirstOrDefault();
            MinColorValueTb.Text = min.ToString();
            RerenderBtn_Click(null, null);
        }

        internal void ExportAsCsvBtn_Click(object sender, RoutedEventArgs e)
        {
            if (fileWriter == null) {
                fileWriter = new FileWriter();
            }
            if (dialogService == null)
            {
                dialogService = new DialogService();
            }
            ExportAsCsvOperation.Operate(HeatPoints, fileWriter, dialogService);
        }
    }
}
