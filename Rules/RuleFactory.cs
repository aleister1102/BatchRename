using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using RenamingRulePlugins;

namespace BatchRename.Rules
{
    public class RuleFactory
    {
        private static RuleFactory? _instance = null;

        private static Dictionary<string, IRule> _prototypes = new();

        private RuleFactory()
        {
            // Add static rules
            var rule1 = new AddPrefixRule();
            var rule2 = new ReplaceSpecialCharsRule();

            _prototypes = new Dictionary<string, IRule>()
            {
                {rule1.Name, rule1 },
                {rule2.Name, rule2 }
            };

            // Add rules from plugins
            var pluginInfos = GetPlugins();
            LoadRulesFrom(pluginInfos);
        }

        private FileInfo[] GetPlugins()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string? folder = Path.GetDirectoryName(exePath);
            var pluginInfos = new DirectoryInfo(folder!).GetFiles("*.dll");

            return pluginInfos;
        }

        private void LoadRulesFrom(FileInfo[] pluginInfos)
        {
            foreach (var pluginInfo in pluginInfos)
            {
                Assembly assembly = LoadAssemblyFrom(pluginInfo);

                Type[] types = assembly.GetTypes();

                AddRuleToPrototypes(types);
            }
        }

        private Assembly LoadAssemblyFrom(FileInfo pluginInfo)
        {
            var domain = AppDomain.CurrentDomain;
            var assemblyName = AssemblyName.GetAssemblyName(pluginInfo.FullName);
            Assembly assembly = domain.Load(assemblyName);

            return assembly;
        }

        private void AddRuleToPrototypes(Type[] ruleTypes)
        {
            foreach (var ruleType in ruleTypes)
            {
                if (ruleType.IsClass && typeof(IRule).IsAssignableFrom(ruleType))
                {
                    (string ruleName, IRule rule) = CreateRuleWith(ruleType);

                    if (_prototypes.ContainsKey(ruleName) == false)
                        _prototypes.Add(ruleName, rule);
                }
            }
        }

        private (string, IRule) CreateRuleWith(Type ruleType)
        {
            string ruleTypeName = ruleType.Name;
            string ruleName = ruleTypeName.Replace("Rule", "");
            IRule rule = (Activator.CreateInstance(ruleType) as IRule)!;

            return (ruleName, rule);
        }

        public static RuleFactory Instance()
        {
            if (_instance == null)
            {
                _instance = new RuleFactory();
            }

            return _instance;
        }

        public static IRule CreateWith(string presetLine)
        {
            (var ruleName, var presetPairs) = Parse(presetLine);

            var rule = (IRule)_prototypes[ruleName].Clone();

            rule.Apply(presetPairs);

            return rule;
        }

        private static (string, string[]) Parse(string presetLine)
        {
            var rulePreset = presetLine.Split(":");
            var ruleName = rulePreset[0];
            var presetPairs = rulePreset[1].Split(";");

            return (ruleName, presetPairs);
        }
    }
}