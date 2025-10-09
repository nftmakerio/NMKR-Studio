using System.Collections.Generic;

namespace NMKR.Shared.Classes
{
    public class CreateMetadataClass
    {
        public string Template { get; set; }
        public string TemplateWithoutFiles { get; set; }
        public string Filessection { get; set; }
        public string CompleteFilesSection { get; set; }
        public string FinalTemplate { get; set; }

        public List<string> Files = new();
    }
}
