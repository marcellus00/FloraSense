using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FloraSense
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<SensorDataModel> KnownDevices { get; } =
            new ObservableCollection<SensorDataModel>
            {
                new SensorDataModel
                {
                    Name = "Mini rose",
                    Moisture = 75,
                    Temperature = 28.3f,
                    Fertility = 3000,
                    Brightness = 150,
                    Battery = 100
                },
                new SensorDataModel
                {
                    Name = "Fig rubber",
                    Moisture = 75,
                    Temperature = 28.3f,
                    Fertility = 3000,
                    Brightness = 150,
                    Battery = 100
                }
            };

        public ObservableCollection<SensorDataModel> NewDevices { get; } = new ObservableCollection<SensorDataModel>();

        public MainPage()
        {
            this.InitializeComponent();
        }
    }
}
