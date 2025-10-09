using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Blockfrost;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuickType;
using StackExchange.Redis;

namespace NMKR.Shared.Functions.Blockfrost
{
    public static class BlockfrostFunctions
    {
        public static async Task<SubmissionResultClass> SubmitTransactionFileAsync(string matxsignedfile)
        {
            if (!File.Exists(matxsignedfile))
                return new SubmissionResultClass() {ErrorMessage = matxsignedfile+" does not exists", Success = false};

            string signedTxStr = ConsoleCommand.GetCbor(await File.ReadAllTextAsync(matxsignedfile));
            return await SubmitTransactionAsync(Convert.FromHexString(signedTxStr));
        }

        public static SubmissionResultClass SubmitTransaction(byte[] signedTx)
        {
            var res = Task.Run(async () => await SubmitTransactionAsync(signedTx));
            return res.Result;
        }
        public static async Task<SubmissionResultClass> SubmitTransactionAsync(byte[] signedTx)
        {
            if (signedTx == null || signedTx.Length == 0)
                return new SubmissionResultClass() { ErrorMessage = "SignedTx is null", Success = false };

            try
            {
                using var httpClient = new HttpClient();
                using var request =
                    new HttpRequestMessage(new("POST"), $"{GeneralConfigurationClass.BlockfrostUrl}tx/submit");
                request.Headers.TryAddWithoutValidation("project_id", GeneralConfigurationClass.BlockfrostApikey);

                await using var ms = new MemoryStream(signedTx);

                request.Content = new StreamContent(ms);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/cbor");

                var response = await httpClient.SendAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                    return new SubmissionResultClass()
                        { ErrorMessage = "Statuscode: " + response.StatusCode, Success = false };

                string responseString = response.Content.ReadAsStringAsync().Result;
                return new SubmissionResultClass() { Success = true, TxHash = responseString };
            }
            catch (Exception e)
            {
                return new SubmissionResultClass() { ErrorMessage = e.Message + Environment.NewLine + e.InnerException?.StackTrace + Environment.NewLine, Success = false };
            }
        }

        

        public static BlockfrostTransaction GetTransactionInformation(string txhash)
        {
            var res = Task.Run(async () => await GetTransactionInformationAsync(txhash));
            return res.Result;
        }

        public static async Task<BlockfrostTransaction> GetTransactionInformationAsync(string txhash)
        {
            if (string.IsNullOrEmpty(txhash))
                return null;
            txhash = txhash.Replace("\n", "").Replace("\r", "");

            do
            {
                using var httpClient = new HttpClient();
                if (txhash.Contains("#"))
                    txhash = txhash.Split('#')[0];
                using var request = new HttpRequestMessage(new("GET"),
                    $"{GeneralConfigurationClass.BlockfrostUrl}txs/{txhash}");
                request.Headers.TryAddWithoutValidation("project_id",
                    GeneralConfigurationClass.BlockfrostApikey);
                var response = await httpClient.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;
                    if (responseString != "")
                    {
                        var z = JsonConvert.DeserializeObject<BlockfrostTransaction>(responseString);
                        return z;
                    }

                    break;
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        Console.WriteLine(@"Blockfrost (1) - Too many requests");
                        throw new BlockfrostException(response.StatusCode.ToString());
                    }
                    else
                        break;
                }
            } while (true);

            return null;
        }


        public static string GetDatumFromDatumHash(string datumhash)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.BlockfrostUrl}scripts/datum/{datumhash}");
            request.Headers.TryAddWithoutValidation("project_id", GeneralConfigurationClass.BlockfrostApikey);

            var response = httpClient.Send(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;


                JObject jsonData = JObject.Parse(responseString);
                    // get JSON result objects into a list
                    string res = jsonData["json_value"]?.ToString();

                    return res;
            }
            return null;
        }
        public static string GetDatumCborFromDatumHash(string datumhash)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.BlockfrostUrl}scripts/datum/{datumhash}/cbor");
            request.Headers.TryAddWithoutValidation("project_id", GeneralConfigurationClass.BlockfrostApikey);

            var response = httpClient.Send(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                var res=JsonConvert.DeserializeObject<BlockfrostCborClass>(responseString);
                if (res == null)
                    return null;

                return res.Cbor;
            }
            return null;
        }

        public static async Task<string> GetSenderAsync(string hash)
        {
            var z = await GetTransactionUtxoFromBlockfrostAsync(hash);
            if (z != null && z.Inputs.Any())
            {
                return z.Inputs[0].Address;
            }

            return null;
        }

        public static string GetSender(string hash)
        {
            // Call Async Version and wait
            var res = Task.Run(async () => await GetSenderAsync(hash));
            return res.Result;
        }


        public static async Task<AssetTransactionsClass> GetLastAssetTransactionAsync(string policyid,
            string assetnameinhex)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.BlockfrostUrl}assets/{policyid}{assetnameinhex}/transactions?order=desc&count=1");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("project_id", GeneralConfigurationClass.BlockfrostApikey);

            var response = await httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                var res = JsonConvert.DeserializeObject<AssetTransactionsClass[]>(responseString);
                if (res == null)
                    return null;

                return res.FirstOrDefault();
            }

            return null;
           
        }
       
        public static BlockfrostTransactionUtxo GetTransactionUtxoFromBlockfrost(string hash)
        {
            // Call Async Version and wait
            var res = Task.Run(async () => await GetTransactionUtxoFromBlockfrostAsync(hash));
            return res.Result;
        }


        public static async Task<BlockfrostTransactionUtxo> GetTransactionUtxoFromBlockfrostAsync(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return null;

            using var httpClient = new HttpClient();
            if (hash.Contains("#"))
                hash = hash.Split('#')[0];
            using var request = new HttpRequestMessage(new("GET"),
                $"{GeneralConfigurationClass.BlockfrostUrl}txs/{hash}/utxos");
            request.Headers.TryAddWithoutValidation("project_id",
                GeneralConfigurationClass.BlockfrostApikey);
            var response = await httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                if (responseString != "")
                {
                    var z = JsonConvert.DeserializeObject<BlockfrostTransactionUtxo>(responseString);
                    return z;
                }
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine(@"Blockfrost (2) - Too many requests");
                throw new BlockfrostException(response.StatusCode.ToString());
            }

            return null;
        }

        public static GetAddressesFromStakeClass[] GetAllAddressesFromSingleAddress(IConnectionMultiplexer redis, string address)
        {
            var res = Task.Run(async () => await GetAllAddressesFromSingleAddressAsync(redis,address));
            return res.Result;
        }
        public static async Task<GetAddressesFromStakeClass[]> GetAllAddressesFromSingleAddressAsync(IConnectionMultiplexer redis, string address)
        {
            string stakeaddress = address;
            if (!address.StartsWith("stake"))
            {
                stakeaddress = Bech32Engine.GetStakeFromAddress(address);
                if (string.IsNullOrEmpty(stakeaddress))
                    return null;
            }

            var assets = await GetAllAddressesWithThisStakeAddressAsync(redis, stakeaddress);

            return assets;
        }

        public static GetAddressesFromStakeClass[] GetAllAddressesWithThisStakeAddress(IConnectionMultiplexer redis,
            string stakeAddress)
        {
            var res = Task.Run(async () => await GetAllAddressesWithThisStakeAddressAsync(redis, stakeAddress));
            return res.Result;
        }

        public static async Task<GetAddressesFromStakeClass[]> GetAllAddressesWithThisStakeAddressAsync(IConnectionMultiplexer redis, string stakeAddress)
        {
            List<GetAddressesFromStakeClass> addresses = new List<GetAddressesFromStakeClass>();
            string rediskey = $"GetAllAddressesWithThisStakeAddressAsync_{stakeAddress}";
            if (redis != null)
            {
                var cachedAssetsJsonString = GlobalFunctions.GetStringFromRedis(redis, rediskey);
                if (!string.IsNullOrEmpty(cachedAssetsJsonString))
                {
                    return JsonConvert.DeserializeObject<GetAddressesFromStakeClass[]>(cachedAssetsJsonString);
                }
            }

            int pageNo = 0;
            do
            {
                pageNo++;
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(new("GET"),
                    $"{GeneralConfigurationClass.BlockfrostUrl}accounts/{stakeAddress}/addresses?page={pageNo}");
                request.Headers.TryAddWithoutValidation("project_id", GeneralConfigurationClass.BlockfrostApikey);
                HttpResponseMessage response;
                response = await httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;
                    var adr = JsonConvert.DeserializeObject<GetAddressesFromStakeClass[]>(responseString);
                    if (adr == null || !adr.Any())
                    {
                        if (redis != null)
                            GlobalFunctions.SaveStringToRedis(redis, rediskey, responseString, 3600);
                        return addresses.ToArray();
                    }
                    addresses.AddRange(adr);
                }
                else
                {
                    return addresses.ToArray();
                }

                if (pageNo > 200)
                    break;
            } while (true);

            return addresses.ToArray();
        }

        public static async Task<BlockfrostAddressInformationClass> GetAddressInformationAsync(string address)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new("GET"),
                $"{GeneralConfigurationClass.BlockfrostUrl}addresses/{address}");
            request.Headers.TryAddWithoutValidation("project_id", GeneralConfigurationClass.BlockfrostApikey);

            var response = await httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<BlockfrostAddressInformationClass>(responseString);
                return result;
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine(@"Blockfrost (4) - Too many requests");
                throw new BlockfrostException(response.StatusCode.ToString());
            }

            return null;
        }


        public static TxInAddressesClass ToTxInAddresses(this BlockfrostAddressUtxo[] utxo, string address)
        {
            TxInAddressesClass txout = new TxInAddressesClass()
            {
                Address = address,
                DataProvider = Dataproviders.Blockfrost,
                StakeAddress = Bech32Engine.GetStakeFromAddress(address),
                TxIn = new TxInClass[] { }
            };

            List<TxInClass> txinlist = new List<TxInClass>();
            foreach (var addressUtxo in utxo.OrEmptyIfNull())
            {
                TxInClass txic = new TxInClass()
                {
                    Lovelace = addressUtxo.Amount.FirstOrDefault(x => x.Unit == "lovelace").Quantity ?? 0,
                    TxHash = addressUtxo.TxHash,
                    TxId = addressUtxo.OutputIndex
                };
                List<TxInTokensClass> tokens = new List<TxInTokensClass>();
                if (addressUtxo.Amount != null)
                {
                    foreach (var amount in addressUtxo.Amount)
                    {
                        if (amount.Unit == "lovelace")
                            continue;

                        TxInTokensClass tok = new TxInTokensClass();
                        tok.Quantity = amount.Quantity ?? 1;
                        if (amount.Unit.Length == 56)
                            tok.PolicyId = amount.Unit;
                        else
                        {
                            tok.PolicyId = amount.Unit.Substring(0, 56);
                            tok.TokennameHex = amount.Unit.Substring(56);
                            tok.Tokenname = GlobalFunctions.FromHexString(tok.TokennameHex);
                        }

                        tokens.Add(tok);
                    }
                }

                if (tokens.Any())
                    txic.Tokens = tokens.ToList();
                txinlist.Add(txic);
            }

            txout.TxIn = txinlist.ToArray();
            return txout;
        }

        public static async Task<BlockfrostAddressUtxo[]> GetUtxoAsync(string address)
        {
            List<BlockfrostAddressUtxo> allUtxo = new();

            int pageNo = 0;
            do
            {
                pageNo++;
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(new("GET"),
                    $"{GeneralConfigurationClass.BlockfrostUrl}addresses/{address}/utxos?page={pageNo}");
                request.Headers.TryAddWithoutValidation("project_id",
                    GeneralConfigurationClass.BlockfrostApikey);
                HttpResponseMessage response;
                response = await httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(responseString))
                    {
                        var z = JsonConvert.DeserializeObject<BlockfrostAddressUtxo[]>(responseString);
                        if (!z.Any())
                            break;
                        allUtxo.AddRange(z);

                        if (z.Length < 100)
                            break;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        Console.WriteLine($@"Blockfrost (5) - {response.StatusCode.ToString()}");
                        throw new BlockfrostException(response.StatusCode.ToString());
                    }
                    else
                    {
                        break;
                    }
                }
            } while (true);

            return allUtxo.ToArray();
        }

        public static async Task<string> GetTransactionIdAsync(string address)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new("GET"),
                $"{GeneralConfigurationClass.BlockfrostUrl}addresses/{address}/txs");
            request.Headers.TryAddWithoutValidation("project_id",
                GeneralConfigurationClass.BlockfrostApikey);
            var response = await httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                if (responseString == "") return "";
                var z = JsonConvert.DeserializeObject<string[]>(responseString);
                if (z.Any())
                    return z.FirstOrDefault();
            }
            else
            {
                Console.WriteLine($@"Blockfrost (6) - {response.StatusCode.ToString()}");
                throw new BlockfrostException(response.StatusCode.ToString());
            }

            return null;
        }

        public static BlockfrostAssetClass? GetAssetInformation(string assetid)
        {
            var res = Task.Run(async () => await GetAssetInformationAsync(assetid));
            return res.Result;
        }

        public static async Task<BlockfrostAssetClass?> GetAssetInformationAsync(string assetid)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new("GET"),
                $"{GeneralConfigurationClass.BlockfrostUrl}assets/{assetid}");
            request.Headers.TryAddWithoutValidation("project_id",
                GeneralConfigurationClass.BlockfrostApikey);
            var response = await httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(responseString)) return null;
                var z = JsonConvert.DeserializeObject<BlockfrostAssetClass>(responseString);
                if (z != null)
                    z.Blockchain = Blockchain.Cardano;
                return z;
            }

            return null;
        }

        public static async Task<long?> GetSlotAsync()
        {
            var tip = await GetQueryTipAsync();
            return tip?.Slot;
        }

        public static Querytip GetQueryTip()
        {
            var res = Task.Run(async () => await GetQueryTipAsync());
            return res.Result;
        }

        private static Querytip _cachedQuerytip;
        private static DateTime _cachedQuerytipDateTime;

        public static async Task<Querytip> GetQueryTipAsync()
        {
            if (_cachedQuerytip != null && _cachedQuerytipDateTime > DateTime.Now.AddSeconds(-10))
                return _cachedQuerytip;

            using var httpClient = new HttpClient();
            using var request =
                new HttpRequestMessage(new("GET"), $"{GeneralConfigurationClass.BlockfrostUrl}blocks/latest");
            request.Headers.TryAddWithoutValidation("project_id", GeneralConfigurationClass.BlockfrostApikey);

            var response = await httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;

                var z = JsonConvert.DeserializeObject<BlockfrostLatestBlock>(responseString);
                if (z == null)
                    return null;

                Querytip querytip = new Querytip()
                {
                    Block = z.Height,
                    Epoch = z.Epoch,
                    Slot = z.Slot,
                    Hash =z.Hash,
                    Time=z.Time
                };
                _cachedQuerytip = querytip;
                _cachedQuerytipDateTime = DateTime.Now;
                return querytip;
            }

            return null;
        }

        public static async Task<string> GetIpfsFromMetadata(string policyid, string tokennamehex)
        {
            string ipfs = "";
            try
            {
                // Catch Metadata from the Token

                var metadata = await GetMetadata(policyid, tokennamehex);
                if (string.IsNullOrEmpty(metadata))
                    return null;


                if (!metadata.Contains("721"))
                {
                    metadata="{\r\n  \"721\": {\r\n    \""+policyid+"\": {\r\n      \""+tokennamehex.FromHex()+"\": "+metadata+"\r\n    }\r\n  }\r\n}";
                }

                var metadata1 = MetadataParsingHelper.ParseNormalMetadata(metadata, policyid, tokennamehex.FromHex());
                if (metadata1.PreviewImage != null)
                    ipfs = metadata1.PreviewImage.Url;

                if (metadata1.PreviewImage != null && string.IsNullOrEmpty(metadata1.PreviewImage.Url) &&
                    metadata1.Files.Any())
                {
                    ipfs = metadata1.Files.First().Url;
                }
            }
            catch
            {
                return null;
            }

            return ipfs;
        }

        public static async Task<string> GetMetadata(string policyid, string tokennamehex)
        {
            try
            {
                // Catch Metadata from the Token
                var ai = await ConsoleCommand.GetAssetFromCardanoBlockchainAsync(policyid, "", tokennamehex.FromHex());
                if (ai != null)
                {
                    var metadata = ai.OnchainMetadata?.ToString();

                    return metadata;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        public static async Task<AssetsAssociatedWithAccount[]> GetAccountAssetListAsync(string stakeaddress)
        {
            List<AssetsAssociatedWithAccount> results = new();
            int page = 0;
            do
            {
                page++;
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(new("GET"),
                    $"{GeneralConfigurationClass.BlockfrostUrl}accounts/{stakeaddress}/addresses/assets?page={page}");
                request.Headers.TryAddWithoutValidation("project_id",
                    GeneralConfigurationClass.BlockfrostApikey);
                HttpResponseMessage response;
                response = await httpClient.SendAsync(request);
              

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(responseString))
                    {
                        var assets =
                            JsonConvert.DeserializeObject<BlockfrostAssetsResultClass[]>(responseString);
                        if (assets != null && assets.Any())
                        {
                            foreach (var asset in assets)
                            {
                                results.Add(new AssetsAssociatedWithAccount(asset, stakeaddress));
                            }
                        }
                           
                        else break;
                    }
                    else break;
                }
                else
                {
                    Console.WriteLine($@"Blockfrost (7) - {response.StatusCode.ToString()}");
                    throw new BlockfrostException(response.StatusCode.ToString());
                }
            } while (true);

            return results.ToArray();
        }

    }
}
