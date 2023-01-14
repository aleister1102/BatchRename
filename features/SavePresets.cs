using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;

namespace BatchRename
{
    public partial class MainWindow : Window
    {
        private async void SaveActiveRulesButton_Click(object sender, RoutedEventArgs e)
        {
            var savingScreen = new SaveFileDialog()
            {
                DefaultExt = ".txt",
                AddExtension = true,
                Filter = "Text Files|*.txt|Markdown|*.md|All Files|*.*"
            };

            if (savingScreen.ShowDialog() == true)
            {
                string path = savingScreen.FileName;

                List<string> configLines = GenerateConfigLinesForActiveRules();

                await System.IO.File.WriteAllLinesAsync(path, configLines);
            }
        }

        private List<string> GenerateConfigLinesForActiveRules()
        {
            var configLines = new List<string>();

            foreach (var ruleInfo in _viewModel.RulesInfo)
            {
                if (ruleInfo.IsActive())
                {
                    string configLine = GenerateConfigLine(ruleInfo);
                    configLines.Add(configLine);
                }
            }

            return configLines;
        }
    }
}