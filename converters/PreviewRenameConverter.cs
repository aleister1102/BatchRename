using BatchRenamePlugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static BatchRename.MainWindow;

namespace BatchRename.converters
{
    internal class PreviewRenameConverter : IValueConverter
    {
        public ObservableCollection<RuleInfo> RulesInfo { get; set; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string origin = (string)value;
            string newName = origin;

            foreach (var ruleInfo in RulesInfo)
            {
                if (ruleInfo.IsActive())
                {
                    newName = ruleInfo.Rule.Rename(newName);
                }
            }

            return newName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}