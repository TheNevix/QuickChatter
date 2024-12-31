using QuickChatter.Models.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace QuickChatter.Client.Converters
{
    public class UsernameToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.ToString() == UserSettings.Username)
            {
                return HorizontalAlignment.Right; // Return a style, color, or boolean
            }
            else
            {
                return HorizontalAlignment.Left;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
