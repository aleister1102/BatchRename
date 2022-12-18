using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using BatchRenamePlugins;

namespace BatchRename
{
    public class PreviewRenameConverter : IValueConverter
    {
        public List<IRule> rules = new List<IRule>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string origin = (string)value;
            string previewName = origin;

            foreach (var rule in rules)
            {
                previewName = rule.Rename(previewName);
            }

            return previewName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}