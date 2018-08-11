using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using FloraSense.Helpers;
using MiFlora;

namespace FloraSense
{
    public sealed partial class MainPage : Page
    {
        private readonly MiFloraReader _reader;

        public SensorDataCollection KnownDevices { get; }

        public bool AddMode => AddButton.Visibility == Visibility.Collapsed;
        
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            
            KnownDevices = SaveData.Load<SensorDataCollection>() ?? new SensorDataCollection();
            RefreshButton.IsEnabled = KnownDevices.Any();
            _reader = new MiFloraReader();
            _reader.OnSensorDataRecieved += OnSensorDataRecieved;
        }

        private async void OnSensorDataRecieved(SensorData sensorData)
        {
            await RunAsync(() =>
            {
                var knownDevice = KnownDevices.FirstOrDefault(data => data.DeviceId == sensorData.DeviceId);
                if (knownDevice != null)
                {
                    var knownName = knownDevice.Name;
                    knownDevice.Update(sensorData);
                    knownDevice.Name = knownName;
                }
                else
                {
                    var model = new SensorDataModel();
                    KnownDevices.Add(model);
                    model.Update(sensorData);
                }
            });
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;
            ProgressBar.Show(true);

            foreach (var sensorDataModel in KnownDevices)
                await _reader.PollDevice(sensorDataModel.DeviceId);

            RefreshButton.IsEnabled = true;
            ProgressBar.Show(false);
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;
            AddButton.Show(false);
            FinishButton.Show(true);

            EnumerateDevices();
        }

        private void FinishAddButton_OnClick(object sender, RoutedEventArgs e)
        {
            RefreshButton.IsEnabled = true;
            AddButton.Show(true);
            FinishButton.Show(false);

            _reader.OnEnumerationCompleted = null;
            _reader.OnWatcherStoped = null;
            _reader.StopDeviceWatcher();

            ProgressBar.Show(false);
            foreach (var model in KnownDevices.ToList())
                if(!model.Known)
                    KnownDevices.Remove(model);
            
            SaveData.Save(KnownDevices);
        }

        private void EnumerateDevices()
        {
            ProgressBar.Show(true);

            _reader.OnEnumerationCompleted += OnEnumerationCompleted;
            _reader.StartDeviceWatcher();
        }

        private async void OnEnumerationCompleted()
        {
            await RunAsync(() => { ProgressBar.Show(false); });
        }

        private async Task RunAsync(DispatchedHandler callback)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, callback);
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }
    }
}
