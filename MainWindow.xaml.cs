using BatchRenamePlugins;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.RightsManagement;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
    }

    public class ViewModel
    {
        public ObservableCollection<string> Presets { get; set; } = new();

        public ObservableCollection<IRule> AvailableRules { get; set; } = new();
        public ObservableCollection<IRule> ActiveRules { get; set; } = new();

        public ObservableCollection<File> OriginalFiles { get; set; } = new();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Presets.Add("no presets");
            PresetComboBox.ItemsSource = _viewModel.Presets;

            _viewModel.AvailableRules = LoadDefaultRulesFromPrototypes();
            AvailableRulesListView.ItemsSource = _viewModel.AvailableRules;

            ActiveRulesListView.ItemsSource = _viewModel.ActiveRules;

            OriginalFilesListView.ItemsSource = _viewModel.OriginalFiles;

            PreviewFilesListView.ItemsSource = _viewModel.PreviewFiles;
        }

        private ObservableCollection<IRule> LoadDefaultRulesFromPrototypes()
        {
            var result = new ObservableCollection<IRule>(RuleFactory.GetPrototypes().Values.ToList());
            return result;
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
                var presetLines = System.IO.File.ReadAllLines(presetPath);
                UpdateRules(presetLines);
            }
        }

        private void UpdateRules(string[] presetLines)
        {
            foreach (var presetLine in presetLines)
            {
                var newRule = RuleFactory.CreateWith(presetLine);

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

        private void BrowseFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new OpenFileDialog { Multiselect = true };

            if (browsingScreen.ShowDialog() == true)
            {
                LoadFilesFrom(browsingScreen);

                ApplyActiveRules();
            }
        }

        private void LoadFilesFrom(OpenFileDialog browsingScreen)
        {
            var filePaths = browsingScreen.FileNames;

            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);

                _viewModel.OriginalFiles.Add(new File() { Path = filePath, Name = fileName });
            }
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

                    ApplyActiveRules();
                }
            }
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

                    ApplyActiveRules();
                }
            }
        }

        private void ActivateAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var rule in _viewModel.AvailableRules)
            {
                if (_viewModel.ActiveRules.Any(activeRule => activeRule.Name == rule.Name) is false)
                {
                    var ruleToBeActivated = (IRule)rule.Clone();
                    _viewModel.ActiveRules.Add(ruleToBeActivated);

                    ApplyActiveRules();
                }
            }
        }

        private void DeactivateAllButton_Click(object sender, RoutedEventArgs e)
        {
            ResetActiveRules();

            ApplyActiveRules();
        }

        private void RefreshPresetsButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void SaveActiveRulesButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void AvailableSortButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ActiveSortButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}