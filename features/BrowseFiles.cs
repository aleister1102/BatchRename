using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Linq;
using System.Windows;

namespace BatchRename

{
    public partial class MainWindow : Window

    {
        private void BrowseFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new CommonOpenFileDialog { Multiselect = true };

            if (browsingScreen.ShowDialog() == CommonFileDialogResult.Ok)
            {
                LoadFilesFrom(browsingScreen.FileNames.ToArray());

                UpdateActiveRulesForConverters();
            }
        }

        private void LoadFilesFrom(string[] filesPaths)
        {
            foreach (var filePath in filesPaths)
            {
                string fileName = Path.GetFileName(filePath);

                if (_viewModel.Files.Any(file => file.Name == fileName) is false)
                {
                    _viewModel.Files.Add(new File() { Path = filePath, Name = fileName });
                }
            }
        }
    }
}