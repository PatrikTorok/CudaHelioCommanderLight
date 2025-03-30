using System.Windows;

namespace CudaHelioCommanderLight.Interfaces
{
    public interface IDialogService
    {
        bool SaveFileDialog(out string filePath, string filter);
        void ShowMessage(string text, string caption, MessageBoxButton button, MessageBoxImage icon);
        bool ShowOpenFileDialog(out string filePath);
        bool ShowFolderDialog();
        string SelectedFolderPath { get; }
        bool SaveFileDialogWithTitle(out string filePath, string filter, string title);
    }
}

