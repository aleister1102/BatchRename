using BaseRule;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace BatchRename
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RuleFactory.Instance();
        }

        private readonly ViewModel _viewModel = new();

        private int _selectedRuleIndex;

        private readonly RuleCounter _ruleCounter = RuleCounter.GetInstance();

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

        private static ObservableCollection<RuleInfo> InitDefaultRulesInfo()
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

        private void ResetRules()
        {
            _viewModel.RulesInfo = InitDefaultRulesInfo();
            RefreshRulesListView();
        }

        private void RestoreSelectedRule()
        {
            RulesListView.SelectedIndex = _selectedRuleIndex;
            UpdateSelectedRule();
        }
    }
}