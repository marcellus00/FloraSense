using System;
using Windows.UI.Xaml.Data;

namespace FloraSense
{
    public class PercentFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return $"{System.Convert.ToString(value)}%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ((string) value).Replace("%", "");
        }
    }
}
