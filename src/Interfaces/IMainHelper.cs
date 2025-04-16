using CudaHelioCommanderLight.Models;
using System.Collections.Generic;

namespace CudaHelioCommanderLight.Helpers
{
    public interface IMainHelper
    {
        bool ExtractOutputDataFile(string filePath, out Models.OutputFileContent outputFileContent);
        ExecutionStatus ExtractOfflineExecStatus(string offlineResultDirPath);
        AmsExecutionDetail ExtractMultipleOfflineStatus(IEnumerable<string> offlineFilePaths);
        bool ExtractForceFieldOutputDataFile(string filePath, out Models.OutputFileContent outputFileContent);
        bool TryConvertToDouble(string value, out double result);
        bool TryConvertToDecimal(string value, out decimal result);
    }
}
