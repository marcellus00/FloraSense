using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using FloraSense.Helpers;
using Microsoft.Advertising.WinRT.UI;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using MiFlora;
using Newtonsoft.Json.Bson;

namespace FloraSense
{
    public sealed partial class MainPage : Page
    {
        private readonly MiFloraReader _reader;
        private readonly SensorDataModel _adModel;
        private readonly DataTemplate _adTemplate;
        private readonly ControlTemplate _blankTemplate;

        private GridViewItem _adItem;

        private bool IsBusy => ProgressBar.Visibility == Visibility.Visible;

        public SensorDataCollection KnownDevices { get; }
        
        public MainPage()
        {
            Application.Current.Suspending += OnSuspend;
            this.Loaded += OnLoaded;
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            _reader = new MiFloraReader();
            _adModel = new SensorDataModel {Known = true};
            _adTemplate = (DataTemplate)Resources["AdTemplate"];
            _blankTemplate = (ControlTemplate)Resources["BlankTemplate"];

            KnownDevices = SaveData.Load<SensorDataCollection>() ?? new SensorDataCollection();
            KnownDevices.Add(_adModel);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _adItem = DataGridView.ContainerFromItem(_adModel) as GridViewItem;
            _adItem.ContentTemplate = _adTemplate;
            _adItem.Template = _blankTemplate;
            _adItem.Show(false);

            CheckList();
        }

        private void OnSuspend(object sender, SuspendingEventArgs e)
        {
            KnownDevices.Remove(_adModel);
            SaveData.Save(KnownDevices);
        }

        private void CheckList()
        {
            var anySensors = KnownDevices.Any(model => model != _adModel);
            RefreshButton.IsEnabled = anySensors;
            WelcomeTip.Show(!anySensors);
            if(anySensors)
                _adItem.Show(true);
        }

        private async void OnSensorDataRecieved(SensorData sensorData)
        {
            await RunAsync(() =>
            {
                var sensorDevice = KnownDevices.FirstOrDefault(data => data.DeviceId == sensorData.DeviceId);
                if (sensorDevice == null)
                {
                    sensorDevice = new SensorDataModel();
                    KnownDevices.Insert(KnownDevices.Count - 1, sensorDevice);
                }
                sensorDevice.Update(sensorData);
            });
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;
            AddButton.IsEnabled = false;
            ProgressBar.Show(true);

            foreach (var model in KnownDevices)
            {
                if (!model.IsValid) continue;
                var data = await _reader.PollDevice(model.DeviceId);
                if (string.IsNullOrEmpty(data.Error))
                    model.Update(data);
                else
                    model.LastUpdate = "Error";
            }

            RefreshButton.IsEnabled = true;
            AddButton.IsEnabled = true;
            ProgressBar.Show(false);
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            _adItem.Show(false);
            RefreshButton.IsEnabled = false;
            AddButton.Show(false);
            FinishButton.Show(true);
            WelcomeTip.Show(false);

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

            SaveData.Save(KnownDevices);
            CheckList();
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
            if(IsBusy) return;
            var editDialog = new EditPlantDialog();
            var item = e.ClickedItem as SensorDataModel;
            if (item == _adModel) return;
            if (!item.IsValid)
            {
                KnownDevices.Remove(item);
                return;
            }
            editDialog.Model = item;
            await editDialog.ShowAsync();
        }
        
        private void SensorData_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if(IsBusy) return;
            (sender as FrameworkElement).FindDescendantByName("EditIcon").Show(true);
        }

        private void SensorData_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            (sender as FrameworkElement).FindDescendantByName("EditIcon").Show(false);
        }

        private void Test2Button_OnClick(object sender, RoutedEventArgs e)
        {
            SaveData.Clear();
            KnownDevices.Clear();
            SaveData.Save(KnownDevices);
            CheckList();
        }
    }
}
