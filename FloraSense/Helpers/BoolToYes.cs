using System;
using Windows.UI.Xaml.Data;

namespace FloraSense
{
    public class BoolToYes : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            (bool) value ? "Yes" : "No";

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            ((string) value).Equals("Yes");

    }
}
