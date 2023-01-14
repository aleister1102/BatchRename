using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace BatchRename
{
    internal class PascalCaseToRegularConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var pascalCase = (string)value;
            var builder = new StringBuilder();

            for (int i = 0; i < pascalCase.Length; i++)
            {
                var c = pascalCase[i];

                if (char.IsUpper(c) && i > 0)
                {
                    builder.Append(' ');
                    builder.Append(char.ToLower(c));
                }
                else
                {
                    builder.Append(c);
                }
            }

            var result = builder.ToString();
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}