using QuickChatter.Models.Settings;
using System.Globalization;
using System.Windows.Data;

namespace QuickChatter.Client.Converters
{
    public class UsernameToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value as string == UserSettings.Username)
            {
                return "#eb6134";
            }
            else
            {
                return "#8c8988";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
