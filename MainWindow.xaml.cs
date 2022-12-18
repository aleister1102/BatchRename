using BatchRename.Rules;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using BatchRenamePlugins;

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
            RuleFactory.Instance();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog { Multiselect = true };

            if (browsingScreen.ShowDialog() == true)
            {
                LoadFilesFromBrowsingScreen(browsingScreen);

                FileListView.ItemsSource = _sourceFiles;
            }
        }

        private void LoadFilesFromBrowsingScreen(OpenFileDialog browsingScreen)
        {
            var filePaths = browsingScreen.FileNames;

            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);

                _sourceFiles.Add(new { Path = filePath, Name = fileName });
            }
        }

        private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog() { Multiselect = true };

            if (browsingScreen.ShowDialog() == true)
            {
                LoadPresetsFromBrowsingScreen(browsingScreen);

                ApplyPresetsToConverter();

                PreviewListView.ItemsSource = _sourceFiles;
            }
        }

        private void LoadPresetsFromBrowsingScreen(OpenFileDialog browsingScreen)
        {
            var presetFilePaths = browsingScreen.FileNames;

            foreach (var presetFilePath in presetFilePaths)
            {
                var presetLines = File.ReadAllLines(presetFilePath);
                AddRules(presetLines);
            }
        }

        private void AddRules(string[] presetLines)
        {
            foreach (var presetLine in presetLines)
            {
                var rule = RuleFactory.CreateWith(presetLine);
                _activeRules.Add(rule);
            }
        }

        private void ApplyPresetsToConverter()
        {
            var converter = (PreviewRenameConverter)FindResource("PreviewRenameConverter");
            converter.rules = _activeRules;
        }
    }
}