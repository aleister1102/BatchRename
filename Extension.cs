using BatchRenamePlugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BatchRename.MainWindow;

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
    }
}