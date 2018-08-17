using System.Linq;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FloraSense
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        private readonly SettingsModel _model;
        public SettingsModel Backup { get; }

        public SettingsDialog(SettingsModel model)
        {
            this.InitializeComponent();
            _model = model;
            Backup = new SettingsModel();
            Backup.Update(_model);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _model.Update(Backup);
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }

}

