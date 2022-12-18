using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BatchRenamePlugins;

namespace BatchRename
{
    public class RuleFactory
    {
        private static RuleFactory? _instance = null;

        private static Dictionary<string, IRule> _prototypes = new();

        private RuleFactory()
        {
            var pluginPaths = GetPlugins();
            LoadRulesFrom(pluginPaths);
        }

        private string[] GetPlugins()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string? folder = Path.GetDirectoryName(exePath);
            var pluginPaths = Directory.GetFiles(folder!, "*.dll");

            return pluginPaths;
        }

        private void LoadRulesFrom(string[] pluginPaths)
        {
            foreach (var pluginPath in pluginPaths)
            {
                Assembly assembly = LoadAssemblyFrom(pluginPath);

                Type[] types = assembly.GetTypes();

                AddRuleToPrototypes(types);
            }
        }

        private Assembly LoadAssemblyFrom(string pluginPath)
        {
            var domain = AppDomain.CurrentDomain;
            var assemblyName = AssemblyName.GetAssemblyName(pluginPath);
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
            var presetPairs = ParsePresetPairs(rulePreset);

            return (ruleName, presetPairs);
        }

        private static string[] ParsePresetPairs(string[] rulePreset)
        {
            var presetPairs = rulePreset.Length > 1
                ? rulePreset[1].Split(";")
                : new string[] { "" };

            return presetPairs;
        }
    }
}