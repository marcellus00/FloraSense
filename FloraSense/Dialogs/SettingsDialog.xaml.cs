using System;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FloraSense
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        private readonly SettingsModel _model;

        private bool Poll { get; set; }
        private SettingsModel.Units Units { get; set; }

        public SettingsDialog(SettingsModel model)
        {
            this.InitializeComponent();
            _model = model;
            Poll = _model.PollOnStart;
            Units = _model.Temp;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _model.PollOnStart = Poll;
            _model.Temp = Units;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }

}

