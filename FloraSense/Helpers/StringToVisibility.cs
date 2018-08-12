using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FloraSense
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
             string.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, string language) => ((Visibility) value).ToString();

    }
}
