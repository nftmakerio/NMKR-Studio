using System.Security.Cryptography;
using NMKR.Shared.Functions;

namespace NMKR.Shared.Classes
{
    public class EditNotificationsClass
    {
        public EditNotificationsClass(EditNotificationsClass old)
        {
            Id = old.Id;
            Notificationtype=old.Notificationtype;
            Address=old.Address;
            State=old.State;
            Secret=old.Secret;
            Uid = GlobalFunctions.GetGuid();
            Deleted = false;
        }

        public EditNotificationsClass()
        {
            string apikey = GlobalFunctions.GetGuid();
            Secret = HashClass.GetHash(SHA256.Create(), apikey);
            Uid= GlobalFunctions.GetGuid();
            Deleted = false;
        }


        public int? Id { get; set; }
        public PaymentTransactionNotificationTypes Notificationtype { get; set; }
        public string Address { get; set; } = "";
        public bool State { get; set; }
        public string Secret { get; set; }
        public string Uid { get; set; }
        public bool Deleted { get; set; }
    }
}
