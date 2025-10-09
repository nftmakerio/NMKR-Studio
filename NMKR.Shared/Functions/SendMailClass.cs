using System;
using System.Net.Mail;
using System.Linq;
using System.Collections.Generic;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;

namespace NMKR.Shared.Classes
{
    public enum ConfirmationTypes
    {
        ConfirmNewWalletAddress,
        ConfirmEMail,
        ConfirmPayout,
        ForgotPassword,
        ChangePassword,
        SendMailOnLogin,
        SendMailOnLoginFailure,
        SendMailOnPayout,
        UnlockAccount
    }

    public  class SendMailClass
    {
        private  readonly EasynftprojectsContext _db = new(GlobalFunctions.optionsBuilder.Options);

        public  void SendConfirmationMail(ConfirmationTypes confirmationType, string recipient, Dictionary<string, string> myDictionary, int userid)
        {
               var e = (from a in _db.Emailtemplates
                        where a.Templatename == confirmationType.ToString()
                        select a).AsNoTracking().FirstOrDefault();
            
            if (e == null)
                return;

            var user = (from a in _db.Customers
                    .Include(a=>a.Country).AsSplitQuery()
                        where a.Id == userid
                select a).AsNoTracking().FirstOrDefault();
            if (user == null)
                return;



            string testnet = "";
            if (!GlobalFunctions.IsMainnet())
                testnet = "."+GeneralConfigurationClass.EnvironmentName.ToLower();

            switch (confirmationType)
            {
                case ConfirmationTypes.ConfirmNewWalletAddress:
                    e.Htmlemail = e.Htmlemail.Replace("{confirmationlink}",
                        "https://studio" + testnet + ".nmkr.io/confirm/wallet/{userid}/{confirmationcode}");
                    break;
                case ConfirmationTypes.ConfirmPayout:
                    e.Htmlemail = e.Htmlemail.Replace("{confirmationlink}",
                        "https://studio" + testnet + ".nmkr.io/confirm/payout/{userid}/{walletid}/{confirmationcode}");
                    break;
                case ConfirmationTypes.ForgotPassword:
                    e.Htmlemail = e.Htmlemail.Replace("{confirmationlink}",
                        "https://studio" + testnet + ".nmkr.io/confirm/forgotpassword/{userid}/{confirmationcode}");
                    break;
                case ConfirmationTypes.ChangePassword:
                    e.Htmlemail = e.Htmlemail.Replace("{confirmationlink}",
                        "https://studio" + testnet + ".nmkr.io/confirm/passwordchange/{userid}/{confirmationcode}");
                    break;
                case ConfirmationTypes.ConfirmEMail:
                    e.Htmlemail = e.Htmlemail.Replace("{confirmationlink}",
                        "https://studio" + testnet + ".nmkr.io/confirm/register/{userid}/{confirmationcode}");
                    break;
                case ConfirmationTypes.UnlockAccount:
                    e.Htmlemail = e.Htmlemail.Replace("{confirmationlink}",
                        "https://studio" + testnet + ".nmkr.io/confirm/unlock/{userid}/{confirmationcode}");
                    break;
            }


            e.Htmlemail = e.Htmlemail.Replace("{firstname}", user.Firstname, StringComparison.OrdinalIgnoreCase);
            e.Textemail = e.Textemail.Replace("{firstname}", user.Firstname, StringComparison.OrdinalIgnoreCase);

            e.Htmlemail = e.Htmlemail.Replace("{lastname}", user.Lastname, StringComparison.OrdinalIgnoreCase);
            e.Textemail = e.Textemail.Replace("{lastname}", user.Lastname, StringComparison.OrdinalIgnoreCase);

            e.Htmlemail = e.Htmlemail.Replace("{company}", user.Company, StringComparison.OrdinalIgnoreCase);
            e.Textemail = e.Textemail.Replace("{company}", user.Company, StringComparison.OrdinalIgnoreCase);

            e.Htmlemail = e.Htmlemail.Replace("{street}", user.Street, StringComparison.OrdinalIgnoreCase);
            e.Textemail = e.Textemail.Replace("{street}", user.Street, StringComparison.OrdinalIgnoreCase);

            e.Htmlemail = e.Htmlemail.Replace("{city}", user.City, StringComparison.OrdinalIgnoreCase);
            e.Textemail = e.Textemail.Replace("{city}", user.City, StringComparison.OrdinalIgnoreCase);

            e.Htmlemail = e.Htmlemail.Replace("{zip}", user.Zip, StringComparison.OrdinalIgnoreCase);
            e.Textemail = e.Textemail.Replace("{zip}", user.Zip, StringComparison.OrdinalIgnoreCase);

            e.Htmlemail = e.Htmlemail.Replace("{country}", user.Country.Nicename, StringComparison.OrdinalIgnoreCase);
            e.Textemail = e.Textemail.Replace("{country}", user.Country.Nicename, StringComparison.OrdinalIgnoreCase);

            e.Htmlemail = e.Htmlemail.Replace("{userid}", user.Id.ToString(), StringComparison.OrdinalIgnoreCase);
            e.Textemail = e.Textemail.Replace("{userid}", user.Id.ToString(), StringComparison.OrdinalIgnoreCase);

            if (myDictionary != null)
            {
                foreach (var a in myDictionary)
                {
                    e.Htmlemail = e.Htmlemail.Replace(a.Key, a.Value, StringComparison.OrdinalIgnoreCase);
                    e.Textemail = e.Textemail.Replace(a.Key, a.Value, StringComparison.OrdinalIgnoreCase);
                }
            }

            SendEmails(recipient, e.Htmlemail, e.Emailsubject, true);
        }

        private bool SendEmails(string toAddresss, string email, string subject, bool isHTML)
        {
            

            SmtpClient smtpClient = new(GeneralConfigurationClass.AWSEmailServer) { Credentials = new System.Net.NetworkCredential(GeneralConfigurationClass.AWSEmailUsername, GeneralConfigurationClass.AWSEmailPassword) };
            MailMessage message = new();

            try
            {
                MailAddress fromAddress = new("info@nmkr.io", "NMKR Studio");
              //    smtpClient.Port = 465;
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                message.From = fromAddress;
                message.To.Add(toAddresss);
             //   message.Bcc.Add("sascha@tobler.de");
               
                message.Subject = subject;
                message.IsBodyHtml = isHTML;
                message.Body = email;

                // Send SMTP mail

                smtpClient.SendCompleted += (s, e) =>
                {
                    SmtpClient callbackClient = s as SmtpClient;
                    MailMessage callbackMailMessage = e.UserState as MailMessage;
                    if (callbackClient != null) callbackClient.Dispose();
                    if (callbackMailMessage != null) callbackMailMessage.Dispose();
                };

               smtpClient.SendAsync(message, message);

             //   smtpClient.Send(message);

              
            }

            catch 
            {
                return false;
            }
            return true;
        }

        public bool SendNotificationMail(string receiver, string message)
        {
            return SendEmails(receiver, message, "Payment Transaction Notification", false);
        }
    }
}
