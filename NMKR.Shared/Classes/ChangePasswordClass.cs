using System;
using System.ComponentModel.DataAnnotations;

namespace NMKR.Shared.Classes
{
    public class ChangePasswordClass
    {
        public event EventHandler Change;
        protected virtual void OnChanged(EventArgs e)
        {
            EventHandler handler = Change;
            handler?.Invoke(this, e);
        }


        private string _oldpassword;
        private string _password;
        private string _password2;


        [Required]
        [StringLength(30, MinimumLength = 8)]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must have at least 3 of the 4 following things: uppercase letters, lowercase letters, numbers and special characters (e.g.! @ # $% ^ & *)")]

        public string OldPassword
        {
            get { return _oldpassword; }
            set
            {
                _oldpassword = value;
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
        [Compare("Password", ErrorMessage = "The Confirmation Password must be equal")]
        public string Password2
        {
            get { return _password2; }
            set
            {
                _password2 = value;
                OnChanged(EventArgs.Empty);
            }
        }
    }
}


