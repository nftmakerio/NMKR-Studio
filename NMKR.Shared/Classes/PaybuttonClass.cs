using System;
using System.Linq;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;

namespace NMKR.Shared.Classes
{
    public class PaybuttonClass
    {
        public PaybuttonClass(Nftproject project)
        {
            _nftproject = project;
            Style = 1;
            Color = 1;

            BlockchainString=Blockchain.Cardano.ToString();
            if (project.Enabledcoins.Contains(Coin.ADA.ToString()) == false && project.Enabledcoins.Contains(Coin.SOL.ToString()))
            {
                BlockchainString = Blockchain.Solana.ToString();
            }
            if (project.Enabledcoins.Contains(Coin.ADA.ToString()) == false && project.Enabledcoins.Contains(Coin.APT.ToString()))
            {
                BlockchainString = Blockchain.Aptos.ToString();
            }
            if (project.Enabledcoins.Contains(Coin.ADA.ToString()) == false && project.Enabledcoins.Contains(Coin.BTC.ToString()))
            {
                BlockchainString = Blockchain.Bitcoin.ToString();
            }

        }

       // public event Notify OnValueChanged;

        private readonly Nftproject _nftproject;
        private DateTime? _paymentGatewaySaleStart = null;
        private TimeSpan? _paymentGatewaySaleStartTime = null;
        public int Style { get; set; }

        public DateTime? PaymentGatewaySaleStart
        {
            get => _paymentGatewaySaleStart;
            set
            {
                _paymentGatewaySaleStart= value;
              //  OnValueChanged?.Invoke();
            }
        }

        public TimeSpan? PaymentGatewaySaleStartTime
        {
            get => _paymentGatewaySaleStartTime;
            set
            {
                _paymentGatewaySaleStartTime = value;
              //  OnValueChanged?.Invoke();

            }
        }

        public int Color { get; set; }
        public Pricelist? Pricelist { get; set; }
        public string BlockchainString { get; set; }
        public string PaywindowCode
        {
            get
            {
                return GetPaywindowCode();
            }
            set
            {

            }
        }

        public string PaywindowLink
        {
            get
            {
                return GetPaywindowLink();
            }
            set
            {

            }
        }
        public string Buttonlink
        {
            get
            {
                return GetButtonLink();
            }
            set
            {

            }
        }

        public string Custompropertycondition { get; set; }

        private string GetPaywindowLink()
        {
            if (Pricelist == null)
                return "";



            string paylink = GeneralConfigurationClass.Paywindowlink + "p=" + _nftproject.Uid.Replace("-", "") + "&c=" +
                             Pricelist.Countnftortoken;

            if (!string.IsNullOrEmpty(Custompropertycondition))
            {
                paylink+= "&cp=" + Custompropertycondition;
            }

            return paylink;
        }


        private string GetButtonLink()
        {
            string buttonlink = "https://studio.nmkr.io/images/buttons/paybutton_" + Style.ToString() + "_" +
                                Color.ToString() + ".svg";
            return buttonlink;
        }
        private string GetPaywindowCode()
        {
            if (Pricelist == null)
                return "";
            using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);

            var paywindow = (from a in db.Paybuttoncodes
                where a.Id == 1
                select a).FirstOrDefault();

            if (paywindow==null)
               return "";

            string res= paywindow.Code.Replace("{paylink}", GetPaywindowLink());
            res = res.Replace("{buttonlink}", GetButtonLink());
            return res;
        }
    }
}
