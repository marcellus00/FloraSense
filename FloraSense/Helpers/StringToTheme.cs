using System;
using System.Linq;
using Windows.UI.Xaml.Data;
using FloraSense.Helpers;

namespace FloraSense
{
    public class StringToTheme : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var theme = Helpers.Helpers.Themes.FirstOrDefault(t => t.Name == (string)value);
            return theme ?? Helpers.Helpers.Themes.First();
        }
            

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            ((Theme) value)?.Name;
    }
}
