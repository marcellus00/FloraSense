using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FloraSense
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        public SettingsModel Model { get; set; }

        private bool Poll { get; set; }
        private SettingsModel.Units Units { get; set; }

        public SettingsDialog()
        {
            this.InitializeComponent();
            Poll = Model.PollOnStart;
            Units = Model.Temp;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Model.PollOnStart = Poll;
            Model.Temp = Units;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void OnReset_Click(object sender, RoutedEventArgs e)
        {
            var resetDialog = new ContentDialog
            {
                Title = "Reset data",
                Content = "Are you sure want to reset all the data?",
                PrimaryButtonText = "Erase",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Secondary
            };

            var result = await resetDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                SaveData.Clear();
                Model = new SettingsModel();
            }
        }
    }
}
