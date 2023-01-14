using System.Windows;
using System.Windows.Controls;

namespace BatchRename
{
    public partial class MainWindow : Window
    {
        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSelectedPresets();

            RestoreSelectedRule();

            UpdateActiveRulesForConverters();
        }

        private void LoadSelectedPresets()
        {
            _ruleCounter.Reset();
            ResetRules();

            var presetPath = (string)PresetComboBox.SelectedItem;

            if (System.IO.File.Exists(presetPath))
            {
                string[] configLines = System.IO.File.ReadAllLines(presetPath);
                UpdateRules(configLines);
            }
            else
            {
                // TODO: handle error
            }
        }
    }
}