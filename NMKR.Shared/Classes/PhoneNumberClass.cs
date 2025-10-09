using System.ComponentModel.DataAnnotations;

namespace NMKR.Shared.Classes
{
    public class PhoneNumberClass
    {
        [Required]
        [StringLength(20, MinimumLength = 7)]
        [DataType(DataType.PhoneNumber)]
        public string MobileNumber { get; set; }
    }
}