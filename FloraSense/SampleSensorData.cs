using System.Collections.ObjectModel;

namespace FloraSense
{
    public class SampleSensorData
    {
        public ObservableCollection<SensorDataDisplay> KnownDevices { get; set; }

        public SampleSensorData()
        {
            KnownDevices = new ObservableCollection<SensorDataDisplay>
            {
                new SensorDataDisplay
                {
                    Name = "Mini rose",
                    Moisture = 75,
                    Temperature = 28.3f,
                    Fertility = 3000,
                    Brightness = 150,
                    Battery = 100
                },
                new SensorDataDisplay
                {
                    Name = "Fig elastic",
                    Moisture = 64,
                    Temperature = 28.2f,
                    Fertility = 2500,
                    Brightness = 175,
                    Battery = 100
                },
                new SensorDataDisplay
                {
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
}
