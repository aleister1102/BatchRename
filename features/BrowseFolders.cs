using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Linq;
using System.Windows;

namespace BatchRename
{
    public partial class MainWindow : Window
    {
        private void BrowseFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new CommonOpenFileDialog { Multiselect = true, IsFolderPicker = true };

            if (browsingScreen.ShowDialog() == CommonFileDialogResult.Ok)
            {
                LoadFilesFromFolders(browsingScreen.FileNames.ToArray());

                UpdateActiveRulesForConverters();
            }
        }

        private void LoadFilesFromFolders(string[] foldersPath)
        {
            foreach (var folderPath in foldersPath)
            {
                var filesPath = Directory.GetFiles(folderPath);
                LoadFilesFrom(filesPath);
            }
        }
    }
}