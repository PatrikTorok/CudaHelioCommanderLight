using CudaHelioCommanderLight.Constants;
using CudaHelioCommanderLight.Enums;
using CudaHelioCommanderLight.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CudaHelioCommanderLight.Helpers
{
    public class MainHelper : IMainHelper
    {
        public ExecutionStatus ExtractOfflineExecStatus(string offlineResultDirPath)
        {
            ExecutionStatus executionStatus = new ExecutionStatus();

            foreach (string localMainFolderPath in Directory.GetDirectories(offlineResultDirPath))
            {
                string localRunningInfoFile = Path.Combine(localMainFolderPath, GlobalFilesToDownload.RunningInfoFile);

                ExecutionDetail executionDetail = ParseDetailFromRunningFile(localRunningInfoFile);

                if (executionDetail == null) continue;

                foreach (string subDirName in Directory.GetDirectories(localMainFolderPath))
                {
                    string localSubFolderPath = Path.Combine(localMainFolderPath, subDirName);

                    executionDetail.AddExecutionFilePathsToExecution(localSubFolderPath, string.Empty, localMainFolderPath, string.Empty);
                }

                executionStatus.GetActiveExecutions().Add(executionDetail);
            }

            return executionStatus;
        }

        public AmsExecutionDetail ExtractMultipleOfflineStatus(IEnumerable<string> offlineFilePaths)
        {
            //List<DirectoryInfo> dirInfoList = ExecutionHelper.GetExecutionDirectories
            AmsExecutionDetail amsExecutionDetail = new AmsExecutionDetail();

            foreach (string localFilePath in offlineFilePaths)
            {
                if (!File.Exists(localFilePath))
                {
                    Console.WriteLine("Error opening file " + localFilePath);
                    continue;
                }

                AmsExecution amsExecution = new AmsExecution();
                amsExecution.FilePath = localFilePath;
                amsExecution.FileName = Path.GetFileName(localFilePath);
                amsExecution.TKin = new List<double>();
                amsExecution.Spe1e3 = new List<double>();

                // ####
                string[] lines = System.IO.File.ReadAllLines(localFilePath);

                foreach (string line in lines)
                {
                    if (line.StartsWith('#'))
                    {
                        continue;
                    }
                    else if (string.IsNullOrEmpty(line.Trim()))
                    {
                        continue;
                    }

                    // Accepting Tkin | spe1e3
                    string[] parsedLine = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parsedLine.Count() == 2)
                    {
                        var parseSuccess = TryConvertToDouble(parsedLine[0], out double tKin);
                        parseSuccess &= TryConvertToDouble(parsedLine[1], out double spe1e3);

                        if (!parseSuccess)
                        {
                            throw new Exception("Exception during parsing tKin and spe1e3 of ams spectra");
                        }

                        amsExecution.TKin.Add(tKin);
                        amsExecution.Spe1e3.Add(spe1e3);
                    }
                    else if (parsedLine.Count() == 4)
                    {
                        var parseSuccess = TryConvertToDouble(parsedLine[0], out double tKin);
                        parseSuccess &= TryConvertToDouble(parsedLine[2], out double spe1e3);

                        if (!parseSuccess)
                        {
                            throw new Exception("Exception during parsing tKin and spe1e3 of ams spectra");
                        }

                        amsExecution.TKin.Add(tKin);
                        amsExecution.Spe1e3.Add(spe1e3);
                    }
                    else
                    {
                        Console.WriteLine(localFilePath + " - error, Accepting Tkin | spe1e3 [or count 4 for AMS solarprop], got line count " + parsedLine.Count());
                    }
                }

                if (amsExecution.TKin.Count > 0)
                {
                    amsExecutionDetail.AmsExecutions.Add(amsExecution);
                }
            }

            return amsExecutionDetail;
        }

        public bool ExtractForceFieldOutputDataFile(string filePath, out OutputFileContent outputFileContent)
        {
            // TODO: refactor
            outputFileContent = new OutputFileContent();
            outputFileContent.IsValid = false;
            outputFileContent.FilePath = filePath;

            List<double> tKinList = new List<double>(); // GeV
            List<double> intensities = new List<double>(); // Intenzita v pocte protonov na m2 s sr GeV
            List<double> intensitiesLIS = new List<double>(); // Intenzita LIS v pocte protonov na m2 s sr GeV
            List<double> rigidities = new List<double>(); // Rigidity RV

            if (File.Exists(filePath))
            {
                string[] fileContent = File.ReadAllLines(filePath);

                for (int idx = 0; idx < fileContent.Length; idx++)
                {
                    string line = fileContent[idx];

                    string[] lineData = line.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    // Initialize info
                    if (idx == 0)
                    {
                        outputFileContent.NumberOfColumns = lineData.Length;
                        outputFileContent.FirstLine = line;
                    }

                    if (lineData.Length > 4 || lineData.Length < 2)
                    {
                        continue;
                    }

                    bool convertSuccess = true;
                    double tKin = 0.0, intensity = 0.0, intensityLIS = 0.0, rigidity = 0.0;

                    if (lineData.Length == 4)
                    {
                        convertSuccess &= TryConvertToDouble(lineData[0], out tKin);
                        convertSuccess &= TryConvertToDouble(lineData[1], out intensity);
                        convertSuccess &= TryConvertToDouble(lineData[2], out intensityLIS);
                        convertSuccess &= TryConvertToDouble(lineData[2], out rigidity);

                        if (convertSuccess)
                        {
                            tKinList.Add(tKin);
                            intensities.Add(intensity);
                            intensitiesLIS.Add(intensityLIS);
                            rigidities.Add(rigidity);
                        }
                    }
                }

                outputFileContent.IsValid = true;

                // Potrebne porovnavatL spektrum 2 stlpec, nie treti, TODO: refactor, ale 2. stlpec davame na spe1e3
                outputFileContent.Spe1e3List = intensities;
                outputFileContent.Spe1e3NList = intensitiesLIS;
                outputFileContent.TKinList = tKinList;
                outputFileContent.StdDevList = rigidities;
            }
            else
            {
                return false;
            }

            return true;
        }

        /*
         * Extracts output file (output1e3bin) file to outputFileContent class
         * Supported formats:
         * TKin | Spe1e3
         * TKin | Spe1e3N | Spe1e3 | StdDev
         * If any other format is used, it can be modified with LoadedOutputFileChecker window
        */

        public bool ExtractOutputDataFile(string filePath, out OutputFileContent outputFileContent)
        {
            outputFileContent = new OutputFileContent();
            outputFileContent.IsValid = false;
            outputFileContent.FilePath = filePath;

            List<double> tKinList = new List<double>();
            List<double> spe1e3NList = new List<double>();
            List<double> spe1e3List = new List<double>();
            List<double> stDeviationList = new List<double>();

            if (File.Exists(filePath))
            {
                string[] fileContent = File.ReadAllLines(filePath);

                for (int idx = 0; idx < fileContent.Length; idx++)
                {
                    string line = fileContent[idx];

                    string[] lineData = line.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    // Initialize info
                    if (idx == 0)
                    {
                        outputFileContent.NumberOfColumns = lineData.Length;
                        outputFileContent.FirstLine = line;
                    }

                    if (lineData.Length > 4 || lineData.Length < 2)
                    {
                        continue;
                    }

                    bool convertSuccess = true;
                    double tKin = 0.0, spe1e3N = 0.0, spe1e3 = 0.0, stdDeviation = 0.0;

                    if (lineData.Length == 4)
                    {
                        convertSuccess &= TryConvertToDouble(lineData[0], out tKin);
                        convertSuccess &= TryConvertToDouble(lineData[1], out spe1e3N);
                        convertSuccess &= TryConvertToDouble(lineData[2], out spe1e3);
                        convertSuccess &= TryConvertToDouble(lineData[2], out stdDeviation);

                        if (convertSuccess)
                        {
                            tKinList.Add(tKin);
                            spe1e3NList.Add(spe1e3N);
                            spe1e3List.Add(spe1e3);
                            stDeviationList.Add(stdDeviation);
                        }
                    }
                    else if (lineData.Length == 3)
                    {

                        convertSuccess &= TryConvertToDouble(lineData[0], out tKin);
                        convertSuccess &= TryConvertToDouble(lineData[1], out spe1e3N);
                        convertSuccess &= TryConvertToDouble(lineData[2], out spe1e3);

                        if (convertSuccess)
                        {
                            tKinList.Add(tKin);
                            spe1e3NList.Add(spe1e3N);
                            spe1e3List.Add(spe1e3);
                        }
                    }
                    else if (lineData.Length == 2)
                    {

                        convertSuccess &= TryConvertToDouble(lineData[0], out tKin);
                        convertSuccess &= TryConvertToDouble(lineData[1], out spe1e3);


                        if (convertSuccess)
                        {
                            tKinList.Add(tKin);
                            spe1e3List.Add(spe1e3);
                        }
                    }
                }

                outputFileContent.IsValid = true;
                outputFileContent.Spe1e3List = spe1e3List;
                outputFileContent.Spe1e3NList = spe1e3NList;
                outputFileContent.TKinList = tKinList;
                outputFileContent.StdDevList = stDeviationList;
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool TryConvertToDouble(string value, out double result)
        {
            var converted = double.TryParse(value, out result);

            if (converted) return true;

            converted = double.TryParse(value.Replace(".", ","), out result);

            if (converted) return true;

            return false;
        }

        public bool TryConvertToDecimal(string value, out decimal result)
        {
            var converted = decimal.TryParse(value, out result);

            if (converted) return true;

            converted = decimal.TryParse(value.Replace(".", ","), out result);

            if (converted) return true;

            return false;
        }

        private Dictionary<string, string> keyWords = new Dictionary<string, string>()
        {
            { "grid", "Grid-run" },
            { "type", "Type" },
            { "v-size", "V-size" },
            { "k-size", "K0-size" },
            { "dt-size", "dt-size" },
            { "n-size", "N-size" },
            { "v-params", "V-params" },
            { "k-params", "K0-params" },
            { "n-params", "N-params" },
            { "dt-params", "dt-params" },
            { "algorithm-started", "Algorithm-started" },
            { "curand-started", "Curand-Initialization-Started" },
            { "curand-ended", "Curand-Initialization-Ended" },
            { "progress", "Progress" },
            { "iterations", "Iterations" }
        };
        private ExecutionDetail ParseDetailFromRunningFile(string runFilePath)
        {
            ExecutionDetail executionDetail = new ExecutionDetail();

            if (!File.Exists(runFilePath))
            {
                return null;
            }

            string[] lines = System.IO.File.ReadAllLines(runFilePath);

            int iterations = 1;
            Execution execution = null;

            executionDetail.FolderName = Path.GetFileName(Path.GetDirectoryName(runFilePath)); // TODO: prone to errors..

            foreach (string line in lines)
            {
                if (line.Contains(keyWords["grid"]))
                {
                    executionDetail.IsGrid = line.Split(' ')[1] == "1";
                }
                else if (line.Contains(keyWords["type"]))
                {
                    string param = parseParam(line);
                    if (param.Equals("FWMethod"))
                    {
                        executionDetail.MethodType = MethodType.FP_1D;
                    }
                    else if (param.Equals("BPMethod"))
                    {
                        executionDetail.MethodType = MethodType.BP_1D;
                    }
                    else if (param.Equals("BPHeliumMethod"))
                    {
                        executionDetail.MethodType = MethodType.BP_HELIUM_1D;
                    }
                }
                else if (line.Contains(keyWords["v-size"]))
                {
                    executionDetail.VSize = Int32.Parse(parseParam(line));
                }
                else if (line.Contains(keyWords["k-size"]))
                {
                    executionDetail.K0Size = Int32.Parse(parseParam(line));
                }
                else if (line.Contains(keyWords["dt-size"]))
                {
                    executionDetail.DtSize = Int32.Parse(parseParam(line));
                }
                else if (line.Contains(keyWords["n-size"]))
                {
                    executionDetail.NSize = Int32.Parse(parseParam(line));
                }
                else if (line.Contains(keyWords["iterations"]))
                {
                    iterations = Int32.Parse(parseParam(line));
                }
                else if (line.Contains(keyWords["v-params"]))
                {
                    List<string> vParams = parseMultiParams(line);
                    foreach (string vParam in vParams)
                    {
                        TryConvertToDouble(vParam, out double v);
                        executionDetail.AddV(v);
                    }
                }
                else if (line.Contains(keyWords["k-params"]))
                {
                    List<string> kParams = parseMultiParams(line);
                    foreach (string kParam in kParams)
                    {
                        TryConvertToDouble(kParam, out double k0);
                        executionDetail.AddK0(k0);
                    }
                }
                else if (line.Contains(keyWords["n-params"]))
                {
                    List<string> nParams = parseMultiParams(line);
                    foreach (string nParam in nParams)
                    {
                        TryConvertToDouble(nParam, out double n);
                        executionDetail.AddN(n);
                    }
                }
                else if (line.Contains(keyWords["dt-params"]))
                {
                    List<string> dtParams = parseMultiParams(line);
                    foreach (string dtParam in dtParams)
                    {
                        TryConvertToDouble(dtParam, out double dt);
                        executionDetail.AddDt(dt);
                    }
                }
                else if (line.Contains(keyWords["algorithm-started"]))
                {
                    List<string> multiParams = parseMultiParams(line);
                    if (multiParams.Count == 5)
                    {
                        TryConvertToDouble(multiParams[1], out double v);
                        TryConvertToDouble(multiParams[2], out double k0);
                        TryConvertToDouble(multiParams[3], out double n);
                        TryConvertToDouble(multiParams[4], out double dt);
                        execution = new Execution(v, k0, n, dt, executionDetail.MethodType);
                    }
                    else
                    {
                        TryConvertToDouble(multiParams[0], out double v);
                        TryConvertToDouble(multiParams[1], out double k0);
                        TryConvertToDouble(multiParams[2], out double n);
                        TryConvertToDouble(multiParams[3], out double dt);
                        execution = new Execution(v, k0, n, dt, executionDetail.MethodType);
                    }
                    executionDetail.Executions.Add(execution);
                }
                else if (line.Contains(keyWords["progress"]))
                {
                    if (execution == null) continue;
                    switch (executionDetail.MethodType)
                    {
                        case MethodType.BP_1D:
                        case MethodType.BP_HELIUM_1D:
                            execution.Percentage = (int)(line.Count(c => c == '|') / (double)iterations * 100.0);
                            break;
                        case MethodType.FP_1D:
                            execution.Percentage = (int)(line.Count(c => c == '|') / (double)iterations * 100.0);
                            break;
                    }
                }
            }

            return executionDetail;
        }

        private string parseParam(string line)
        {
            int pFrom = line.IndexOf("[");
            int pTo = line.IndexOf("]");

            return line.Substring(pFrom + 1, pTo - pFrom - 1);
        }

        private List<string> parseMultiParams(string line)
        {
            List<string> parsedParams = new List<string>();
            string[] dirtyParsedValues = line.Split('[');

            foreach (string dirtyParsed in dirtyParsedValues)
            {
                if (dirtyParsed.StartsWith('#')) continue;
                parsedParams.Add(dirtyParsed.Substring(0, dirtyParsed.Length - 1));
            }

            return parsedParams;
        }
    }
}
