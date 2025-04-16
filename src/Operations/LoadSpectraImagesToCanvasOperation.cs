using CudaHelioCommanderLight.Constants;
using CudaHelioCommanderLight.Models;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;

namespace CudaHelioCommanderLight.Operations
{
    public class LoadSpectraImagesToCanvas : Operation<WindowExecutionModel, object>
    {
        public static new void Operate(WindowExecutionModel windowExecutionModel)
        {
            var mainWindow = windowExecutionModel.MainWindow;
            var execution = windowExecutionModel.Execution;
            string localDirPath = execution.LocalDirPath;

            string localGraphSpe1e3FilePath = Path.Combine(localDirPath, GlobalFilesToDownload.DetailSpe1e3GraphFile);
            string localGraphSpe1e3NFilePath = Path.Combine(localDirPath, GlobalFilesToDownload.DetailSpe1e3NGraphFile);
            string localGraphSpe1e3FitFilePath = Path.Combine(localDirPath, GlobalFilesToDownload.DetailSpe1e3FitGraphFile);
            string localGraphSpe1e3Fit30FilePath = Path.Combine(localDirPath, GlobalFilesToDownload.DetailSpe1e3Fit30GraphFile);


            if (File.Exists(localGraphSpe1e3NFilePath))
            {
                mainWindow.spe1e3nCanvas.Background = new ImageBrush(new BitmapImage(new Uri(localGraphSpe1e3NFilePath)));
            }
            else
            {
                mainWindow.spe1e3nCanvas.Background = new SolidColorBrush(Colors.Transparent);
            }

            if (File.Exists(localGraphSpe1e3FilePath))
            {
                mainWindow.spe1e3Canvas.Background = new ImageBrush(new BitmapImage(new Uri(localGraphSpe1e3FilePath)));
                mainWindow.mainCanvas.Background = new ImageBrush(new BitmapImage(new Uri(localGraphSpe1e3FilePath)));
            }
            else
            {
                mainWindow.spe1e3Canvas.Background = new SolidColorBrush(Colors.Transparent);
                mainWindow.mainCanvas.Background = new SolidColorBrush(Colors.Transparent);
            }

            if (File.Exists(localGraphSpe1e3Fit30FilePath))
            {
                mainWindow.spe1e3Fit30gevCanvas.Background = new ImageBrush(new BitmapImage(new Uri(localGraphSpe1e3Fit30FilePath)));
            }
            else
            {
                mainWindow.spe1e3Fit30gevCanvas.Background = new SolidColorBrush(Colors.Transparent);
            }

            if (File.Exists(localGraphSpe1e3FitFilePath))
            {
                mainWindow.spe1e3FitCanvas.Background = new ImageBrush(new BitmapImage(new Uri(localGraphSpe1e3FitFilePath)));
            }
            else
            {
                mainWindow.spe1e3FitCanvas.Background = new SolidColorBrush(Colors.Transparent);
            }
        }
    }
}
