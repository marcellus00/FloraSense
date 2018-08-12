namespace FloraSense
{
    public class SettingsModel
    {
        public bool PollOnStart { get; set; }
        public Units Temp { get; set; }

        public enum Units
        {
            C,
            F
        }
    }
}
