#define PLANT_LIST_BANNER
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FloraSense.Annotations;
using FloraSense.Helpers;
using Microsoft.Advertising.WinRT.UI;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using MiFlora;
using Windows.UI.Xaml.Media.Imaging;

namespace FloraSense
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public const string AppId = "9NP89FZM6N5F";
        public const string AdIdRegular = "1100028866";
        public const string AdIdNative = "1100029460";

        private readonly MiFloraReader _reader;
        private readonly SettingsModel _settings;

        private readonly SensorDataModel _adModel;
#if PLANT_LIST_BANNER
        private readonly DataTemplate _adTemplate;
        private readonly ControlTemplate _blankTemplate;
        private GridViewItem _adItem;
#endif
        
        private bool IsBusy => ProgressBar.Visibility == Visibility.Visible;
        private bool IsMobile => Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile";
        private App App { get; } = (App) Application.Current;

        public bool Debug { get; } =
#if DEBUG
            true;
#else
            false;
#endif

        public SensorDataCollection KnownDevices { get; }
        public bool IsCelsius => _settings.TempUnits == SettingsModel.Units.C;
        public string ThemeName => _settings.ThemeName ?? Helpers.Helpers.Themes.First().Name;
        public Brush TextColor => Helpers.Helpers.GetTheme(ThemeName).TextColor;
        
        private bool _hasSensors;

        public MainPage()
        {
            _settings = SaveData.Load<SettingsModel>() ?? new SettingsModel();

            Application.Current.Suspending += OnSuspend;
            Loaded += OnLoaded;
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            
            _reader = new MiFloraReader();

            var knownDevices = SaveData.Load<SensorDataCollection>();
            knownDevices?.RemoveInvalid();
            KnownDevices = knownDevices ?? new SensorDataCollection();
            foreach (var sensorDataModel in new SampleData().KnownDevices)
            {
                KnownDevices.Add(sensorDataModel);
            }

#if PLANT_LIST_BANNER
            _adModel = new SensorDataModel {Known = true};
            _adTemplate = (DataTemplate) Resources["AdTemplate"];
            _blankTemplate = (ControlTemplate) Resources["BlankTemplate"];
#endif
            KnownDevices.Add(_adModel);
        }

        private void UpdateAdVisibility()
        {
           _adItem?.Show(!App.FloraSenseAdFreePurchased && _hasSensors);
        }

        private void MainPage_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
#if DEBUG
            if (e.Key == (VirtualKey) 192)
                DebugGrid.Toggle();
#endif
        }
        
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_settings.BgUpdate && !BgTaskHelper.TaskRegistered)
                await BgTaskHelper.RegisterTask(_settings.BgUpdateRate);

#if PLANT_LIST_BANNER
            var container = DataGridView.ContainerFromItem(_adModel);
            _adItem = container as GridViewItem;
            _adItem.ContentTemplate = _adTemplate;
            _adItem.Template = _blankTemplate;
            _adItem.IsTabStop = false;
            _adItem.IsHitTestVisible = false;
#endif
            CheckList(true);
            if (_settings.PollOnStart)
                await Refresh();
        }

        private void OnSuspend(object sender, SuspendingEventArgs e)
        {
            if (IsBusy) return;
            KnownDevices.Remove(_adModel);
            KnownDevices.RemoveInvalid();
            SaveData.Save(KnownDevices);
        }

        private async void CheckList(bool updatePurchases = false)
        {
            _hasSensors = KnownDevices.Any(model => model != _adModel);
            RefreshButton.IsEnabled = _hasSensors;
            WelcomeTip.Show(!_hasSensors);
            if(updatePurchases)
                await App.UpdatePurchasesInfo();
            UpdateAdVisibility();
        }

        private async void OnSensorDataReceived(SensorData sensorData)
        {
            await RunAsync(() =>
            {
                var sensorDevice = KnownDevices.FirstOrDefault(data => data.DeviceId == sensorData.DeviceId);
                if (sensorDevice == null)
                {
                    sensorDevice = new SensorDataModel();
                    KnownDevices_Add(sensorDevice);
                }

                sensorDevice.Update(sensorData);
            });
        }

        private void KnownDevices_Add(SensorDataModel model)
        {
            KnownDevices.Insert(KnownDevices.Count - 1, model);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        private void ToggleButtons(bool value)
        {
            RefreshButton.IsEnabled = value && _hasSensors;
            AddButton.IsEnabled = value;
            ProgressBar.Show(!value);
        }

        private async Task Refresh()
        {
            if (!await CheckBluetooth())
                return;

#if ENUMERATE_ON_REFRESH
            _reader.StartDeviceWatcher();
#endif
            foreach (var model in KnownDevices.ToList())
            {
                if (!model.IsValid) continue;
                var pollTask = MiFloraReader.PollDevice(model.DeviceId);
                var data = await pollTask;

                if (data.Error == SensorData.ErrorType.None)
                    model.Update(data);
                else
                    model.LastUpdate = data.ErrorDetails;
            }
#if ENUMERATE_ON_REFRESH
            _reader.StopDeviceWatcher();
#endif

            ToggleButtons(true);
        }

        private async Task<bool> CheckBluetooth()
        {
            ToggleButtons(false);

            var hw = await MiFloraReader.IsBleEnabledAsync();

            switch (hw)
            {
                case null:
                    await Helpers.Helpers.ShowMessage("Bluetooth device is missing or unavailable");
                    break;
                case false:
                    await Helpers.Helpers.ShowMessage("Bluetooth is turned off");
                    break;
            }

            if (hw == true) return true;

            ToggleButtons(true);
            return false;

        }

        private async void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!await CheckBluetooth())
                return;

            RefreshButton.IsEnabled = false;
            AddButton.Show(false);
            ProgressBar.Show(true);

            FinishButton.Show(true);
            WelcomeTip.Show(false);

            EnumerateDevices();
        }

        private void FinishAddButton_OnClick(object sender, RoutedEventArgs e)
        {
            _reader.OnEnumerationCompleted = null;
            _reader.OnSensorDataRecieved = null;
            _reader.StopDeviceWatcher();

            ToggleButtons(true);
            AddButton.Show(true);
            FinishButton.Show(false);

            ProgressBar.Show(false);
            foreach (var model in KnownDevices.ToList())
                if (!model.Known)
                    KnownDevices.Remove(model);

            SaveData.Save(KnownDevices);
            CheckList();
        }

        private void EnumerateDevices()
        {
            _reader.OnSensorDataRecieved += OnSensorDataReceived;
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
            _settings.BgUpdate = BgTaskHelper.TaskRegistered;
            var settingsDialog = new SettingsDialog(_settings, LogDebug);
            settingsDialog.OnRemoveAds += UpdateAdVisibility;
            var result = await settingsDialog.ShowAsync();
            UpdateAdVisibility();
            
            if (result != ContentDialogResult.Primary) return;
            SaveData.Save(_settings);
            OnPropertyChanged(nameof(IsCelsius));
            OnPropertyChanged(nameof(ThemeName));
            OnPropertyChanged(nameof(TextColor));
            CheckList();

            BgTaskHelper.UnregisterTask();
            if (!_settings.BgUpdate) return;
            _settings.BgUpdate = await BgTaskHelper.RegisterTask(_settings.BgUpdateRate);
        }

        private void LogDebug(string log)
        {
            DebugLog.Text += $"{log}\n";
        }

        private async void PlantsList_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (IsBusy) return;
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
            if (IsBusy) return;
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

        private void SensorData_OnLoading(FrameworkElement sender, object args)
        {
            if (!IsMobile) return;
            // ReSharper disable once PossibleNullReferenceException
            (sender as RelativePanel).Width = Window.Current.Bounds.Width * 0.95f;
        }

        private void AdControl_OnErrorOccurred(object sender, AdErrorEventArgs e)
        {
#if DEBUG
            var ad = sender as AdControl;
            LogDebug($"[E] {ad.AdUnitId} {e.ErrorCode} {e.ErrorMessage}");
#endif
        }

        private void AdControl_OnAdRefreshed(object sender, RoutedEventArgs e)
        {
#if DEBUG
            var ad = sender as AdControl;
            LogDebug($"[S] {ad.AdUnitId} {ad.HasAd} {ad.IsAutoRefreshEnabled}");
#endif
        }

        private async void TestButton_OnClick(object sender, RoutedEventArgs e)
        {
            await BgTaskHelper.RegisterTask(15);
            BgTaskHelper.GetTask().Completed += OnCompleted;
        }

        private void OnCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            DebugLog.Text += $"Background task completed at {DateTime.Now}";
        }

        private void CancelTestButton_OnClick(object sender, RoutedEventArgs e)
        {
            BgTaskHelper.UnregisterTask();
        }

        private void ToggleRegularAd_OnClick(object sender, RoutedEventArgs e)
        {
            _adItem?.Show(_adItem?.Visibility != Visibility.Visible);
        }

        private void AddMockSensor(object sender, RoutedEventArgs e)
        {
            KnownDevices_Add(new SensorDataModel {DeviceId = KnownDevices.Count.ToString(), Name = "Mock"});
        }

        private void RemoveMockSensor(object sender, RoutedEventArgs e)
        {
            var mock = KnownDevices.FirstOrDefault(model => model.Name == "Mock");
            if (mock != null)
                KnownDevices.Remove(mock);
        }
    }
}
