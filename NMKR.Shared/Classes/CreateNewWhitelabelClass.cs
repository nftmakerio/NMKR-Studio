using System.ComponentModel.DataAnnotations;
using NMKR.Shared.Model;

namespace NMKR.Shared.Classes
{
    public class CreateNewWhitelabelClass
    {
        public Whitelabelstoresetting[] whitelabelstoresettings;

        [Required]
        [StringLength(64, MinimumLength = 1)]
        public string Projectname { get; set; }
        public string ProjectLogo { get; set; }

        [StringLength(100)]
        public string Description { get; set; }
        public string Twitterhandle { get; set; }
        public float MarketplaceWhitelabelFee { get; set; }
        public int WalletId { get; set; }
        [Required]
        [StringLength(255, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z]+[a-z0-9A-Z]*$", ErrorMessage = "Use Characters and numbers only")]
        public string Projecturl { get; set; }
    }
}
