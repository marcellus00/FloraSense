using System;
using System.Linq;
using Windows.ApplicationModel.Resources.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FloraSense.Helpers;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FloraSense
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        private readonly SettingsModel _model;
        private readonly Action<string> _debugLog;
        private readonly StoreController _storeController;

        public SettingsModel Backup { get; }
        public Action OnRemoveAds;

        public SettingsDialog(SettingsModel model, StoreController storeController, Action<string> debugLog = null)
        {
            InitializeComponent();
            _model = model;
            _storeController = storeController;
            Backup = new SettingsModel();
            Backup.Update(_model);
            LangBox.SelectedItem = LangBox.Items.FirstOrDefault(i => ((ComboBoxItem) i).Tag.ToString() == Backup.Language);

            InApps.Show(!_storeController.FloraSenseAdFreePurchased);
            _debugLog = debugLog;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var oldLang = _model.Language;
            Backup.Language = ((ComboBoxItem) LangBox.SelectedItem).Tag.ToString();
            _model.Update(Backup);

            if (_model.Language != oldLang)
            {
                _model.RefreshLanguage();
                ResourceContext.GetForViewIndependentUse().Reset();
                ResourceContext.GetForCurrentView().Reset();
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void RemoveAdsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_storeController.FloraSenseAdFreePurchased) return;
            RemoveAds.IsEnabled = false;
            var result = await _storeController.StoreContext.RequestPurchaseAsync(StoreController.FloraSenseAdFree);

            var extendedError = string.Empty;
            var message = string.Empty;

            if (result.ExtendedError != null)
                extendedError = result.ExtendedError.Message;

            _debugLog?.Invoke($"[Purchase] {message} {extendedError}");
            
            await _storeController.UpdatePurchasesInfo();

            var ads = !_storeController.FloraSenseAdFreePurchased;
            RemoveAds.IsEnabled = ads;
            InApps.Show(ads);
            if(_storeController.FloraSenseAdFreePurchased)
                OnRemoveAds?.Invoke();
            OnRemoveAds = null;
        }
    }
}

