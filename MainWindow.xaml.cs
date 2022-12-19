using BatchRenamePlugins;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BatchRename
{
    public static class Utils
    {
        public static ObservableCollection<File> Clone(this ObservableCollection<File> files)
        {
            var result = new ObservableCollection<File>();

            foreach (var file in files)
            {
                var clone = (File)file.Clone();
                result.Add(clone);
            }

            return result;
        }

        public static ObservableCollection<IRule> Clone(this ObservableCollection<IRule> files)
        {
            var result = new ObservableCollection<IRule>();

            foreach (var file in files)
            {
                var clone = (IRule)file.Clone();
                result.Add(clone);
    }

            return result;
        }
    }

    public class ViewModel
    {
        public ObservableCollection<string> Presets { get; set; } = new();

        public ObservableCollection<BaseRule> Rules { get; set; } = new();

        public ObservableCollection<File> SourceFiles { get; set; } = new();

        public ObservableCollection<File> PreviewFiles { get; set; } = new();
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RuleFactory.Instance();
        }

        private readonly ViewModel _viewModel = new();

        private BindingList<File> _previewFiles = new();

        private BindingList<IRule> _activeRules = new();

        // TODO: create none preset option
        private BindingList<string> _presets = new();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Presets.Add("no presets");
            PresetComboBox.ItemsSource = _viewModel.Presets;
            PresetComboBox.SelectedIndex = 0;

            RuleListView.ItemsSource = RuleFactory.GetPrototypes().Keys;

            RuleListView.ItemsSource = _viewModel.Rules;

            FileListView.ItemsSource = _viewModel.SourceFiles;

            PreviewListView.ItemsSource = _viewModel.PreviewFiles;
        }

        private void BrowsePresets_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog() { Multiselect = true };

            if (browsingScreen.ShowDialog() == true)
            {
                GetPresetsFrom(browsingScreen);
            }
        }

        private void GetPresetsFrom(OpenFileDialog browsingScreen)
        {
            var presetPaths = browsingScreen.FileNames;

            foreach (var presetPath in presetPaths)
            {
                if (_viewModel.Presets.Contains(presetPath) is false)
                {
                    _viewModel.Presets.Add(presetPath);
                }
            }
        }

        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearOldRules();

            LoadSelectedPresets();

            ApplyRules();
        }

        private void LoadSelectedPresets()
        {
            var presetPath = (string)PresetComboBox.SelectedItem;

            if (System.IO.File.Exists(presetPath))
            {
            var presetLines = System.IO.File.ReadAllLines(presetPath);
                UpdateRules(presetLines);
        }
        }

        private void ClearOldRules()
        {
            _viewModel.Rules = new();
        }

        private void UpdateRules(string[] presetLines)
        {
            foreach (var presetLine in presetLines)
            {
                var rule = RuleFactory.CreateWith(presetLine);
                if (_activeRules.Contains(rule) is false)
                {
                    _activeRules.Add(rule);
                }
            }
        }

            private void ApplyRules()
        {
                _viewModel.PreviewFiles = _viewModel.SourceFiles.Clone();

                foreach (var previewFile in _viewModel.PreviewFiles)
            {
                    foreach (var rule in _viewModel.Rules)
                    {
                        if (rule.IsActive())
                {
                    string previewName = rule.Rename(previewFile.Name);
                    previewFile.Name = previewName;
                }
            }
                }

                PreviewListView.ItemsSource = _viewModel.PreviewFiles;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog { Multiselect = true };

            if (browsingScreen.ShowDialog() == true)
            {
                LoadFilesFrom(browsingScreen);

                    ApplyRules();
            }
        }

        private void LoadFilesFrom(OpenFileDialog browsingScreen)
        {
            var filePaths = browsingScreen.FileNames;

            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);

                    _viewModel.SourceFiles.Add(new File() { Path = filePath, Name = fileName });
                }
            }

            private void CheckBox_Checked(object sender, RoutedEventArgs e)
            {
                //string ruleName = (string)((CheckBox)sender).Content;

                //foreach (var ruleInfo in _viewModel.Rules)
                //{
                //    if (ruleInfo.Rule.Name == ruleName)
                //    {
                //        ruleInfo.Status = RuleManager.Active;
                //    }
                //}

                //ApplyRules();
            }

            private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
            {
                //string ruleName = (string)((CheckBox)sender).Content;

                //foreach (var ruleInfo in _viewModel.Rules)
                //{
                //    if (ruleInfo.Rule.Name == ruleName)
                //    {
                //        ruleInfo.Status = RuleManager.Unactive;
                //    }
                //}

                //ApplyRules();
        }
    }
}