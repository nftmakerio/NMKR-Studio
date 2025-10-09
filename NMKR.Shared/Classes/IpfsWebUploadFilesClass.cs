namespace NMKR.Shared.Classes
{
    public class IpfsWebUploadFilesClass
    {
        public string ImageHash { get; set; }
        public string MimeType { get; set; }
        public string UriWithGateway =>string.IsNullOrEmpty(ImageHash) ? "" : ImageHash.StartsWith("http") ? ImageHash : GeneralConfigurationClass.IPFSGateway + ImageHash;
    }
}
