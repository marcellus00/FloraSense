using System.ComponentModel;
using System.Runtime.CompilerServices;
using FloraSense.Annotations;
using MiFlora;

namespace FloraSense
{
    public class SensorDataDisplay: SensorData, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public new string Name
        {
            get => base.Name;
            set => base.Name = value;
        }
        public new int Moisture
        {
            get => base.Moisture;
            set => base.Moisture = value;
        }
        public new ushort Fertility
        {
            get => base.Fertility;
            set => base.Fertility = value;
        } 
        public new uint Brightness
        {
            get => base.Brightness;
            set => base.Brightness = value;
        }
        public new float Temperature
        {
            get => base.Temperature;
            set => base.Temperature = value;
        }
        public new int Battery
        {
            get => base.Battery;
            set => base.Battery = value;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
