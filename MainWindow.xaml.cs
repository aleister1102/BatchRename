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

        public ObservableCollection<IRule> DefaultRules { get; set; } = new();
        public ObservableCollection<IRule> ActiveRules { get; set; } = new();

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Presets.Add("no presets");
            PresetComboBox.ItemsSource = _viewModel.Presets;

            _viewModel.DefaultRules = LoadDefaultRulesFromPrototypes();
            RuleListView.ItemsSource = _viewModel.DefaultRules;

            ActiveRuleListView.ItemsSource = _viewModel.ActiveRules;

            FileListView.ItemsSource = _viewModel.SourceFiles;

            PreviewListView.ItemsSource = _viewModel.PreviewFiles;
        }

        private ObservableCollection<IRule> LoadDefaultRulesFromPrototypes()
        {
            var result = new ObservableCollection<IRule>(RuleFactory.GetPrototypes().Values.ToList());
            return result;
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
            _viewModel.DefaultRules = LoadDefaultRulesFromPrototypes();
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
            for (int i = _viewModel.DefaultRules.Count - 1; i >= 0; i--)
            {
                if (_viewModel.DefaultRules[i].Name == newRule.Name)
                {
                    _viewModel.DefaultRules[i] = (IRule)newRule.Clone();
                }
            }
        }

        private void ApplyActiveRules()
        {
            _viewModel.PreviewFiles = _viewModel.SourceFiles.Clone();

            foreach (var previewFile in _viewModel.PreviewFiles)
            {
                foreach (var rule in _viewModel.ActiveRules)
                {
                    string previewName = rule.Rename(previewFile.Name);
                    previewFile.Name = previewName;
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

                ApplyActiveRules();
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

        private void DeactivateButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var rule = (IRule)button.DataContext;

            // ! Use regular for loop to avoid runtime exception
            for (int i = _viewModel.ActiveRules.Count - 1; i >= 0; i--)
            {
                if (_viewModel.ActiveRules[i].Name == rule.Name)
                {
                    _viewModel.ActiveRules.RemoveAt(i);
                }
            }

            ApplyActiveRules();
        }

        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var rule = (IRule)button.DataContext;

            for (int i = _viewModel.DefaultRules.Count - 1; i >= 0; i--)
            {
                if (_viewModel.DefaultRules[i].Name == rule.Name &&
                    _viewModel.ActiveRules.Any(activeRule => activeRule.Name == rule.Name) is false)
                {
                    _viewModel.ActiveRules.Add(_viewModel.DefaultRules[i]);
                }
            }

            ApplyActiveRules();
        }
    }
}