using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using BaseRule;
using BatchRename.converters;
using System.Windows.Documents;
using System.Reflection;

namespace BatchRename
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RuleFactory.Instance();
        }

        private class RuleCounter
        {
            private int _counter = 0;
            private static RuleCounter _instance;

            private RuleCounter()
            { }

            public static RuleCounter GetInstance()
            {
                if (_instance is null)
                    _instance = new RuleCounter();
                return _instance;
            }

            public void Increment() => _counter++;

            internal void Decrement() => _counter--;

            public int GetValue() => _counter;

            public void Reset() => _counter = 0;
        }

        public class RuleConfig
        {
            public string Name { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;

            public RuleConfig(string name, string value, string message = "Validation message")
            {
                Name = name;
                Value = value;
                Message = message;
            }
        }

        private class RuleStatus
        {
            public static bool Active { get; set; } = true;
            public static bool Inactive { get; set; } = false;
        }

        public class RuleInfo : INotifyPropertyChanged, ICloneable
        {
            public IRule Rule { get; set; }
            public bool State { get; set; } = false;
            public int Order { get; set; } = 0;
            public ObservableCollection<RuleConfig> Configs { get; set; } = new();

            public event PropertyChangedEventHandler? PropertyChanged;

            public object Clone()
            {
                return MemberwiseClone();
            }

            public void Activate() => State = RuleStatus.Active;

            public void Deactivate() => State = RuleStatus.Inactive;

            public bool IsActive() => State == RuleStatus.Active;

            public void ChangeOrder(int value) => Order = value;

            public void ReduceOrder() => Order -= 1;
        }

        private class ViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<string> Presets { get; set; } = new();

            public ObservableCollection<RuleInfo> RulesInfo { get; set; } = new();

            public ObservableCollection<File> Files { get; set; } = new();

            public RuleInfo SelectedRule { get; set; } = new();

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        private readonly ViewModel _viewModel = new();
        private int _selectedRuleIndex;
        private RuleCounter _ruleCounter = RuleCounter.GetInstance();
        private string _moveToPath = string.Empty;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Presets.Add("no presets");
            PresetComboBox.ItemsSource = _viewModel.Presets;

            AlsoActivateActiveRulesCheckBox.IsChecked = true;

            _viewModel.RulesInfo = InitDefaultRulesInfo();
            RulesListView.ItemsSource = _viewModel.RulesInfo;

            OriginalFilesListView.ItemsSource = _viewModel.Files;

            SelectedRuleTitle.DataContext = _viewModel.SelectedRule;
            SelectedRuleConfigs.ItemsSource = _viewModel.SelectedRule.Configs;
        }

        private void RefreshRulesListView()
        {
            RulesListView.ItemsSource = null;
            RulesListView.ItemsSource = _viewModel.RulesInfo;
        }

        private void RefreshPreviewFilesListView()
        {
            PreviewFilesListView.ItemsSource = null;
            PreviewFilesListView.ItemsSource = _viewModel.Files;
        }

        private void RefreshSelectedRuleTitle()
        {
            SelectedRuleTitle.DataContext = null;
            SelectedRuleTitle.DataContext = _viewModel.SelectedRule;
        }

        private void RefreshSelectedRuleConfigs()
        {
            SelectedRuleConfigs.ItemsSource = null;
            SelectedRuleConfigs.ItemsSource = _viewModel.SelectedRule.Configs;
        }

        private ObservableCollection<RuleInfo> InitDefaultRulesInfo()
        {
            List<IRule> rulesPrototype = RuleFactory.GetPrototypes().Values.ToList();
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
            var browsingScreen = new CommonOpenFileDialog { Multiselect = true };

            if (browsingScreen.ShowDialog() == CommonFileDialogResult.Ok)
            {
                LoadFilesFrom(browsingScreen.FileNames.ToArray());

                UpdateActiveRulesForConverters();
            }
        }

        private void LoadFilesFrom(string[] filesPaths)
        {
            foreach (var filePath in filesPaths)
            {
                string fileName = Path.GetFileName(filePath);

                if (_viewModel.Files.Any(file => file.Name == fileName) is false)
                {
                    _viewModel.Files.Add(new File() { Path = filePath, Name = fileName });
                }
            }
        }

        private void BrowseFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new CommonOpenFileDialog { Multiselect = true, IsFolderPicker = true };

            if (browsingScreen.ShowDialog() == CommonFileDialogResult.Ok)
            {
                LoadFilesFromFolders(browsingScreen.FileNames.ToArray());

                UpdateActiveRulesForConverters();
            }
        }

        private void LoadFilesFromFolders(string[] foldersPath)
        {
            foreach (var folderPath in foldersPath)
            {
                var filesPath = Directory.GetFiles(folderPath);
                LoadFilesFrom(filesPath);
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
            string[] presetPaths = browsingScreen.FileNames;

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

        private void ResetRules()
        {
            _viewModel.RulesInfo = InitDefaultRulesInfo();
            RefreshRulesListView();
        }

        private void UpdateRules(string[] configLines)
        {
            foreach (var configLine in configLines)
            {
                IRule newRule = RuleFactory.CreateWith(configLine);
                UpdateRule(newRule, AlsoActivateActiveRulesCheckBox.IsChecked);
            }

            RefreshRulesListView();
        }

        private void UpdateRule(IRule update, bool? alsoActivate = false)
        {
            try
            {
                RuleInfo ruleInfo = _viewModel.RulesInfo.First(ruleInfo => ruleInfo.Rule.Name == update.Name);
                ruleInfo.Rule = (IRule)update.Clone();

                if (alsoActivate == true)
                {
                    IncreaseOrder(ruleInfo);
                    ruleInfo.Activate();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ActivateAllRules()
        {
            foreach (var ruleInfo in _viewModel.RulesInfo)
            {
                if (ruleInfo.IsActive() is false)
                {
                    IncreaseOrder(ruleInfo);
                    ruleInfo.Activate();
                }
            }
        }

        private void DeactivateAllRules()
        {
            foreach (var ruleInfo in _viewModel.RulesInfo)
            {
                if (ruleInfo.IsActive())
                {
                    DecreaseOrder(ruleInfo);
                    ruleInfo.Deactivate();
                }
            }
        }

        private void UpdateActiveRulesForConverters()
        {
            var activeRulesInfo = _viewModel.RulesInfo.Clone();
            var sortedAvtiveRulesInfo = activeRulesInfo.Sort();

            var converter = (PreviewRenameConverter)FindResource("PreviewRenameConverter");
            converter.RulesInfo = sortedAvtiveRulesInfo;

            RefreshPreviewFilesListView();
        }

        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            var senderButton = (Button)sender;
            var currentRule = (RuleInfo)senderButton.DataContext;

            if (currentRule.IsActive() is false)
            {
                IncreaseOrder(currentRule);
                currentRule.Activate();
            }

            RefreshRulesListView();
            UpdateActiveRulesForConverters();
        }

        private void IncreaseOrder(RuleInfo currentRule)
        {
            _ruleCounter.Increment();
            currentRule.ChangeOrder(_ruleCounter.GetValue());
        }

        private void DeactivateButton_Click(object sender, RoutedEventArgs e)
        {
            var senderButton = (Button)sender;
            var currentRule = (RuleInfo)senderButton.DataContext;

            if (currentRule.IsActive())
            {
                DecreaseOrder(currentRule);
                currentRule.Deactivate();
            }

            RefreshRulesListView();
            UpdateActiveRulesForConverters();
        }

        private void DecreaseOrder(RuleInfo currentRule)
        {
            _ruleCounter.Decrement();

            foreach (var ruleInfo in _viewModel.RulesInfo)
            {
                if (ruleInfo.Order > currentRule.Order)
                {
                    ruleInfo.ReduceOrder();
                }
            }

            currentRule.ChangeOrder(0);
        }

        private void ActivateAllButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateAllRules();
            RefreshRulesListView();
            UpdateActiveRulesForConverters();
        }

        private void DeactivateAllButton_Click(object sender, RoutedEventArgs e)
        {
            DeactivateAllRules();
            RefreshRulesListView();
            UpdateActiveRulesForConverters();
        }

        private void RefreshPresetsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSelectedPresets();
            UpdateActiveRulesForConverters();
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

        private void RulesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RulesListView.SelectedIndex >= 0)
                _selectedRuleIndex = RulesListView.SelectedIndex;

            UpdateSelectedRule();
        }

        private void UpdateSelectedRule()
        {
            var selectedRule = (RuleInfo)RulesListView.SelectedItem;

            if (selectedRule is not null)
            {
                UpdateSelectedRuleTitle(selectedRule);
                UpdateSelectedRuleConfigs(selectedRule);
            }
        }

        private void UpdateSelectedRuleTitle(RuleInfo selectedRule)
        {
            _viewModel.SelectedRule = selectedRule;

            RefreshSelectedRuleTitle();
        }

        private void UpdateSelectedRuleConfigs(RuleInfo selectedRule)
        {
            ObservableCollection<RuleConfig> ruleConfigs = CreateRuleConfigs(selectedRule.Rule);

            _viewModel.SelectedRule.Configs = ruleConfigs;

            RefreshSelectedRuleConfigs();
        }

        private ObservableCollection<RuleConfig> CreateRuleConfigs(IRule rule)
        {
            var configs = new ObservableCollection<RuleConfig>();

            foreach (var prop in rule.ConfigurableProps)
            {
                var propValue = GetRuleProp(rule, prop);
                var config = new RuleConfig(prop, propValue);

                configs.Add(config);
            }

            return configs;
        }

        private string GetRuleProp(IRule rule, string prop)
        {
            var propInfo = rule.GetType().GetProperty(prop);
            var propValue = propInfo!.GetValue(rule, null);
            return propValue!.ToString()!;
        }

        private void RestoreSelectedRule()
        {
            RulesListView.SelectedIndex = _selectedRuleIndex;
            UpdateSelectedRule();
        }

        private void SaveConfigsButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRule = (RuleInfo)RulesListView.SelectedItem;

            if (selectedRule is not null)
            {
                string configLine = GenerateConfigLine(selectedRule);
                var rule = RuleFactory.CreateWith(configLine);

                UpdateRule(rule);
                UpdateActiveRulesForConverters();
            }
        }

        private string GenerateConfigLine(RuleInfo ruleInfo)
        {
            var builder = new StringBuilder();

            var ruleName = ruleInfo.Rule.Name;
            var ruleConfigs = ruleInfo.Configs;

            builder.Append(ruleName);
            builder.Append(':');

            foreach (var config in ruleConfigs)
            {
                builder.Append(config.Name);
                builder.Append('=');
                builder.Append(config.Value);
                builder.Append(';');
            }

            var result = builder.ToString();
            return result;
        }

        private void RuleGrid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var grid = (Grid)sender;
            var ruleName = ((RuleInfo)grid.DataContext).Rule.Name;
            int ruleIndex = _viewModel.RulesInfo.ToList().FindIndex(ruleInfo => ruleInfo.Rule.Name == ruleName);

            _selectedRuleIndex = ruleIndex;
            RestoreSelectedRule();
        }

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

        private void AlsoActivateActiveRulesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            LoadSelectedPresets();
            UpdateActiveRulesForConverters();
        }

        private void AlsoActivateActiveRulesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DeactivateAllRules();
            RefreshRulesListView();
            UpdateActiveRulesForConverters();
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            RenameWith(RenameFileAction);
        }


        private void RenameWith(RenameAction action)
        {
            var rulesInfo = _viewModel.RulesInfo.Clone();
            var sortedRulesInfo = rulesInfo.Sort();

            foreach (var file in _viewModel.Files)
            {
                string oldPath = file.Path;
                string filePath = file.Path;
                string fileName = file.Name;
                string fileDirectory = ChangeDirectory(file.Path);

                foreach (var ruleInfo in sortedRulesInfo)
                {
                    if (ruleInfo.IsActive())
                    {
                        fileName = ruleInfo.Rule.Rename(fileName);
                        filePath = Path.Combine(fileDirectory, fileName);
                    }
                }

                action.Invoke(oldPath, filePath);

            }
        }

        private string ChangeDirectory(string filePath)
        {
            string newDirectory = Path.GetDirectoryName(filePath)!;

            if (_moveToPath != string.Empty)
            {
                newDirectory = _moveToPath;
            }

            return newDirectory;
        }

        private delegate void RenameAction(string oldPath, string newPath);

        private void RenameFileAction(string oldPath, string newPath)
        {
            try
            {
                newPath = EnumerateDuplicates(newPath);
                System.IO.File.Move(oldPath, newPath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void RenameAndCopyFileAction(string oldPath, string newPath)
        {
            try
            {
                newPath = EnumerateDuplicates(newPath);
                System.IO.File.Copy(oldPath, newPath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private string EnumerateDuplicates(string path)
        {
            int index = 1;

            while (System.IO.File.Exists(path))
            {
                string directory = Path.GetDirectoryName(path)!;
                string fileName = Path.GetFileNameWithoutExtension(path);
                string extension = Path.GetExtension(path);
                
                path = Path.Combine(directory, $"{fileName} ({index++}){extension}");
            }

            return path;
        }

        private void RenameAndCopyButton_Click(object sender, RoutedEventArgs e)
        {
            var browsingScreen = new CommonOpenFileDialog() { Multiselect = false, IsFolderPicker = true };

            _moveToPath = browsingScreen.ShowDialog() == CommonFileDialogResult.Ok ? browsingScreen.FileName : string.Empty;

            RenameWith(RenameAndCopyFileAction);
        }
    }
}