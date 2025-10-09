using System.ComponentModel.DataAnnotations;
namespace NMKR.Shared.Classes
{
    public class ForgotPasswordClass
    {
        [Required]
        [StringLength(50, MinimumLength = 7)]
        [EmailAddress]
        public string Username { get; set; }

    }
}

