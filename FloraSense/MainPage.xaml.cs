using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using FloraSense.Annotations;
using FloraSense.Helpers;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using MiFlora;

namespace FloraSense
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private readonly MiFloraReader _reader;
        private readonly SensorDataModel _adModel;
        private readonly DataTemplate _adTemplate;
        private readonly ControlTemplate _blankTemplate;
        private readonly SettingsModel _settings;

        private GridViewItem _adItem;

        private bool IsBusy => ProgressBar.Visibility == Visibility.Visible;

        public SensorDataCollection KnownDevices { get; }
        public bool IsCelsius => _settings.Temp == SettingsModel.Units.C;
        
        public MainPage()
        {
            _settings = SaveData.Load<SettingsModel>() ?? new SettingsModel();

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

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            _adItem = DataGridView.ContainerFromItem(_adModel) as GridViewItem;
            _adItem.ContentTemplate = _adTemplate;
            _adItem.Template = _blankTemplate;
            _adItem.Show(false);

            await CheckList();
        }

        private void OnSuspend(object sender, SuspendingEventArgs e)
        {
            KnownDevices.Remove(_adModel);
            SaveData.Save(KnownDevices);
        }

        private async Task CheckList()
        {
            var anySensors = KnownDevices.Any(model => model != _adModel);
            RefreshButton.IsEnabled = anySensors;
            WelcomeTip.Show(!anySensors);
            if (anySensors)
            {
                if(_settings.PollOnStart)
                    await Refresh();
                _adItem.Show(true);
            }
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
            await Refresh();
        }

        private async Task Refresh()
        {
            SettingsButton.IsEnabled = false;
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


            SettingsButton.IsEnabled = true;
            RefreshButton.IsEnabled = true;
            AddButton.IsEnabled = true;
            ProgressBar.Show(false);
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            _adItem.Show(false);
            SettingsButton.IsEnabled = false;
            RefreshButton.IsEnabled = false;
            AddButton.Show(false);
            FinishButton.Show(true);
            WelcomeTip.Show(false);

            EnumerateDevices();
        }

        private async void FinishAddButton_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsButton.IsEnabled = true;
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
            await CheckList();
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

        private async void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var settingsDialog = new SettingsDialog(_settings);
            var result = await settingsDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                SaveData.Save(_settings);
                OnPropertyChanged(nameof(IsCelsius));
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
