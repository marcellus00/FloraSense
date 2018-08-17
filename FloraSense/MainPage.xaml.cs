using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FloraBackground;
using FloraSense.Annotations;
using FloraSense.Helpers;
using Microsoft.Advertising.WinRT.UI;
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
        private bool IsMobile => Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile";

        public bool Debug { get; } =
#if DEBUG
            true;
#else
            false;
#endif

        public SensorDataCollection KnownDevices { get; }
        public bool IsCelsius => _settings.TempUnits == SettingsModel.Units.C;
        public string ThemeName => _settings.ThemeName ?? Extensions.Themes.First().Name;
        public Brush TextColor => Extensions.GetTheme(ThemeName).TextColor;
        
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

        private void MainPage_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
#if DEBUG
            if (e.Key == (VirtualKey) 192)
                DebugGrid.Toggle();
#endif
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var container = DataGridView.ContainerFromItem(_adModel);
            _adItem = container as GridViewItem;
            _adItem.ContentTemplate = _adTemplate;
            _adItem.Template = _blankTemplate;
            _adItem.IsTabStop = false;
            _adItem.Show(false);

            CheckList();
            if (_settings.PollOnStart)
                await Refresh();
        }

        private void OnSuspend(object sender, SuspendingEventArgs e)
        {
            if(IsBusy) return;
            KnownDevices.Remove(_adModel);
            SaveData.Save(KnownDevices);
        }

        private void CheckList()
        {
            var anySensors = KnownDevices.Any(model => model != _adModel);
            RefreshButton.IsEnabled = anySensors;
            WelcomeTip.Show(!anySensors);
            _adItem.Show(anySensors);
        }

        private async void OnSensorDataRecieved(SensorData sensorData)
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
            RefreshButton.IsEnabled = value;
            AddButton.IsEnabled = value;
            ProgressBar.Show(!value);
        }

        private async Task Refresh()
        {
            if (!await CheckBluetooth())
                return;

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

            ToggleButtons(true);
        }

        private async Task<bool> CheckBluetooth()
        {
            ToggleButtons(false);

            var hw = await MiFloraReader.IsBleEnabledAsync();

            switch (hw)
            {
                case null:
                    await ShowMessage("Bluetooth device is missing or unavailable");
                    break;
                case false:
                    await ShowMessage("Bluetooth is turned off");
                    break;
            }

            if (hw == true) return true;

            ToggleButtons(true);
            return false;

        }

        private async Task ShowMessage(string message)
        {
            await new MessageDialog(message).ShowAsync();
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
            //_adItem.Show(false);

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
                if(!model.Known)
                    KnownDevices.Remove(model);

            SaveData.Save(KnownDevices);
            CheckList();
        }

        private void EnumerateDevices()
        {
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
                OnPropertyChanged(nameof(ThemeName));
                OnPropertyChanged(nameof(TextColor));
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

        private void SensorData_OnLoading(FrameworkElement sender, object args)
        {
            if(!IsMobile) return;
            // ReSharper disable once PossibleNullReferenceException
            (sender as RelativePanel).Width = Window.Current.Bounds.Width * 0.95f;
        }

        private void AddMockButton_OnClick(object sender, RoutedEventArgs e)
        {
            KnownDevices_Add(new SampleData().KnownDevices.FirstOrDefault());
            CheckList();
        }

        private void RemoveMockButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (KnownDevices.Count > 1)
            {
                KnownDevices.RemoveAt(KnownDevices.Count - 2);
                CheckList();
            }

        }

        private void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            while (KnownDevices.Count > 1)
                RemoveMockButton_OnClick(sender, e);
        }

        private void AdControl_OnAdRefreshed(object sender, RoutedEventArgs e)
        {
#if DEBUG
            var ad = sender as AdControl;
            DebugLog.Text += $"[R] {ad.AdUnitId} {ad.HasAd}";
#endif
        }

        private void AdControl_OnErrorOccurred(object sender, AdErrorEventArgs e)
        {
#if DEBUG
            var ad = sender as AdControl;
            DebugLog.Text += $"[E] {ad.AdUnitId} {e.ErrorCode} {e.ErrorMessage}";
#endif
        }

        private async void TestButton_OnClick(object sender, RoutedEventArgs e)
        {
            var bgTaskType = typeof(FloraBackgroundTask);
            if (BackgroundTaskRegistration.AllTasks.Any(pair => pair.Value.Name == bgTaskType.Name))
                return;

            var access = await BackgroundExecutionManager.RequestAccessAsync();

            if (access != BackgroundAccessStatus.AlwaysAllowed)
            {
                await ShowMessage("Insufficient permissions to schedule a background task");
                return;
            }

            var builder = new BackgroundTaskBuilder
            {
                Name = bgTaskType.Name,
                TaskEntryPoint = bgTaskType.FullName
            };
            builder.SetTrigger(new TimeTrigger(15, false));
            var task = builder.Register();
        }
    }
}
