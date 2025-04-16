using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Models;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using static CudaHelioCommanderLight.HeatMapGraph;

namespace CudaHelioCommanderLight.Operations
{
    public class ExportAsCsvOperation : Operation<HeatPoint[,]>
    {
        public static new void Operate(HeatPoint[,] HeatPoints, IFileWriter fileWriter, IDialogService dialogService)
        {

            if (!dialogService.SaveFileDialog(out string filePath, "CSV Files (*.csv)|*.csv"))
                return;

            StringBuilder csvData = new StringBuilder();

            foreach (HeatPoint heatPoint in HeatPoints)
            {
                string csvRow = $"{ConvertToString(heatPoint.X)},{ConvertToString(heatPoint.Y)},{ConvertToString(heatPoint.Intensity)}";
                csvData.AppendLine(csvRow);
            }

            try
            {
                fileWriter.WriteToFile(filePath, csvData.ToString());
                dialogService.ShowMessage("Export successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (IOException ex)
            {
                dialogService.ShowMessage($"An error occurred while exporting the CSV file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void Operate(IEnumerable<ErrorStructure> errors, IFileWriter fileWriter, IDialogService dialogService)
        {
            // Show a save file dialog for the user to select the output file
            if (!dialogService.SaveFileDialog(out string filePath, "CSV Files (*.csv)|*.csv"))
                return; // Exit if the user cancels the save dialog

            // Create a StringBuilder to store the CSV data
            StringBuilder csvData = new StringBuilder();

            // Optionally add a CSV header
            csvData.AppendLine("K0,V,Error");

            // Iterate over each ErrorStructure and add its data to the CSV
            foreach (ErrorStructure error in errors)
            {
                // Build the CSV row for each error
                string csvRow = $"{ConvertToString(error.K0)},{ConvertToString(error.V)},{ConvertToString(error.Error)}";
                csvData.AppendLine(csvRow);
            }

            try
            {
                // Write the CSV data to the selected file
                fileWriter.WriteToFile(filePath, csvData.ToString());
                dialogService.ShowMessage("Export successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (IOException ex)
            {
                // Show an error message if an exception occurs during file writing
                dialogService.ShowMessage($"An error occurred while exporting the CSV file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string ConvertToString(double number)
        {
            return number.ToString().Replace(',', '.');
        }
    }
}