using BatchRenamePlugins;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace BatchRename
{
    public static class Utils
    {
        public static BindingList<File> Clone(this BindingList<File> files)
        {
            var result = new BindingList<File>();
            foreach (var file in files)
            {
                var clone = (File)file.Clone();
                result.Add(clone);
            }
            return result;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private BindingList<File> _sourceFiles = new();

        private BindingList<File> _previewFiles = new();

        private BindingList<IRule> _activeRules = new();

        // TODO: create none preset option
        private BindingList<string> _presets = new();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RuleFactory.Instance();

            RuleListView.ItemsSource = RuleFactory.GetPrototypes().Keys;

            PresetComboBox.ItemsSource = _presets;

            FileListView.ItemsSource = _sourceFiles;

            PreviewListView.ItemsSource = _previewFiles;
        }

        private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog() { Multiselect = true };

            if (browsingScreen.ShowDialog() == true)
            {
                LoadPresetsFrom(browsingScreen);
            }
        }

        private void LoadPresetsFrom(OpenFileDialog browsingScreen)
        {
            var presetPaths = browsingScreen.FileNames;

            foreach (var presetPath in presetPaths)
            {
                if (_presets.Contains(presetPath) is false)
                {
                    _presets.Add(presetPath);
                }
            }
        }

        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSelectedPresets();
            ApplyPresets();
        }

        private void LoadSelectedPresets()
        {
            var presetPath = (string)PresetComboBox.SelectedItem;
            var presetLines = System.IO.File.ReadAllLines(presetPath);
            AddRules(presetLines);
        }

        private void AddRules(string[] presetLines)
        {
            _activeRules = new BindingList<IRule>();

            foreach (var presetLine in presetLines)
            {
                var rule = RuleFactory.CreateWith(presetLine);
                if (_activeRules.Contains(rule) is false)
                {
                    _activeRules.Add(rule);
                }
            }
        }

        private void ApplyPresets()
        {
            // TODO: try to change each items in _previewFiles instead of create the clone
            _previewFiles = _sourceFiles.Clone();

            foreach (var previewFile in _previewFiles)
            {
                foreach (var rule in _activeRules)
                {
                    string previewName = rule.Rename(previewFile.Name);
                    previewFile.Name = previewName;
                }
            }

            PreviewListView.ItemsSource = _previewFiles;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog { Multiselect = true };

            if (browsingScreen.ShowDialog() == true)
            {
                LoadFilesFrom(browsingScreen);

                ApplyPresets();
            }
        }

        private void LoadFilesFrom(OpenFileDialog browsingScreen)
        {
            var filePaths = browsingScreen.FileNames;

            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);

                _sourceFiles.Add(new File() { Path = filePath, Name = fileName });
            }
        }
    }
}