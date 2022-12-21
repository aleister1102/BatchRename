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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RuleFactory.Instance();
        }

        private class ViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<string> Presets { get; set; } = new();

            public ObservableCollection<IRule> AvailableRules { get; set; } = new();
            public ObservableCollection<IRule> ActiveRules { get; set; } = new();

            public ObservableCollection<File> OriginalFiles { get; set; } = new();
            public ObservableCollection<File> PreviewFiles { get; set; } = new();

            public string SelectedRule { get; set; } = string.Empty;

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        private readonly ViewModel _viewModel = new();
        private readonly string _configPanelKey = "RuleConfigs";

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _viewModel;

            _viewModel.Presets.Add("no presets");
            PresetComboBox.ItemsSource = _viewModel.Presets;

            _viewModel.AvailableRules = LoadDefaultRulesFromPrototypes();
            AvailableRulesListView.ItemsSource = _viewModel.AvailableRules;
            AvailableRulesListView.SelectedIndex = 0;

            ActiveRulesListView.ItemsSource = _viewModel.ActiveRules;

            OriginalFilesListView.ItemsSource = _viewModel.OriginalFiles;

            PreviewFilesListView.ItemsSource = _viewModel.PreviewFiles;
        }

        private ObservableCollection<IRule> LoadDefaultRulesFromPrototypes()
        {
            var result = new ObservableCollection<IRule>(RuleFactory.GetPrototypes().Values.ToList());
            return result;
        }

        private void BrowseFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog { Multiselect = true };

            if (browsingScreen.ShowDialog() == true)
            {
                LoadFilesFrom(browsingScreen.FileNames);

                ApplyActiveRules();
            }
        }

        private void LoadFilesFrom(string[] filePaths)
        {
            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);

                if (_viewModel.OriginalFiles.Any(file => file.Name == fileName) is false)
                {
                    _viewModel.OriginalFiles.Add(new File() { Path = filePath, Name = fileName });
                }
            }
        }

        private void BrowsePresetsButton_Click(object sender, RoutedEventArgs e)
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
            ResetActiveRules();

            ResetDefaultRules();

            LoadSelectedPresets();

            ApplyActiveRules();
        }

        private void ResetActiveRules()
        {
            _viewModel.ActiveRules.Clear();
            ActiveRulesListView.ItemsSource = _viewModel.ActiveRules;
        }

        private void ResetDefaultRules()
        {
            _viewModel.AvailableRules = LoadDefaultRulesFromPrototypes();
        }

        private void LoadSelectedPresets()
        {
            var presetPath = (string)PresetComboBox.SelectedItem;

            if (System.IO.File.Exists(presetPath))
            {
                var configLines = System.IO.File.ReadAllLines(presetPath);
                UpdateRules(configLines);
            }
        }

        private void UpdateRules(string[] configLines)
        {
            foreach (var configLine in configLines)
            {
                var newRule = RuleFactory.CreateWith(configLine);

                UpdateActiveRule(newRule);
                UpdateDefaultRule(newRule);
            }
        }

        private void UpdateActiveRule(IRule newRule)
        {
            if (_viewModel.ActiveRules.Contains(newRule) is false)
            {
                _viewModel.ActiveRules.Add(newRule);
            }
        }

        private void UpdateDefaultRule(IRule newRule)
        {
            for (int i = _viewModel.AvailableRules.Count - 1; i >= 0; i--)
            {
                if (_viewModel.AvailableRules[i].Name == newRule.Name)
                {
                    _viewModel.AvailableRules[i] = (IRule)newRule.Clone();
                }
            }
        }

        private void ApplyActiveRules()
        {
            _viewModel.PreviewFiles = _viewModel.OriginalFiles.Clone();

            foreach (var activeRule in _viewModel.ActiveRules)
            {
                var ruleToBeApplied = (IRule)activeRule.Clone();

                foreach (var previewFile in _viewModel.PreviewFiles)
                {
                    string previewName = ruleToBeApplied.Rename(previewFile.Name);
                    previewFile.Name = previewName;
                }
            }

            PreviewFilesListView.ItemsSource = _viewModel.PreviewFiles;
        }

        private void DeactivateButton_Click(object sender, RoutedEventArgs e)
        {
            var senderButton = (Button)sender;
            var currentRule = (IRule)senderButton.DataContext;

            // ! Use regular for loop to avoid runtime exception
            for (int i = _viewModel.ActiveRules.Count - 1; i >= 0; i--)
            {
                if (_viewModel.ActiveRules[i].Name == currentRule.Name)
                {
                    _viewModel.ActiveRules.RemoveAt(i);
                }
            }

            ApplyActiveRules();
        }

        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            var senderButton = (Button)sender;
            var currentRule = (IRule)senderButton.DataContext;

            for (int i = _viewModel.AvailableRules.Count - 1; i >= 0; i--)
            {
                if (_viewModel.AvailableRules[i].Name == currentRule.Name &&
                    _viewModel.ActiveRules.Any(activeRule => activeRule.Name == currentRule.Name) is false)
                {
                    var ruleToBeActivated = (IRule)_viewModel.AvailableRules[i].Clone();

                    _viewModel.ActiveRules.Add(ruleToBeActivated);
                }
            }

            ApplyActiveRules();
        }

        private void ActivateAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var rule in _viewModel.AvailableRules)
            {
                if (_viewModel.ActiveRules.Any(activeRule => activeRule.Name == rule.Name) is false)
                {
                    var ruleToBeActivated = (IRule)rule.Clone();
                    _viewModel.ActiveRules.Add(ruleToBeActivated);
                }

                ApplyActiveRules();
            }
        }

        private void DeactivateAllButton_Click(object sender, RoutedEventArgs e)
        {
            ResetActiveRules();

            ApplyActiveRules();
        }

        private void RefreshPresetsButton_Click(object sender, RoutedEventArgs e)
        {
            ResetActiveRules();

            LoadSelectedPresets();

            ApplyActiveRules();
        }

        private void SaveActiveRulesButton_Click(object sender, RoutedEventArgs e)
        {
        }

        // ! Reference: https://stackoverflow.com/questions/5662509/drag-and-drop-files-into-wpf
        private void OriginalFilesListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);

                LoadFilesFrom(filePaths);
            }
        }

        private void AvailableRulesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedRule = (IRule)((ListView)sender).SelectedItem;

            _viewModel.SelectedRule = selectedRule.Name;
            UpdateConfigPanelFor(selectedRule);
        }

        private void UpdateConfigPanelFor(IRule rule)
        {
            var currentConfigPanel = RuleDetails.Children.OfType<Panel>().FirstOrDefault(panel => panel.Name == _configPanelKey);

            var configPanelIndex = RuleDetails.Children.IndexOf(currentConfigPanel);
            RuleDetails.Children.RemoveAt(configPanelIndex);

            var newConfigPanel = (Panel)rule.Create();

            newConfigPanel.Name = _configPanelKey;
            newConfigPanel.Style = (Style)FindResource(_configPanelKey);

            RuleDetails.Children.Insert(configPanelIndex, newConfigPanel);
        }
    }
}