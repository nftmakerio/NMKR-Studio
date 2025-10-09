using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Blockfrost;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;
using StackExchange.Redis;
using NBitcoin;
using NMKR.Shared.Functions;
using Newtonsoft.Json;
using NMKR.Shared.Classes.Bitcoin;
using System.IO;
using System.Net.Http.Headers;
using GlobalFunctions = NMKR.Shared.Functions.GlobalFunctions;
using System.Net;
using NMKR.Shared.Functions.Metadata;
using Coin = NBitcoin.Coin;
using Script = NBitcoin.Script;
using System.Text;
using NBitcoin.RPC;
using System.Net.Http.Json;
using Transaction = NBitcoin.Transaction;

namespace NMKR.Shared.Blockchains.BITCOIN
{
    public class BitcoinBlockchainFunctions : IBlockchainFunctions
    {
        public async Task<ulong> GetWalletBalanceAsync(string walletAddress)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"),
                $"{GeneralConfigurationClass.MaestroBitcoinConfiguration.ApiUrl}/addresses/{walletAddress}/balance");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("api-key",
                GeneralConfigurationClass.MaestroBitcoinConfiguration.ApiKey);

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                var res = JsonConvert.DeserializeObject<BitcoinGetBalance>(responseString);
                return (ulong)(res.Data ?? 0);
            }

            return 0;
        }

        public CreateNewPaymentAddressClass CreateNewWallet()
        {
            var network = GlobalFunctions.IsMainnet() ? Network.Main : Network.TestNet4;
            Key privateKey = new Key();
            BitcoinAddress segwitAddress = privateKey.PubKey.GetAddress(ScriptPubKeyType.Segwit, network);
            string privateKeyWif = privateKey.GetWif(network).ToString();

            CreateNewPaymentAddressClass res = new CreateNewPaymentAddressClass()
            {
                Address = segwitAddress.ToString(),
                Blockchain = Blockchain.Bitcoin,
                privateskey = privateKeyWif,
                privatevkey = privateKey.PubKey.ToString(),
                ErrorMessage = "",
                ErrorCode = 0,
            };
            return res;
        }


        /*    public CreateNewPaymentAddressClass CreateNewWallet()
            {
                var privateKey = new Key();
                var taprootAddress = privateKey.PubKey.GetAddress(ScriptPubKeyType.TaprootBIP86, (GlobalFunctions.IsMainnet() ? Network.Main : Network.TestNet4));
                var bitcoinPrivateKey = privateKey.GetWif(GlobalFunctions.IsMainnet() ? Network.Main : Network.TestNet4);
                var bitcoinPublicKey = bitcoinPrivateKey.PubKey;
                var address = bitcoinPublicKey.GetAddress(ScriptPubKeyType.Segwit, GlobalFunctions.IsMainnet() ? Network.Main : Network.TestNet4);

                CreateNewPaymentAddressClass res = new CreateNewPaymentAddressClass()
                {
                    Address = taprootAddress.ToString(),
                    Blockchain = Blockchain.Bitcoin,
                    privateskey = bitcoinPrivateKey.ToString(),
                    privatevkey = bitcoinPublicKey.ToString(), ErrorMessage = "", ErrorCode = 0
                };
                return res;

            }*/


        public async Task<long> GetMintingCostsAsync(Nft nft, Nftproject project, string receiveraddress)
        {
            var stringContent = await GetStringContentForInscriptionPrice(nft, project, receiveraddress);
            var price= await GetInscriptionPriceAsync(stringContent);

            return price;
        }
        private async Task<long> GetInscriptionPriceAsync(string stringContent)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri =
                    new Uri($"https://{(GlobalFunctions.IsMainnet() ? "" : "testnet")}-api.ordinalsbot.com/price"),
                Headers =
                {
                    { "accept", "application/json" },
                },
                Content = new StringContent(stringContent)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };
            using var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var res= JsonConvert.DeserializeObject<BitcoinIncribePriceResultClass>(body);
                return res.Amount ?? -1;
            }

            return -1;
        }

        public async Task<BuildTransactionClass> MintAndSend(Nft nft, Nftproject project, string receiveraddress,
            BlockchainKeysClass paywallet,
            BuildTransactionClass buildtransaction)
        {
            // First Create the StringContent for the inscription
            var stringContent = await GetStringContentForInscription(new Nft[]{nft}, project, receiveraddress);
            // Then create the Inscription and send it to Ordinalsbot
            buildtransaction = await CreateInscriptionAsync(stringContent, buildtransaction);
            // Then Get the Orderstatus
            if (!string.IsNullOrEmpty(buildtransaction.ErrorMessage) || buildtransaction.BitcoinInscribeResult == null)
                return buildtransaction;

            await Task.Delay(1000); // Wait a second to ensure the order is processed
            buildtransaction = await GetOrderStatusAsync(buildtransaction);

            // Fund the address with BTC from our paywallets
            List<TxOutClass> txouts = new List<TxOutClass>
            {
                new TxOutClass
                {
                    ReceiverAddress = buildtransaction.BitcoinOrderstatusResult.Charge.Address,
                    Amount = buildtransaction.BitcoinOrderstatusResult.Charge.Amount ?? 0
                }
            };

            buildtransaction = await SendCoins(new CreateNewPaymentAddressClass()
            {
                Address = paywallet.Address,
                SeedPhrase = paywallet.Seed,
                privateskey = paywallet.SecretKey,
                Blockchain = Blockchain.Bitcoin,
                privatevkey = paywallet.PublicKey
            }, txouts.ToArray(), buildtransaction);

            if (!string.IsNullOrEmpty(buildtransaction.TxHash))
            {
                buildtransaction.BuyerTxOut = new TxOutClass()
                {
                    Amount = buildtransaction.BitcoinOrderstatusResult.Charge.Amount ?? 0,
                    ReceiverAddress = receiveraddress,
                };
            }


            return buildtransaction;

        }

        private async Task<BuildTransactionClass> GetOrderStatusAsync(BuildTransactionClass buildtransaction)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri =
                    new Uri(
                        $"https://{(GlobalFunctions.IsMainnet() ? "" : "testnet")}-api.ordinalsbot.com/order?id={buildtransaction.BitcoinInscribeResult.Id}"),
            };
            using var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode == false)
            {
                buildtransaction.ErrorMessage = $"Error getting order status: {response.ReasonPhrase}";
                buildtransaction.LogFile = await response.Content.ReadAsStringAsync();
                return buildtransaction;
            }

            var body = await response.Content.ReadAsStringAsync();
            var orderstatus = JsonConvert.DeserializeObject<BitcoinOrderStatusResult>(body);

            if (orderstatus == null)
            {
                buildtransaction.ErrorMessage = "Error parsing order status response.";
                return buildtransaction;
            }

            buildtransaction.BitcoinOrderstatusResult = orderstatus;



            return buildtransaction;
        }

        static async Task<decimal> GetMempoolFeeRateAsync()
        {
            using var http = new HttpClient();
            string url = "https://mempool.space/testnet/api/v1/fees/recommended";
            var fee = await http.GetFromJsonAsync<MempoolFeeResponse>(url);
            return fee.halfHourFee;
        }

        private async Task<BuildTransactionClass> CreateInscriptionAsync(string stringContent,
            BuildTransactionClass buildtransaction)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri =
                    new Uri($"https://{(GlobalFunctions.IsMainnet() ? "" : "testnet")}-api.ordinalsbot.com/inscribe"),
                Headers =
                {
                    { "accept", "application/json" },
                },
                Content = new StringContent(stringContent)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };
            using var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                buildtransaction.BitcoinInscribeResult = JsonConvert.DeserializeObject<BitcoinInscribeResult>(body);
            }
            else
            {
                buildtransaction.ErrorMessage = $"Error sending transaction: {response.ReasonPhrase}";
                buildtransaction.LogFile = await response.Content.ReadAsStringAsync();
                buildtransaction.TxHash = "";
            }

            return buildtransaction;
        }

        private async Task<string> GetStringContentForInscription(Nft[] nfts, Nftproject project, string receiveraddress)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            BitcoinInscribe bitcoinInscribe = new BitcoinInscribe()
            {
                ReceiveAddress = receiveraddress,
                Postage = 546,
                Fee = 5
            };
            List<BitcoinInscribeFile> files = new List<BitcoinInscribeFile>();

            foreach (var nft in nfts)
            {
                string downloadfilename = GlobalFunctions.GetGuid();

                // Download the file from IPFS
                using var httpClient = new HttpClient();
                var url = GeneralConfigurationClass.IPFSGateway + nft.Ipfshash;
                var filePath = Path.Combine(GeneralConfigurationClass.TempFilePath,
                    downloadfilename + GlobalFunctions.GetExtension(db, nft.Mimetype));

                using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    await response.Content.CopyToAsync(fs);
                }

                // Create the Metadata - CIP25
                GetMetadataClass gm = new(nft.Id, "", true);
                string metadata = (await gm.MetadataResultAsync()).Metadata;
                string btcmetadata = GetMetadataFromCip25Metadata(metadata, project);
                var btcbase64metadata = ConvertMetadataToBitcoinMetadataUrl(btcmetadata);
                var extension = GlobalFunctions.GetExtension(db, nft.Mimetype);
                files.Add(
                    new BitcoinInscribeFile
                    {
                        DataUrl =
                            $"data:{nft.Mimetype};base64,{Convert.ToBase64String(await System.IO.File.ReadAllBytesAsync(filePath))}",
                        MetadataDataUrl = btcbase64metadata,
                        Name = nft.Name + extension,
                        Type = nft.Mimetype,
                        Size = GlobalFunctions.GetFileSize(filePath),
                        MetadataSize = btcmetadata.Length,
                    });

                System.IO.File.Delete(filePath);
            }
            bitcoinInscribe.Files = files.ToArray();
            return JsonConvert.SerializeObject(bitcoinInscribe);
        }
        private async Task<string> GetStringContentForInscriptionPrice(Nft nft, Nftproject project, string receiveraddress)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            string downloadfilename = GlobalFunctions.GetGuid();

            // Download the file from IPFS
            using var httpClient = new HttpClient();
            var url = GeneralConfigurationClass.IPFSGateway + nft.Ipfshash;
            var filePath = Path.Combine(GeneralConfigurationClass.TempFilePath,
                downloadfilename + GlobalFunctions.GetExtension(db, nft.Mimetype));

            using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fs);
            }

            // Create the Metadata - CIP25
            GetMetadataClass gm = new(nft.Id, "", true);
            string metadata = (await gm.MetadataResultAsync()).Metadata;
            string btcmetadata = GetMetadataFromCip25Metadata(metadata, project);
            var btcbase64metadata = ConvertMetadataToBitcoinMetadataUrl(btcmetadata);
            var extension = GlobalFunctions.GetExtension(db, nft.Mimetype);
            BitcoinIncribePriceClass bitcoinInscribe = new BitcoinIncribePriceClass
            {
                Type = "direct",
                Order = new BitcoinIncribePriceOrder
                {
                    RareSats = "random",
                    Compress = false,
                    Postage = 546,
                    ReceiveAddress = receiveraddress,
                    Fee = 5,
                    Files = new BitcoinIncribePriceFile[]
                    {
                        new BitcoinIncribePriceFile()
                        {
                            Size = GlobalFunctions.GetFileSize(filePath),
                            Name = nft.Name + extension,
                            Type = nft.Mimetype,
                            MetadataSize = btcmetadata.Length,
                        }
                    }
                }
            };


            System.IO.File.Delete(filePath);
            return JsonConvert.SerializeObject(bitcoinInscribe);
        }
        private string ConvertMetadataToBitcoinMetadataUrl(string getMetadataFromCip25Metadata)
        {
            return "data:application/json;base64," +
                   Convert.ToBase64String(Encoding.UTF8.GetBytes(getMetadataFromCip25Metadata));
        }

        public Task<BuildTransactionClass> SendAllCoinsAndTokens(ulong utxo, CreateNewPaymentAddressClass wallet,
            string receiveraddress,
            BuildTransactionClass bt, string sendbackmessage)
        {
            throw new NotImplementedException();
        }

        public Task<GenericTransaction> GetTransactionInformation(string txhash)
        {
            throw new NotImplementedException();
        }

        public bool CheckForValidAddress(string address, bool mainnet)
        {
            var network = mainnet ? Network.Main : Network.TestNet4;
            var bitcoinAddress = BitcoinAddress.Create(address, network);

            return bitcoinAddress switch
            {
                TaprootAddress or BitcoinWitPubKeyAddress or BitcoinWitScriptAddress => true,
                _ => false
            };
        }

        public Task<string> CreateCollectionUri(CreateCollectionParameterClass createCollectionParameterClass)
        {
            throw new NotImplementedException();
        }

        public Task<string> CreateCollectionAsync(CollectionClass collectionClass)
        {
            throw new NotImplementedException();
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
        public BlockchainKeysClass ConvertToBlockchainKeysClass(Nftproject project)
        {
            string password = project.Password;
            var seedphrase = Encryption.DecryptString(project.Bitcoinseedphrase, password);
            var publickey = Encryption.DecryptString(project.Bitcoinpublickey, password);
            var privatekey = Encryption.DecryptString(project.Bitcoinprivatekey, password);

            return new BlockchainKeysClass()
                { Address = project.Bitcoinaddress, PublicKey = publickey, SecretKey = privatekey, Seed = seedphrase };
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
                PublicKey = Encryption.DecryptString(address.Privatevkey, GeneralConfigurationClass.Masterpassword + address.Salt),
                SecretKey = Encryption.DecryptString(address.Privateskey, GeneralConfigurationClass.Masterpassword + address.Salt),
                Seed = Encryption.DecryptString(address.Seed, GeneralConfigurationClass.Masterpassword + address.Salt)
            };
            return res;
        }

        public Task<CollectionClass> CreateNewCollectionClass(Nftproject project, Adminmintandsendaddress paywallet)
        {
            throw new NotImplementedException();
        }

        public Task<CollectionClass> CreateNewCollectionClass(Nftproject project)
        {
            throw new NotImplementedException();
        }

        public Task<BuildTransactionClass> SendCoins(Nftaddress nftaddress, TxOutClass[] addresses,
            BuildTransactionClass bt)
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
                Blockchain = Blockchain.Bitcoin,
                privatevkey = paywallet.PublicKey
            }, addresses, bt);
        }

        public async Task<BuildTransactionClass> SendCoins(CreateNewPaymentAddressClass wallet, TxOutClass[] addresses,
            BuildTransactionClass bt)
        {

            // Taproot Addresses
            if (wallet.Address.StartsWith("tb1p") || wallet.Address.StartsWith("bc1p"))
                return await SendCoinsTaproot(wallet, addresses, bt);

            var utxo = await GetUtxoAsync(wallet.Address);

            if (utxo.TxIn.Length == 0)
            {
                bt.ErrorMessage = "No UTXO found for the address.";
                return bt;
            }

            Network network = GlobalFunctions.IsMainnet() ? Network.Main : Network.TestNet4;
            var privateKey = Key.Parse(wallet.privateskey, network);

            var wif = privateKey.GetWif(network);

            // 🔐 WIF-Private Key
            BitcoinSecret senderSecret = new BitcoinSecret(wif.ToString(), network);
            BitcoinAddress senderAddress = senderSecret.GetAddress(ScriptPubKeyType.Segwit);




            decimal feeRateSatPerVByte = await GetMempoolFeeRateAsync();


            var fee = Money.Satoshis(1000);
            // ✍️ Transaktion bauen
            var tx = CreateSendcoinTransaction(addresses, network, utxo, senderAddress, senderSecret, fee);

            int vsize = tx.GetVirtualSize();
            long newfee = (long)Math.Ceiling(feeRateSatPerVByte * vsize);

            var tx1 = CreateSendcoinTransaction(addresses, network, utxo, senderAddress, senderSecret, newfee);


            // 📤 Hex ausgeben
            var hex = tx1.ToHex();



            // 📤 Senden
            try
            {
                bt.TxHash = await SendTransaction(tx1, network);
                bt.SignedTransaction = hex;
                bt.ErrorMessage = "";
                bt.Fees = newfee;
            }
            catch (Exception ex)
            {
                bt.ErrorMessage = ex.Message;
            }

            return bt;
        }

        private Transaction CreateSendcoinTransaction(TxOutClass[] addresses, Network network, TxInAddressesClass utxo,
            BitcoinAddress senderAddress, BitcoinSecret senderSecret, long fee)
        {
            long sumtxin = 0;
            var tx = network.CreateTransaction();
            List<Coin> coins = new List<Coin>();
            foreach (var txIn in utxo.TxIn)
            {
                var outPoint = new OutPoint(uint256.Parse(txIn.TxHash), (int)txIn.TxId);
                tx.Inputs.Add(new TxIn(outPoint));
                sumtxin += txIn.Lovelace;
                var coin = new Coin(outPoint, new TxOut(Money.Satoshis(txIn.Lovelace), senderAddress.ScriptPubKey));
                coins.Add(coin);
            }

            long sumtxout = 0;
            foreach (var txout in addresses)
            {
                BitcoinAddress recipientAddress = BitcoinAddress.Create(txout.ReceiverAddress, network);
                tx.Outputs.Add(new TxOut(txout.Amount, recipientAddress));
                sumtxout += txout.Amount;
            }


            // 🔄 Change zurück, falls nötig
            var change = Money.Satoshis(sumtxin) - sumtxout - fee;
            if (change > Money.Zero)
            {
                tx.Outputs.Add(new TxOut(change, senderAddress));
            }

            // ✍️ Signieren
            tx.Sign(senderSecret, coins);
            return tx;
        }

        public async Task<BuildTransactionClass> SendCoinsTaproot(CreateNewPaymentAddressClass wallet,
            TxOutClass[] addresses, BuildTransactionClass bt)
        {
            var network = GlobalFunctions.IsMainnet() ? Network.Main : Network.TestNet4;
            var fee = Money.Satoshis(1000); // z.B. 500 Satoshi

            // Privater Schlüssel und Adresse
            var privateKey = Key.Parse(wallet.privateskey, network);

            var dest = BitcoinAddress.Create(wallet.Address, network);



            var utxo = await GetUtxoAsync(wallet.Address);


            // 2. Transaktion bauen
            var txBuilder = network.CreateTransactionBuilder();


            // Add Txin
            List<Coin> coins = (from txin in utxo.TxIn
                let s = new Script(Convert.FromHexString(txin.ScriptPubKey))
                select new Coin(new OutPoint(uint256.Parse(txin.TxHash), (int)txin.TxId),
                    new TxOut(Money.Satoshis(txin.Lovelace), s)
                )).ToList();


            txBuilder
                .AddCoins(coins)
                .AddKeys(privateKey)
                .SendFees(fee)
                .SetChange(dest);

            foreach (var txOutClass in addresses)
            {
                var recipientAddress = BitcoinAddress.Create(txOutClass.ReceiverAddress, network);
                var amountToSend = Money.Satoshis(txOutClass.Amount);
                txBuilder.Send(recipientAddress, amountToSend);
            }




            var tx = txBuilder.BuildTransaction(sign: true);

            /*     var txSize=txBuilder.EstimateSize(tx);
                 int feeRate = 20; // sat/Byte
                 long newfee = txSize * feeRate; // 2800 Satoshi

                 txBuilder.SendFees(Money.Satoshis(newfee));
                 tx = txBuilder.BuildTransaction(sign: true);
            */

            // 3. (Optional) Transaktion validieren
            bool valid = txBuilder.Verify(tx);
            Console.WriteLine("Transaktion gültig: " + valid);

            string tx1 = tx.ToString();
            string tx2 = tx.ToHex();
            var tx3 = tx.ToBytes();
            var tx4 = Encoding.ASCII.GetString(tx3, 0, tx3.Length);

            //   bt.TxHash=await SendTransaction(tx.ToBytes());
            var rpc = new RPCClient(
                new RPCCredentialString
                {
                    UserPassword = new NetworkCredential("meinusername", "meinpassword"), // <-- Deine Daten hier
                    Server = "http://185.51.184.85:48332" // Testnet Port
                },
                network
            );
            try
            {
                //   bt.TxHash = await SendTransaction(tx.ToBytes());


                uint256 txId = rpc.SendRawTransaction(tx);
                bt.TxHash = txId.ToString();
                bt.SignedTransaction = tx2;
                bt.ErrorMessage = "";
                bt.Fees = fee.Satoshi;
            }
            catch (Exception ex)
            {
                bt.ErrorMessage = ex.Message;
            }

            return bt;
        }

        public async Task<AssetsAssociatedWithAccount[]> GetAllAssetsInWalletAsync(IConnectionMultiplexer redis,
            string address)
        {
            // TODO
            return new AssetsAssociatedWithAccount[] { };
        }

        public BlockfrostAssetClass GetAsset(Nftproject project, Nft nft1)
        {
            // TODO
            return null;
        }

        public string GetMetadataFromCip25Metadata(string cip25metadata, Nftproject project)
        {
            try
            {
                IConvertCardanoMetadata conv = new ConvertCardanoToBitcoinMetadata();
                return conv.ConvertCip25CardanoMetadata(cip25metadata);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<string> GetLastSenderAddressAsync(string addressOrHash)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"),
                $"{GeneralConfigurationClass.MaestroBitcoinConfiguration.ApiUrl}/addresses/{addressOrHash}/txs?order=desc");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("api-key",
                GeneralConfigurationClass.MaestroBitcoinConfiguration.ApiKey);

            var response = await httpClient.SendAsync(request);

            var txhash = "";
            var address = "";
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                var res = JsonConvert.DeserializeObject<BitcoinAddressTxs>(responseString);
                txhash=res.Data.FirstOrDefault()?.TxHash;
            }

            if (!string.IsNullOrEmpty(txhash))
            {
                address=await GetLastSenderAddressAsyncFromTxHash(txhash);
            }

            return address;
        }

        private async Task<string> GetLastSenderAddressAsyncFromTxHash(string txhash)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"),
                $"{GeneralConfigurationClass.MaestroBitcoinConfiguration.ApiUrl}/transactions/{txhash}");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("api-key",
                GeneralConfigurationClass.MaestroBitcoinConfiguration.ApiKey);

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                var res = JsonConvert.DeserializeObject<BitcoinTransactionInfo>(responseString);
                var inputaddress = res.Data.Inputs.FirstOrDefault()?.Address;
                return inputaddress;
            }

            return "";
        }

        public Task<BuildTransactionClass> SendAllCoinsAndTokensFromNftaddress(Nftaddress address,
            string receiveraddress, BuildTransactionClass bt,
            string sendbackmessage)
        {
            throw new NotImplementedException();
        }

        public async Task<BuildTransactionClass> MintFromNftAddressCoreAsync(ulong amountOnAddress, Nftaddress address,
            string receiveraddress,
            Pricelistdiscount discount, Nftprojectsadditionalpayout[] additionalPayoutWallets, BuildTransactionClass bt)
        {
            var paywallet = ConvertToBlockchainKeysClass(address);
            // First Create the StringContent for the inscription

            var nfts=(from a in address.Nfttonftaddresses
                select a.Nft).ToArray();


        var stringContent = await GetStringContentForInscription(nfts, address.Nfttonftaddresses.First().Nft.Nftproject, receiveraddress);
            // Then create the Inscription and send it to Ordinalsbot
            bt = await CreateInscriptionAsync(stringContent, bt);
            // Then Get the Orderstatus
            if (!string.IsNullOrEmpty(bt.ErrorMessage) || bt.BitcoinInscribeResult == null)
                return bt;

            await Task.Delay(1000); // Wait a second to ensure the order is processed
            bt = await GetOrderStatusAsync(bt);

            if (bt.BitcoinInscribeResult.Charge.Amount == null || bt.BitcoinInscribeResult.Charge.Amount <= 0)
            {
                bt.ErrorMessage = "No charge amount found in the order status.";
                return bt;
            }

            ulong rest = amountOnAddress - 1000; // 1000 Satoshi are the minimum fee for a transaction

            // Fund the address with BTC 
            List<TxOutClass> txouts = new List<TxOutClass>
            {
                // To Ordinalsbot
                new TxOutClass
                {
                    ReceiverAddress = bt.BitcoinOrderstatusResult.Charge.Address,
                    Amount = bt.BitcoinOrderstatusResult.Charge.Amount ?? 0
                }
            };


            var mintingcosts = GlobalFunctions.GetMintingcosts2((int)address.NftprojectId, address.Nfttonftaddresses.Count,
                address.Price ?? 0);
            if (mintingcosts.CostsBitcoin != 0)
            {
                bt.MintingcostsTxOut = new TxOutClass()
                {
                    Amount = mintingcosts.CostsBitcoin,
                    ReceiverAddress = mintingcosts.MintingcostsreceiverBitcoin
                };
                txouts.Add(bt.MintingcostsTxOut);
            }

            // Add additional payout wallets
            bt.AdditionalPayouts = additionalPayoutWallets;
            foreach (var nftprojectsadditionalpayout in additionalPayoutWallets.OrEmptyIfNull())
            {
                ulong addvalue = GetAdditionalPayoutwalletsValueBitcoin(nftprojectsadditionalpayout,
                    amountOnAddress - (ulong)(bt.Discount ?? 0), address.Nfttonftaddresses.Count);
                if (addvalue <= 0) continue;

                var addval = new TxOutClass()
                {
                    Amount = (long)addvalue,
                    ReceiverAddress = nftprojectsadditionalpayout.Wallet.Walletaddress
                };
                txouts.Add(addval);

                nftprojectsadditionalpayout.Valuetotal = (long)addvalue;
            }

            // TODO: Discount


            rest -= (ulong)txouts.Sum(x => x.Amount);

            // Rest to the project wallet
            bt.ProjectTxOut = new TxOutClass()
            {
                Amount = (long)rest,
                ReceiverAddress = address.Nftproject.Bitcoincustomerwallet.Walletaddress
            };
            txouts.Add(bt.ProjectTxOut);

            bt = await SendCoins(new CreateNewPaymentAddressClass()
            {
                Address = paywallet.Address,
                SeedPhrase = paywallet.Seed,
                privateskey = paywallet.SecretKey,
                Blockchain = Blockchain.Bitcoin,
                privatevkey = paywallet.PublicKey
            }, txouts.ToArray(), bt);

            if (!string.IsNullOrEmpty(bt.TxHash))
            {
                bt.BuyerTxOut = new TxOutClass()
                {
                    Amount = bt.BitcoinOrderstatusResult.Charge.Amount ?? 0,
                    ReceiverAddress = receiveraddress,
                };
            }


            return bt;
        }
        public static ulong GetAdditionalPayoutwalletsValueBitcoin(Nftprojectsadditionalpayout nftprojectsadditionalpayout,
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
        public Task<CheckCustomerAddressAddressesClass[]> GetCustomersAsync(EasynftprojectsContext db,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public string CheckPolicyId(string policyid)
        {
            return String.Empty;
        }

        public Task<BuildTransactionClass> SendCoinsFromCustomerAddressAsync(Customer customer, TxOutClass[] addresses,
            BuildTransactionClass buildtransaction)
        {
            throw new NotImplementedException();
        }

        public async Task<TxInAddressesClass> GetUtxoAsync(string address)
        {
            TxInAddressesClass txInAddresses = new TxInAddressesClass();
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"),
                $"{GeneralConfigurationClass.MaestroBitcoinConfiguration.ApiUrl}/addresses/{address}/utxos?filter_dust=true");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("api-key",
                GeneralConfigurationClass.MaestroBitcoinConfiguration.ApiKey);

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                var res = JsonConvert.DeserializeObject<BitcoinGetUtxos>(responseString);

                foreach (var bitcoinGetUtxosDatum in res.Data)
                {
                    // TODO: Inscriptions / Ordinals
                    txInAddresses.AddTxIn(bitcoinGetUtxosDatum);
                }
            }

            return txInAddresses;
        }

       

        private async Task<string> SendTransaction(Transaction tx, Network network)
        {

            var rpc = new RPCClient(
                new RPCCredentialString
                {
                    UserPassword = new NetworkCredential("meinusername", "meinpassword"),
                    Server = "http://185.51.184.85:48332" // Testnet Port
                },
                network
            );

            uint256 txId = rpc.SendRawTransaction(tx);
            return txId.ToString();
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
                Price = adrreq.BitcoinSatoshi,
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
                Paymentmethod = Enums.Coin.BTC.ToString(),
                Freemint = adrreq.Freemint,
            };

            await db.Nftaddresses.AddAsync(newaddress);
            await db.SaveChangesAsync();

            return newaddress;
        }

    }
}
