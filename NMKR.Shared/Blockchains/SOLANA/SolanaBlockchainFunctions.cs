using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Blockfrost;
using NMKR.Shared.Classes.Solana;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Metadata;
using NMKR.Shared.Functions.Solana;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Solnet.Rpc;
using Solnet.Wallet;
using Solnet.Wallet.Bip39;
using StackExchange.Redis;
using File = NMKR.Shared.Classes.Solana.File;

namespace NMKR.Shared.Blockchains.Solana
{
    public class SolanaBlockchainFunctions : IBlockchainFunctions
    {
        public async Task<ulong> GetWalletBalanceAsync(string walletAddress)
        {
            return await SolanaFunctions.GetWalletBalanceAsync(walletAddress);
        }

        public CreateNewPaymentAddressClass CreateNewWallet()
        {
            var wallet = new Wallet(Solnet.Wallet.Bip39.WordCount.TwentyFour, WordList.English);

            CreateNewPaymentAddressClass cn = new CreateNewPaymentAddressClass()
            {
                Address = wallet.Account.PublicKey.Key,
                privatevkey = wallet.Account.PrivateKey.Key,
                privateskey = wallet.Account.PrivateKey.Key,
                SeedPhrase = wallet.Mnemonic.ToString(),
                ErrorCode = 0, Blockchain = Blockchain.Solana
            };
            return cn;
        }

        public async Task<BuildTransactionClass> MintAndSend(Nft nft, Nftproject project, string receiveraddress, BlockchainKeysClass paywallet,
            BuildTransactionClass buildtransaction)
        {
            var wallet = new Wallet(paywallet.Seed);
            return await SolanaFunctions.MintAndSendCoreAsync(nft, project, receiveraddress, wallet,
                buildtransaction);
        }

        public async Task<BuildTransactionClass> SendAllCoinsAndTokens(ulong utxo, CreateNewPaymentAddressClass wallet, string receiveraddress,
            BuildTransactionClass bt, string sendbackmessage)
        {
            return await SolanaFunctions.SendAllCoinsAndTokens(utxo, wallet.SeedPhrase, receiveraddress, bt, sendbackmessage);
        }

        public async Task<GenericTransaction> GetTransactionInformation(string txhash)
        {
            return (await SolanaFunctions.GetTransactionAsync(txhash)).ToGenericTransaction();
        }

        public bool CheckForValidAddress(string address, bool mainnet)
        {
            try
            {
                var publicKey = new PublicKey(address);
                return publicKey.KeyBytes.Length == 32;
            }
            catch
            {
                return false;
            }
        }

        public Task<ICreateCollectionResult> CreateCollection(ICreateCollection createCollection)
        {
            throw new NotImplementedException();
        }
        private static SolanaOffchainCollectionMetadataClass GetSolanaCollectionMetadata(string description, string image, string mimetype, string name, int sellerFeeBasisPoints, string symbol, string externalurl)
        {
            var solanaMetadata = new SolanaOffchainCollectionMetadataClass()
            {
                Description = description ?? "",
                Image = image,
                Name = name,
                SellerFeeBasisPoints = sellerFeeBasisPoints,
                Symbol = symbol,
                ExternalUrl = externalurl ?? ""
            };
            if (image != null)
            {
                solanaMetadata.Properties ??= new Properties();

                solanaMetadata.Properties.Files = new File[]
                {
                    new File()
                    {
                        Uri = image,
                        Type = mimetype
                    }
                };
            }

            return solanaMetadata;

        }
        public async Task<string> CreateCollectionUri(CreateCollectionParameterClass createCollectionParameterClass)
        {
            var solanaMetadata = GetSolanaCollectionMetadata(createCollectionParameterClass.Description, createCollectionParameterClass.Image, createCollectionParameterClass.Mimetype, createCollectionParameterClass.Name, createCollectionParameterClass.SellerFeeBasisPoints, createCollectionParameterClass.Symbol, createCollectionParameterClass.Externalurl);

            // Save To IPFS
            var path = GeneralConfigurationClass.TempFilePath;
            string filename = GlobalFunctions.GetGuid();
            await System.IO.File.WriteAllTextAsync(path + filename, JsonConvert.SerializeObject(solanaMetadata, Formatting.Indented));
            var ipfs = await IpfsFunctions.AddFileAsync(path + filename);
            Ipfsadd ia = Ipfsadd.FromJson(ipfs);

            System.IO.File.Delete(path + filename);

            return $"{GeneralConfigurationClass.IPFSGateway}{ia.Hash}";
        }

        public async Task<string> CreateCollectionAsync(CollectionClass collectionClass)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.SolanaApiUrl}/createSolanaCollectionx");
            string st = JsonConvert.SerializeObject(collectionClass, Formatting.Indented);

            request.Content = new StringContent(st);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var res= JsonConvert.DeserializeObject<CreateCollectionResultClass>(body);
                return res.Result;
            }

            return null;
        }
        public BlockchainKeysClass ConvertToBlockchainKeysClass(Nftproject project)
        {
            string password = project.Password;
            var seedphrase = Encryption.DecryptString(project.Solanaseedphrase, password);
            var wallet = new Wallet(seedphrase);
            return new BlockchainKeysClass() { Address = wallet.Account.PublicKey, PublicKey = wallet.Account.PublicKey, SecretKey = wallet.Account.PrivateKey };
        }
        public BlockchainKeysClass ConvertToBlockchainKeysClass(Burnigendpoint address)
        {
            string payskey = Encryption.DecryptString(address.Privateskey,
                address.Salt + GeneralConfigurationClass.Masterpassword);
            return new BlockchainKeysClass() { Address = address.Address, PublicKey = address.Address, SecretKey = payskey };
        }
        public BlockchainKeysClass ConvertToBlockchainKeysClass(Adminmintandsendaddress address)
        {
            string payskey = Encryption.DecryptString(address.Privateskey,
                GeneralConfigurationClass.Masterpassword + address.Salt);
            return new BlockchainKeysClass() { Address = address.Address, PublicKey = address.Address, SecretKey = payskey };
        }

        public async Task<CollectionClass> CreateNewCollectionClass(Nftproject project, Adminmintandsendaddress paywallet)
        {
            var col = new CollectionClass
            {
                Name = project.Projectname,
                Uri = await CreateCollectionUri(new CreateCollectionParameterClass(project.Description,
                    project.Solanacollectionimage, "", project.Projectname, project.SellerFeeBasisPoints ?? 0,
                    project.Solanasymbol, project.Projecturl)),
                Symbol = project.Solanasymbol,
                UpdateAuthority = ConvertToBlockchainKeysClass(project),
                Payer = ConvertToBlockchainKeysClass(paywallet),
                SellerFeeBasisPoints = project.SellerFeeBasisPoints ?? 0
            };
            return col;
        }
        public async Task<CollectionClass> CreateNewCollectionClass(Nftproject project)
        {
            var col = new CollectionClass
            {
                Name = project.Projectname,
                Uri = await CreateCollectionUri(new CreateCollectionParameterClass(project.Description,
                    project.Solanacollectionimage, "", project.Projectname, project.SellerFeeBasisPoints ?? 0,
                    project.Solanasymbol, project.Projecturl)),
                Symbol = project.Solanasymbol,
                UpdateAuthority = ConvertToBlockchainKeysClass(project),
                Payer = ConvertToBlockchainKeysClass(project),
                SellerFeeBasisPoints = project.SellerFeeBasisPoints ?? 0
            };
            return col;
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
                Blockchain = Blockchain.Solana,
                privatevkey = paywallet.PublicKey
            }, addresses, bt);
        }

        public async Task<AssetsAssociatedWithAccount[]> GetAllAssetsInWalletAsync(IConnectionMultiplexer redis, string address)
        {
            return await SolanaFunctions.GetAllAssetsInWalletAsync(redis, address);
        }

        public BlockfrostAssetClass GetAsset(Nftproject project, Nft nft1)
        {
            return SolanaFunctions.GetAssetFromSolanaBlockchain(project, nft1.Solanatokenhash);
        }

        public string GetMetadataFromCip25Metadata(string cip25metadata, Nftproject project)
        {
            try
            {
                IConvertCardanoMetadata conv = new ConvertCardanoToSolanaMetadata();
                return conv.ConvertCip25CardanoMetadata(cip25metadata, project.Solanasymbol, project.Solanacollectiontransaction,project.SellerFeeBasisPoints);

                //  return aptosMetadata;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            /*
            return SolanaFunctions.GetSolanaMetadataFromCip25Metadata(cip25metadata, project);
            */

        }

        public async Task<string> GetLastSenderAddressAsync(string addressOrHash)
        {
            IRpcClient rpcClient = ClientFactory.GetClient($"{GeneralConfigurationClass.HeliosConfiguration.ApiUrl}/?api-key={GeneralConfigurationClass.HeliosConfiguration.ApiKey}", null, null, null);

            var accountInfo = await rpcClient.GetSignaturesForAddressAsync(addressOrHash);
            if (accountInfo.Result.Count == 0) return null;
            var sig = accountInfo.Result.First().Signature;
            var tx = await rpcClient.GetTransactionAsync(sig);

            if (tx == null || tx.Result == null || tx.Result.Transaction == null || tx.Result.Transaction.Message == null) return null;
            foreach (var messageAccountKey in tx.Result.Transaction.Message.AccountKeys)
            {
                if (!messageAccountKey.Equals(addressOrHash) && messageAccountKey.Length >= 32 && messageAccountKey.Length <= 44 && SolanaFunctions.IsValidSolanaPublicKey(messageAccountKey) && !messageAccountKey.Contains("11111111"))
                {
                    return messageAccountKey;
                }
            }
            return null;
        }

        public async Task<BuildTransactionClass> SendAllCoinsAndTokensFromNftaddress(Nftaddress address, string receiveraddress, BuildTransactionClass bt,
            string sendbackmessage)
        {
            return await SolanaFunctions.SendAllCoinsAndTokensFromNftaddress(address, receiveraddress, bt, sendbackmessage);
        }

        public async Task<BuildTransactionClass> MintFromNftAddressCoreAsync(ulong lamportsOnAddress, Nftaddress address, string receiveraddress,
            Pricelistdiscount discount, Nftprojectsadditionalpayout[] additionalPayoutWallets, BuildTransactionClass bt)
        {
            return await SolanaFunctions.MintFromNftAddressCoreAsync(lamportsOnAddress, address, receiveraddress,
                discount, additionalPayoutWallets, bt);
        }

        public async Task<CheckCustomerAddressAddressesClass[]> GetCustomersAsync(EasynftprojectsContext db, CancellationToken cancellationToken)
        {
            var cust = await(from a in db.Customers
                    .Include(a => a.Loggedinhashes)
                    .Include(a => a.Defaultsettings)
                    .AsSplitQuery()
                where a.State == "active" && a.Solanapublickey != "" && a.Solanapublickey != null && ((a.Loggedinhashes.Any() && a.Loggedinhashes.OrderByDescending(x => x.Id).First().Lastlifesign > DateTime.Now.AddMinutes(-30)) ||
                    a.Checkaddressalways || a.Checkaddresscount > 0 || a.Sollastcheckforutxo == null || a.Soladdressblocked || a.Sollastcheckforutxo < DateTime.Now.AddDays(-5))
                orderby a.Sollastcheckforutxo
                select new CheckCustomerAddressAddressesClass{
                    Address = a.Solanapublickey, 
                    Blockcounter = a.Blockcounter, 
                    PublicKey = a.Solanapublickey, 
                    Seed = a.Solanaseedphrase, 
                    LastcheckForUtxo=a.Sollastcheckforutxo, 
                    Addressblocked=a.Soladdressblocked,
                    Amount = a.Lamports,
                    InternalAccount = a.Internalaccount,
                    CustomerId = a.Id,
                    Salt = a.Salt,
                    MintCouponsReceiverAddress = a.Defaultsettings.Mintingaddresssolana,
                    PriceMintCoupons = a.Defaultsettings.Pricemintcouponssolana,
                }).AsNoTracking().Take(1000).ToArrayAsync(cancellationToken: cancellationToken);

            return cust;
        }

        public string CheckPolicyId(string policyid)
        {
            if (string.IsNullOrEmpty(policyid))
            {
                return "A collection id is empty";
            }
            if (!string.IsNullOrEmpty(policyid) && policyid.Length != 44)
            {
                return "A valid Solana collection id has 44 Characters";
            }
            if (!GlobalFunctions.IsAllLettersOrDigits(policyid))
            {
                return "Only alphanumeric characters and numbers allowed";
            }

            return "";
        }

        public async Task<BuildTransactionClass> SendCoinsFromCustomerAddressAsync(Customer customer, TxOutClass[] addresses, BuildTransactionClass buildtransaction)
        {
            var paywallet = SolanaFunctions.GetWallet(customer);
            return await SolanaFunctions.SendSolAsync(paywallet, addresses, buildtransaction);
        }

        public Task<TxInAddressesClass> GetUtxoAsync(string address)
        {
            throw new NotImplementedException();
        }

        public async Task<Nftaddress?> CreateAddress(IConnectionMultiplexer redis, ReserveAddressQueueClass adrreq,
         int? prepardpaymenttransactionid,
         EasynftprojectsContext db, Nftproject project, string reservationType,
         List<Nftreservation> selectedreservations, long sendback,
         Referer? referer)
        {

            // Create new Address & Keys
            var cn = CreateNewWallet();
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
                Coin = Enums.Coin.SOL.ToString(),
                Addresstype = adrreq.Addresstype.ToString(),
                Price = adrreq.SolanaLamport,
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
                Paymentmethod = Enums.Coin.SOL.ToString(),
                Freemint = adrreq.Freemint,
            };

            await db.Nftaddresses.AddAsync(newaddress);
            await db.SaveChangesAsync();

            return newaddress;
        }
    }
}
