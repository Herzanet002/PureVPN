using System;
using System.Globalization;
using System.Windows.Data;

namespace PureVPN.Converters
{
    public class DataGridValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString()!.Replace(".opengw.net", string.Empty);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
