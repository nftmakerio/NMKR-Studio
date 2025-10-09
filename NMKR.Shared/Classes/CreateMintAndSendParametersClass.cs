using NMKR.Shared.Model;
using System.Collections.Generic;
using NMKR.Shared.Classes.CardanoSerialisationLibClasses;

namespace NMKR.Shared.Classes
{
    public class CreateMintAndSendParametersClass
    {
        public long ttl;

        public Nftproject project { get; set; }
        public long BuyerHasToPayInLovelace { get; set; }
        public TxInAddressesClass[] utxofinal { get; set; }
        public string BuyerChangeAddress { get; set; }
        public List<Nftreservation> selectedreservations { get; set; }
        public string MintingcostsAddress { get; set; }
        public long Mintingcosts { get; set; }
        public long MinUtxo { get; set; }
        public Token[] MintTokens { get; set; }
      //  public string MetadataJsonFile { get; set; }
      //  public string OutFilename { get; set; }
      //  public string PolicyScriptFile { get; set; }
        public Nftprojectsadditionalpayout[] AdditionalPayouts { get; set; }
        public string SellerAddress { get; set; }
        public long SellerGetsInLovelace
        {
            get
            {
                return BuyerHasToPayInLovelace>0? BuyerHasToPayInLovelace - MinUtxo - Mintingcosts : 0;
            }
        }
        public long? Fees { get; set; }
        public string PaymentAddress { get; set; }
        public string MetadataResult { get; set; }
       // public string MintTokensString { get; set; }
        public bool IncludeMetadataHashOnly { get; set; }
        public string Createroyaltytokenaddress { get; set; }
        public double? Createroyaltytokenpercentage { get; set; }
        public string Burningaddress { get; set; }
        public List<PreparedpaymenttransactionsTokenprice> AdditionalPriceInTokens { get; set; }
        public PromotionClass Promotion { get; set; }
        public long Discount { get; set; }
        public long Stakerewards { get; set; }
        public long TokenRewards { get; set; }
        public string ReferenceAddress { get; set; }
        public string OptionalReceiverAddress { get; set; }
    }
}
