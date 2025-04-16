using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Models;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace CudaHelioCommanderLight.Operations
{
    public static class ExportListAsCsvOperation
    {
        public static void Operate(IEnumerable<ErrorStructure> exportList, IFileWriter fileWriter, IDialogService dialogService)
        {
            // Show a save file dialog for the user to select the output file
            if (!dialogService.SaveFileDialog(out string filePath, "CSV Files (*.csv)|*.csv"))
                return;

            // Create a StringBuilder to store the CSV data
            StringBuilder csvData = new StringBuilder();

            // Iterate over each ErrorStructure and add its data to the CSV
            foreach (var record in exportList)
            {
                // Build the CSV row
                string csvRow = $"{record.DisplayName},{record.Error},{record.MaxError},{record.FilePath}";
                csvData.AppendLine(csvRow);
            }

            try
            {
                // Use the injected file writer to write the CSV data to the selected file
                fileWriter.WriteToFile(filePath, csvData.ToString());

                dialogService.ShowMessage("Export successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (IOException ex)
            {
                dialogService.ShowMessage($"An error occurred while exporting the CSV file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
