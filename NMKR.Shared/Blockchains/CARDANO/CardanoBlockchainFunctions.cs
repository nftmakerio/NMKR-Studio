using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CardanoSharp.Wallet.Enums;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Blockfrost;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using StackExchange.Redis;

namespace NMKR.Shared.Blockchains.Cardano
{
    public class CardanoBlockchainFunctions : IBlockchainFunctions
    {
        public async Task<ulong> GetWalletBalanceAsync(string walletAddress)
        {
            var utxo=await ConsoleCommand.GetNewUtxoAsync(walletAddress);
            return (ulong)utxo.LovelaceSummary;
        }
        public CreateNewPaymentAddressClass CreateNewWallet()
        {
            return ConsoleCommand.CreateNewPaymentAddress(GlobalFunctions.IsMainnet(), true);
        }


        public Task<BuildTransactionClass> MintAndSend(Nft nft, Nftproject project, string receiveraddress, BlockchainKeysClass paywallet,
            BuildTransactionClass buildtransaction)
        {
            throw new NotImplementedException();
        }

        public Task<BuildTransactionClass> SendAllCoinsAndTokens(ulong utxo, CreateNewPaymentAddressClass wallet, string receiveraddress,
            BuildTransactionClass bt, string sendbackmessage)
        {
            throw new NotImplementedException();
        }

        public async Task<GenericTransaction> GetTransactionInformation(string txhash)
        {
            if (string.IsNullOrEmpty(txhash)) return null;

            var txinfo = (await ConsoleCommand.GetTransactionAsync(txhash)).ToGenericTransaction();
            return txinfo;
        }

        public bool CheckForValidAddress(string address, bool mainnet)
        {
            return ConsoleCommand.IsValidCardanoAddress(address, mainnet);
        }

        public Task<ICreateCollectionResult> CreateCollection(ICreateCollection createCollection)
        {
            throw new NotImplementedException();
        }

        public async Task<string> CreateCollectionUri(CreateCollectionParameterClass createCollectionParameterClass)
        {
            // Not needed for Cardano
            return "";
        }

        public Task<string> CreateCollectionAsync(CollectionClass collectionClass)
        {
            throw new NotImplementedException();
        }

        public BlockchainKeysClass ConvertToBlockchainKeysClass(Nftproject project)
        {
            throw new NotImplementedException();
        }

        public BlockchainKeysClass ConvertToBlockchainKeysClass(Burnigendpoint address)
        {
            throw new NotImplementedException();
        }

        public BlockchainKeysClass ConvertToBlockchainKeysClass(Adminmintandsendaddress address)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionClass> CreateNewCollectionClass(Nftproject project, Adminmintandsendaddress paywallet)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionClass> CreateNewCollectionClass(Nftproject project)
        {
            throw new NotImplementedException();
        }

        public Task<BuildTransactionClass> SendCoins(Nftaddress nftaddress, TxOutClass[] addresses, BuildTransactionClass bt)
        {
            throw new NotImplementedException();
        }

        public Task<BuildTransactionClass> SendCoins(CreateNewPaymentAddressClass wallet, TxOutClass[] addresses, BuildTransactionClass bt)
        {
            throw new NotImplementedException();
        }

        public async Task<BuildTransactionClass> SendCoins(Adminmintandsendaddress wallet, TxOutClass[] addresses,
            BuildTransactionClass bt)
        {
            var paywallet = ConvertToBlockchainKeysClass(wallet);
            return await SendCoins(new CreateNewPaymentAddressClass()
            {
                Address = paywallet.Address,
                SeedPhrase = paywallet.Seed,
                privateskey = paywallet.SecretKey,
                Blockchain = Blockchain.Cardano,
                privatevkey = paywallet.PublicKey
            }, addresses, bt);
        }

        public async Task<AssetsAssociatedWithAccount[]> GetAllAssetsInWalletAsync(IConnectionMultiplexer redis, string address)
        {
            return await ConsoleCommand.GetAllAssetsInWalletAsync(redis, address);
        }

        public BlockfrostAssetClass GetAsset(Nftproject project, Nft nft1)
        {
            return ConsoleCommand.GetAssetFromBlockchain(nft1, project);
        }

        public string GetMetadataFromCip25Metadata(string cip25metadata, Nftproject project)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetLastSenderAddressAsync(string addressOrHash)
        {
            return await ConsoleCommand.GetSenderAsync(addressOrHash); // This is not an address but an txhash
        }

        public Task<BuildTransactionClass> SendAllCoinsAndTokensFromNftaddress(Nftaddress address, string receiveraddress, BuildTransactionClass bt,
            string sendbackmessage)
        {
            throw new NotImplementedException();
        }

        public Task<BuildTransactionClass> MintFromNftAddressCoreAsync(ulong lamportsOnAddress, Nftaddress address, string receiveraddress,
            Pricelistdiscount discount, Nftprojectsadditionalpayout[] additionalPayoutWallets, BuildTransactionClass bt)
        {
            throw new NotImplementedException();
        }

        public async Task<CheckCustomerAddressAddressesClass[]> GetCustomersAsync(EasynftprojectsContext db, CancellationToken cancellationToken)
        {
            var cust = await(from a in db.Customers
                    .Include(a => a.Loggedinhashes)
                    .Include(a => a.Defaultsettings)
                    .AsSplitQuery()
                where a.State == "active" && a.Adaaddress != "" && a.Adaaddress != null && ((a.Loggedinhashes.Any() && a.Loggedinhashes.OrderByDescending(x => x.Id).First().Lastlifesign > DateTime.Now.AddMinutes(-30)) ||
                    a.Checkaddressalways || a.Checkaddresscount > 0 || a.Lastcheckforutxo == null || a.Addressblocked ||
                    a.Lastcheckforutxo < DateTime.Now.AddDays(-5))
                orderby a.Lastcheckforutxo
                select new CheckCustomerAddressAddressesClass
                {
                    Address = a.Adaaddress,
                    Addressblocked = a.Addressblocked,
                    Blockcounter = a.Blockcounter,
                    LastcheckForUtxo = a.Lastcheckforutxo,
                    PrivateKey = a.Privateskey,
                    PublicKey = a.Privatevkey,
                    CustomerId = a.Id,
                    InternalAccount = a.Internalaccount,
                    Amount = a.Lovelace??0,
                    Salt = a.Salt,
                    MintCouponsReceiverAddress = a.Defaultsettings.Mintingaddress,
                    PriceMintCoupons = a.Defaultsettings.Pricemintcoupons,
                }).AsNoTracking().Take(1000).ToArrayAsync(cancellationToken: cancellationToken);
            return cust;
        }

        public string CheckPolicyId(string policyid)
        {
            if (!string.IsNullOrEmpty(policyid) && policyid.Length != 56)
            {
                return "A valid Cardano policy id has 56 Characters";
            }
            if (!GlobalFunctions.IsAllLettersOrDigits(policyid))
            {
                return "Only alphanumeric characters and numbers allowed";
            }
            return "";
        }

        public Task<BuildTransactionClass> SendCoinsFromCustomerAddressAsync(Customer customer, TxOutClass[] addresses, BuildTransactionClass buildtransaction)
        {
            throw new NotImplementedException();
        }

        public Task<TxInAddressesClass> GetUtxoAsync(string address)
        {
            throw new NotImplementedException();
        }
        public async Task<Nftaddress?> CreateAddress(IConnectionMultiplexer redis, ReserveAddressQueueClass adrreq,
         int? prepardpaymenttransactionid,
         EasynftprojectsContext db, Nftproject project, string reservationType,
         List<Nftreservation> selectedreservations, long sendback, Referer? referer)
        {
            Nftaddress? address = null;
            List<Nftaddress>? freeaddress1 = null;

            if (adrreq.Addresstype == AddressType.Enterprise)
            {
                freeaddress1 = db.Nftaddresses
                    .FromSqlRaw("Call ReserveAddress2(@token)", new MySqlParameter("token", adrreq.Uid)).ToList();
            }


            if (freeaddress1 == null || !freeaddress1.Any() || freeaddress1.First().Reservationtoken != adrreq.Uid)
            {
                // Create new Address & Keys

                var cn = ConsoleCommand.CreateNewPaymentAddress(GlobalFunctions.IsMainnet(),
                    adrreq.Addresstype == AddressType.Enterprise);
                if (cn.ErrorCode != 0)
                {
                    await NftReservationClass.ReleaseAllNftsAsync(db, redis, adrreq.Uid);
                    return null;
                }

                CryptographyProcessor cp = new();
                string salt = cp.CreateSalt(30);
                string password = salt + GeneralConfigurationClass.Masterpassword;

                Nftaddress newaddress = new()
                {
                    Created = DateTime.Now,
                    State = "active",
                    Lovelace = 0,
                    Privatevkey = Encryption.EncryptString(cn.privatevkey, password),
                    Privateskey = Encryption.EncryptString(cn.privateskey, password),
                    Stakeskey = Encryption.EncryptString(cn.stakeskey, password),
                    Stakevkey = Encryption.EncryptString(cn.stakevkey, password),
                    Seedphrase = Encryption.EncryptString(cn.SeedPhrase, password),
                    Coin = Coin.ADA.ToString(),
                    Addresstype = adrreq.Addresstype.ToString(),
                    Price = adrreq.CardanoLovelace == -1 ? -1 : adrreq.CardanoLovelace > 0 ? adrreq.CardanoLovelace : 2000000,
                    Address = cn.Address,
                    Expires = DateTime.Now.AddMinutes(project.Expiretime),
                    NftprojectId = project.Id,
                    Salt = salt,
                    Utxo = 0,
                    Reservationtype = reservationType,
                    Countnft = adrreq.CountNft,
                    Tokencount = 1, // will not be used at the moment
                    Reservationtoken = adrreq.Uid,
                    Serverid = selectedreservations.First().Serverid,
                    Priceintoken = adrreq.PriceInToken,
                    Tokenpolicyid = adrreq.TokenPolicyId,
                    Tokenassetid = adrreq.TokenAssetId,
                    Tokenmultiplier = adrreq.Multiplier ?? 1,
                    Sendbacktouser = sendback,
                    RefererId = referer?.Id,
                    Customproperty = adrreq.CustomProperty,
                    Optionalreceiveraddress = adrreq.OptionalReceiverAddress,
                    PreparedpaymenttransactionsId = prepardpaymenttransactionid,
                    Refererstring = adrreq.Referer,
                    Refundreceiveraddress = adrreq.OptionalRefundAddress,
                    Lovelaceamountmustbeexact = (!adrreq.AcceptHeigherAmounts) ?? true,
                    Freemint = adrreq.Freemint,
                };

                await db.Nftaddresses.AddAsync(newaddress);
                await db.SaveChangesAsync();
                address = newaddress;
            }
            else
            {
                // Check if there is a free address available
                var freeaddress = await (from a in db.Nftaddresses
                                         where a.Id == freeaddress1.First().Id
                                         select a).FirstOrDefaultAsync();

                if (freeaddress == null)
                {
                    await NftReservationClass.ReleaseAllNftsAsync(db, redis, adrreq.Uid);
                    return null;
                }

                freeaddress.NftprojectId = project.Id;
                freeaddress.Created = DateTime.Now;
                freeaddress.State = "active";
                freeaddress.Lovelace = 0;
                freeaddress.Price = adrreq.CardanoLovelace == -1 ? -1 : adrreq.CardanoLovelace > 0 ? adrreq.CardanoLovelace : 2000000;
                freeaddress.Expires = DateTime.Now.AddMinutes(project.Expiretime);
                freeaddress.Reservationtype = reservationType;
                freeaddress.Countnft = adrreq.CountNft;
                freeaddress.Tokencount = 1; // will not be used at the moment
                freeaddress.Reservationtoken = adrreq.Uid;
                freeaddress.Serverid = selectedreservations.First().Serverid;
                freeaddress.Priceintoken = adrreq.PriceInToken;
                freeaddress.Tokenmultiplier = adrreq.Multiplier ?? 1;
                freeaddress.Tokenpolicyid = adrreq.TokenPolicyId;
                freeaddress.Tokenassetid = adrreq.TokenAssetId;
                freeaddress.Sendbacktouser = sendback;
                freeaddress.RefererId = referer?.Id;
                freeaddress.Refererstring = adrreq.Referer;
                freeaddress.Customproperty = adrreq.CustomProperty;
                freeaddress.Optionalreceiveraddress = adrreq.OptionalReceiverAddress;
                freeaddress.PreparedpaymenttransactionsId = prepardpaymenttransactionid;
                freeaddress.Refundreceiveraddress = adrreq.OptionalRefundAddress;
                freeaddress.Lovelaceamountmustbeexact = (!adrreq.AcceptHeigherAmounts) ?? true;
                await db.SaveChangesAsync();
                address = freeaddress;
            }

            return address;
        }
    }
}
