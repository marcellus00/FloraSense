using System.Collections.Generic;

namespace FloraSense
{
    public class TemperatureUnits : List<string>
    {
        public TemperatureUnits()
        {
            Add("°C");
            Add("°F");
        }
    }
}
