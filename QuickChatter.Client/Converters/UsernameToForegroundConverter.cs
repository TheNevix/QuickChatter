using QuickChatter.Models.Settings;
using System.Globalization;
using System.Windows.Data;

namespace QuickChatter.Client.Converters
{
    public class UsernameToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value as string == UserSettings.Username)
            {
                return "#e3dad8";
            }
            else
            {
                return "#262524";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
