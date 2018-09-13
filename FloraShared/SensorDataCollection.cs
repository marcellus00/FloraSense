using System.Collections.ObjectModel;
using System.Linq;

namespace FloraSense
{
    public class SensorDataCollection : ObservableCollection<SensorDataModel>
    {
        public void RemoveInvalid()
        {
            foreach (var sensorDataModel in Items.ToList())
            {
                if (!sensorDataModel.IsValid)
                    Remove(sensorDataModel);
            }
        }
    }
}
