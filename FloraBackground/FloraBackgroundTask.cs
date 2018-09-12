using Windows.ApplicationModel.Background;
using FloraSense;
using MiFlora;

namespace FloraBackground
{
    public sealed class FloraBackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var knownDevices = SaveData.Load<SensorDataCollection>() ?? new SensorDataCollection();
            var reader = new MiFloraReader();
            reader.StartDeviceWatcher();
            foreach (var device in knownDevices)
            {
                _deferral = taskInstance.GetDeferral();
                var data = await MiFloraReader.PollDevice(device.DeviceId);
                _deferral.Complete();
                if (data.Error == SensorData.ErrorType.None)
                    device.Update(data);
            }
            reader.StopDeviceWatcher();
            SaveData.Save(knownDevices);
        }
    }
}
