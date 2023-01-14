using System;
using System.Globalization;
using System.Windows.Data;

namespace BatchRename
{
    internal class RuleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rule = (RuleInfo)value;

            if (rule.IsActive())
            {
                return "#fff";
            }
            else
            {
                return "#000";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}