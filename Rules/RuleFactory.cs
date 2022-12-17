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
            // TODO: preset from DLL files
            var rule1 = new AddPrefixRule();
            var rule2 = new ReplaceSpecialCharsRule();

            _prototypes = new Dictionary<string, IRule>()
            {
                {rule1.Name, rule1 },
                {rule2.Name, rule2 }
            };
        }

        public static IRule CreateWith(string presetLine)
        {
            (var ruleName, var presetPairs) = ParsePresetLine(presetLine);

            var rule = (IRule)_prototypes[ruleName].Clone();

            rule.Apply(presetPairs);

            return rule;
        }

        private static (string, string[]) ParsePresetLine(string presetLine)
        {
            var rulePreset = presetLine.Split(":");
            var ruleName = rulePreset[0];
            var presetPairs = rulePreset[1].Split(";");

            return (ruleName, presetPairs);
        }
    }
}