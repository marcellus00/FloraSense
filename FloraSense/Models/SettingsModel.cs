using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using FloraSense.Annotations;

namespace FloraSense.Models
{
    public class SettingsModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool RefreshOnStartup { get; set; }
        
        public TempUnit TemperatureUnit { get; set; }
        public List<string> TempUnits => Enum.GetNames(typeof(TempUnit)).ToList();

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Save()
        {
            OnPropertyChanged(nameof(TemperatureUnit));
        }

        public enum TempUnit
        {
            Celsius,
            Fahrenheit
        }
    }
}
