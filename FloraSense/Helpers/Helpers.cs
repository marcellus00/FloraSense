﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace FloraSense.Helpers
{
    public static class Helpers
    {
        public static void Show(this FrameworkElement element, bool visible)
        {
            element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public static void Toggle(this FrameworkElement element)
        {
            element.Show(element.Visibility != Visibility.Visible);
        }

        public static ThemeCollection Themes
        {
            get
            {
                Application.Current.Resources.TryGetValue("ThemeCollection", out var themeCollection);
                return (ThemeCollection)themeCollection;
            }
        }

        public static Theme GetTheme(string name)
        {
            return Themes.FirstOrDefault(theme => theme.Name == name);
        }

        public static async Task ShowMessage(string message)
        {
            await new MessageDialog(message).ShowAsync();
        }
    }
}
