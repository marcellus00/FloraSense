using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FloraSense
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public SensorDataCollection KnownDevices { get; set; }
        public SensorDataCollection NewDevices { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            KnownDevices = new SensorDataCollection
            {
                new SensorDataDisplay
                {
                    Name = "Mini rose",
                    Moisture = 75,
                    Temperature = 28.3f,
                    Fertility = 3000,
                    Brightness = 150,
                    Battery = 100
                },
                new SensorDataDisplay
                {
                    Name = "Fig rubber",
                    Moisture = 75,
                    Temperature = 28.3f,
                    Fertility = 3000,
                    Brightness = 150,
                    Battery = 100
                }
            };
            NewDevices = new SensorDataCollection();
            DataContext = KnownDevices;
        }
    }
}
