using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using MiFlora;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FloraSense
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly MiFloraReader _reader;

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
            DataContext = this;
            _reader = new MiFloraReader();
            _reader.OnSensorDataRecieved += OnSensorDataRecieved;
            _reader.OnEnumerationCompleted += OnEnumerationCompleted;
        }

        private async void OnEnumerationCompleted(bool b)
        {
            await RunAsync(() => { RefreshButton.IsEnabled = true; });
        }

        private async void OnSensorDataRecieved(SensorData sensorData)
        {
            var model = new SensorDataModel();
            model.Update(sensorData);

            await RunAsync(() => KnownDevices.Add(model));
        }

        private void RefreshButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            RefreshButton.IsEnabled = false;
            KnownDevices.Clear();
            _reader.StartDeviceWatcher();
        }

        private async Task RunAsync(DispatchedHandler callback)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, callback);
        }
    }
}
