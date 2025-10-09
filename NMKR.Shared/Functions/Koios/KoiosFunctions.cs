using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Blockfrost;
using NMKR.Shared.Classes.Koios;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions.Cli;
using NMKR.Shared.Functions.DbSync;
using NMKR.Shared.Functions.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuickType;
using StackExchange.Redis;
using Amount = NMKR.Shared.Classes.Blockfrost.Amount;

namespace NMKR.Shared.Functions.Koios
{
    public static class KoiosFunctions
    {
        private static Querytip _cachedQuerytip;
        private static DateTime _cachedQuerytipDateTime;


        private static HttpClientHandler IgnoreCertificate()
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
            return handler;
        }

        public static AssetTransactionsClass GetLastAssetTransaction(string policyid,
            string assetnameinhex)
        {
            var res = Task.Run(async () => await GetLastAssetTransactionAsync(policyid, assetnameinhex));
            return res.Result;
        }


        public static async Task<AssetTransactionsClass> GetLastAssetTransactionAsync(string policyid,
            string assetnameinhex)
        {

            using var httpClient = new HttpClient(IgnoreCertificate());
            httpClient.Timeout = TimeSpan.FromSeconds(20);
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.KoiosApi}/asset_txs?_asset_policy={policyid}&_asset_name={assetnameinhex}&_after_block_height=0&_history=false&order=block_time.desc&limit=1");
            request.Headers.TryAddWithoutValidation("accept", "application/json");

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responsestring = await response.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<AssetTransactionsClass[]>(responsestring);
                if (res == null)
                    return null;

                return res.FirstOrDefault();
            }

            return null;
        }
        public static KoiosNftAddressesClass[] GetNftAddress( string policyid,
            string assetnameinhex)
        {
            var res = Task.Run(async () => await GetNftAddressAsync(policyid, assetnameinhex));
            return res.Result;
        }

        public static async Task<KoiosNftAddressesClass[]> GetNftAddressAsync( string policyid, string assetnameinhex)
        {
            using var httpClient = new HttpClient(IgnoreCertificate());
            httpClient.Timeout = TimeSpan.FromSeconds(20);
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.KoiosApi}/asset_nft_address?_asset_policy={policyid}&_asset_name={assetnameinhex}");
            request.Headers.TryAddWithoutValidation("accept", "application/json");

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responsestring = await response.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<KoiosNftAddressesClass[]>(responsestring);
                return res;
            }

            return null;
        }
        public static Querytip GetQueryTip()
        {
            var res = Task.Run(async () => await GetQueryTipAsync());
            return res.Result;
        }

        public static async Task<Querytip> GetQueryTipAsync()
        {
            if (_cachedQuerytip != null && _cachedQuerytipDateTime > DateTime.Now.AddSeconds(-10))
                return _cachedQuerytip;

            try
            {
                using var httpClient = new HttpClient(IgnoreCertificate());
                httpClient.Timeout = TimeSpan.FromSeconds(20);
                using var request =
                    new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.KoiosApi}/tip");
                request.Headers.TryAddWithoutValidation("Accept", "application/json");

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    KoiosQueryTipClass[] kqtc =
                        JsonConvert.DeserializeObject<KoiosQueryTipClass[]>(responseString);
                    if (kqtc != null && kqtc.Any())
                    {
                        Querytip qt2 = new Querytip()
                        {
                            Block = (long) kqtc.First().BlockNo,
                            Epoch = kqtc.First().EpochNo,
                            Slot = (long) kqtc.First()?.AbsSlot,
                            Hash = kqtc.First().Hash
                        };
                        return qt2;
                    }

                }
            }
            catch
            {

            }

            // Fallback over cli

            BuildTransactionClass bt = new BuildTransactionClass();
            var qt = CliFunctions.GetQueryTipFromCli(GlobalFunctions.IsMainnet(), ref bt);
            _cachedQuerytip = qt;
            _cachedQuerytipDateTime = DateTime.Now;
            return qt;
        }

       

        public static KoiosAccountAssetListClass[] GetAccountAssetList(string stakeaddress)
        {
            var res = Task.Run(async () => await GetAccountAssetListAsync(stakeaddress));
            return res.Result;
        }


        public static async Task<KoiosAccountAssetListClass[]> GetAccountAssetListAsync(string stakeaddress)
        {
            List<KoiosAccountAssetListClass> result = new List<KoiosAccountAssetListClass>();
            int offset = 0;

            do
            {
                using var httpClient = new HttpClient(IgnoreCertificate());
                httpClient.Timeout = TimeSpan.FromSeconds(20);
                using var request = new HttpRequestMessage(new HttpMethod("POST"),
                    $"{GeneralConfigurationClass.KoiosApi}/account_assets" + (offset != 0 ? $"&offset={offset}" : ""));
                request.Headers.TryAddWithoutValidation("accept", "application/json");

                request.Content = new StringContent("{\"_stake_addresses\":[\"" + stakeaddress + "\"]}");
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responsestring = await response.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<KoiosAccountAssetListClass[]>(responsestring);
                    if (res == null || !res.Any())
                        break;
                    result.AddRange(res);
                    if (res.Length < 1000)
                        break;
                    offset += 1000;
                }
            } while (true);

            return result.ToArray();
        }


        public static AssetsAssociatedWithAccount[] ToAssetsAssociatedWithAccount(
            this KoiosAccountAssetListClass[] assets)
        {
            List<AssetsAssociatedWithAccount> res = new List<AssetsAssociatedWithAccount>();
            foreach (var koiosAccountAssetListClass in assets)
            {
                foreach (var assetList in koiosAccountAssetListClass.AssetList)
                {
                 res.Add(new AssetsAssociatedWithAccount(assetList.PolicyId,assetList.AssetName, assetList.Quantity, Blockchain.Cardano));   
                }
            }

            return res.ToArray();
        }




        public static async Task<KoiosAssetListClass> GetPolicyidAndAssetnameFromFingerprintAsync(string fingerprint)
        {
            using var httpClient = new HttpClient(IgnoreCertificate());
            httpClient.Timeout = TimeSpan.FromSeconds(20);
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.KoiosApi}/asset_list?fingerprint=eq.{fingerprint}");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responsestring= await response.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<KoiosAssetListClass[]>(responsestring);
                if (res.Any())
                    return res.First();
            }

            return null;
        }
        public static SubmissionResultClass SubmitTransactionFileViaKoios(string matxsignedfile)
        {
            var res = Task.Run(async () => await SubmitTransactionFileAsync(matxsignedfile));
            return res.Result;
        }
        public static SubmissionResultClass SubmitTransactionViaKoios(byte[] signedTx)
        {
            var res = Task.Run(async () => await SubmitTransactionAsync(signedTx));
            return res.Result;
        }

        public static async Task<SubmissionResultClass> SubmitTransactionFileAsync(string matxsignedfile)
        {
            SubmissionResultClass res=new SubmissionResultClass();

            if (!File.Exists(matxsignedfile))
                return res;

            string signedTxStr = ConsoleCommand.GetCbor(await File.ReadAllTextAsync(matxsignedfile));

            return await SubmitTransactionAsync(Convert.FromHexString(signedTxStr));
        }
        public static async Task<SubmissionResultClass> SubmitTransactionAsync(byte[] signedTx)
        {
            SubmissionResultClass res = new SubmissionResultClass();

            if (signedTx==null || signedTx.Length==0)
                return new SubmissionResultClass() {ErrorMessage = "No CBOR found", Success = false};


            try
            {
                using var httpClient = new HttpClient(IgnoreCertificate());
                httpClient.Timeout = TimeSpan.FromSeconds(20);
                using var request = new HttpRequestMessage(new HttpMethod("POST"),
                    $"{GeneralConfigurationClass.KoiosApi}/submittx");
                await using var ms = new MemoryStream(signedTx);

                request.Content = new StreamContent(ms);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/cbor");

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    res.TxHash = await response.Content.ReadAsStringAsync();
                    res.Success = true;
                    return res;
                }
                else
                {
                    res.ErrorMessage = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                res.ErrorMessage = ex.Message;
            }

            return res;
        }
        public static RoyaltyClass GetRoyaltiesFromPolicyId(string policyId)
        {
            var res = Task.Run(async () => await GetRoyaltiesFromPolicyIdAsync(policyId));
            return res.Result;
        }

        public static async Task<RoyaltyClass> GetRoyaltiesFromPolicyIdAsync(string policyId)
        {
            var assets = await GetAssetInformationAsync(policyId, "");

            if (assets == null || !assets.Any())
                return null;

            try
            {
                var asset = assets.FirstOrDefault();
                if (asset != null)
                {
                    var str = asset.MintingTxMetadata.ToString();
                    if (string.IsNullOrEmpty(str))
                        return null;
                    KoiosOldRoyaltyTokenClass kortc = JsonConvert.DeserializeObject<KoiosOldRoyaltyTokenClass>(str);
                    if (kortc != null)
                    {
                        float perc = 0;
                        if (!string.IsNullOrEmpty(kortc.The777.Rate))
                            perc = (float) GlobalFunctions.ConvertToDouble(kortc.The777.Rate) * 100f;

                        if (!string.IsNullOrEmpty(kortc.The777.Pct))
                            perc = (float) GlobalFunctions.ConvertToDouble(kortc.The777.Pct) * 100f;

                        if (perc == 0)
                            return null;

                        RoyaltyClass rc = new RoyaltyClass()
                        {
                            Address = kortc.The777.Addr,
                            Percentage = perc,
                            Pkh = GlobalFunctions.GetPkhFromAddress(kortc.The777.Addr)
                        };
                        return string.IsNullOrEmpty(rc.Pkh) ? null : rc;
                    }
                }
            }
            catch
            {

            }

            return null;
        }

        public static KoiosAssetInformationClass[] GetAssetInformation(string policyid,
            string assetnameinhex)
        {
            var res = Task.Run(async () => await GetAssetInformationAsync(policyid, assetnameinhex));
            return res.Result;
        }

        public static async Task<KoiosAssetInformationClass[]> GetAssetInformationAsync(string policyid,
            string assetnameinhex)
        {
            try
            {
                using var httpClient = new HttpClient(IgnoreCertificate());
            httpClient.Timeout = TimeSpan.FromSeconds(20);

            string url = $"{GeneralConfigurationClass.KoiosApi}/asset_info";

            using var request = new HttpRequestMessage(new HttpMethod("POST"), url);
            request.Headers.TryAddWithoutValidation("accept", "application/json");


                // https://api.koios.rest/api/v1/asset_token_registry?policy_id=eq.97bbb7db0baef89caefce61b8107ac74c7a7340166b39d906f174bec&asset_name_ascii=eq.Agent

                request.Content = new StringContent($"{{\"_asset_list\":[[\"{policyid}\",\"{assetnameinhex}\"]]}}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                    var assets1 = JsonConvert.DeserializeObject<KoiosAssetInformationClass[]>(responseString);
                    return assets1;
            }
            }
            catch (Exception e)
            {
                await GlobalFunctions.LogExceptionAsync(null, "Exception on GetAssetInformationAsync", GeneralConfigurationClass.KoiosApi+"/asset_info "+Environment.NewLine+
                    policyid + " " + assetnameinhex + Environment.NewLine + e.Message+Environment.NewLine+(e.StackTrace??"") + Environment.NewLine+Environment.NewLine+"");
                return null;
            }
            return null;
        }
        public static TokenInformationClass[] GetTokenInformation(string policyid,
            string assetnameinhex)
        {
            var res = Task.Run(async () => await GetTokenInformationAsync(policyid, assetnameinhex));
            return res.Result;
        }

        public static async Task<TokenInformationClass[]> GetTokenInformationAsync(string policyid,
            string assetnameinhex)
        {
            try
            {
                using var httpClient = new HttpClient(IgnoreCertificate());
                httpClient.Timeout = TimeSpan.FromSeconds(20);

                   string url = $"{GeneralConfigurationClass.KoiosApi}/asset_token_registry?policy_id=eq.{policyid}&asset_name=eq.{assetnameinhex}";
                //string url = $"https://api.koios.rest/api/v1/asset_token_registry?policy_id=eq.{policyid}&asset_name_ascii=eq.{assetnameinhex.FromHex()}";

                using var request = new HttpRequestMessage(new HttpMethod("GET"), url);
                request.Headers.TryAddWithoutValidation("accept", "application/json");

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    var assets1 = JsonConvert.DeserializeObject<TokenInformationClass[]>(responseString);
                    return assets1;
                }
            }
            catch (Exception e)
            {
                await GlobalFunctions.LogExceptionAsync(null, "Exception on GetAssetInformationAsync", GeneralConfigurationClass.KoiosApi + "/asset_info " + Environment.NewLine +
                    policyid + " " + assetnameinhex + Environment.NewLine + e.Message + Environment.NewLine + (e.StackTrace ?? "") + Environment.NewLine + Environment.NewLine + "");
                return null;
            }
            return null;
        }






        public static BlockfrostAssetClass ToBlockfrostAssetClass(this KoiosAssetInformationClass[] assets)
        {
            if (assets==null || !assets.Any())
                return null;

            BlockfrostAssetClass bfa = new BlockfrostAssetClass()
            {
                Asset = assets.First().AssetNameAscii,
                Quantity = (assets.First().MintCnt ?? 0) - (assets.First().BurnCnt ?? 0),
                Fingerprint = assets.First().Fingerprint, AssetName = assets.First().AssetName,
                InitialMintTxHash = assets.First().MintingTxHash,
                Metadata = assets.First().MintingTxMetadata, PolicyId = assets.First().PolicyId,
                MintOrBurnCount = (assets.First().MintCnt ?? 0), OnchainMetadata = assets.First().MintingTxMetadata,
                OnchainMetadataExtra = assets.First().TokenRegistryMetadata, Blockchain = Blockchain.Cardano
            };
            return bfa;
        }


        public static string GetFirstAssetAddressList(string policyid, string assetnameinhex)
        {
            var res = Task.Run(async () => await GetFirstAssetAddressListAsync(policyid,assetnameinhex));
            return res.Result;
        }

        public static async Task<string> GetFirstAssetAddressListAsync(string policyid, string assetnameinhex)
        {
            using var httpClient = new HttpClient(IgnoreCertificate());
            httpClient.Timeout = TimeSpan.FromSeconds(20);
            string url= $"{GeneralConfigurationClass.KoiosApi}/asset_addresses?_asset_policy={policyid}&_asset_name={assetnameinhex}";
            using var request = new HttpRequestMessage(new HttpMethod("GET"), url);
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                try
                {
                    var assets1 = JsonConvert.DeserializeObject<KoiosAssetAddressListClass[]>(responseString);
                    if (assets1 != null && assets1.Any())
                        return assets1.First().PaymentAddress;
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }



        public static async Task<KoiosAssetPolicyInformationClass[]> GetAssetPolicyInformationAsync(string policyid)
        {
            using var httpClient = new HttpClient(IgnoreCertificate());
            httpClient.Timeout = TimeSpan.FromSeconds(20);
            using var request = new HttpRequestMessage(new HttpMethod("GET"),
                $"{GeneralConfigurationClass.KoiosApi}/asset_policy_info?_asset_policy={policyid}");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                try
                {
                    var assets1 = JsonConvert.DeserializeObject<KoiosAssetPolicyInformationClass[]>(responseString);
                    return assets1;
                }
                catch 
                {
                    return null;
                }
            }

            return null;
        }


      

        public static KoiosAccountInfoClass GetStakePoolInformation(IConnectionMultiplexer redis, string address)
        {
            var res = Task.Run(async () => await GetStakePoolInformationAsync(redis,address));
            return res.Result;
        }



        public static async Task<KoiosAccountInfoClass> GetStakePoolInformationAsync(IConnectionMultiplexer redis, string address)
        {
            var cachedStakepoolJsonString = GlobalFunctions.GetStringFromRedis(redis, $"GetStakePoolInformationAsync_{address}");
            if (!string.IsNullOrEmpty(cachedStakepoolJsonString))
            {
                return JsonConvert.DeserializeObject<KoiosAccountInfoClass>(cachedStakepoolJsonString);
            }

            try
            {
                string stakeAddress = Bech32Engine.GetStakeFromAddress(address);
                if (string.IsNullOrEmpty(stakeAddress))
                    return null;


                using var httpClient = new HttpClient(IgnoreCertificate());
                using var request = new HttpRequestMessage(new HttpMethod("POST"),
                    $"{GeneralConfigurationClass.KoiosApi}/account_info");
                request.Headers.TryAddWithoutValidation("Accept", "application/json");

                request.Content = new StringContent($"{{\"_stake_addresses\":[\"{stakeAddress}\"]}}");
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var infos = JsonConvert.DeserializeObject<KoiosAccountInfoClass[]>(responseString);
                        if (infos != null && infos.Any())
                        {
                            var json = JsonConvert.SerializeObject(infos.First());
                            GlobalFunctions.SaveStringToRedis(redis, $"GetStakePoolInformationAsync_{address}", json, 3600);

                            return infos.First();
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }

            catch
            {
                return null;
            }

            return null;
        }

        public static string GetSender(string txhash)
        {
            var res = Task.Run(async () => await GetSenderAsync(txhash));
            return res.Result;
        }

        public static async Task<string> GetSenderAsync(string txhash)
        {
            var transactions = await GetTransactionInformationAsync(txhash);
            if (transactions == null)
                return null;

            if (!transactions.Any())
                return null;

            if (!transactions.First().Inputs.Any())
                return "";
            return transactions.First().Inputs.First().PaymentAddr.Bech32;
        }

        public static long GetTransactionFromKoios(string txhash)
        {
            var res = Task.Run(async () => await GetTransactionFromKoiosAsync(txhash));
            return res.Result;
        }
        public static async Task<long> GetTransactionFromKoiosAsync(string txhash)
        {
            txhash = txhash.Replace("\n", "").Replace("\r", "");
            using var httpClient = new HttpClient(IgnoreCertificate());
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.KoiosApi}/tx_status");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            request.Content = new StringContent($"{{\"_tx_hashes\":[\"{txhash}\"]}}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                try
                {
                    var infos = JsonConvert.DeserializeObject<KoiosTransactionStatusClass[]>(responseString);
                    if (infos != null && infos.Any())
                    {
                        return infos.First().NumConfirmations??0;
                    }
                }
                catch
                {
                    return 0;
                }
            }

            return 0;
        }

        public static async Task<KoiosAddressTransactionsClass[]> GetAllTransactionsForSpecificAddressesAsync(KoiosGetTransactionsAddressesClass addresses)
        {
            using var httpClient = new HttpClient(IgnoreCertificate());
            httpClient.Timeout = TimeSpan.FromSeconds(20);
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.KoiosApi}/address_txs?order=block_time.desc");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            request.Content = new StringContent(JsonConvert.SerializeObject(addresses));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                var addresstx = JsonConvert.DeserializeObject<KoiosAddressTransactionsClass[]>(responseString);
                return addresstx;
            }

            return null;
        }

        public static KoiosPolicyAssetsClass[] GetAllAssetsFromPolicyid(IConnectionMultiplexer redis, string policyId)
        {
            var res = Task.Run(async () => await GetAllAssetsFromPolicyidAsync(redis,policyId));
            return res.Result;
        }

        public static async Task<KoiosPolicyAssetsClass[]> GetAllAssetsFromPolicyidAsync(IConnectionMultiplexer redis, string policyId)
        {
            string redisKey = $"GetAllAssetsFromPolicyidAsync_{policyId}";
            var cachedAssetsJsonString = GlobalFunctions.GetStringFromRedis(redis, redisKey);
            if (!string.IsNullOrEmpty(cachedAssetsJsonString))
            {
                var cached= JsonConvert.DeserializeObject<KoiosPolicyAssetsClass[]>(cachedAssetsJsonString);
                if (cached.Any())
                    return cached;
            }


            int offset = 0;
            int limit = 1000;
            List<KoiosPolicyAssetsClass> assets = new List<KoiosPolicyAssetsClass>();
            do
            {
                using (var httpClient = new HttpClient(IgnoreCertificate()))
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(30);
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"),
                               $"{GeneralConfigurationClass.KoiosApi}/policy_asset_info?_asset_policy={policyId}&offset={offset}&limit={limit}"))
                    {
                        var response = await httpClient.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            string responseString = await response.Content.ReadAsStringAsync();
                            var assets1 = JsonConvert.DeserializeObject<KoiosPolicyAssetsClass[]>(responseString);

                            if (assets1 != null && assets1.Any())
                            {
                                assets.AddRange(assets1);

                                if (assets1.Length < limit)
                                    break;
                            }
                            else
                                break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                offset += limit;
            } while (true);

            var json = JsonConvert.SerializeObject(assets.ToArray());
            GlobalFunctions.SaveStringToRedis(redis, redisKey, json, 120);


            return assets.ToArray();
        }


        internal static TxInAddressesClass ToTxInAddresses(this KoiosAddressInformationClass addressInformation, string address)
        {
            TxInAddressesClass txout = new TxInAddressesClass()
            {
                Address = address,
                DataProvider = Dataproviders.Koios,
                StakeAddress = Bech32Engine.GetStakeFromAddress(address),
                TxIn = new TxInClass[] { }
            };


            if (addressInformation != null)
            {
                var inf = addressInformation;
                if (inf.UtxoSet != null && inf.UtxoSet.Any())
                {
                    //  var transactions = GetTimeStampFromKoios(inf.UtxoSets.Select(x => x.TxHash).ToList(), mainnet);

                    txout.StakeAddress = inf.StakeAddress;
                    List<TxInClass> txinclass = new List<TxInClass>();
                    foreach (var utxoSet in inf.UtxoSet.OrderBy(x => x.BlockTime))
                    {

                        TxInClass tx1 = new TxInClass()
                        {
                            Lovelace = utxoSet.Value ?? 0,
                            TxId = (int)utxoSet.TxIndex,
                            TxHash = utxoSet.TxHash,
                            //  TXTimestamp = transactions.FirstOrDefault(x=>x.TxHash==utxoSet.TxHash).TxTimestamp
                        };

                        List<TxInTokensClass> tokens = new List<TxInTokensClass>();
                        foreach (var asset in utxoSet.AssetList.OrEmptyIfNull())
                        {
                            TxInTokensClass tok = new TxInTokensClass()
                            {
                                PolicyId = asset.PolicyId,
                                Quantity = asset.Quantity ?? 0,
                                TokennameHex = asset.AssetName,
                                Tokenname = GlobalFunctions.FromHexString(asset.AssetName)
                            };
                            tokens.Add(tok);
                        }

                        tx1.Tokens = tokens.ToList();
                        txinclass.Add(tx1);
                    }

                    txout.TxIn = txinclass.ToArray();
                }
            }

            return txout;
        }
        public static async Task<KoiosTransactionClass[]> GetTransactionInformationAsync(string txhash)
        {
            if (string.IsNullOrEmpty(txhash))
                return null;
            txhash = txhash.Replace("\n", "").Replace("\r", "");

            // Filter out the txid
            string[] txhashes = txhash.Split('#');

            using var httpClient = new HttpClient(IgnoreCertificate());
            httpClient.Timeout = TimeSpan.FromSeconds(20);
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.KoiosApi}/tx_info");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            KoiosTxhash kth = new KoiosTxhash() { TxHashes = new[] { txhashes.First() } };
            string jsonString = JsonConvert.SerializeObject(kth);
            request.Content = new StringContent(jsonString);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string rep = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(rep))
                    return null;


                KoiosTransactionClass[] tx = JsonConvert.DeserializeObject<KoiosTransactionClass[]>(rep);
                return tx;
            }

            return null;
        }

        public static BlockfrostTransaction ToBlockfrostTransaction(this KoiosTransactionClass[] transactions)
        {
            if (transactions== null || !transactions.Any())
                return null;

            BlockfrostTransaction res = new BlockfrostTransaction()
            {
                Block = transactions.First().BlockHash, 
                AssetMintOrBurnCount = transactions.First().AssetsMinted.Length,
                BlockHeight = transactions.First().BlockHeight,
                BlockTime = transactions.First().TxTimestamp, 
                Fees = transactions.First().Fee,
                Hash = transactions.First().TxHash,
                Index = transactions.First().TxBlockIndex, 
                InvalidBefore = transactions.First().InvalidBefore,
                InvalidHereafter = transactions.First().InvalidAfter,
                Size = transactions.First().TxSize, 
                Slot = transactions.First().AbsoluteSlot, 
                UtxoCount = transactions.First().Outputs.Length,
            };

            return res;
        }
        public static GenericTransaction ToGenericTransaction(this KoiosTransactionClass[] transactions)
        {
            if (transactions == null || !transactions.Any())
                return null;

            GenericTransaction res = new GenericTransaction()
            {
                Block = transactions.First().BlockHash,
                Fees = transactions.First().Fee,
                Hash = transactions.First().TxHash,
                Index = transactions.First().TxBlockIndex,
                Blockchain = Blockchain.Cardano,
            };

            return res;
        }

        public static async Task<KoiosDatumInformationClass[]> GetDatumInformationNewAsync(string datumhash)
        {
            if (string.IsNullOrEmpty(datumhash))
                return null;
            datumhash = datumhash.Replace("\n", "").Replace("\r", "");

            // Filter out the txid
            string[] datumhashes = datumhash.Split('#');

            using var httpClient = new HttpClient(IgnoreCertificate());
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.KoiosApi}/datum_info");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            KoiosDatumhash kth = new KoiosDatumhash() { DatumHashes = new[] { datumhashes.First() } };
            string jsonString = JsonConvert.SerializeObject(kth);
            request.Content = new StringContent(jsonString);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string rep = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(rep))
                    return null;


                KoiosDatumInformationClass[] tx = JsonConvert.DeserializeObject<KoiosDatumInformationClass[]>(rep);
                return tx;
            }

            return null;
        }
        public static async Task<KoiosJpgStoreMetadataClass[]> GetMetadataAsync(string txhash)
        {
            if (string.IsNullOrEmpty(txhash))
                return null;
            txhash = txhash.Replace("\n", "").Replace("\r", "");

            // Filter out the txid
            string[] txhashes = txhash.Split('#');

            using var httpClient = new HttpClient(IgnoreCertificate());
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.KoiosApi}/tx_metadata");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            KoiosTxhash kth = new KoiosTxhash() { TxHashes = new[] { txhashes.First() } };
            string jsonString = JsonConvert.SerializeObject(kth);
            request.Content = new StringContent(jsonString);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string rep = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(rep))
                    return null;


                KoiosJpgStoreMetadataClass[] tx = JsonConvert.DeserializeObject<KoiosJpgStoreMetadataClass[]>(rep);
                return tx;
            }

            return null;
        }

        public static EpochInformationFallback GetEpochInformation()
        {
            var res = Task.Run(async () => await GetEpochInformationAsync());
            return res.Result;
        }
        public static async Task<EpochInformationFallback> GetEpochInformationAsync()
        {

            var tip = await GetQueryTipAsync();


            string url = (tip == null || tip.Epoch==null)? $"{GeneralConfigurationClass.KoiosApi}/epoch_params" : $"{GeneralConfigurationClass.KoiosApi}/epoch_params?_epoch_no={tip.Epoch}";

            using var httpClient = new HttpClient(IgnoreCertificate());
            httpClient.Timeout = TimeSpan.FromSeconds(20);
            using var request = new HttpRequestMessage(new HttpMethod("GET"), url);
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                var epochInformation = JsonConvert.DeserializeObject<EpochInformationFallback[]>(responseString);

                if (epochInformation != null && epochInformation.Any())
                {
                    return epochInformation.OrderByDescending(x => x.EpochNo).FirstOrDefault(x => x.MinFeeA != null);
                }
            }

            return null;
        }


        public static async Task<MultiplierClass> GetFtTokensMultiplierAsync(string policyid, string assetnameinhex)
        {
            MultiplierClass res = new MultiplierClass();
            if (string.IsNullOrEmpty(policyid))
                return res;

            if (string.IsNullOrEmpty(assetnameinhex))
                return res;

            var assetinfo = await GetTokenInformationAsync(policyid, assetnameinhex);
            if (assetinfo == null || !assetinfo.Any())
                return res;


            if (assetinfo.First().Decimals == null)
                return res;

            var decimals = assetinfo.First().Decimals;
            if (decimals != null)
            {
                res.Decimals = decimals??0;
                res.Multiplier= (long)Math.Pow(10f, (double)decimals);
            }

            return res;
        }
        public static MultiplierClass GetFtTokensMultiplier(string policyid, string assetnameinhex)
        {
            var res = Task.Run(async () => await GetFtTokensMultiplierAsync(policyid,assetnameinhex));
            return res.Result;
        }



        public static async Task<KoiosAddressInformationClass> GetUtxoAsync(string address)
        {
            using var httpClient = new HttpClient(IgnoreCertificate());
            httpClient.Timeout = TimeSpan.FromSeconds(10);
            using var request = new HttpRequestMessage(new HttpMethod("POST"),address== "addr1vxrmu3m2cc5k6xltupj86a2uzcuq8r4nhznrhfq0pkwl4hgqj2v8w" ? $"{GeneralConfigurationClass.KoiosApi}/address_info_nmkr" : $"{GeneralConfigurationClass.KoiosApi}/address_info");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            string r = $"{{\"_addresses\":[\"{address}\"]}}";
            request.Content = new StringContent(r);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode) throw new Exception(response.ReasonPhrase);
            string responseString = await response.Content.ReadAsStringAsync();
            var epochInformation = JsonConvert.DeserializeObject<KoiosAddressInformationClass[]>(responseString);

            if (epochInformation != null && epochInformation.Any())
            {
                return epochInformation.First();
            }

            return new KoiosAddressInformationClass();

        }
        public static async Task<string> GetTransactionIdAsync(string address)
        {
            var res = await GetAddressTransactionsAsync(address);
            if (res == null) 
                return null;

            return res.TxHash;
        }

        public static async Task<KoiosAddressTransactionsClass> GetAddressTransactionsCachedAsync(IConnectionMultiplexer redis, string address)
        {
            var cachedAssetsJsonString = GlobalFunctions.GetStringFromRedis(redis, $"GetAddressTransactionsFromKoiosAsync_{address}");
            if (!string.IsNullOrEmpty(cachedAssetsJsonString))
            {
                return JsonConvert.DeserializeObject<KoiosAddressTransactionsClass>(cachedAssetsJsonString);
            }

            var res = await GetAddressTransactionsAsync(address);
            if(res!=null)
                GlobalFunctions.SaveStringToRedis(redis, $"GetAddressTransactionsFromKoiosAsync_{address}", JsonConvert.SerializeObject(res), 3600);

            return res;
        }


        public static async Task<KoiosAddressTransactionsClass> GetAddressTransactionsAsync(string address)
        {
            using var httpClient = new HttpClient(IgnoreCertificate());
            httpClient.Timeout = TimeSpan.FromSeconds(20);
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.KoiosApi}/address_txs?order=block_time.asc&limit=1");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            request.Content = new StringContent($"{{\"_addresses\":[\"{address}\"]}}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                var addresstx = JsonConvert.DeserializeObject<KoiosAddressTransactionsClass[]>(responseString);

                if (addresstx != null && addresstx.Any())
                {
                    return addresstx.First();
                }
            }

            return null;
        }


        public static async Task<long?> GetSlotAsync()
        {
            var tip = await GetQueryTipAsync();
            return tip?.Slot;
        }

        public static async Task<KoiosAccountAddressesClass[]> GetAllAddressesWithThisStakeAddressAsync(IConnectionMultiplexer redis, string stakeAddress)
        {
            var cachedAssetsJsonString = GlobalFunctions.GetStringFromRedis(redis, $"GetAllAddressesWithThisStakeAddressAsync_{stakeAddress}");
            if (!string.IsNullOrEmpty(cachedAssetsJsonString))
            {
                return JsonConvert.DeserializeObject<KoiosAccountAddressesClass[]>(cachedAssetsJsonString);
            }

            using var httpClient = new HttpClient(IgnoreCertificate());
            httpClient.Timeout = TimeSpan.FromSeconds(20);
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.KoiosApi}/account_addresses");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");

            request.Content = new StringContent($"{{\"_stake_addresses\":[\"{stakeAddress}\"]}}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;
            string rep = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(rep))
                return null;

            KoiosAccountAddressesClass[] tx = JsonConvert.DeserializeObject<KoiosAccountAddressesClass[]>(rep);
            var json = JsonConvert.SerializeObject(tx);
            GlobalFunctions.SaveStringToRedis(redis, $"GetAllAddressesWithThisStakeAddressAsync_{stakeAddress}", json, 3600);

            return tx;

        }

        public static GetAddressesFromStakeClass[] ToGetAddressesFromStakeClass(
            this KoiosAccountAddressesClass[] adresses)
        {
            List<GetAddressesFromStakeClass> res = new List<GetAddressesFromStakeClass>();
            foreach (var koiosAccountAddressesClass in adresses)
            {
                foreach (var address in koiosAccountAddressesClass.Addresses)
                {
                    res.Add(new GetAddressesFromStakeClass(){Address = address});
                }
            }

            return res.ToArray();
        }

        public static async Task<string> GetIpfsHashFromMetadata(string policyid, string tokennamehex)
        {
            string ipfs = "";
            try
            {
                var metadata = await GetMetadataAsync(policyid, tokennamehex);
                if (string.IsNullOrEmpty(metadata))
                    return null;

                var metadata1 = MetadataParsingHelper.ParseNormalMetadata(metadata, policyid, tokennamehex.FromHex());
                if (metadata1.PreviewImage != null)
                    ipfs = metadata1.PreviewImage.Hash;

                if (metadata1.PreviewImage != null && string.IsNullOrEmpty(metadata1.PreviewImage.Url) &&
                    metadata1.Files.Any())
                {
                    ipfs = metadata1.Files.First().Hash;
                }

            }
            catch
            {
                return null;
            }

            return ipfs;
        }
        public static async Task<string> GetMetadataFromFingerprintAsync(string fingerprint)
        {
            var asset=await DbSyncFunctions.GetPolicyidAndAssetnameFromFingerprintAsync(fingerprint);
            if (asset == null)
                return null;
            return await GetMetadataAsync(asset.PolicyId, asset.AssetName);
        }
        public static async Task<string> GetMetadataAsync(string policyid, string tokennamehex)
        {
            try
            {
                // Catch Metadata from the Token
                var ai = await GetAssetInformationAsync(policyid, tokennamehex);
                if (ai != null && ai.Any())
                {
                    var metadata = ai.First().MintingTxMetadata?.ToString();
                    if (string.IsNullOrEmpty(metadata))
                    {
                        metadata = ai.First().Cip68Metadata?.ToString();
                        if (!string.IsNullOrEmpty(metadata))
                        {
                            JObject jsonObject = JObject.Parse(metadata); // Konvertieren Sie den JSON-String in ein JObject
                            JToken firstElement = jsonObject.First?.First; // Holen Sie sich das erste Element des JObject

                            metadata = firstElement.ToString();
                        }
                    }
                    else
                    {
                        metadata = MetadataParsingHelper.GetMetadataForSpecificToken(metadata, policyid, tokennamehex);
                    }

                    return metadata;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }


        public static async Task<BlockfrostAddressInformationClass> GetAddressInformationAsync(string addressAddress)
        {
            var utxo = await GetUtxoAsync(addressAddress);
            List<Amount> amount = new List<Amount>();
            amount.Add(new Amount(){Quantity = utxo.Balance, Unit = "lovelace"});
            BlockfrostAddressInformationClass res = new BlockfrostAddressInformationClass()
            {
                Address = addressAddress, Amount = amount.ToArray(), StakeAddress = utxo.StakeAddress,
                Script = utxo.ScriptAddress
            };
            return res;
        }
    }
}
