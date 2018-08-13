using System.Collections.ObjectModel;

namespace FloraSense.SampleData
{
    public class SampleData
    {
        public ObservableCollection<SensorDataModel> KnownDevices { get; } = new ObservableCollection<SensorDataModel>
        {
            new SensorDataModel
            {
                DeviceId = "1",
                Name = "Mini rose",
                Moisture = 75,
                Temperature = 28.3f,
                Fertility = 3000,
                Brightness = 150,
                Battery = 100
            },
            new SensorDataModel
            {
                DeviceId = "2",
                Name = "Fig elastic",
                Moisture = 64,
                Temperature = 28.2f,
                Fertility = 2500,
                Brightness = 175,
                Battery = 100
            },
            new SensorDataModel
            {
                DeviceId = "3",
                Name = "Fig rubber",
                Moisture = 50,
                Temperature = 28.3f,
                Fertility = 2100,
                Brightness = 174,
                Battery = 99
            }
        };
    }
}

