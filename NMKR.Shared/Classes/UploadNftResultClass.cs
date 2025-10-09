namespace NMKR.Shared.Classes
{
    public class UploadNftResultClass
    {
        public int NftId { get; set; }
        public string NftUid { get; set; }
        public string IpfsHashMainnft { get; set; }
        public string[] IpfsHashSubfiles { get; set; }
        public string Metadata { get; set; }
        public string AssetId { get; set; }
        public string MetadataAptos { get; set; }

        public string MetadataSolana { get; set; }

    }

}
