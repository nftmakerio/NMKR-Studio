using System;

namespace NMKR.Shared.Classes.IPFS
{
    public class GetAllUploadedFilesResultClass
    {
        public string IpfsHash { get; set; }
        public string Name { get; set; }
        public long FileSize { get; set; }
        public DateTime Uploaded { get; set; }
    
    }
}
