using System.Windows;

namespace BatchRename
{
    public partial class MainWindow : Window
    {
        private void AlsoActivateActiveRulesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            LoadSelectedPresets();

            UpdateActiveRulesForConverters();
        }

        private void AlsoActivateActiveRulesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DeactivateAllRules();

            RefreshRulesListView();

            UpdateActiveRulesForConverters();
        }
    }
}