using System;
using Windows.UI.Xaml.Data;

namespace FloraSense
{
    public class UnitsToIdx : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            ((SettingsModel.Units) value) == SettingsModel.Units.C ? 0 : 1;

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            ((int) value) == 0 ? SettingsModel.Units.C : SettingsModel.Units.F;

    }
}
