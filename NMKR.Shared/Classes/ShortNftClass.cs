using System;

namespace NMKR.Shared.Classes
{
    public class ShortNftClass
    {
        public int Id { get; set; }
        public string Tokenname { get; set; }
        public string Displayname { get; set; }
        public string Ipfshash { get; set; }
        public string State { get; set; }
        public Int64? Filesize { get; set; }
        public string Mimetype { get; set; }
        public bool Minted { get; set; }
        public string Fingerprint { get; set; }
        public DateTime? Selldate { get; set; }
        public DateTime? Created { get; set; }
        public string Metadataoverride { get; set; }
        public string MetadataoverrideCip68 { get; set; }
        public string Filename { get; set; }
    }
}
