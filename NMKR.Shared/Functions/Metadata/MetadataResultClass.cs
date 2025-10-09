namespace NMKR.Shared.Functions.Metadata
{
    public enum MetadataSourceTypes
    {
        Cip25,
        Cip68,
        Unknown
    }
    public class MetadataResultClass
    {
        public string MetadataCip25 { get; set; }
        public string MetadataCip68 { get; set; }

        public string Metadata
        {
            get
            {
                if (string.IsNullOrEmpty(MetadataCip68))
                    return MetadataCip25;

                return MetadataCip68;
            }
        }

        public MetadataSourceTypes SourceType { get; set; }
        public string Error { get; set; }
        public string MetadataSolana { get; set; }
        public string MetadataAptos { get; set; }
        public string MetadataBitcoin { get; set; }
        public MetadataResultClass()
        {
            SourceType = MetadataSourceTypes.Unknown;
            MetadataCip68 = "";
            MetadataCip25 = "";
        }

       
    }
}
