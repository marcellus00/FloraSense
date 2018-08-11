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
        
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            
            KnownDevices = SaveData.Load<SensorDataCollection>() ?? new SensorDataCollection();
            RefreshButton.IsEnabled = KnownDevices.Any();
            _reader = new MiFloraReader();
        }

        private async void OnSensorDataRecieved(SensorData sensorData)
        {
            await RunAsync(() =>
            {
                var knownDevice = KnownDevices.FirstOrDefault(data => data.DeviceId == sensorData.DeviceId);
                if (knownDevice == null)
                {
                    knownDevice = new SensorDataModel();
                    KnownDevices.Add(knownDevice);
                }
                knownDevice.Update(sensorData);
            });
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;
            AddButton.IsEnabled = false;
            ProgressBar.Show(true);

            foreach (var model in KnownDevices)
            {
                var data = await _reader.PollDevice(model.DeviceId);
                model.Update(data);
            }

            RefreshButton.IsEnabled = true;
            AddButton.IsEnabled = true;
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
            _reader.OnSensorDataRecieved = null;
            _reader.StopDeviceWatcher();

            ProgressBar.Show(false);
            foreach (var model in KnownDevices.ToList())
                if(!model.Known)
                    KnownDevices.Remove(model);

            RefreshButton.IsEnabled = KnownDevices.Any();
            SaveData.Save(KnownDevices);
        }

        private void EnumerateDevices()
        {
            ProgressBar.Show(true);

            _reader.OnSensorDataRecieved += OnSensorDataRecieved;
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

        private async void PlantsList_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var editDialog = new EditPlantDialog();
            var item = e.ClickedItem as SensorDataModel;
            editDialog.Plant = item.Name;

            var result = await editDialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.Secondary:
                case ContentDialogResult.None:
                    break;
                case ContentDialogResult.Primary:
                    if(item.Name != editDialog.Plant)
                    {
                        item.Name = editDialog.Plant;
                        SaveData.Save(KnownDevices);
                    }
                    break;
            }
        }
    }
}
