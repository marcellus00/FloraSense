using MiFlora;

namespace FloraSense
{
    public class SensorDataModel
    {
        public string Name { get; set; }
        public int Moisture { get; set; }
        public ushort Fertility { get; set; }
        public uint Brightness { get; set; }
        public float Temperature { get; set; }
        public int Battery { get; set; }
        
        public void Update(SensorData sensorData)
        {
            Name = sensorData.Name;
            Moisture = sensorData.Moisture;
            Fertility = sensorData.Fertility;
            Brightness = sensorData.Brightness;
            Temperature = sensorData.Temperature;
            Battery = sensorData.Battery;
        }
    }
}
