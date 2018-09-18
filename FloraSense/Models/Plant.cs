using System;

namespace FloraSense.Data
{
    [Serializable]
    public class Plant
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }

        public MinMaxInt MoistureRange { get; set; }
        public MinMaxInt FertilityRange { get; set; }
        public MinMaxInt BrightnessRange { get; set; }
        public MinMaxFloat TemperatureRange { get; set; }

        public Plant()
        {
            MoistureRange = new MinMaxInt(0, 100);
            FertilityRange = new MinMaxInt(0, 9000);
            BrightnessRange = new MinMaxInt(0, 60000);
            TemperatureRange = new MinMaxFloat(0f, 100f);
        }
    }
}
