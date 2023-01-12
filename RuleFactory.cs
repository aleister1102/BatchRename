using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BaseRule;

namespace BatchRename
{
    public class RuleFactory
    {
        private static RuleFactory? _instance = null;

        private static Dictionary<string, IRule> _prototypes = new();

        private RuleFactory()
        {
            LoadPlugins();
        }

        public static void LoadPlugins()
        {
            var pluginPaths = GetPlugins();
            LoadRulesFrom(pluginPaths);
        }

        private static string[] GetPlugins()
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string? folder = Path.GetDirectoryName(exePath);
            var pluginPaths = Directory.GetFiles(folder!, "*.dll");

            return pluginPaths;
        }

        private static void LoadRulesFrom(string[] pluginPaths)
        {
            foreach (var pluginPath in pluginPaths)
            {
                Assembly assembly = LoadAssemblyFrom(pluginPath);

                Type[] types = assembly.GetTypes();

                AddRuleToPrototypes(types);
            }
        }

        private static Assembly LoadAssemblyFrom(string pluginPath)
        {
            var domain = AppDomain.CurrentDomain;
            var assemblyName = AssemblyName.GetAssemblyName(pluginPath);
            Assembly assembly = domain.Load(assemblyName);

            return assembly;
        }

        private static void AddRuleToPrototypes(Type[] ruleTypes)
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

        private static (string, IRule) CreateRuleWith(Type ruleType)
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

        public static IRule CreateWith(string configLine)
        {
            (var ruleName, var configs) = ParseConfigLine(configLine);

            var rule = (IRule)_prototypes[ruleName].Clone();

            rule.Config(configs);

            return rule;
        }

        private static (string, string) ParseConfigLine(string configLine)
        {
            var configPairs = configLine.Split(":");
            var ruleName = configPairs[0];

            if (configPairs.Length < 2) return (ruleName, "");

            var configs = configPairs[1];
            return (ruleName, configs);
        }

        public static Dictionary<string, IRule> GetPrototypes()
        {
            return _prototypes;
        }
    }
}