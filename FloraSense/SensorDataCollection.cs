using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FloraSense
{
    public class SensorDataCollection : ObservableCollection<SensorDataDisplay>
    {
        public new List<SensorDataDisplay> Items => base.Items.ToList();
    }
}
