using System.Windows;
using System.Windows.Controls;

namespace BatchRename
{
    public partial class MainWindow : Window
    {
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

        private void ActivateAllButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateAllRules();

            RefreshRulesListView();

            UpdateActiveRulesForConverters();
        }
    }
}