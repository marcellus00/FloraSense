using System;
using Windows.UI.Xaml.Data;

namespace FloraSense
{
    public class BoolToYes : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            (bool) value ? 1 : 0;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (int) value == 1;
        }
    }
}
