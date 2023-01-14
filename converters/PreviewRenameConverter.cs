using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace BatchRename
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