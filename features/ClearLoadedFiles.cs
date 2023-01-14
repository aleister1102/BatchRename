using System.Windows;

namespace BatchRename
{
    public partial class MainWindow : Window
    {
        private void ClearFilesButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Files.Clear();
            RefreshPreviewFilesListView();
        }
    }
}