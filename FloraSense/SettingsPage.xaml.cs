using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using FloraSense.Models;

namespace FloraSense
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsModel Settings { get; }

        public SettingsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Settings = SaveData.Load<SettingsModel>() ?? new SettingsModel();

            var currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentViewOnBackRequested;
        }

        private void CurrentViewOnBackRequested(object sender, BackRequestedEventArgs backRequestedEventArgs)
        {
            SaveData.Save(Settings);
            Frame.GoBack();
        }
    }
}
