using System.Collections.ObjectModel;
using System.Windows;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.Models;
using CudaHelioCommanderLight.Operations;
using Microsoft.Win32;

namespace CudaHelioCommanderLight.MainWindowServices;

public class ButtonService : IButtonService
{
    private readonly IDialogService _dialogService;
    public ButtonService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }
    public void AboutUsButton()
    {
        string message = "Slovak Academy of Sciences\n\nDeveloped by: Martin Nguyen, Pavol Bobik\n\nCopyright 2023";
        _dialogService.ShowMessage(message, "About Us", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ExportJsonBtn(ObservableCollection<ExecutionDetail> executionDetailList, int executionDetailSelectedIdx)
    {
        ExecutionDetail executionDetail = executionDetailList[executionDetailSelectedIdx];

        if (executionDetail == null)
        {
            return;
        }

        if (_dialogService.SaveFileDialogWithTitle(out string filePath, "JSON File|*.json", "Save JSON File"))
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var exportModel = new ExecutionListExportModel
                {
                    Executions = executionDetail.Executions,
                    FilePath = filePath
                };
                ExportAsJsonOperation.Operate(exportModel);
            }
        }
    }


}