using System.Linq;
using Windows.Globalization;

namespace FloraSense
{
    public class SettingsModel
    {
        public bool PollOnStart { get; set; }
        public Units TempUnits { get; set; }
        public string ThemeName { get; set; }
        public bool BgUpdate { get; set; }
        public uint BgUpdateRate { get; set; }

        public void Update(SettingsModel model)
        {
            PollOnStart = model.PollOnStart;
            TempUnits = model.TempUnits;
            ThemeName = model.ThemeName ?? Helpers.Helpers.Themes.First().Name;
            BgUpdate = model.BgUpdate;
            BgUpdateRate = model.BgUpdateRate;
        }

        public enum Units
        {
            C,
            F
        }
    }
}
