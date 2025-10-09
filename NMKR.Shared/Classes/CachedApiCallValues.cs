namespace NMKR.Shared.Classes
{
    public class CachedApiCallValues
    {
        public string Apikey { get; set; }
        public string Apiparameter { get; set; }
        public string Apifunction { get; set; }

        public CachedApiCallValues(string apikey, string apiparameter, string apifunction)
        {
            Apikey = apikey;
            Apiparameter = apiparameter;
            Apifunction = apifunction;
        }

        public string GetRedisString()
        {
            return Apifunction + "_" + Apikey + "_" + Apiparameter;
        }
    }
}
