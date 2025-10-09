using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NMKR.Shared.Classes.Iagon;

namespace NMKR.Shared.Classes
{
    public class EditNftClass
    {
        [Required]
        [StringLength(32, MinimumLength = 1)]
        [RegularExpression(@"^[^\u0022]+$|^$", ErrorMessage = "Quotation marks are not allowed")]

        public string Tokenname { get; set; }

        [StringLength(255)]
        [RegularExpression(@"^[^\u0022]+$|^$", ErrorMessage = "Quotation marks are not allowed")]

        public string Description { get; set; }
        public string Displayname { get; set; }
        public float? Price { get; set; }
        public float? PriceSolana { get; set; }
        public float? PriceAptos { get; set; }
        public long Sold { get; set; }
        public long Reserved { get; set; }
        public long Error { get; set; }
        public long Multiplier { get; set; }
        public int? Testmarker { get; set; }
        public string Uploadsource { get; set; }
        public int SelectedPrice { get; set; } = 0;

        public List<uploadclass> ipfshashes = new();
        public string State { get; set; }
        public string MetadataOverride{ get; set; }
        public string MetadataOverrideCip68 { get; set; }
    }
    public class uploadclass
    {
        public bool deleted { get; set; } = false;
        public long filesize { get; set; }
        public string ipfshash { get; set; }
        public string mimetype { get; set; }
        public bool result { get; set; }
        [StringLength(255)]
        [RegularExpression(@"^[^\u0022]+$|^$", ErrorMessage = "Quotation marks are not allowed")]
        public string detaildata { get; set; }
        [Required]
        [StringLength(30, MinimumLength = 1)]
        [RegularExpression(@"^[^\u0022]+$|^$", ErrorMessage = "Quotation marks are not allowed")]
        public string name { get; set; }
        public int Id { get; set; }
        public string filename { get; set; }

        public List<MetadataPlaceholderClass> placeholder = new();
        public IagonUploadResultClass iagon { get; set; }
    }
}
