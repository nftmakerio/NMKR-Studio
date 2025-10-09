using Newtonsoft.Json;

namespace NMKR.Shared.Functions.Metadata
{
    public class CheckMetadataFieldsResult
    {
        public string FormatedMetadata;

        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class CheckMetadataForCip25Fields : ICheckMetadataFields
    {
        public CheckMetadataFieldsResult CheckMetadata(string metadata, string policyid, string tokenname, bool checkPolicyId, bool checkTokenName)
        {
            var result = new CheckMetadataFieldsResult
            {
                IsValid = true,
                ErrorMessage = ""
            };
            GlobalFunctions.IsValidJson(metadata, out result.FormatedMetadata);
            if (string.IsNullOrEmpty(result.FormatedMetadata))
            {
                result.IsValid = false;
                result.ErrorMessage = "Metadata is not a valid JSON File";
                return result;
            }

            var obj = JsonConvert.DeserializeObject(metadata);
            metadata = JsonConvert.SerializeObject(obj, Formatting.None);
           
            if (string.IsNullOrEmpty(metadata))
            {
                result.IsValid = false;
                result.ErrorMessage = "Metadata is not a valid JSON File";
                return result;
            }

            if (checkPolicyId)
            {
                if (!metadata.Contains("<policy_id>") &&
                    (!metadata.Contains(policyid) || string.IsNullOrEmpty(policyid)))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Metadata does not contain <policy_id> field or the policy id";
                    return result;
                }
               
            }

            if (checkTokenName)
            {
                if (!metadata.Contains("<asset_name>") && 
                    (!metadata.Contains(tokenname) || string.IsNullOrEmpty(tokenname)))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Metadata does not contain <asset_name> field or the tokenname/assetname";
                    return result;
                }
            }

            if (!metadata.Contains("\"721\"") && !metadata.Contains("\"20\""))
            {
                result.IsValid = false;
                result.ErrorMessage = "Metadata does not contain 721 or 20 field";
                return result;
            }

            if (!GlobalFunctions.CheckMetaDataLength(metadata))
            {
                result.IsValid = false;
                result.ErrorMessage = "Parts of the Metadata exceeds 64 characters.Please check Metadata and trim to data fields to max. 64 chars";
                return result;
            }

            return result;
        }
    }
}
