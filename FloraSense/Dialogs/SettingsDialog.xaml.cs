using System;
using System.Linq;
using Windows.ApplicationModel.Store;
using Windows.Services.Store;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FloraSense.Helpers;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FloraSense
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        private readonly SettingsModel _model;
        public SettingsModel Backup { get; }
        private App App => (App)Application.Current;
        private Action<string> _debugLog;

        public SettingsDialog(SettingsModel model, Action<string> debugLog = null)
        {
            InitializeComponent();
            _model = model;
            Backup = new SettingsModel();
            Backup.Update(_model);

            InApps.Show(!App.FloraSenseAdFreePurchased);
            _debugLog = debugLog;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _model.Update(Backup);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void RemoveAdsButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.FloraSenseAdFreePurchased) return;
            RemoveAds.IsEnabled = false;
            var result = await App.StoreContext.RequestPurchaseAsync(App.FloraSenseAdFree);

            var extendedError = string.Empty;
            var message = string.Empty;

            if (result.ExtendedError != null)
                extendedError = result.ExtendedError.Message;

            _debugLog?.Invoke($"[Purchase] {message} {extendedError}");

            await App.UpdatePurchasesInfo();
            InApps.Show(!App.FloraSenseAdFreePurchased);
            RemoveAds.IsEnabled = true;
        }
    }

}

