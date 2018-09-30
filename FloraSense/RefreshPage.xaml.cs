using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FloraSense
{
    public sealed partial class RefreshPage : Page
    {
        private object _parameter;

        public RefreshPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage), _parameter);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _parameter = e.Parameter;
        }

    }
}
