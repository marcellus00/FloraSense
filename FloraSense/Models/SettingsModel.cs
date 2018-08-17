using System.Linq;
using FloraSense.Helpers;

namespace FloraSense
{
    public class SettingsModel
    {
        public bool PollOnStart { get; set; }
        public Units TempUnits { get; set; }
        public string ThemeName { get; set; }

        public void Update(SettingsModel model)
        {
            PollOnStart = model.PollOnStart;
            TempUnits = model.TempUnits;
            ThemeName = model.ThemeName ?? Extensions.Themes.First().Name;
        }

        public enum Units
        {
            C,
            F
        }
    }
}
