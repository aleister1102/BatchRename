using BatchRename.Rules;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace BatchRename
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ObservableCollection<object> _sourceFiles = new();
        private List<IRule> _activeRules = new();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RuleFactory.Configure();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog { Multiselect = true };

            if (browsingScreen.ShowDialog() == true)
            {
                var filePaths = browsingScreen.FileNames;

                foreach (var filePath in filePaths)
                {
                    var fileInfo = new FileInfo(filePath);
                    var fileName = fileInfo.Name;

                    _sourceFiles.Add(new { Path = filePath, Name = fileName });
                }
            }

            FileListView.ItemsSource = _sourceFiles;
        }

        private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();

            if (screen.ShowDialog() == false) return;

            var configFilePaths = screen.FileNames;

            // Load config files
            foreach (var configFilePath in configFilePaths)
            {
                var configLines = File.ReadAllLines(configFilePath);

                // Load rules
                foreach (var configLine in configLines)
                {
                    var rule = RuleFactory.CreateWith(configLine);
                    _activeRules.Add(rule);
                }
            }

            // Apply configs to converter for renameing
            var converter = (PreviewRenameConverter)FindResource("PreviewRenameConverter");
            converter.rules = _activeRules;

            // Refresh preview list
            PreviewListView.ItemsSource = _sourceFiles;
        }
    }
}