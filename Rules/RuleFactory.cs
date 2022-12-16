using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BatchRename.Rules
{
    public class RuleFactory
    {
        // TODO: implement singleton pattern
        private static Dictionary<string, IRule> _prototypes = new();

        public static void Configure()
        {
            // TODO: config from DLL files
            var rule1 = new AddPrefixRule();
            var rule2 = new ReplaceSpecialCharsRule();

            _prototypes = new Dictionary<string, IRule>()
            {
                {rule1.Name, rule1 },
                {rule2.Name, rule2 }
            };
        }

        public static IRule CreateWith(string configLine)
        {
            // Split config string by ":" to get rule name and config pairs
            var ruleConfig = configLine.Split(":");
            var ruleName = ruleConfig[0];
            var configPairs = ruleConfig[1].Split(";");

            // Create a new rule with the rule name
            var rule = (IRule)_prototypes[ruleName].Clone();

            // Apply the config pairs to rule
            rule.Apply(configPairs);

            return rule;
        }
    }
}