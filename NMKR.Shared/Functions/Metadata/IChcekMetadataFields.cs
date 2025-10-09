namespace NMKR.Shared.Functions.Metadata
{
    public interface ICheckMetadataFields
    {
        public CheckMetadataFieldsResult CheckMetadata(string metadata, string policyid, string tokenname,
            bool checkPolicyId, bool checkTokenName);
    }
}
