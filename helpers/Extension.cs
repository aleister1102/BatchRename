using BaseRule;
using System.Collections.ObjectModel;
using System.Linq;

namespace BatchRename
{
    public static class Extension
    {
        public static ObservableCollection<File> Clone(this ObservableCollection<File> collection)
        {
            var result = new ObservableCollection<File>();

            foreach (var item in collection)
            {
                var clone = (File)item.Clone();
                result.Add(clone);
            }

            return result;
        }

        public static ObservableCollection<RuleInfo> Clone(this ObservableCollection<RuleInfo> collection)
        {
            var result = new ObservableCollection<RuleInfo>();

            foreach (var item in collection)
            {
                var clone = (RuleInfo)item.Clone();
                clone.Rule = (IRule)item.Rule.Clone();
                result.Add(clone);
            }

            return result;
        }

        public static ObservableCollection<RuleInfo> Sort(this ObservableCollection<RuleInfo> collection)
        {
            var result = new ObservableCollection<RuleInfo>(collection.OrderBy(ruleInfo => ruleInfo.Order));

            return result;
        }
    }
}