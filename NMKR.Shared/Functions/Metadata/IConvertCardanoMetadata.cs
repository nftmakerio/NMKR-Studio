using System.Collections.Generic;

namespace NMKR.Shared.Functions.Metadata
{
    public interface IConvertCardanoMetadata
    {
        public string ConvertCip25CardanoMetadata(string cardanoMetadataString, string symbol="", string collection = "", int? sellerFeeBasisPoints = null, List<string> creators = null);
    }
}
