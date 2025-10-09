using System;

namespace NMKR.Shared.Classes
{
    public class CreateNewProjectResultClass
    {
        public int ProjectId { get; set; }
        public string Metadata { get; set; }
        public string PolicyId { get; set; }
        public string PolicyScript { get; set; }
        public DateTime? PolicyExpiration { get; set; }
        public string Uid { get; set; }
        public string MetadataTemplateAptos { get; set; }
        public string MetadataTemplateSolana { get; set; }
        public string EnabledCoins { get; set; }
        public string SolanaUpdateAuthority { get; set; }
        public string AptosCollectionAddress { get; set; }
        public DateTime? Created { get; set; }
    }
}
