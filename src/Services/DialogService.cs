using CudaHelioCommanderLight.Interfaces;
using System.Windows;
using Microsoft.Win32;

namespace CudaHelioCommanderLight.Services
{
    public class DialogService : IDialogService
    {
        public bool SaveFileDialog(out string filePath, string filter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = filter };
            bool result = saveFileDialog.ShowDialog() == true;
            filePath = result ? saveFileDialog.FileName : null;
            return result;
        }

        public void ShowMessage(string text, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            MessageBox.Show(text, caption, button, icon);
        }
        public bool ShowOpenFileDialog(out string filePath)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                filePath = fileDialog.FileName;
                return true;
            }

            filePath = null;
            return false;
        }
        public bool ShowFolderDialog()
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            bool result = folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;
            SelectedFolderPath = result ? folderDialog.SelectedPath : null;
            return result;
        }

        public string SelectedFolderPath { get; private set; }

        public bool SaveFileDialogWithTitle(out string filePath, string filter, string title)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                Title = title
            };
            bool result = saveFileDialog.ShowDialog() == true;
            filePath = result ? saveFileDialog.FileName : null;
            return result;
        }

    }
}
