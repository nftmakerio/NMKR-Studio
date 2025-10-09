using System;

namespace NMKR.Shared.Classes.CustomerData
{
    public class CustomersList
    {
        public int Id
        { get; set; }
        public string Firstname
        { get; set; }
        public string Lastname
        { get; set; }
        public string Company
        { get; set; }
        public string Email
        { get; set; }
        public DateTime Created
        { get; set; }
        public string State
        { get; set; }
        public float Newpurchasedmints
        { get; set; }
        public long UsedStorage
        { get; set; }
        public string Kycstatus
        { get; set; }
        public bool Internalaccount
        { get; set; }
        public string Adaaddress
        { get; set; }
        public string Solanapublickey
        { get; set; }
        public string Aptosaddress
        { get; set; }
        public int? SubcustomerId
        {
            get;
            set;
        }
    }
}
