using BaseRule;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BatchRename
{
    public partial class MainWindow : Window
    {
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

        private static ObservableCollection<RuleConfig> CreateRuleConfigs(IRule rule)
        {
            var configs = new ObservableCollection<RuleConfig>();

            foreach (var propName in rule.ConfigurableProps)
            {
                var propValue = GetRuleProp(rule, propName);
                var config = new RuleConfig(propName, propValue);

                configs.Add(config);
            }

            return configs;
        }

        private static string GetRuleProp(IRule rule, string prop)
        {
            var propInfo = rule.GetType().GetProperty(prop);
            var propValue = propInfo!.GetValue(rule, null);
            return propValue!.ToString()!;
        }

        private void RuleGrid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var grid = (Grid)sender;
            var ruleName = ((RuleInfo)grid.DataContext).Rule.Name;
            int ruleIndex = _viewModel.RulesInfo.ToList().FindIndex(ruleInfo => ruleInfo.Rule.Name == ruleName);

            _selectedRuleIndex = ruleIndex;
            RestoreSelectedRule();
        }
    }
}