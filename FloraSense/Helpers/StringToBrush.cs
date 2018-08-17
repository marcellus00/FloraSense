using System;
using System.Linq;
using Windows.UI.Xaml.Data;
using FloraSense.Helpers;

namespace FloraSense
{
    public class StringToBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var theme = Extensions.Themes.FirstOrDefault(t => t.Name == (string)value) ?? Extensions.Themes.First();
            var param = (string) parameter ?? string.Empty;
            return param.Equals("Text") ? theme.TextColor : theme.Brush;
        }
            

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            ((Theme) value)?.Name;
    }
}
