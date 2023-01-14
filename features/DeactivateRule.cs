using System.Windows;
using System.Windows.Controls;

namespace BatchRename
{
    public partial class MainWindow : Window
    {
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

        private void DeactivateAllButton_Click(object sender, RoutedEventArgs e)
        {
            DeactivateAllRules();

            RefreshRulesListView();

            UpdateActiveRulesForConverters();
        }
    }
}