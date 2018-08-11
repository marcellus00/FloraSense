using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FloraSense
{
    public sealed partial class Pin : UserControl
    {
        public Pin()
        {
            this.DefaultStyleKey = typeof(Pin);
            this.InitializeComponent();
        }
        
        public bool IsPinned
        {
            get => (bool)GetValue(IsPinnedProperty);
            set => SetValue(IsPinnedProperty, value);
        }
        
        public static readonly DependencyProperty IsPinnedProperty =
            DependencyProperty.Register("IsPinned", typeof(bool), typeof(Pin), PropertyMetadata.Create(false));

        private void OnClick(object sender, RoutedEventArgs e)
        {
            IsPinned = !IsPinned;
        }

    }
}
