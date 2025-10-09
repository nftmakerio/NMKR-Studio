using Newtonsoft.Json;

namespace NMKR.Shared.Functions.Metadata
{
    public class CheckMetadataForCip68Fields : ICheckMetadataFields
    {
        public CheckMetadataFieldsResult CheckMetadata(string metadata, string policyid, string tokenname,
            bool checkPolicyId, bool checkTokenName)
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


            return result;
        }
    }
}
