using CudaHelioCommanderLight.Enums;
using CudaHelioCommanderLight.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CudaHelioCommanderLight.Models
{
    public class ErrorStructure
    {
        public string ErrorStr
        {
            get
            {
                return Error.ToString("0.##") + "% (Max: " + MaxError.ToString("0.##") + "%) " + DisplayName;
            }
        }
        public double Error { get; set; }
        public double MaxError { get; set; }
        public string DisplayName { get; set; }
        public string FilePath { get; set; }
        public List<double> TKinList { get; set; }
        public List<double> Spe1e3binList { get; set; }
        public int V { get; set; }
        public double K0 { get; set; }

        private readonly IMainHelper _mainHelper;

        public ErrorStructure(IMainHelper mainHelper)
        {
            _mainHelper = mainHelper ?? throw new ArgumentNullException(nameof(mainHelper));
        }

        public void TrySetVAndK0(string str, LibStructureType libStructureType)
        {
            if (libStructureType == LibStructureType.DIRECTORY_SEPARATED)
            {
                var split = splitStrFileName(str);
                if (split.Count % 2 != 0)
                {
                    return;
                }

                for (int idx = 0; idx < split.Count; idx += 2)
                {
                    if (split[idx].ToLower().Equals("k0"))
                    {
                        var success = _mainHelper.TryConvertToDouble(split[idx + 1], out double outK0);
                        if (success)
                        {
                            K0 = outK0;
                        }
                    }
                    else if (split[idx].ToLower().Equals("v"))
                    {
                        var success = _mainHelper.TryConvertToDouble(split[idx + 1], out double outV);
                        if (success)
                        {
                            V = (int)outV;
                        }
                    }
                }
            }
            else if (libStructureType == LibStructureType.FILES_SOLARPROP_LIB)
            {
                var split = str.Split('_');
                if (split.Length < 3)
                {
                    Console.WriteLine($"Incorrect file {str} in library");
                    return;
                }

                // outfil_0.0001_0_Burger2000LIS_spectrum_interpolated.dat
                var success = _mainHelper.TryConvertToDouble(split[1], out double outK0);
                if (success)
                {
                    K0 = outK0;
                }

                success = _mainHelper.TryConvertToDouble(split[2], out double theta);
                if (success)
                {
                    V = (int)theta;
                }
            }
        }

        private List<string> splitStrFileName(string str)
        {
            List<string> splittedStr = new List<string>();
            var split = str.ToLower().Split('=');
            for (int idx = 0; idx < split.Count(); idx++)
            {
                if (IsParamTerm(split[idx]) || !IsContainingParamTerm(split[idx]))
                {
                    splittedStr.Add(split[idx]);
                    continue;
                }

                if (IsContainingParamTerm(split[idx]))
                {
                    var index = split[idx].ToLower().IndexOf('d');
                    if (index == -1)
                    {
                        index = split[idx].ToLower().IndexOf('k');
                    }
                    if (index == -1)
                    {
                        index = split[idx].ToLower().IndexOf('v');
                    }
                    splittedStr.Add(split[idx].Substring(0, index));
                    splittedStr.Add(split[idx].Substring(index));
                }
            }
            return splittedStr;
        }

        private bool IsParamTerm(string str)
        {
            var lower = str.ToLower();
            return lower.Equals("dt") || lower.Equals("k0") || lower.Equals("v");
        }

        private bool IsContainingParamTerm(string str)
        {
            var lower = str.ToLower();
            return lower.Contains("dt") || lower.Contains("k0") || lower.Contains("v");
        }
    }
}
