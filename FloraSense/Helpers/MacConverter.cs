using System;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Data;

namespace FloraSense
{
    public class MacConverter : IValueConverter
    {
        private const string Mac = "([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})";
        private static readonly Regex IdRegex = new Regex($"^BluetoothLE\\#BluetoothLE{Mac}-(?<mac>{Mac})$", RegexOptions.Compiled);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var match = IdRegex.Match((string)value);
            return !match.Success ? string.Empty : match.Groups["mac"].Value.ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (string) value;
        }
    }
}
