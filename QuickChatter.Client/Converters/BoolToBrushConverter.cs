using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace QuickChatter.Client.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAvailable)
            {
                return isAvailable ? Brushes.Green : Brushes.Red;
            }
            return Brushes.Gray; // Default fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
