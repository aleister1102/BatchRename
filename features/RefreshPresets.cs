using System.Windows;

namespace BatchRename
{
    public partial class MainWindow : Window
    {
        private void RefreshPresetsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSelectedPresets();
            UpdateActiveRulesForConverters();
        }
    }
}