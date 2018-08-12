using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using FloraSense.Annotations;
using MiFlora;

namespace FloraSense
{
    public class SensorDataModel : INotifyPropertyChanged
    {
        public bool IsValid => !string.IsNullOrEmpty(DeviceId);

        public string DeviceId { get; set; }
        public int Moisture { get; set; }
        public ushort Fertility { get; set; }
        public uint Brightness { get; set; }
        public float Temperature { get; set; }
        public int Battery { get; set; }
        public string Firmware { get; set; }
        public string LastUpdate { get; set; }
        
        #region ManuallyUpdated

        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private bool _known;
        public bool Known
        {
            get => _known;
            set
            {
                _known = value;
                OnPropertyChanged(nameof(Known));
            }
        }
        #endregion

        public void Update(SensorData sensorData)
        {
            DeviceId = sensorData.DeviceId;
            OnPropertyChanged(nameof(DeviceId));
            OnPropertyChanged(nameof(IsValid));

            if (!Known)
                Name = sensorData.Name;
            OnPropertyChanged(nameof(Name));

            Moisture = sensorData.Moisture;
            OnPropertyChanged(nameof(Moisture));

            Fertility = sensorData.Conductivity;
            OnPropertyChanged(nameof(Fertility));

            Brightness = sensorData.Brightness;
            OnPropertyChanged(nameof(Brightness));

            Temperature = sensorData.Temperature;
            OnPropertyChanged(nameof(Temperature));

            Battery = sensorData.Battery;
            OnPropertyChanged(nameof(Battery));

            Firmware = sensorData.Verison;
            OnPropertyChanged(nameof(Firmware));

            LastUpdate = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            OnPropertyChanged(nameof(LastUpdate));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
