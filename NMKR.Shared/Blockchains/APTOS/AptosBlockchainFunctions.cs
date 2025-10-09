using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Aptos;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.AptosClasses;
using NMKR.Shared.Classes.Blockfrost;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Metadata;
using NMKR.Shared.Model;
using HDWallet.Aptos;
using HDWallet.Core;
using Microsoft.EntityFrameworkCore;
using NBitcoin;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Shared.Blockchains.APTOS
{
   

    public class AptosBlockchainFunctions : IBlockchainFunctions
    {
        private const long MINIMUM_OCTA_INACCOUNT = 100000;

        public async Task<ulong> GetWalletBalanceAsync(string walletAddress)
        {
            if (string.IsNullOrEmpty(walletAddress))
            {
                return 0;
            }

            string url = $"https://api.{(GlobalFunctions.IsMainnet()?"mainnet":"testnet")}.aptoslabs.com/v1/accounts/{walletAddress}/balance/0x1::aptos_coin::AptosCoin";

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new("GET"),url);
            if (!string.IsNullOrEmpty(GeneralConfigurationClass.AptosNodeApiKey))
                request.Headers.TryAddWithoutValidation("Authorization",
                    "Bearer " + GeneralConfigurationClass.AptosNodeApiKey);
            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return 0;

            string responseBody = await response.Content.ReadAsStringAsync();
            /*var balance = JsonConvert.DeserializeObject<AptosGetBalanceClass[]>(responseBody);

            var amount= (from resource in balance where resource.Type == "0x1::coin::CoinStore<0x1::aptos_coin::AptosCoin>" select resource.Data.Coin.Value ?? 0).FirstOrDefault();
            // Aptos accounts always needs to have a minimum of 10,000 octas
            */

            ulong amount = Convert.ToUInt64(responseBody);


            if (amount==0)
                amount=await GetWalletBalanceResourcesAsync(walletAddress);

            if (amount < 10000)
                amount = 0;

            return amount;
        }

        // Sometimes, the APT is stored as a resource, sometimes as a balance
        private async Task<ulong> GetWalletBalanceResourcesAsync(string walletAddress)
        {
            if (string.IsNullOrEmpty(walletAddress))
            {
                return 0;
            }

            string url = $"https://api.{(GlobalFunctions.IsMainnet() ? "mainnet" : "testnet")}.aptoslabs.com/v1/accounts/{walletAddress}/resources";

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new("GET"), url);
            if (!string.IsNullOrEmpty(GeneralConfigurationClass.AptosNodeApiKey))
                request.Headers.TryAddWithoutValidation("Authorization",
                    "Bearer " + GeneralConfigurationClass.AptosNodeApiKey);
            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return 0;

            string responseBody = await response.Content.ReadAsStringAsync();
            var balance = JsonConvert.DeserializeObject<AptosGetBalanceClass[]>(responseBody);

            var amount = (from resource in balance where resource.Type == "0x1::coin::CoinStore<0x1::aptos_coin::AptosCoin>" select resource.Data.Coin.Value ?? 0).FirstOrDefault();
            // Aptos accounts always needs to have a minimum of 10,000 octas
            if (amount < 10000)
                amount = 0;

            return amount;
        }


        public CreateNewPaymentAddressClass CreateNewWallet()
        {
            Mnemonic mnemo = new Mnemonic(Wordlist.English, WordCount.Twelve);
            var n= mnemo.ToString();
            IHDWallet<AptosWallet> aptosWallet = new AptosHDWallet(n,"");
            CreateNewPaymentAddressClass res = new CreateNewPaymentAddressClass()
            {
                Blockchain = Blockchain.Aptos,
                SeedPhrase = mnemo.ToString(),
                Address = aptosWallet.GetAccountWallet(0).Address,
                privateskey = aptosWallet.GetAccountWallet(0).PrivateKeyBytes.ToHexString(),
                privatevkey = aptosWallet.GetAccountWallet(0).PublicKeyBytes.ToHexString(),
                expandedPrivateKey = aptosWallet.GetAccountWallet(0).ExpandedPrivateKey.ToHexString(),
            };

            return res;
        }

        private CreateNewPaymentAddressClass ConvertToPaymentAddressClass(Account account, Ed25519PrivateKey privatekey)
        {
            return new CreateNewPaymentAddressClass()
            {
                Address = account.Address.ToStringLong(),
                privatevkey = account.VerifyingKey.AuthKey().ToString(),
                privateskey = privatekey.ToHexString(),
                Blockchain = Blockchain.Aptos
                
            };
        }
        private CreateNewPaymentAddressClass ConvertToPaymentAddressClass(Nftaddress nftaddress)
        {
            string password = nftaddress.Salt + GeneralConfigurationClass.Masterpassword;
           // var seedphrase = Encryption.DecryptString(nftaddress.Seedphrase, password);
            var privatevkey = Encryption.DecryptString(nftaddress.Privatevkey, password);
            var privateskey = Encryption.DecryptString(nftaddress.Privateskey, password);
            string address=nftaddress.Address;

            return new CreateNewPaymentAddressClass()
            {
                Address = address,
                privatevkey = privatevkey,
                privateskey = privateskey,
                Blockchain = Blockchain.Aptos
            };
        }

        private CreateNewPaymentAddressClass ConvertToPaymentAddressClass(Customer customer)
        {
            string password = customer.Salt;
            var seedphrase = Encryption.DecryptString(customer.Aptosseed, password);
            var privatekey = Encryption.DecryptString(customer.Aptosprivatekey, password);
            return new CreateNewPaymentAddressClass()
            {
                Address = customer.Aptosaddress,
                privatevkey = privatekey,
                privateskey = privatekey,
                SeedPhrase = seedphrase,
                Blockchain = Blockchain.Aptos
            };
        }
      
        public async Task<BuildTransactionClass> MintAndSend(Nft nft, Nftproject project, string receiveraddress, BlockchainKeysClass paywallet,
            BuildTransactionClass buildtransaction)
        {
            buildtransaction.BuyerTxOut = new TxOutClass()
            {
                Amount = 0,
                ReceiverAddress = receiveraddress
            };


            MintAptosNftClass mintObject = new MintAptosNftClass()
            {
                ReceiverAddress = receiveraddress,
                Collection = project.Aptoscollectionname,
                Payer = paywallet,
                UpdateAuthority =ConvertToBlockchainKeysClass(project),
                Uri = await GetNftOffchainMetadataLink(nft, project)??"",
                Name = (project.Tokennameprefix??"") + nft.Name,
                Description = nft.Detaildata??project.Description??"",
                Network = GlobalFunctions.IsMainnet() ? "mainnet" : "testnet",
            };
         /*   if (nft.Nftproject.SellerFeeBasisPoints == null ||
                nft.Nftproject.SellerFeeBasisPoints == 0)
            {
                mintObject.Creators = new[] { creator };
            }*/

            buildtransaction.Log(JsonConvert.SerializeObject(mintObject, Formatting.Indented));

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.AptosApiUrl}MintAptosNft");
            string st = JsonConvert.SerializeObject(mintObject, Formatting.Indented);

            request.Content = new StringContent(st);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return buildtransaction;

            var body = await response.Content.ReadAsStringAsync();
            var mintresult = JsonConvert.DeserializeObject<AptosMintResultClass>(body);
            if (mintresult == null || mintresult.state.Contains("error"))
            {
                buildtransaction.Log(body);
                buildtransaction.ErrorMessage = mintresult != null ? mintresult.state : "Aptos Mint & Send - Error - Result was null";
                buildtransaction.LastTransaction = "Error";
            }
            else
            {
                buildtransaction.MintAssetAddress.Add(new MintCoreNftsResult() { MintAddress = mintresult.transferSender, TxHash = mintresult.transferTransactionHash, MintTxHash = mintresult.mintTransactionHash, NftId = nft.Id });
                buildtransaction.TxHash = mintresult.transferTransactionHash;
                buildtransaction.Log($"Mint was successful - TXHASH: {mintresult.transferTransactionHash} - MintAdddress: {mintresult.transferSender}");
                buildtransaction.LastTransaction = "OK";
                buildtransaction.TxHash = mintresult.transferTransactionHash;
            }

            return buildtransaction;
        }

        private static async Task<string> GetNftOffchainMetadataLink(Nft nfttonftaddressNft, Nftproject project)
        {
            var metadata = await GetAptosMetadataAsync(nfttonftaddressNft, project);

            if (string.IsNullOrEmpty(metadata))
                return null;

            try
            {
                // Save To IPFS
                var path = GeneralConfigurationClass.TempFilePath;
                string filename = GlobalFunctions.GetGuid();
                await System.IO.File.WriteAllTextAsync(path + filename, metadata);
                var ipfs = await IpfsFunctions.AddFileAsync(path + filename);
                Ipfsadd ia = Ipfsadd.FromJson(ipfs);

                System.IO.File.Delete(path + filename);

                return $"{GeneralConfigurationClass.IPFSGateway}{ia.Hash}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        public static async Task<string> GetAptosMetadataAsync(Nft nfttonftaddressNft, Nftproject project)
        {
            GetMetadataClass gmc = new GetMetadataClass(nfttonftaddressNft.Id, "");
            IMetadataParserInterface parser;
            var mra = await gmc.MetadataResultAsync();
            switch (mra.SourceType)
            {
                case MetadataSourceTypes.Cip25:
                    parser = new ParseCip25Metadata();
                    break;
                case MetadataSourceTypes.Cip68:
                    parser = new ParseCip68Metadata();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            try
            {
                var parsedMetadata = parser.ParseMetadata(mra.Metadata);
                var aptosMetadata = parsedMetadata.ToAptosMetadata(project);

                return aptosMetadata;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        public async Task<BuildTransactionClass> SendAllCoinsAndTokens(ulong utxo, CreateNewPaymentAddressClass wallet, string receiveraddress, BuildTransactionClass bt, string sendbackmessage)
        {
            var config = new AptosConfig(GlobalFunctions.IsMainnet() ? Aptos.Networks.Mainnet : Aptos.Networks.Testnet);
            var client = new AptosClient(config);
            var signer = new Ed25519Account(new Ed25519PrivateKey(wallet.privateskey.HexStringToByteArray()),new AccountAddress(wallet.Address.HexStringToByteArray()));

            // 2. Build the transaction
                 var transaction = await client.Transaction.Build(
                     sender: signer,
                     data: new GenerateEntryFunctionPayloadData(
                         function: "0x1::aptos_account::transfer_coins",
                         typeArguments: new List<object> { "0x1::aptos_coin::AptosCoin" },
                         functionArguments: new List<object> { receiveraddress, utxo-MINIMUM_OCTA_INACCOUNT }
                     )
                 );

            try
            {
                var pendingTransaction = await client.Transaction.SignAndSubmitTransaction(signer, transaction);

                // 4. (Optional) Wait for the transaction to be committed
                var committedTransaction = await client.Transaction.WaitForTransaction(pendingTransaction);
                Console.WriteLine(committedTransaction.Success);
                Console.WriteLine(committedTransaction.GasUsed);
                Console.WriteLine(committedTransaction.Hash);
                Console.WriteLine(committedTransaction.Type);
                Console.WriteLine(committedTransaction.VmStatus);
                bt.Fees = (long)committedTransaction.GasUsed;
                bt.TxHash = committedTransaction.Hash.ToString();
                bt.SubmissionResult = committedTransaction.Success.ToString();

                return bt;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<BuildTransactionClass> SendCoins(Nftaddress nftaddress,
            TxOutClass[] addresses,
            BuildTransactionClass bt)
        {
            var wallet = ConvertToPaymentAddressClass(nftaddress);
            return await SendCoins(wallet, addresses, bt);
        }

        public async Task<BuildTransactionClass> SendCoins(CreateNewPaymentAddressClass wallet,
            TxOutClass[] addresses,
            BuildTransactionClass buildtransaction)
        {
            var paywallet = ConvertToBlockchainKeysClass(wallet);

            List<TransferDetails> transfers= new List<TransferDetails>();
            foreach (var txOutClass in addresses)
            {
                transfers.Add(new TransferDetails(){Amount = txOutClass.Amount, ReceiverAddress = txOutClass.ReceiverAddress});
            }

            TransferAptosClass tac = new TransferAptosClass()
            {
                Payer = paywallet,
                Network = GlobalFunctions.IsMainnet() ? "mainnet" : "testnet",
                Transfers = transfers.ToArray()
            };
            buildtransaction.Log(JsonConvert.SerializeObject(tac, Formatting.Indented));

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.AptosApiUrl}TransferApt");
            string st = JsonConvert.SerializeObject(tac, Formatting.Indented);

            request.Content = new StringContent(st);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return buildtransaction;

            var body = await response.Content.ReadAsStringAsync();
            var mintresult = JsonConvert.DeserializeObject<string>(body);
            if (mintresult == null || mintresult.Contains("error"))
            {
                buildtransaction.Log(body);
                buildtransaction.ErrorMessage = mintresult != null ? mintresult : "Aptos Transfer - Error - Result was null";
                buildtransaction.LastTransaction = "Error";
            }
            else
            {
                buildtransaction.TxHash = mintresult;
                buildtransaction.Log($"Transfer was successful - TXHASH: {mintresult}");
                buildtransaction.LastTransaction = "OK";
            }

            return buildtransaction;


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
                Blockchain = Blockchain.Aptos,
                privatevkey = paywallet.PublicKey
            }, addresses, bt);
        }

        public async Task<BuildTransactionClass> SendCoinsOld(CreateNewPaymentAddressClass wallet,
            TxOutClass[] addresses,
            BuildTransactionClass bt)
        {
            var config = new AptosConfig(GlobalFunctions.IsMainnet()?Aptos.Networks.Mainnet: Aptos.Networks.Testnet);
            var client = new AptosClient(config);
            var signer = new Ed25519Account(new Ed25519PrivateKey(wallet.privateskey.HexStringToByteArray()), new AccountAddress(wallet.Address.HexStringToByteArray()));

            var receiver = new List<object>();

            var receiveraddresses = new List<object>();
            var receiveramounts = new List<object>();

            foreach (var address in addresses)
            {
                receiveraddresses.Add(address.ReceiverAddress);
                receiveramounts.Add(((ulong)address.Amount).ToString());
            }
            receiver.Add(receiveraddresses);
            receiver.Add(receiveramounts);

            int i = 0;
            do
            {
                try
                {
                    // 2. Build the transaction
                           var transaction = await client.Transaction.Build(
                               sender: signer,
                               data: new GenerateEntryFunctionPayloadData(
                                   function: "0x1::aptos_account::batch_transfer_coins",
                                   typeArguments: new List<object> { "0x1::aptos_coin::AptosCoin" },
                                   functionArguments: receiver
                               )
                           );
                       

                    // 3. Sign and submit the transaction
                    var pendingTransaction = await client.Transaction.SignAndSubmitTransaction(signer, transaction);

                    // 4. (Optional) Wait for the transaction to be committed
                    var committedTransaction = await client.Transaction.WaitForTransaction(pendingTransaction);
                    Console.WriteLine(committedTransaction.Success);
                    Console.WriteLine(committedTransaction.GasUsed);
                    Console.WriteLine(committedTransaction.Hash);
                    Console.WriteLine(committedTransaction.Type);
                    Console.WriteLine(committedTransaction.VmStatus);
                    bt.Fees = (long)committedTransaction.GasUsed;
                    bt.TxHash = committedTransaction.Hash.ToString();
                    bt.SubmissionResult = committedTransaction.Success.ToString();
                    bt.Log("Aptos send - " + bt.TxHash + " " + bt.SubmissionResult);
                    return bt;
                }
                catch (Exception e)
                {
                    receiveramounts.Clear();
                    receiveraddresses.Clear();
                    receiver.Clear();
                    int i1 = 0;
                    foreach (var address in addresses)
                    {
                        i1++;
                        receiveraddresses.Add(address.ReceiverAddress);
                        if (i1 == addresses.Length)
                            receiveramounts.Add(((ulong)address.Amount - (ulong)((i + 1) * 1000)).ToString());
                    }
                    receiver.Add(receiveraddresses);
                    receiver.Add(receiveramounts);


                    await Task.Delay(1000);
                    bt.Log(e.Message);
                }
            } while (i++ < 3);

            bt.TxHash = "";
            bt.SubmissionResult = "ERROR";
            return bt;
        }

        public async Task<AssetsAssociatedWithAccount[]> GetAllAssetsInWalletAsync(IConnectionMultiplexer redis, string address)
        {
            HttpClient httpClient = new HttpClient();
            string OperationsDoc = $@"

query GetAssetsByOwner {{
  current_token_ownerships_v2(
    where: {{owner_address: {{_eq: ""{address}""}}, amount: {{_gt: ""0""}}}}
  ) {{
    amount
    token_standard
    current_token_data {{
      aptos_name {{
        domain
        subdomain
        token_name
        registered_address
        domain_with_suffix
        domain_expiration_timestamp
        expiration_timestamp
        token_standard
      }}
      collection_id
      current_collection {{
        collection_name
        description
      }}
      description
      token_name
      token_properties
      token_uri
    }}
  }}
}}
    ";
            var requestBody = new
            {
                query = OperationsDoc,
                variables = new { },
                operationName = "GetAssetsByOwner"
            };
            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.{(GlobalFunctions.IsMainnet() ? "mainnet" : "testnet")}.aptoslabs.com/v1/graphql")
            {
                Content = requestContent
            };

            // Füge den Authorization-Header hinzu
            if (!string.IsNullOrEmpty(GeneralConfigurationClass.AptosNodeApiKey))
            {
               // request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + GeneralConfigurationClass.AptosNodeApiKey);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GeneralConfigurationClass.AptosNodeApiKey);
            }

            // Sende die Anfrage
            var response = await httpClient.SendAsync(request);





           // var response = await httpClient.PostAsync($"https://api.{(GlobalFunctions.IsMainnet() ? "mainnet" : "testnet")}.aptoslabs.com/v1/graphql", requestContent);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                AptosGetAssetsByOwnerClass tokens = JsonConvert.DeserializeObject<AptosGetAssetsByOwnerClass>(responseContent);
                var res = tokens.Data.CurrentTokenOwnershipsV2.Select(token => new AssetsAssociatedWithAccount(token.CurrentTokenData.CollectionId, token.CurrentTokenData.TokenName.ToHex(), token.Amount, Blockchain.Aptos, address)).ToArray();
                return res;
            }
            return null;
        }

        public BlockfrostAssetClass GetAsset(Nftproject project, Nft nft1)
        {
            // TODO: Implement this method
            return null;
        }

        public async Task<GenericTransaction> GetTransactionInformation(string txhash)
        {
            throw new NotImplementedException();
        }

        public bool CheckForValidAddress(string address, bool mainnet)
        {
            if (string.IsNullOrEmpty(address))
            {
                return false;
            }

            var regex = new Regex(@"^(0x)?[0-9a-fA-F]{64}$");
            return regex.IsMatch(address);
        }

        /*public Task<ICreateCollectionResult> CreateCollection(ICreateCollection createCollection)
        {
            var cc=(CreateAptosCollectionClass)createCollection;

            return null;
        }*/

        public async Task<string> CreateCollectionUri(CreateCollectionParameterClass createCollectionParameterClass)
        {
            // The collection has only the image on aptos
            return createCollectionParameterClass.Image;
        }

        public async Task<string> CreateCollectionAsync(CollectionClass collectionClass)
        {
            // On Aptos, first fund the project address (update authority) before creating the collection
            var utxo = await GetWalletBalanceAsync(collectionClass.UpdateAuthority.Address);
            if (utxo == 0)
            {
                var r1=await SendCoins(
                    new CreateNewPaymentAddressClass()
                    {
                        Address = collectionClass.Payer.Address, SeedPhrase = collectionClass.Payer.Seed,
                        privateskey = collectionClass.Payer.SecretKey, Blockchain = Blockchain.Aptos,
                        privatevkey = collectionClass.Payer.PublicKey
                    },
                    new TxOutClass[] {new TxOutClass(){Amount = 25000000 , ReceiverAddress = collectionClass.UpdateAuthority.Address} },
                    new BuildTransactionClass());

                if (r1==null || string.IsNullOrEmpty(r1.TxHash))
                    return null;
            }


            // we create the collection via a webservice
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.AptosApiUrl}CreateAptosCollection");
            string st = JsonConvert.SerializeObject(collectionClass, Formatting.Indented);

            request.Content = new StringContent(st);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<string>(body);
            }

            return null;
        }

        public BlockchainKeysClass ConvertToBlockchainKeysClass(Nftproject project)
        {
            string password = project.Password;
            var seedphrase = Encryption.DecryptString(project.Aptosseedphrase, password);
            IHDWallet<AptosWallet> aptosWallet = new AptosHDWallet(seedphrase, "");
            var accountWallet = aptosWallet.GetAccountWallet(0);
            return new BlockchainKeysClass() { Address = project.Aptosaddress, PublicKey = project.Aptospublickey, SecretKey = accountWallet.PrivateKeyBytes.ToHexString(), Seed = seedphrase};
        }

        private BlockchainKeysClass ConvertToBlockchainKeysClass(CreateNewPaymentAddressClass address)
        {
            return new BlockchainKeysClass() { Address = address.Address, PublicKey = address.Address, SecretKey = address.privateskey, Seed = address.SeedPhrase };
        }

        private BlockchainKeysClass ConvertToBlockchainKeysClass(Nftaddress nftaddress)
        {
            string password = nftaddress.Salt + GeneralConfigurationClass.Masterpassword;
             var seedphrase = Encryption.DecryptString(nftaddress.Seedphrase, password);
            var privatevkey = Encryption.DecryptString(nftaddress.Privatevkey, password);
            var privateskey = Encryption.DecryptString(nftaddress.Privateskey, password);
            string address = nftaddress.Address;

            return new BlockchainKeysClass()
            {
                Address = address,
                PublicKey = privatevkey,
                SecretKey = privateskey,
                Seed = seedphrase
            };
        }
        public BlockchainKeysClass ConvertToBlockchainKeysClass(Burnigendpoint address)
        {
            throw new NotImplementedException();
        }

        public BlockchainKeysClass ConvertToBlockchainKeysClass(Adminmintandsendaddress address)
        {
           BlockchainKeysClass res = new BlockchainKeysClass()
           {
               Address = address.Address,
               PublicKey = address.Address,
               SecretKey = Encryption.DecryptString(address.Privateskey, GeneralConfigurationClass.Masterpassword + address.Salt),
               Seed = Encryption.DecryptString(address.Seed, GeneralConfigurationClass.Masterpassword + address.Salt)
           };
            return res;
        }

        public async Task<CollectionClass> CreateNewCollectionClass(Nftproject project, Adminmintandsendaddress paywallet)
        {
            try
            {
                var col = new CollectionClass
                {
                    Name = project.Aptoscollectionname,
                    Uri = project.Aptoscollectionimage,
                    Symbol = "",
                    UpdateAuthority = ConvertToBlockchainKeysClass(project),
                    Payer = ConvertToBlockchainKeysClass(paywallet),
                    SellerFeeBasisPoints = project.SellerFeeBasisPoints ?? 0
                };
                return col;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        public async Task<CollectionClass> CreateNewCollectionClass(Nftproject project)
        {
            var col = new CollectionClass
            {
                Name = project.Projectname,
                Uri = await CreateCollectionUri(new CreateCollectionParameterClass(project.Description,
                    project.Aptoscollectionimage, project.Aptoscollectionimagemimetype, project.Projectname, project.SellerFeeBasisPoints ?? 0,
                    "", project.Projecturl)),
                Symbol = "",
                UpdateAuthority = ConvertToBlockchainKeysClass(project),
                Payer = ConvertToBlockchainKeysClass(project),
                SellerFeeBasisPoints = project.SellerFeeBasisPoints ?? 0
            };
            return col;
        }
        public string GetMetadataFromCip25Metadata(string cip25metadata, Nftproject project)
        {
            /*   var parser = new ParseCip25Metadata();
               try
               {
                   var parsedMetadata = parser.ParseMetadata(cip25metadata);
                   var aptosMetadata = parsedMetadata.ToAptosMetadata(project);

                   return aptosMetadata;
               }
               catch (Exception e)
               {
                   return null;
               }*/
            try
            {
               IConvertCardanoMetadata conv = new ConvertCardanoToAptosMetadata();
               return conv.ConvertCip25CardanoMetadata(cip25metadata);

              //  return aptosMetadata;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<string> GetLastSenderAddressAsync(string addressOrHash)
        {
             HttpClient httpClient = new HttpClient();
             string OperationsDoc = $@"
        query GetTransactions {{
            account_transactions(
                limit: 100
                where: {{account_address: {{_eq: ""{addressOrHash}""}}}}
                order_by: {{transaction_version: desc}}
            ) {{
                transaction_version
                user_transaction {{
                    block_height
                    version
                    timestamp
                    sender
                    entry_function_function_name
                    entry_function_id_str
                    entry_function_module_name
                    sequence_number
                }}
                coin_activities(where: {{activity_type: {{_eq: ""0x1::coin::DepositEvent""}}}}) {{
                    amount
                    coin_type
                    activity_type
                    is_gas_fee
                    is_transaction_success
                }}
            }}
        }}
    ";
            var requestBody = new
            {
                query = OperationsDoc,
                variables = new {},
                operationName = "GetTransactions"
            };
            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.{(GlobalFunctions.IsMainnet() ? "mainnet" : "testnet")}.aptoslabs.com/v1/graphql")
            {
                Content = requestContent
            };

            // Füge den Authorization-Header hinzu
            if (!string.IsNullOrEmpty(GeneralConfigurationClass.AptosNodeApiKey))
            {
                // request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + GeneralConfigurationClass.AptosNodeApiKey);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GeneralConfigurationClass.AptosNodeApiKey);
            }

            // Sende die Anfrage
            var response = await httpClient.SendAsync(request);


          //  var response = await httpClient.PostAsync($"https://api.{(GlobalFunctions.IsMainnet() ? "mainnet" : "testnet")}.aptoslabs.com/v1/graphql", requestContent);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                AptosTransactionsAccountGraphQlResult res =
                    JsonConvert.DeserializeObject<AptosTransactionsAccountGraphQlResult>(responseContent);
                return res.Data.AccountTransactions.FirstOrDefault(x=>x.UserTransaction.Sender!= addressOrHash)?.UserTransaction.Sender;
            }

            return null;
        }

        public async Task<BuildTransactionClass> SendAllCoinsAndTokensFromNftaddress(Nftaddress address, string receiveraddress, BuildTransactionClass bt,
            string sendbackmessage)
        {


            return await SendAllCoinsAndTokens(await GetWalletBalanceAsync(address.Address), ConvertToPaymentAddressClass(address),
                receiveraddress, bt, sendbackmessage);
        }

        public async Task<BuildTransactionClass> MintFromNftAddressCoreAsync(ulong amountOnAddress, Nftaddress address, string receiveraddress,
            Pricelistdiscount discount, Nftprojectsadditionalpayout[] additionalPayoutWallets, BuildTransactionClass bt)
        {
            var addressWallet = ConvertToBlockchainKeysClass(address);
            // First send the nfts
            foreach (var nfttonftaddress in address.Nfttonftaddresses)
            {
                int i = 0;
                do
                {
                    i++;
                    bt = await MintAndSend(nfttonftaddress.Nft, nfttonftaddress.Nft.Nftproject,
                        receiveraddress, addressWallet, bt);
                    if (bt.LastTransaction == "OK")
                        break;
                    await Task.Delay(1000);
                    if (i == 4)
                        break;
                } while (true);

                if (bt.LastTransaction != "OK")
                    break;
            }

            if (bt.LastTransaction != "OK")
            {
                bt.TxHash = ""; // This transaction will be marked as error
                return bt;
            }

            List<TxOutClass> txouts = new List<TxOutClass>();
            // Then calculate the discounts and send them back
            if (discount != null)
            {
                ulong discountInLamport = (ulong)(amountOnAddress * discount.Sendbackdiscount / 100);
                bt.Discount = (long)discountInLamport;
                txouts.Add(
                    new TxOutClass()
                    {
                        Amount = (long)discountInLamport,
                        ReceiverAddress = addressWallet.Address,
                    }
                );
            }

            // Then send the mintingfees
            var mintingcosts = GlobalFunctions.GetMintingcosts2((int)address.NftprojectId, address.Nfttonftaddresses.Count,
                address.Price ?? 0);
            if (mintingcosts.CostsAptos != 0)
            {
                bt.MintingcostsTxOut = new TxOutClass()
                {
                    Amount = mintingcosts.CostsAptos,
                    ReceiverAddress = mintingcosts.MintingcostsreceiverAptos
                };
                txouts.Add(bt.MintingcostsTxOut);
            }

            var lamports = amountOnAddress;
            int i2 = 0;
            do
            {
                // And finally send the rest to the seller
                lamports = await GetWalletBalanceAsync(address.Address);
                if (lamports == amountOnAddress)
                    await Task.Delay(1000);
                
                i2++;
            } while (lamports == amountOnAddress && i2 < 10);


            if (i2 >= 10 || lamports == amountOnAddress)
            {
                bt.ErrorMessage= "Timeout while waiting for the balance to change. " +
                                "Please check the Aptos Explorer for the transaction and the balance of the address.";
                bt.TxHash = ""; // This transaction will be marked as error
                return bt;
            }


            ulong addvaluesum = 0;
            // Calculate the additional payout wallets

            bt.AdditionalPayouts = additionalPayoutWallets;
            foreach (var nftprojectsadditionalpayout in additionalPayoutWallets.OrEmptyIfNull())
            {
                ulong addvalue = GetAdditionalPayoutwalletsValueAptos(nftprojectsadditionalpayout,
                    lamports - (ulong)(bt.Discount ?? 0), address.Nfttonftaddresses.Count);
                if (addvalue <= 0) continue;

                var addval = new TxOutClass()
                {
                    Amount = (long)addvalue,
                    ReceiverAddress = nftprojectsadditionalpayout.Wallet.Walletaddress
                };
                txouts.Add(addval);

                nftprojectsadditionalpayout.Valuetotal = (long)addvalue;
                addvaluesum += addvalue;
            }


            ulong rest = lamports - (ulong)txouts.Sum(x => x.Amount);
            if (rest > MINIMUM_OCTA_INACCOUNT)
            {
                // On Free Mints, we send the rest back to the customer
                if (address.Freemint)
                {
                    bt.Discount = (long)rest - MINIMUM_OCTA_INACCOUNT;
                    bt.ProjectTxOut = new TxOutClass()
                    {
                        Amount = 0,
                        ReceiverAddress = address.Nftproject.Aptoscustomerwallet.Walletaddress
                    };
                    txouts.Add(new TxOutClass(){ReceiverAddress = receiveraddress, Amount = (long)rest- MINIMUM_OCTA_INACCOUNT });
                }
                else
                {
                    bt.ProjectTxOut = new TxOutClass()
                    {
                        Amount = (long)rest - MINIMUM_OCTA_INACCOUNT,
                        ReceiverAddress = address.Nftproject.Aptoscustomerwallet.Walletaddress
                    };
                    txouts.Add(bt.ProjectTxOut);
                }
            }

            if (txouts.Any())
            {
                bt = await SendCoins(ConvertToPaymentAddressClass(address), txouts.ToArray(), bt);
            }
            else
            {
                bt.TxHash = bt.MintAssetAddress.FirstOrDefault()?.TxHash;
            }

            // Set the project txout also with the additional wallets - to display the correct value in the transactions
            bt.ProjectTxOut.Amount += (long)addvaluesum;
            bt.Fees = (long)(amountOnAddress - lamports);

            return bt;
        }



        public static ulong GetAdditionalPayoutwalletsValueAptos(Nftprojectsadditionalpayout nftprojectsadditionalpayout,
            ulong hastopay, long nftcount)
        {
            if (nftprojectsadditionalpayout.Valuetotal != null)
                return (ulong)nftprojectsadditionalpayout.Valuetotal * (ulong)nftcount;

            if (nftprojectsadditionalpayout.Valuepercent == null ||
                !(nftprojectsadditionalpayout.Valuepercent > 0)) return 0;
            var v = hastopay / 100 * nftprojectsadditionalpayout.Valuepercent;
            long v1 = Convert.ToInt64(v);
            v1 = Math.Max(1, v1);
            return (ulong)v1;
        }

        public async Task<CheckCustomerAddressAddressesClass[]> GetCustomersAsync(EasynftprojectsContext db, CancellationToken cancellationToken)
        {
            var cust = await(from a in db.Customers
                    .Include(a => a.Loggedinhashes)
                    .Include(a => a.Defaultsettings)
                    .AsSplitQuery()
                where a.State == "active" && a.Aptosprivatekey != null && a.Aptosprivatekey !="" &&  
                      ((a.Loggedinhashes.Any() && a.Loggedinhashes.OrderByDescending(x => x.Id).First().Lastlifesign > DateTime.Now.AddMinutes(-30)) ||
                    a.Checkaddressalways || a.Checkaddresscount > 0 || a.Aptlastcheckforutxo == null || a.Aptaddressblocked || a.Aptlastcheckforutxo < DateTime.Now.AddDays(-5))
                orderby a.Aptlastcheckforutxo
                select new CheckCustomerAddressAddressesClass
                {
                    Address = a.Aptosaddress,
                    Blockcounter = a.Blockcounter,
                    PrivateKey = a.Aptosprivatekey,
                    Seed = a.Aptosseed,
                    LastcheckForUtxo = a.Aptlastcheckforutxo,
                    Addressblocked = a.Aptaddressblocked,
                    CustomerId = a.Id,
                    InternalAccount = a.Internalaccount,
                    Amount = a.Octas,
                    Salt = a.Salt,
                    MintCouponsReceiverAddress = a.Defaultsettings.Mintingaddresssaptos,
                    PriceMintCoupons = a.Defaultsettings.Pricemintcouponsaptos,
                }).AsNoTracking().Take(1000).ToArrayAsync(cancellationToken: cancellationToken);
            return cust;
        }

        public string CheckPolicyId(string policyid)
        {
            if (string.IsNullOrEmpty(policyid))
            {
                return "A collection id is empty";
            }

            if (!string.IsNullOrEmpty(policyid) && policyid.Length != 66)
            {
                return "A valid Aptos collection id has 66 Characters and must start with 0x";
            }

            if (!string.IsNullOrEmpty(policyid) && !policyid.StartsWith("0x"))
            {
                return "The collection id must start with 0x";
            }
            if (!GlobalFunctions.IsAllLettersOrDigits(policyid.Substring(2)))
            {
                return "Only alphanumeric characters and numbers allowed (excluding the leading 0x) in the collection id";
            }

            return "";
        }

        public async Task<BuildTransactionClass> SendCoinsFromCustomerAddressAsync(Customer customer, TxOutClass[] addresses, BuildTransactionClass bt)
        {
            bt = await SendCoins(ConvertToPaymentAddressClass(customer), addresses.ToArray(), bt);
            return bt;
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
                Coin = Enums.Coin.APT.ToString(),
                Addresstype = adrreq.Addresstype.ToString(),
                Price = adrreq.AptosOcta,
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
                Paymentmethod = Enums.Coin.APT.ToString(),
                Freemint = adrreq.Freemint,
            };

            await db.Nftaddresses.AddAsync(newaddress);
            await db.SaveChangesAsync();

            return newaddress;
        }
    }
}
