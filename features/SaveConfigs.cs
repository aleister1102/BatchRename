using System.Text;
using System.Windows;

namespace BatchRename
{
    public partial class MainWindow : Window
    {
        private void SaveConfigsButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedRule = (RuleInfo)RulesListView.SelectedItem;

            if (selectedRule is not null)
            {
                if (selectedRule.Validate())
                {
                    string configLine = GenerateConfigLine(selectedRule);
                    var rule = RuleFactory.CreateWith(configLine);

                    UpdateRule(rule);
                    UpdateActiveRulesForConverters();
                }
            }
        }

        private static string GenerateConfigLine(RuleInfo ruleInfo)
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
    }
}