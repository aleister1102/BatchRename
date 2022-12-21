using BatchRenamePlugins;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Serialization;

namespace BatchRename
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RuleFactory.Instance();
        }

        private class RuleConfig
        {
            public string Name { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }

        private class RuleStatus
        {
            public static bool Active { get; set; } = true;
            public static bool Inactive { get; set; } = false;
        }

        public class RuleInfo : INotifyPropertyChanged
        {
            public IRule Rule { get; set; }
            public bool State { get; set; } = false;

            public event PropertyChangedEventHandler? PropertyChanged;

            public void Activate() => State = RuleStatus.Active;

            public void Deactivate() => State = RuleStatus.Inactive;

            public bool IsActive() => State == RuleStatus.Active;
        }

        private class ViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<string> Presets { get; set; } = new();

            public ObservableCollection<RuleInfo> RulesInfo { get; set; } = new();

            public ObservableCollection<File> OriginalFiles { get; set; } = new();
            public ObservableCollection<File> PreviewFiles { get; set; } = new();

            public IRule SelectedRule { get; set; }
            public ObservableCollection<RuleConfig> SelectedRuleConfigs { get; set; } = new();

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        private readonly ViewModel _viewModel = new();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _viewModel;

            _viewModel.Presets.Add("no presets");
            PresetComboBox.ItemsSource = _viewModel.Presets;

            _viewModel.RulesInfo = InitDefaultRulesInfo();
            RulesListView.ItemsSource = _viewModel.RulesInfo;

            OriginalFilesListView.ItemsSource = _viewModel.OriginalFiles;

            PreviewFilesListView.ItemsSource = _viewModel.PreviewFiles;
        }

        private void RefreshRulesListView()
        {
            RulesListView.ItemsSource = null;
            RulesListView.ItemsSource = _viewModel.RulesInfo;
        }

        private void RefreshPreviewFilesListView()
        {
            PreviewFilesListView.ItemsSource = null;
            PreviewFilesListView.ItemsSource = _viewModel.PreviewFiles;
        }

        private ObservableCollection<RuleInfo> InitDefaultRulesInfo()
        {
            var rulesPrototype = RuleFactory.GetPrototypes().Values.ToList();
            var rulesInfo = new ObservableCollection<RuleInfo>();

            foreach (var rulePrototype in rulesPrototype)
            {
                var ruleInfo = new RuleInfo { Rule = rulePrototype };
                rulesInfo.Add(ruleInfo);
            }

            return rulesInfo;
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
            LoadSelectedPresets();

            ApplyActiveRules();
        }

        private void LoadSelectedPresets()
        {
            var presetPath = (string)PresetComboBox.SelectedItem;

            ResetRules();

            if (System.IO.File.Exists(presetPath))
            {
                var configLines = System.IO.File.ReadAllLines(presetPath);
                UpdateRules(configLines);
            }
            else
            {
                // TODO: handle error
            }
        }

        private void ResetRules()
        {
            _viewModel.RulesInfo = InitDefaultRulesInfo();

            RefreshRulesListView();
        }

        private void UpdateRules(string[] configLines)
        {
            foreach (var configLine in configLines)
            {
                var newRule = RuleFactory.CreateWith(configLine);
                UpdateRule(newRule);
            }

            RefreshRulesListView();
        }

        private void UpdateRule(IRule update)
        {
            try
            {
                var ruleInfo = _viewModel.RulesInfo.First(ruleInfo => ruleInfo.Rule.Name == update.Name);
                ruleInfo.Rule = (IRule)update.Clone();
                ruleInfo.Activate();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ApplyActiveRules()
        {
            _viewModel.PreviewFiles = _viewModel.OriginalFiles.Clone();

            foreach (var ruleInfo in _viewModel.RulesInfo)
            {
                var ruleToBeApplied = (IRule)ruleInfo.Rule.Clone();

                if (ruleInfo.IsActive())
                {
                    foreach (var previewFile in _viewModel.PreviewFiles)
                    {
                        string previewName = ruleToBeApplied.Rename(previewFile.Name);
                        previewFile.Name = previewName;
                    }
                }
            }

            RefreshPreviewFilesListView();
        }

        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            var senderButton = (Button)sender;
            var currentRule = (RuleInfo)senderButton.DataContext;

            currentRule.Activate();

            RefreshRulesListView();
            ApplyActiveRules();
        }

        private void DeactivateButton_Click(object sender, RoutedEventArgs e)
        {
            var senderButton = (Button)sender;
            var currentRule = (RuleInfo)senderButton.DataContext;

            currentRule.Deactivate();

            RefreshRulesListView();
            ApplyActiveRules();
        }

        private void ActivateAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var ruleInfo in _viewModel.RulesInfo)
            {
                ruleInfo.Activate();
            }

            RefreshRulesListView();
            ApplyActiveRules();
        }

        private void DeactivateAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var ruleInfo in _viewModel.RulesInfo)
            {
                ruleInfo.Deactivate();
            }

            RefreshRulesListView();
            ApplyActiveRules();
        }

        private void RefreshPresetsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSelectedPresets();
            ApplyActiveRules();
        }

        // Reference: https://stackoverflow.com/questions/5662509/drag-and-drop-files-into-wpf
        private void OriginalFilesListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);

                LoadFilesFrom(filePaths);
            }
        }

        private void SaveActiveRulesButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void RulesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void UpdateSelectedRuleConfigs(IRule selectedRule)
        {
            _viewModel.SelectedRule = selectedRule;
            _viewModel.SelectedRuleConfigs.Clear();
            _viewModel.SelectedRuleConfigs = CreateRuleConfigs(selectedRule);

            RuleConfigs.ItemsSource = _viewModel.SelectedRuleConfigs;
        }

        private ObservableCollection<RuleConfig> CreateRuleConfigs(IRule rule)
        {
            var configs = new ObservableCollection<RuleConfig>();

            foreach (var prop in rule.ConfigurableProps)
            {
                var propInfo = rule.GetType().GetProperty(prop);
                var propValue = propInfo.GetValue(rule, null);

                var config = new RuleConfig
                {
                    Name = prop,
                    Value = propValue.ToString(),
                    Message = "Validation message"
                };

                configs.Add(config);
            }

            return configs;
        }

        private string GenerateConfigLine(string ruleName, ObservableCollection<RuleConfig> configs)
        {
            var builder = new StringBuilder();

            builder.Append(ruleName);
            builder.Append(':');

            foreach (var config in configs)
            {
                builder.Append(config.Name);
                builder.Append('=');
                builder.Append(config.Value);
                builder.Append(';');
            }

            var result = builder.ToString();
            return result;
        }
    }
}