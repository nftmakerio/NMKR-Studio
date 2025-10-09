using System.Collections.Generic;

namespace NMKR.Shared.Classes
{
    public class DeleteAllNftsDetail
    {
        public string NftUid { get; set; }
        public string NftName { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class DeleteAllNftsResultClass
    {
        public int SuccessfullyDeleted { get; set; }

        public int NotDeleted
        {
            get
            {
                return ErrorDetails.Count;
            }
        }

        public List<DeleteAllNftsDetail> ErrorDetails { get; set; } = new List<DeleteAllNftsDetail>();

    }
}
