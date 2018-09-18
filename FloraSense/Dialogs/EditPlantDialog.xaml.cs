using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Controls;
using FloraSense.Annotations;
using FloraSense.Data;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FloraSense
{
    public sealed partial class EditPlantDialog : ContentDialog, INotifyPropertyChanged
    {
        public SensorDataModel Model { get; }
        public Plant PlantModel { get; }

        public EditPlantDialog(SensorDataModel model, Plant plant)
        {
            Model = model;
            PlantModel = plant;
            InitializeComponent();
        }

        private void ContentDialog_SaveButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Model.Name = PlantName.Text;
            PlantModel.Name = Model.Name;
            PlantModel.MoistureRange = new MinMaxInt((int) M.RangeMin, (int) M.RangeMax);
            PlantModel.FertilityRange = new MinMaxInt((int) F.RangeMin, (int) F.RangeMax);
            PlantModel.BrightnessRange = new MinMaxInt((int) B.RangeMin, (int) B.RangeMax);
            PlantModel.TemperatureRange = new MinMaxFloat((float) T.RangeMin, (float) T.RangeMax);
        }

        private void ContentDialog_CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox) sender;
            IsPrimaryButtonEnabled = !string.IsNullOrEmpty(textBox.Text);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
