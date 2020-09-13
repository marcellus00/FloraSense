using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;
using Windows.Storage.Streams;
// ReSharper disable InconsistentNaming
// ReSharper disable InvertIf

namespace MiFlora
{
    public class MiFloraReader
    {
        public Action<SensorData> OnSensorDataRecieved;
        public Action OnEnumerationCompleted;
        public Action OnWatcherStoped;

        private const int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df);

        private const string Mac = "([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})";
        private static readonly Regex MacRegex = new Regex($"^{Mac}$", RegexOptions.Compiled);
        private static readonly Regex IdRegex = new Regex($"^BluetoothLE\\#BluetoothLE{Mac}-(?<mac>{Mac})$", RegexOptions.Compiled);

        private static readonly Guid GenericAccess = new Guid("00001800-0000-1000-8000-00805f9b34fb");
        private static readonly Guid XiaomiService = new Guid("0000fe95-0000-1000-8000-00805f9b34fb");
        private static readonly Guid DataService = new Guid("00001204-0000-1000-8000-00805f9b34fb");

        private static readonly RequestData DeviceName = new RequestData(new Guid("{00002a00-0000-1000-8000-00805f9b34fb}"));
        private static readonly RequestData Firmware = new RequestData(new Guid("00001a02-0000-1000-8000-00805f9b34fb"));
        private static readonly RequestData ModeSet = new RequestData(new Guid("00001a00-0000-1000-8000-00805f9b34fb"));
        private static readonly RequestData Sensors = new RequestData(new Guid("00001a01-0000-1000-8000-00805f9b34fb"));
        private static readonly RequestData SerialNumber = new RequestData(new Guid("00001a01-0000-1000-8000-00805f9b34fb"));

        private static readonly byte[] RealtimeModeOn = { 0xa0, 0x1f };
        private static readonly byte[] RealtimeModeOff = { 0xc0, 0x1f };
        private static readonly byte[] SerialNumberMode = { 0xb0, 0xff };

        private readonly string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };
        private const string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

        private DeviceWatcher _deviceWatcher;
        private static Dictionary<RequestData, Action<byte[], SensorData>> FormatProcessors;

        public MiFloraReader()
        {
            InitFormatProcessors();
        }

        public void StartDeviceWatcher()
        {
            if(_deviceWatcher != null)
                StopDeviceWatcher();

            _deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        aqsAllBluetoothLEDevices,
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);
            
            _deviceWatcher.Added += DeviceWatcher_Added;
            _deviceWatcher.Updated += DeviceWatcher_Updated;
            _deviceWatcher.Removed += DeviceWatcher_Removed;
            _deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            _deviceWatcher.Stopped += DeviceWatcher_Stopped;
            _deviceWatcher.Start();
        }

        public void StopDeviceWatcher()
        {
            if (_deviceWatcher == null) return;

            _deviceWatcher.Added -= DeviceWatcher_Added;
            _deviceWatcher.Updated -= DeviceWatcher_Updated;
            _deviceWatcher.Removed -= DeviceWatcher_Removed;
            _deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
            _deviceWatcher.Stopped -= DeviceWatcher_Stopped;
            _deviceWatcher.Stop();
            _deviceWatcher = null;
        }

        public static async Task<SensorData> PollDevice(string deviceId, int retries = 3)
        {
            BluetoothLEDevice device = null;
            var sensorData = new SensorData { DeviceId = deviceId };
            try
            {
                device = await BluetoothLEDevice.FromIdAsync(deviceId);

                if (device == null)
                    sensorData.Error = SensorData.ErrorType.CannotConnect;
            }
            catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
            {
                sensorData.Error = SensorData.ErrorType.BluetoothOff;
            }

            if (device != null)
            {
                sensorData.Error = SensorData.ErrorType.CommError;
                var data = await device.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                if (data.Status == GattCommunicationStatus.Success)
                {
                    var query = new QueryParams(data.Services, DataService, sensorData);
                    var success =
                        await Read(new QueryParams(data.Services, GenericAccess, sensorData).Read(DeviceName)) &&
                        await Read(query.Read(Firmware)) &&
                        await Write(query.Write(ModeSet, SerialNumberMode)) &&
                        await Read(query.Read(SerialNumber)) &&
                        await Write(query.Write(ModeSet, RealtimeModeOn)) &&
                        await Read(query.Read(Sensors)) &&
                        await Write(query.Write(ModeSet, RealtimeModeOff));

                    sensorData.Error = SensorData.ErrorType.None;
                }
            }
            device?.Dispose();

            if ((sensorData.Error == SensorData.ErrorType.CommError || sensorData.IsBad) && retries-- > 0)
            {
                await Task.Delay(500);
                sensorData = await PollDevice(sensorData.DeviceId, retries);
            }

            return sensorData;
        }

        public static async Task<bool?> IsBleEnabledAsync()
        {
            BluetoothAdapter btAdapter = null;

            try
            {
                btAdapter = await BluetoothAdapter.GetDefaultAsync();
            }
            catch (Exception)
            {
                // ignore
            }
            
            if (btAdapter == null)
                return null;
            if (!btAdapter.IsCentralRoleSupported)
                return null;
            // for UWP
            var radio = await btAdapter.GetRadioAsync();
            // for Desktop, see warning bellow
            if (radio == null)
                return null; // probably device just removed
            // await radio.SetStateAsync(RadioState.On);
            return radio.State == RadioState.On;
        }

        private void InitFormatProcessors()
        {
            FormatProcessors = new Dictionary<RequestData, Action<byte[], SensorData>>
            {
                { DeviceName, OnDeviceName},
                { Firmware, OnFirmware },
                { ModeSet, OnCommand },
                { Sensors, OnData },
                { SerialNumber, OnSerial }
            };
        }

        private void OnSerial(byte[] bytes, SensorData sensorData)
        {
            sensorData.SerialNumber = BitConverter.ToString(bytes).Replace("-", string.Empty); ;
        }

        private void OnCommand(byte[] data, SensorData sensorData) { }

        private void OnData(byte[] data, SensorData sensorData)
        {
            sensorData.Moisture = data[7];
            sensorData.Conductivity = BitConverter.ToUInt16(data, 8);
            sensorData.Brightness = BitConverter.ToUInt32(data, 3);
            sensorData.Temperature = BitConverter.ToUInt16(data, 0) / 10f;
        }

        private void OnDeviceName(byte[] bytes, SensorData sensorData)
        {
            sensorData.Name = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
        }

        private void OnFirmware(byte[] bytes, SensorData sensorData)
        {
            sensorData.Battery = Convert.ToInt32(bytes[0]);
            sensorData.Verison = Encoding.ASCII.GetString(bytes, 2, 5);
        }

        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            if (sender != _deviceWatcher) return;

            var match = IdRegex.Match(deviceInfo.Id);
            if (!match.Success)
                return;

            var mac = match.Groups["mac"].Value;
            if (!mac.StartsWith("c4:7c:8d"))
                return;

            try
            {
                var data = PollDevice(deviceInfo.Id).GetAwaiter().GetResult();
                OnSensorDataRecieved?.Invoke(data);
            }
            catch (Exception e)
            {
                OnSensorDataRecieved?.Invoke(new SensorData
                    {Error = SensorData.ErrorType.InvalidOperation, ErrorDetails = e.Message});
            }
        }
        
        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            OnEnumerationCompleted?.Invoke();
        }

        private void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
        {
            OnWatcherStoped?.Invoke();
        }


        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args) { }
        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args) { }

        private static async Task<bool> Write(QueryParams queryParams)
        {
            var characteristic = await GetCharacteristic(queryParams);
            if (characteristic == null) return false;

            var status = await characteristic.WriteValueAsync(queryParams.Payload);
            if (status != GattCommunicationStatus.Success)
            {
                queryParams.SensorData.ErrorDetails = "Error writing values.";
                return false;
            }

            return true;
        }

        private static async Task<bool> Read(QueryParams queryParams)
        {
            var characteristic = await GetCharacteristic(queryParams);
            if (characteristic == null) return false;

            var status = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (status.Status != GattCommunicationStatus.Success)
            {
                queryParams.SensorData.ErrorDetails = "Error reading values.";
                return false;
            }

            var data = status.Value.ToArray();
            FormatProcessors[queryParams.Request](data, queryParams.SensorData);
            return true;
        }

        private static async Task<GattCharacteristic> GetCharacteristic(QueryParams queryParams)
        {
            var service = queryParams.Services.FirstOrDefault(s => s.Uuid == queryParams.ServiceGuid);
            if (service == null)
            {
                queryParams.SensorData.ErrorDetails = $"Service {queryParams.ServiceGuid} is not supported.";
                return null;
            }

            var characteristics = await service.GetCharacteristicsAsync();
            if (characteristics.Status != GattCommunicationStatus.Success)
            {

                queryParams.SensorData.ErrorDetails = "Error reading characteristics.";
                return null;
            }

            var characteristic = characteristics.Characteristics.FirstOrDefault(c => c.Uuid == queryParams.Request.Uuid);
            return characteristic;
        }

        private class QueryParams
        {
            public IEnumerable<GattDeviceService> Services { get; }
            public Guid ServiceGuid { get; }
            public RequestData Request { get; private set; }
            public IBuffer Payload { get; private set; }
            public SensorData SensorData { get; }

            public QueryParams(IEnumerable<GattDeviceService> services, Guid serviceGuid, SensorData sensorData)
            {
                Services = services;
                ServiceGuid = serviceGuid;
                SensorData = sensorData;
            }

            public QueryParams Read(RequestData data)
            {
                Request = data;
                return this;
            }

            public QueryParams Write(RequestData data, byte[] payload)
            {
                Request = data;
                Payload = payload.AsBuffer();
                return this;
            }
        }

        private class RequestData
        {
            public Guid Uuid { get; }

            public RequestData(Guid uuid)
            {
                Uuid = uuid;
            }
        }
    }
}
