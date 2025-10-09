namespace NMKR.Shared.Classes
{
    public class WhiteLabelStoreSettingsClass
    {
        public string StoreName { get; set; }
        public StoreSettings[] Settings { get; set; }
    }

    public class StoreSettings
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
