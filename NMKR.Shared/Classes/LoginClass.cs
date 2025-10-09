using System;
using System.ComponentModel.DataAnnotations;

namespace NMKR.Shared.Classes
{
    public class LoginClass
    {
        public event EventHandler Change;
        protected virtual void OnChanged(EventArgs e)
        {
            EventHandler handler = Change;
            handler?.Invoke(this, e);
        }

        private string _username;
        private string _password;

        [Required(ErrorMessage = "Enter your E-Mail Address")]
        [StringLength(50, MinimumLength = 7)]
            [EmailAddress]
            public string Username
            {
                get { return _username; }
                set
                {
                    _username = value;
                    OnChanged(EventArgs.Empty);
                }
            }

            [Required(ErrorMessage = "Enter your Password")]
            [StringLength(30, MinimumLength = 6)]
            public string Password
            {
                get { return _password; }
                set
                {
                    _password = value;
                    OnChanged(EventArgs.Empty);
                }
            }

           // [RegularExpression("True", ErrorMessage = "You must agree to the Terms")]
            public bool Agreement { get; set; }
}
    }

