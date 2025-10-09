using System.Collections.Generic;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosJpgStoreMetadataClass
    {
        [JsonProperty("tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string TxHash { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, JpgStoreMetadatumValue> Metadata { get; set; }
    }

    public partial class JpgStoreMetadatumClass
    {
        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

    }
    public partial struct JpgStoreMetadatumValue
    {
        public JpgStoreMetadatumClass JpgStoreMetadatumClass;
        public string String;

        public static implicit operator JpgStoreMetadatumValue(JpgStoreMetadatumClass jpgStoreMetadatumClass) => new JpgStoreMetadatumValue { JpgStoreMetadatumClass = jpgStoreMetadatumClass };
        public static implicit operator JpgStoreMetadatumValue(string String) => new JpgStoreMetadatumValue { String = String };
    }

}
