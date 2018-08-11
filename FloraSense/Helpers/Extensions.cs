using Windows.UI.Xaml;

namespace FloraSense.Helpers
{
    public static class Extensions
    {
        public static void Show(this FrameworkElement element, bool visible)
        {
            element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
