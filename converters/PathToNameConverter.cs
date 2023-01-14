using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace BatchRename
{
    internal class PathToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = (string)value;
            string name = Path.GetFileNameWithoutExtension(path);
            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}