using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FloraSense
{
    public sealed partial class EditPlantDialog : ContentDialog
    {
        public SensorDataModel Model { get; set; }

        public EditPlantDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_SaveButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Model.Name = PlantName.Text;
        }

        private void ContentDialog_CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox) sender;
            IsPrimaryButtonEnabled = !string.IsNullOrEmpty(textBox.Text);
        }
    }
}
