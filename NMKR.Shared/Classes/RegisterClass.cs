using System;
using System.ComponentModel.DataAnnotations;

namespace NMKR.Shared.Classes
{
    public class RegisterClass
    {
        public event EventHandler Change;
        protected virtual void OnChanged(EventArgs e)
        {
            EventHandler handler = Change;
            handler?.Invoke(this, e);
        }


        private string _email;
        private string _password;
        private string _password2;


        [Required]
        [StringLength(50, MinimumLength = 7)]
        [EmailAddress]
        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;
                OnChanged(EventArgs.Empty);
            }
        }

        [Required]
        [StringLength(30, MinimumLength = 8)]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must have at least 3 of the 4 following things: uppercase letters, lowercase letters, numbers and special characters (e.g.! @ # $% ^ & *)")]

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnChanged(EventArgs.Empty);
            }
        }

        [Required]
        [StringLength(30, MinimumLength = 8)]
        [Compare("Password",ErrorMessage ="The Confirmation Password must be equal")]
        public string Password2
        {
            get { return _password2; }
            set
            {
                _password2 = value;
                OnChanged(EventArgs.Empty);
            }
        }

        [RegularExpression("True", ErrorMessage = "You must agree to the Terms")]
        public bool Agreement { get; set; }

        public bool Newsletter { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Firstname { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2)] 
        public string Lastname { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2)] 
        public string Street { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Zip { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string City { get; set; }
        public string UstId { get; set; }
        public int CountryId { get; set; } = 1;

        [StringLength(100)]
        public string? Company { get; set; }

        //[Required(ErrorMessage = "Sicherheitsabfrage wird benötigt")]
        //[StringLength(5, ErrorMessage = "Sicherheitsabfrage ist zu lang oder zu kurz (5 Zeichen).", MinimumLength = 5)]
        //public string Captcha { get; set; }

    }
}


