using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using CudaHelioCommanderLight.Config;
using CudaHelioCommanderLight.Enums;
using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Models;

namespace CudaHelioCommanderLight.Operations
{
    public class CompareLibraryOperation
    {
        private readonly IDialogService _dialogService;
        private readonly IMainHelper _mainHelper;
        private readonly IMetricsConfig _metricsConfig;

        public CompareLibraryOperation(IDialogService dialogService, IMainHelper mainHelper,IMetricsConfig metricsConfig)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _mainHelper = mainHelper ?? throw new ArgumentNullException(nameof(mainHelper));
            _metricsConfig = metricsConfig ?? throw new ArgumentNullException(nameof(metricsConfig));
        }

        public List<ErrorStructure> Operate(CompareLibraryModel model, LibStructureType libStructureType)
        {
            var errors = new List<ErrorStructure>();

            var isFileDirectory = IsDirectory(libStructureType);

            var files = GetFile(model, isFileDirectory);

            foreach (var file in files)
            {
                var libFile = isFileDirectory
                    ? Path.Combine(file, "output_1e3bin.dat")
                    : file;

                bool dataExtractSuccess = _mainHelper.ExtractOutputDataFile(libFile, out OutputFileContent outputFileContent);

                if (!dataExtractSuccess)
                {
                    _dialogService.ShowMessage("Cannot read data values from the input file.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return errors;
                }

                var dividedList = new List<double>();
                for (int idx = 0; idx < outputFileContent.TKinList.Count; idx++)
                {
                    dividedList.Add(isFileDirectory
                        ? outputFileContent.Spe1e3List[idx] / outputFileContent.Spe1e3NList[idx]
                        : outputFileContent.Spe1e3List[idx]);
                }
                outputFileContent.Spe1e3List = dividedList;

                var error = new ErrorStructure(_mainHelper);
                var computedError = ComputeErrorOperation.Operate(new ErrorComputeModel()
                {
                    AmsExecution = model.AmsExecution,
                    LibraryItem = outputFileContent,
                },_mainHelper, _metricsConfig);
                error.Error = computedError.Error;
                error.MaxError = computedError.MaxError;
                error.FilePath = outputFileContent.FilePath;
                error.DisplayName = GetDisplayName(outputFileContent.FilePath, isFileDirectory);
                error.TKinList = outputFileContent.TKinList;
                error.Spe1e3binList = outputFileContent.Spe1e3List;

                switch (libStructureType)
                {
                    case LibStructureType.DIRECTORY_SEPARATED:
                        error.TrySetVAndK0(error.DisplayName, libStructureType);
                        break;
                    case LibStructureType.FILES_SOLARPROP_LIB:
                        error.TrySetVAndK0(Path.GetFileName(outputFileContent.FilePath), libStructureType);
                        break;
                    default:
                        {
                            var fileName = Path.GetFileNameWithoutExtension(error.FilePath);
                            var success = int.TryParse(fileName, out int xval);
                            if (success)
                            {
                                error.V = xval;
                            }
                            error.K0 = 0;
                            break;
                        }
                }
                errors.Add(error);
            }
            return errors;
        }

        private string[] GetFile(CompareLibraryModel model, bool isFileDirectory)
        {
            return isFileDirectory ? Directory.GetDirectories(model.LibPath) : Directory.GetFiles(model.LibPath);
        }

        private string? GetDisplayName(string outputFilePath, bool isFileDirectory)
        {
            return isFileDirectory ? Path.GetFileName(Path.GetDirectoryName(outputFilePath)) : Path.GetFileName(outputFilePath);
        }

        private bool IsDirectory(LibStructureType libStructureType)
        {
            return libStructureType == LibStructureType.DIRECTORY_SEPARATED;
        }
    }
}
