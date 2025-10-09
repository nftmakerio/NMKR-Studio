using System;
using System.Collections.Generic;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Extensions;
using NMKR.Shared.Model;

namespace NMKR.Shared.Classes
{

    public class SolanaProjectDetails
    {
        public string Symbol { get; set; }
        public string CollectionFamily { get; set; }
        public string Collectionimage { get; set; }
        public int? SellerFeeBasisPoints { get; set; }
    }
    public class AptosProjectDetails
    {
        public string CollectionImage { get; set; }
        public string CollectionName { get; set; }
    }
    public class NftProjectsDetails
    {
        public int Id { get; set; }
        public string Projectname { get; set; }
        public string Projecturl { get; set; }
        public string ProjectLogo { get; set; }
        public string State { get; set; }
        public long Free { get; set; }
        public long Sold { get; set; }
        public long Reserved { get; set; }
        public long Total { get; set; }
        public long Blocked { get; set; }
        public long TotalBlocked { get; set; }

        public long TotalTokens { get; set; }

        public long Error { get; set; }
        public long UnknownOrBurnedState { get; set; }
        public string Uid { get; set; }
        public long MaxTokenSupply { get; set; }
        public string Description { get; set; }
        public int AddressReservationTime { get; set; }
        public string PolicyId { get; set; }
        public bool EnableCrossSaleOnPaymentGateway { get; set; }
        public string AdaPayoutWalletAddress { get; set; }
        public string UsdcPayoutWalletAddress { get; set; }
        public bool EnableFiatPayments { get; set; }
        public DateTime? PaymentGatewaySaleStart { get; set; }
        public bool EnableDecentralPayments { get; set; }
        public DateTime? PolicyLocks { get; set; }
        public string RoyaltyAddress { get; set; }
        public float? RoyaltyPercent { get; set; }
        public long? Lockslot { get; set; }
        public bool DisableManualMintingbutton { get; set; }
        public bool DisableRandomSales { get; set; }
        public bool DisableSpecificSales { get; set; }
        public string TwitterHandle { get; set; }
        public NmkrAccountOptionsTypes NmkrAccountOptions { get; set; }
        public string CrossmintCollectiondId { get; set; }
        public DateTime Created { get; set; }

        public Blockchain[] Blockchains { get; set; }
        public SolanaProjectDetails SolanaProjectDetails { get; set; }
        public AptosProjectDetails AptosProjectDetails { get; set; }
        public string SolanaPayoutWalletAddress { get; set; }

        public NftProjectsDetails(EasynftprojectsContext db, Nftproject proj)
        {
            if (proj == null)
                return;

            Projectname = proj.Projectname;
            Projecturl = proj.Projecturl;
            ProjectLogo = !string.IsNullOrEmpty(proj.Projectlogo)
                ? GeneralConfigurationClass.IPFSGateway + proj.Projectlogo
                : null;
            Id = proj.Id;


            if (proj.Maxsupply == 1)
            {
                Free = Math.Max(0, proj.Free1 - (proj.Nftsblocked ?? 0));
                Reserved = proj.Reserved1;
                Sold = proj.Sold1;
                Error = proj.Error1;
                TotalTokens = proj.Total1;
                Blocked = (proj.Blocked1 ?? 0);
                TotalBlocked = proj.Nftsblocked ?? 0;
                UnknownOrBurnedState = TotalTokens - Free - Reserved - Error - Blocked - Sold - TotalBlocked;
                Total = proj.Total1;
            }
            else
            {
                Sold = proj.Tokenssold1;
                Reserved = proj.Tokensreserved1;
                Free = Math.Max(0,
                    proj.Totaltokens1 - proj.Tokenssold1 - proj.Tokensreserved1 - (proj.Nftsblocked ?? 0));
                Error = proj.Error1;
                TotalTokens = proj.Totaltokens1;
                TotalBlocked = proj.Nftsblocked ?? 0;
                UnknownOrBurnedState = 0;
                Total = proj.Total1;
            }



            Uid = proj.Uid;
            AddressReservationTime = proj.Expiretime;
            Description = proj.Description;
            MaxTokenSupply = proj.Maxsupply;
            PolicyId = proj.Policyid;
            EnableCrossSaleOnPaymentGateway = proj.Enablecrosssaleonpaywindow ?? false;
            AdaPayoutWalletAddress = proj.Customerwallet?.Walletaddress;
            UsdcPayoutWalletAddress = proj.Usdcwallet?.Walletaddress;
            SolanaPayoutWalletAddress=proj.Solanacustomerwallet?.Walletaddress;
            EnableFiatPayments = proj.Enablefiat && proj.Maxsupply == 1 &&
                                 GlobalFunctions.GetWebsiteSettingsBool(db, "fiatenabled");
            PaymentGatewaySaleStart = proj.Paymentgatewaysalestart?.ToUniversalTime();
            EnableDecentralPayments = proj.Enabledecentralpayments;
            PolicyLocks = proj.Policyexpire?.ToUniversalTime();
            RoyaltyAddress = proj.Hasroyality ? proj.Royalityaddress : null;
            RoyaltyPercent = proj.Hasroyality ? proj.Royalitypercent : null;
            Lockslot = proj.Lockslot;
            DisableManualMintingbutton = proj.Disablemanualmintingbutton;
            DisableRandomSales = proj.Disablerandomsales;
            DisableSpecificSales = proj.Disablespecificsales;
            TwitterHandle = proj.Twitterhandle;
            NmkrAccountOptions = proj.Nmkraccountoptions.ToEnum<NmkrAccountOptionsTypes>();
            CrossmintCollectiondId = proj.Crossmintcollectionid;
            Created = proj.Created ?? DateTime.Now;



            List<Blockchain> bc = new List<Blockchain>();
            if (proj.Enabledcoins.Contains(Coin.SOL.ToString()))
            {
                bc.Add(Blockchain.Solana);
                SolanaProjectDetails = new SolanaProjectDetails()
                {
                    CollectionFamily = proj.Solanacollectionfamily,
                    Collectionimage = proj.Solanacollectionimage,
                    SellerFeeBasisPoints = proj.SellerFeeBasisPoints,
                    Symbol = proj.Solanasymbol,
                };
            }

            if (proj.Enabledcoins.Contains(Coin.ADA.ToString()))
            {
                bc.Add(Blockchain.Cardano);
            }
            if (proj.Enabledcoins.Contains(Coin.APT.ToString()))
            {
                bc.Add(Blockchain.Aptos);
                AptosProjectDetails= new AptosProjectDetails()
                {
                    CollectionImage = proj.Aptoscollectionimage,
                    CollectionName = proj.Aptoscollectionname
                };
            }
            if (proj.Enabledcoins.Contains(Coin.BTC.ToString()))
            {
                bc.Add(Blockchain.Bitcoin);
            }

            Blockchains = bc.ToArray();
        }

      
    }

}
