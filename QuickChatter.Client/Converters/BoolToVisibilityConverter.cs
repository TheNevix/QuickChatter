using QuickChatter.Models;
using QuickChatter.Models.Settings;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QuickChatter.Client.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Check if value is not null
            if (value is not null)
            {
                //Convert it into an user object
                var user = value as User;

                //Check if the user selected himself
                if (user.Username == UserSettings.Username)
                {
                    //If so, hide (invite button)
                    return Visibility.Collapsed;
                }

                if (user.IsAvailable is bool boolValue)
                {
                    return boolValue ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
