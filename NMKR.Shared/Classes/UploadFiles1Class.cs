using System.ComponentModel.DataAnnotations;

namespace NMKR.Shared.Classes
{
    public class UploadFiles1Class
    {
        [Required]
        [StringLength(32, MinimumLength = 1)]
        //   [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Only Alphabets and Numbers allowed. No Spaces")]
        [RegularExpression(@"^[^\u0022]+$|^$", ErrorMessage = "Quotation marks are not allowed")]
        public string Tokenname  { get; set; }
        [StringLength(64)]
        [RegularExpression(@"^[^\u0022]+$|^$", ErrorMessage = "Quotation marks are not allowed")]

        public string Description { get; set; }
        public string Displayname { get; set; }

        public int SelectedPrice { get; set; } = 0;
        public float? Price { get; set; }
        public float? Pricesolana { get; set; }
        public float? Priceaptos { get; set; }

        public string MetadataOverride { get; set; }
    }
}
