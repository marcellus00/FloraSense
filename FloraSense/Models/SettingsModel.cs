using FloraSense.Data;
using System.Collections.Generic;
using System.Linq;
using Windows.Globalization;

namespace FloraSense
{
    public class SettingsModel
    {
        public string Language { get; set; } = string.Empty;
        public bool PollOnStart { get; set; }
        public Units TempUnits { get; set; }
        public string ThemeName { get; set; }
        public bool BgUpdate { get; set; }
        public uint BgUpdateRate { get; set; }

        public List<Plant> Plants { get; set; } = new List<Plant>();

        public void Update(SettingsModel model)
        {
            PollOnStart = model.PollOnStart;
            TempUnits = model.TempUnits;
            ThemeName = model.ThemeName ?? Helpers.Helpers.Themes.First().Name;
            BgUpdate = model.BgUpdate;
            BgUpdateRate = model.BgUpdateRate;
            Language = model.Language;
        }
        
        public void RefreshLanguage()
        {
            if(Language != null)
                ApplicationLanguages.PrimaryLanguageOverride = Language;
        }

        public enum Units
        {
            C,
            F
        }
    }
}
