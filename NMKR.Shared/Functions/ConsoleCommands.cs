using CardanoSharp.Wallet.Enums;
using CardanoSharp.Wallet.Extensions.Models;
using CardanoSharp.Wallet.Models;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.BITCOIN;
using NMKR.Shared.Classes.Blockfrost;
using NMKR.Shared.Classes.Cardano_Sharp;
using NMKR.Shared.Classes.CardanoSerialisationLibClasses;
using NMKR.Shared.Classes.Cli;
using NMKR.Shared.Classes.Koios;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Blockfrost;
using NMKR.Shared.Functions.CSLService;
using NMKR.Shared.Functions.Extensions;
using NMKR.Shared.Functions.Koios;
using NMKR.Shared.Functions.Meastro;
using NMKR.Shared.Functions.Metadata;
using NMKR.Shared.Functions.Solana;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuickType;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Command = NMKR.SimpleExec.Command;
using Exception = System.Exception;
using Metadatum = NMKR.Shared.Classes.CardanoSerialisationLibClasses.Metadatum;
using ProtocolParameters = NMKR.Shared.Classes.CardanoSerialisationLibClasses.ProtocolParameters;
//using static MudBlazor.FilterOperator;

namespace NMKR.Shared.Classes
{
    public static class ConsoleCommand
    {
        public static string CardanoCli(string command, out string errormessage)
        {
            var files2 = ExtractFilenamesWithPrefixes(command);
            CliCommand clicommand = new CliCommand() { Command = command };
            List<CommandFiles> outfiles=new List<CommandFiles>();
            List<CommandFiles> infiles = new List<CommandFiles>();
            foreach (var filename in files2)
            {
                if (File.Exists(filename) && (!command.Contains("--out-file " + filename)))
                {
                    var content=File.ReadAllText(filename);
                    infiles.Add(new CommandFiles() { FileName = GetFilenameOnly(filename), Content = content });
                }
                else
                {
                    outfiles.Add(new CommandFiles(){FileName = GetFilenameOnly(filename)});
                }
            }
            clicommand.InFiles = infiles.ToArray();
            clicommand.OutFiles = outfiles.ToArray();

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.CardanoCliApiUrl}/RemoteCallCardanoCli");
            request.Headers.TryAddWithoutValidation("accept", "application/json");
            clicommand.Command = RemovePathsRobust(clicommand.Command);
            var st = JsonConvert.SerializeObject(clicommand);
            request.Content = new StringContent(st);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var response = httpClient.SendAsync(request).Result; // .Result macht es synchron

            // Response verarbeiten
            string responseContent = response.Content.ReadAsStringAsync().Result;

            RemoteCallCardanoCliResultClass result=JsonConvert.DeserializeObject<RemoteCallCardanoCliResultClass>(responseContent);
            errormessage = result.ErrorMessage;
            foreach (var of in result.OutFiles.OrEmptyIfNull())
            {
                if (!string.IsNullOrEmpty(of.Content))
                {
                    File.WriteAllText($"{GeneralConfigurationClass.TempFilePath}{of.FileName}", of.Content);
                }
            }
            return result.Result;

            //   return CallCardanoCli(command, out errormessage);
        }


        /// <summary>
        /// Calls the cardano-cli binary
        /// </summary>
        /// <param name="command"></param>
        /// <param name="errormessage"></param>
        /// <returns></returns>
        public static string CallCardanoCli(string command, out string errormessage)
        {
            LogToPath($"START: {command}");

            errormessage = null;
            if (!Directory.Exists(GeneralConfigurationClass.TempFilePath))
                Directory.CreateDirectory(GeneralConfigurationClass.TempFilePath);
            string p = GeneralConfigurationClass.CardanoCli;

            try
            {
                string s = Command.Readx(p, out string error, command, GeneralConfigurationClass.TempFilePath);
                if (s == "error exception" ||
                    (!string.IsNullOrEmpty(error) && error.Contains("(The pipe has been ended.)")))
                {
                    Console.WriteLine($@"Error: {s} - {error}");
                    Console.WriteLine(@"Wait 10 seconds");
                    Thread.Sleep(10000);
                    Console.WriteLine(@"Try again");
                    s = Command.Readx(p, out error, command, GeneralConfigurationClass.TempFilePath);
                }

                if (string.IsNullOrEmpty(error))
                {
                    LogToPath($"SUCCESS: {command}");
                    return s;
                }

                errormessage = error;
                Console.WriteLine($@"ERROR: {errormessage}");
                LogToPath($"ERROR: {command} {errormessage}");
                return "ERROR";
            }
            catch (Exception e)
            {
                Console.WriteLine($@"Exception: {e.Message}");
                Thread.Sleep(20000);
                string s = Command.Readx(p, out var error, command, GeneralConfigurationClass.TempFilePath);
                if (string.IsNullOrEmpty(error)) return s;
                errormessage = error;
                Console.WriteLine($@"ERROR: {errormessage}");
                LogToPath($"ERROR: {command} {errormessage}");
                return "ERROR";

            }
        }
        public static string RemovePathsRobust(string command)
        {
            if (string.IsNullOrEmpty(command))
                return command;
            command = command.Replace(GeneralConfigurationClass.TempFilePath, "/tmp/");
            return command;
        }
     
        public static string GetFilenameOnly(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return string.Empty;

            // Entferne Anführungszeichen falls vorhanden
            string cleanPath = fullPath.Trim('"', '\'');

            // Finde den letzten Pfad-Trenner (/ oder \)
            int lastSlash = Math.Max(cleanPath.LastIndexOf('/'), cleanPath.LastIndexOf('\\'));

            // Wenn kein Pfad-Trenner gefunden wurde, ist es bereits nur der Dateiname
            if (lastSlash == -1)
                return cleanPath;

            // Gib den Teil nach dem letzten Pfad-Trenner zurück
            return cleanPath.Substring(lastSlash + 1);
        }


        public static List<string> ExtractFilenamesWithPrefixes(string commandLine, List<string> requiredPrefixes = null)
        {
            if (string.IsNullOrEmpty(commandLine))
                return new List<string>();

            // Standard Prefixe wenn keine angegeben
            if (requiredPrefixes == null)
            {
                requiredPrefixes = new List<string> { "/tmp/", "c:/tmp/", "./", "../", "/", "c:", "d:", "e:" };
            }

            var filenames = new HashSet<string>();

            // Tokenize den Command String
            var tokens = commandLine.Split(new char[] { ' ', '\t', '\n', '\r' },
                                         StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                string cleanToken = token.Trim('"', '\'');

                // Prüfe ob Token eine Datei sein könnte
                if (HasFileExtension(cleanToken) && HasValidPrefix(cleanToken, requiredPrefixes))
                {
                    filenames.Add(cleanToken);
                }
            }

            return filenames.OrderBy(f => f).ToList();
        }

        private static bool HasFileExtension(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            int lastDot = path.LastIndexOf('.');
            int lastSlash = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));

            // Extension muss nach dem letzten Pfad-Trenner kommen
            return lastDot > lastSlash && lastDot < path.Length - 1;
        }

        private static bool HasValidPrefix(string path, List<string> prefixes)
        {
            if (string.IsNullOrEmpty(path)) return false;

            // Absolute Pfade (beginnen mit / oder Laufwerk:)
            if (path.StartsWith("/") || (path.Length > 2 && path[1] == ':'))
                return true;

            // Prüfe spezifische Prefixe
            return prefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }


        /// <summary>
        /// Log Cli Messages to log file
        /// </summary>
        /// <param name="logmessage"></param>
        private static void LogToPath(string logmessage)
        {
            if (!string.IsNullOrEmpty(GeneralConfigurationClass.CliLogPath))
            {
                if (!Directory.Exists(GeneralConfigurationClass.CliLogPath))
                    Directory.CreateDirectory(GeneralConfigurationClass.CliLogPath);
                string logfilename = $"{GeneralConfigurationClass.CliLogPath}{DateTime.Now:ddMMyyyy}.log";
                if (File.Exists(logfilename))
                    File.AppendAllText(logfilename,
                        $"{DateTime.Now.ToLongTimeString()} - {logmessage}{Environment.NewLine}");
                else
                    File.WriteAllText(logfilename,
                        $"{DateTime.Now.ToLongTimeString()} - {logmessage}{Environment.NewLine}");
            }
        }

        /// <summary>
        /// Captures the version of the node version from the cli
        /// </summary>
        /// <returns></returns>
        public static string GetNodeVersion()
        {
            var v = CardanoCli("version", out var errormessage);
            if (!string.IsNullOrEmpty(v))
            {
                string[] s = v.Split(' ');
                if (s.Length > 2)
                    return s[1];
            }

            return "";
        }

        public enum Cip68Type
        {
            None,
            NftUserToken,
            ReferenceToken,
            FtUserToken
        }
     
        public static IEnumerable<KoiosPolicyAssetsClass> RemoveCip68Prefix(KoiosPolicyAssetsClass[] toArray)
        {
            foreach (var s in toArray)
            {
                if (s.AssetName.ToLower().StartsWith("000de140"))
                    s.AssetNameAscii = s.AssetName.Substring(8).FromHex();
                else if (s.AssetName.ToLower().StartsWith("000643b0"))
                    s.AssetNameAscii = s.AssetName.Substring(8).FromHex();
                else if (s.AssetName.ToLower().StartsWith("0014df10"))
                    s.AssetNameAscii = s.AssetName.Substring(8).FromHex();

                yield return s;
            }
        }
        public static string CreateMintTokenname(string prefix, string name, Cip68Type cip68Type = Cip68Type.None)
        {
            string resname = "";
            prefix ??= "";

            // Cip68 User Token NFT
            if (cip68Type == Cip68Type.NftUserToken)
                resname += "000de140"; // 222
            // Cip68 Reference Token
            if (cip68Type == Cip68Type.ReferenceToken)
                resname += "000643b0"; // 100
            // Cip68 User Token FT
            if (cip68Type == Cip68Type.FtUserToken)
                resname += "0014DF10"; // 333


            resname += prefix.ToHex();
            resname += name.ToHex();


            return resname;
        }


        public static async Task<ICollection<(GetParallelAddressInfo url, string data)>> DownloadUrlsAsync(
            IEnumerable<GetParallelAddressInfo> urls, int limit)
        {
            using var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(300) };
            using var semaphore = new SemaphoreSlim(limit, limit);
            var tasks = urls.Select(url => DownloadUrlHelperAsync(url, semaphore, client)).ToArray();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return tasks.Select(x => x.Result).ToArray();
        }

        static async Task<(GetParallelAddressInfo url, string data)> DownloadUrlHelperAsync(GetParallelAddressInfo url,
            SemaphoreSlim semaphore, HttpClient client)
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                using var response = await client.GetAsync(url.url).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    return (url, null);
                var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return (url, data);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public class GetParallelAddressInfo
        {
            public string url { get; set; }
            public NmkrAssetAddress address { get; set; }
        }

        public partial class AssetAddressx
        {
            [JsonProperty("payment_address", NullValueHandling = NullValueHandling.Ignore)]
            public string PaymentAddressPaymentAddress { get; set; }

            [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
            public long? Quantity { get; set; }
        }





        /// <summary>
        /// Creates a new cardano address via the cli
        /// </summary>
        /// <param name="mainnet"></param>
        /// <param name="enterpriseaddress">Enterprise addresses are the "short" addresses, without the stakekey</param>
        /// <returns></returns>
        public static CreateNewPaymentAddressClass CreateNewPaymentAddress(bool mainnet, bool enterpriseaddress = true)
        {
            string t = GlobalFunctions.GetGuid();

            string s = CardanoCli(
                $"address key-gen --verification-key-file {GeneralConfigurationClass.TempFilePath}{t}.vkey " +
                $"--signing-key-file {GeneralConfigurationClass.TempFilePath}{t}.skey",
                out var errormessage);

            CreateNewPaymentAddressClass cnpac = new CreateNewPaymentAddressClass();

            if (s.Trim() != "")
            {
                cnpac.ErrorCode = 13;
                cnpac.ErrorMessage = "Error while creating the address keys";
                return cnpac;
            }

            if (!File.Exists($"{GeneralConfigurationClass.TempFilePath}{t}.vkey"))
            {
                cnpac.ErrorCode = 11;
                cnpac.ErrorMessage = "Error while creating the verification key";
                return cnpac;
            }

            if (!File.Exists($"{GeneralConfigurationClass.TempFilePath}{t}.skey"))
            {
                cnpac.ErrorCode = 12;
                cnpac.ErrorMessage = "Error while creating the signing key";
                return cnpac;
            }


            if (enterpriseaddress == false)
            {
                s = CardanoCli(
                    $"latest stake-address key-gen --verification-key-file {GeneralConfigurationClass.TempFilePath}{t}stake.vkey " +
                    $"--signing-key-file {GeneralConfigurationClass.TempFilePath}{t}stake.skey",
                    out var errormessage2);
                if (s.Trim() != "")
                {
                    cnpac.ErrorCode = 17;
                    cnpac.ErrorMessage = "Error while creating the stake key";
                    return cnpac;
                }

                s = CardanoCli(
                    $"address build --payment-verification-key-file {GeneralConfigurationClass.TempFilePath}{t}.vkey " +
                    $"--stake-verification-key-file {GeneralConfigurationClass.TempFilePath}{t}stake.vkey " +
                    $"--out-file {GeneralConfigurationClass.TempFilePath}{t}.addr {(mainnet ? " --mainnet" : $" --testnet-magic {GeneralConfigurationClass.TestnetMagicId}")}",
                    out var errormessage1);
                if (s.Trim() != "")
                {
                    cnpac.ErrorCode = 14;
                    cnpac.ErrorMessage = "Internal Error";
                    return cnpac;
                }
            }
            else
            {
                s = CardanoCli(
                    $"address build --payment-verification-key-file {GeneralConfigurationClass.TempFilePath}{t}.vkey " +
                    $"--out-file {GeneralConfigurationClass.TempFilePath}{t}.addr {(mainnet ? " --mainnet" : $" --testnet-magic {GeneralConfigurationClass.TestnetMagicId}")}",
                    out var errormessage1);
                if (s.Trim() != "")
                {
                    cnpac.ErrorCode = 14;
                    cnpac.ErrorMessage = "Internal Error";
                    return cnpac;
                }
            }

            if (!File.Exists($"{GeneralConfigurationClass.TempFilePath}{t}.addr"))
            {
                cnpac.ErrorCode = 15;
                cnpac.ErrorMessage = "Internal Error";
                return cnpac;
            }

            cnpac.Address = File.ReadAllText($"{GeneralConfigurationClass.TempFilePath}{t}.addr");
            cnpac.privateskey = File.ReadAllText($"{GeneralConfigurationClass.TempFilePath}{t}.skey");
            cnpac.privatevkey = File.ReadAllText($"{GeneralConfigurationClass.TempFilePath}{t}.vkey");
            if (!enterpriseaddress)
            {
                cnpac.stakevkey = File.ReadAllText($"{GeneralConfigurationClass.TempFilePath}{t}stake.vkey");
                cnpac.stakeskey = File.ReadAllText($"{GeneralConfigurationClass.TempFilePath}{t}stake.skey");
            }

            cnpac.ErrorCode = 0;
            cnpac.ErrorMessage = "";

            GlobalFunctions.DeleteFile($"{GeneralConfigurationClass.TempFilePath}{t}.addr");
            GlobalFunctions.DeleteFile($"{GeneralConfigurationClass.TempFilePath}{t}.skey");
            GlobalFunctions.DeleteFile($"{GeneralConfigurationClass.TempFilePath}{t}.vkey");
            GlobalFunctions.DeleteFile($"{GeneralConfigurationClass.TempFilePath}{t}stake.vkey");
            GlobalFunctions.DeleteFile($"{GeneralConfigurationClass.TempFilePath}{t}stake.skey");
            return cnpac;
        }


        public static string CreatePaymentAddressFromKeyfile(string verificationkey, bool mainnet)
        {
            string t = GlobalFunctions.GetGuid();
            File.WriteAllText($"{GeneralConfigurationClass.TempFilePath}{t}.vkey", verificationkey);

            string s = CardanoCli(
                $"address build --payment-verification-key-file {GeneralConfigurationClass.TempFilePath}{t}.vkey --out-file {GeneralConfigurationClass.TempFilePath}{t}.addr {(mainnet ? " --mainnet" : $" --testnet-magic {GeneralConfigurationClass.TestnetMagicId}")}",
                out var errormessage);
            if (s.Trim() != "")
            {
                return "";
            }

            if (!File.Exists($"{GeneralConfigurationClass.TempFilePath}{t}.addr"))
            {
                return "";
            }

            var address = File.ReadAllText($"{GeneralConfigurationClass.TempFilePath}{t}.addr");

            GlobalFunctions.DeleteFile($"{GeneralConfigurationClass.TempFilePath}{t}.vkey");

            return address;
        }

        public static AllTxInAddressesClass GetNewUtxo(string[] addresses)
        {
            var res = Task.Run(async () => await GetNewUtxoAsync(addresses));
            return res.Result;
        }
        public static async Task<AllTxInAddressesClass> GetNewUtxoAsync(string[] addresses)
        {
            AllTxInAddressesClass res = new AllTxInAddressesClass();
            List<TxInAddressesClass> txinAddresses = new List<TxInAddressesClass>();
            foreach (var address in addresses.OrEmptyIfNull())
            {
                txinAddresses.Add(await GetNewUtxoAsync(address));
            }

            res.TxInAddresses = txinAddresses.ToArray();

            return res;
        }
        /// <summary>
        /// Checks an address via the cli command address info for correctness
        /// </summary>
        /// <param name="address"></param>
        /// <param name="mainnet"></param>
        /// <returns></returns>
        public static bool IsValidCardanoAddress(string address, bool mainnet)
        {
            address = address.Trim().ToLower();
            if (string.IsNullOrEmpty(address))
                return false;

            if (mainnet && address.Contains("_test1"))
                return false;

            if (!mainnet && !address.Contains("_test1"))
                return false;

            var adr = address.ToAddress();

            if (adr.NetworkType == NetworkType.Unknown)
                return false;

            return true;
        }


        public static async Task<BlockfrostTransaction> GetTransactionAsync(string txhash)
        {
            try
            {
                var txinfobf = await BlockfrostFunctions.GetTransactionInformationAsync(txhash);
                return txinfobf;
            }
            catch
            {
                var txinfokoios = (await KoiosFunctions.GetTransactionInformationAsync(txhash)).ToBlockfrostTransaction();
                return txinfokoios;
            }
        }

        public static GenericTransaction ToGenericTransaction(this BlockfrostTransaction transaction)
        {
            if (transaction == null)
                return null;
            return new GenericTransaction()
            {
                Block = transaction.Block, Blockchain = Blockchain.Cardano, Fees = transaction.Fees,
                Hash = transaction.Hash, Index = transaction.Index
            };
        }

       

        public static TxInAddressesClass GetNewUtxo(string addresses, Dataproviders dataprovider = Dataproviders.Default)
        {
            var res = Task.Run(async () => await GetNewUtxoAsync(addresses, dataprovider));
            return res.Result;
        }
        public static async Task<TxInAddressesClass> GetNewUtxoAsync(string addresses, Dataproviders dataprovider = Dataproviders.Default)
        {
            try
            {
                switch (dataprovider)
                {
                    case Dataproviders.Default:
                    case Dataproviders.Blockfrost:
                        return (await BlockfrostFunctions.GetUtxoAsync(addresses)).ToTxInAddresses(addresses);
                    case Dataproviders.Koios:
                        return (await KoiosFunctions.GetUtxoAsync(addresses)).ToTxInAddresses(addresses);
                    case Dataproviders.Maestro:
                        return (await MaestroFunctions.GetUtxoAsync(addresses)).ToTxInAddresses(addresses);
                    case Dataproviders.Cli:
                        return GetUtxoFromCli(addresses);
                }
            }
            catch
            {
                switch (dataprovider)
                {
                    case Dataproviders.Default:
                    case Dataproviders.Blockfrost:
                        return (await KoiosFunctions.GetUtxoAsync(addresses)).ToTxInAddresses(addresses);
                    case Dataproviders.Koios:
                    case Dataproviders.Maestro:
                    case Dataproviders.Cli:
                        return (await BlockfrostFunctions.GetUtxoAsync(addresses)).ToTxInAddresses(addresses);
                }
            }

            return null;
        }
      
        public static TxInAddressesClass GetUtxoFromCli(string address)
        {
            
            TxInAddressesClass txout = new TxInAddressesClass()
            {
                Address = address,
                DataProvider = Dataproviders.Cli,
                StakeAddress = Bech32Engine.GetStakeFromAddress(address),
                TxIn = new TxInClass[] { }
            };

            string t = GlobalFunctions.GetGuid();
            string filename = $"{GeneralConfigurationClass.TempFilePath}utxo_{t}.json";

            string command = CliCommandExtensions.GetQueryUtxo()
                .GetAddress(address)
                .GetNetwork(GlobalFunctions.IsMainnet())
                .GetOutFile(filename);

            ConsoleCommand.CardanoCli(command, out string errormessage);

            if (!string.IsNullOrEmpty(errormessage))
            {
                return null;
            }

            if (File.Exists(filename))
            {
                string jsonString = File.ReadAllText(filename);
                CardanoUTxOParser parser = new CardanoUTxOParser();
                txout.TxIn = parser.ParseUTxOs(jsonString).ToArray();
            }
            GlobalFunctions.DeleteFile(filename);

            return txout;
        }


        public static async Task<string> GetSenderAsync(string hash)
        {
            try
            {
                var res = await BlockfrostFunctions.GetSenderAsync(hash);
                if (string.IsNullOrEmpty(res))
                    return await KoiosFunctions.GetSenderAsync(hash);
                return res;
            }
            catch
            {
                return await KoiosFunctions.GetSenderAsync(hash);
            }
        }

        public static string GetSender(string hash)
        {
            // Call async version and wait
            var res = Task.Run(async () => await GetSenderAsync(hash));
            return res.Result;
        }



        public static async Task<AssetsAssociatedWithAccount[]> GetAllAssetsInWalletAsync(IConnectionMultiplexer redis, string address)
        {
            string rediskey = $"GetAllAssetsInWallet_{address}";
            var cachedAssetsJsonString = GlobalFunctions.GetStringFromRedis(redis, rediskey);
            if (!string.IsNullOrEmpty(cachedAssetsJsonString))
            {
                return JsonConvert.DeserializeObject<AssetsAssociatedWithAccount[]>(cachedAssetsJsonString);
            }

            string stakeaddress = address.StartsWith("stake") ? address : Bech32Engine.GetStakeFromAddress(address);

            if (string.IsNullOrEmpty(stakeaddress))
            {
                var utxo = await GetNewUtxoAsync(address);
                if (utxo == null)
                    return null;
                return utxo.GetAllAssetsAssociatedWithAccounts();
            }

            AssetsAssociatedWithAccount[] assets;
            try
            {
                assets = await BlockfrostFunctions.GetAccountAssetListAsync(stakeaddress);
            }
            catch
            {
                assets = (await KoiosFunctions.GetAccountAssetListAsync(stakeaddress)).ToAssetsAssociatedWithAccount();
            }

            if (assets == null)
                return null;

            var json = JsonConvert.SerializeObject(assets);
            GlobalFunctions.SaveStringToRedis(redis, rediskey, json, 180);

            return assets;
        }



        public static string GetKeyhash(string key)
        {
            string t = GlobalFunctions.GetGuid();
            File.WriteAllText($"{GeneralConfigurationClass.TempFilePath}{t}.vkey", key);
            var p = CardanoCli(
                $"address key-hash --payment-verification-key-file {GeneralConfigurationClass.TempFilePath}{t}.vkey",
                out var errormessage);
            GlobalFunctions.DeleteFile($"{GeneralConfigurationClass.TempFilePath}{t}.vkey");
            return p.Replace("\n", "").Replace("\r", "");
        }

        public static string GetPolicyId(string policyscript)
        {
            string t = GlobalFunctions.GetGuid();
            File.WriteAllText($"{GeneralConfigurationClass.TempFilePath}{t}.script", policyscript);
            var policyid = CardanoCli(
                $"latest transaction policyid --script-file {GeneralConfigurationClass.TempFilePath}{t}.script",
                out var errormessage);
            GlobalFunctions.DeleteFile($"{GeneralConfigurationClass.TempFilePath}{t}.script");
            return policyid.Replace("\n", "").Replace("\r", "");
        }


        public static bool LegacyDirectsaleTransactionSale(IConnectionMultiplexer redis, SmartContractAuctionsParameterClass scapc, bool mainnet,
            ref BuildTransactionClass buildtransaction)
        {
            buildtransaction.BuyerTxOut = new TxOutClass() { ReceiverAddress = scapc.changeaddress };

            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra()
                .GetNetwork(mainnet)
                .GetTxIn(scapc.utxopaymentaddress, ref buildtransaction)
                .GetTxOut(scapc.legacyaddress, scapc.bidamount)
                .GetTxOut(scapc.utxopaymentaddress, scapc.changeaddress, null, "", scapc.bidamount, 3000000,
                    ref buildtransaction)
                .GetChangeAddress(scapc.changeaddress)
                .GetProtocolParamsFile(scapc.protocolParamsFile)
                .GetWitnessOverride(2)
                .GetOutFile(scapc.matxrawfile);

            buildtransaction.Command = command;


            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }

            buildtransaction.Fees = GetEstimatedFees(log);
            if (buildtransaction.Fees == 0)
                return false;

            return true;
        }

      

        /// <summary>
        /// Mint & Send Command with the CLI - using the Buil-Raw Command
        /// </summary>
        /// <param name="redis"></param>
        /// <param name="newMintAndSendClass"></param>
        /// <param name="payaddress"></param>
        /// <param name="payskey"></param>
        /// <param name="mainnet"></param>
        /// <param name="submittransaction"></param>
        /// <param name="ignoresoldstate"></param>
        /// <param name="mintandsendminutxo"></param>
        /// <param name="buildtransaction"></param>
        /// <returns></returns>
        public static string MintAndSendBuildRaw(IConnectionMultiplexer redis, NewMintAndSendClass newMintAndSendClass,
          string payaddress, string payskey,Premintedpromotokenaddress premintedpromotokenaddress, Nftprojectsendpremintedtoken sendpremintedtoken, 
          bool mainnet, bool submittransaction, bool ignoresoldstate, string mintandsendminutxo, ref BuildTransactionClass buildtransaction)
        {
            string guid = GlobalFunctions.GetGuid();

            string metadatafile = $"{GeneralConfigurationClass.TempFilePath}metadata{guid}.json";
            string policyscriptfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.script";
            string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string paymentskeyfile = $"{GeneralConfigurationClass.TempFilePath}payment{guid}.skey";
            string policyskeyfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.skey";

            foreach (var r in newMintAndSendClass.NftReservations)
            {
                if (r.Nft.NftprojectId != newMintAndSendClass.NftProject.Id)
                    return "NFT are not all in the same project (MintAndSend)";

                if (r.Nft.State == "sold" && ignoresoldstate == false)
                    return "Some NFT are already sold (MintAndSend)";
            }

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";

            // Catch Utxo from PayAddress
            var utxopaymentaddress = GetNewUtxo(payaddress);
            if (utxopaymentaddress.LovelaceSummary == 0)
                utxopaymentaddress = GetNewUtxo(payaddress, Dataproviders.Maestro);
            utxopaymentaddress = FilterAllTxInWithTokens(utxopaymentaddress);
            if (utxopaymentaddress.TxIn == null || utxopaymentaddress.TxIn.Length == 0)
                return $"No Payaddress txin available {payaddress}";

            List<string> signfiles = new List<string>();


            // Catch utxo from premintedtokenaddress for Preminted Tokens to send with transaction
            TxInAddressesClass utxoPremintedTokenAddress = null;
            if (sendpremintedtoken != null && premintedpromotokenaddress != null)
            {
                utxoPremintedTokenAddress = GetNewUtxo(premintedpromotokenaddress.Address);
                if (utxoPremintedTokenAddress.LovelaceSummary==0)
                    utxoPremintedTokenAddress = GetNewUtxo(premintedpromotokenaddress.Address, Dataproviders.Maestro);
                if (utxoPremintedTokenAddress.TxIn== null || utxoPremintedTokenAddress.TxIn.Length == 0)
                    return $"No Preminted Token txin available {premintedpromotokenaddress.Address}";

                string password = GeneralConfigurationClass.Masterpassword + premintedpromotokenaddress.Salt;
                string premintedtokenpaykey = Encryption.DecryptString(premintedpromotokenaddress.Privatekey, password);
                string premintedtokenfile = $"{GeneralConfigurationClass.TempFilePath}premintedtoken{guid}.skey";

                File.WriteAllText(premintedtokenfile, premintedtokenpaykey);
                signfiles.Add(premintedtokenfile);
            }



            List<NftWithMintingAddressClass> nfts = new List<NftWithMintingAddressClass>();
            foreach (var nftreservation in newMintAndSendClass.NftReservations)
            {
                var res = newMintAndSendClass.MintAndSends
                    .FirstOrDefault(x => x.Reservationtoken == nftreservation.Reservationtoken)?.Receiveraddress;
                nfts.Add(new NftWithMintingAddressClass(nftreservation.Nft, res));
            }


            string metadata = CreateMetadataNew(nfts.ToArray(), true);

            if (!string.IsNullOrEmpty(metadata))
                File.WriteAllText(metadatafile, metadata);
            else
            {
                return "No Metadata available";
            }

            File.WriteAllText(policyscriptfile, newMintAndSendClass.NftProject.Policyscript);

            File.WriteAllText(paymentskeyfile, payskey);

            int countwitness = 0;
            signfiles.Add(paymentskeyfile);

            bool needpolicyfile = false;
            int z = 0;
            foreach (var nx in newMintAndSendClass.NftReservations)
            {
                if (nx.Nft.InstockpremintedaddressId != null)
                {
                    z++;
                    string premintedfile = $"{GeneralConfigurationClass.TempFilePath}preminted{guid}_{z}.skey";
                    string password = nx.Nft.Instockpremintedaddress.Salt + GeneralConfigurationClass.Masterpassword;
                    string premintedpaykey = Encryption.DecryptString(nx.Nft.Instockpremintedaddress.Privateskey, password);
                    File.WriteAllText(premintedfile, premintedpaykey);
                    signfiles.Add(premintedfile);
                }
                else needpolicyfile = true;
            }

            countwitness = signfiles.Count();

            if (needpolicyfile)
            {
                string polskey = Encryption.DecryptString(newMintAndSendClass.NftProject.Policyskey,
                    newMintAndSendClass.NftProject.Password);
                File.WriteAllText(policyskeyfile, polskey);
                countwitness++;
            }
            else policyskeyfile = "";

            // END -Create the Sign Keys for Signing and to calculate the witnesses

            var mintingcosts = GlobalFunctions.GetMintingcosts2(newMintAndSendClass.NftProject.Id,
                newMintAndSendClass.NftReservations.Count, 0);

            long ttl = (long)q.Slot + 6000;
            bool b = false;
            long fees = 0;

            b = NewMintAndSendCreateTransactionBuildRaw(
                redis,
                utxopaymentaddress,
                newMintAndSendClass,
                !string.IsNullOrEmpty(metadata) ? metadatafile : "",
                policyscriptfile,
                matxrawfile,
                guid,
                payaddress,
                countwitness,
                mainnet,
                ttl,
                mintandsendminutxo,
                fees,
                utxoPremintedTokenAddress,
                premintedpromotokenaddress,
                sendpremintedtoken,
                ref buildtransaction);

            if (!b)
            {
                Console.WriteLine(@"Error while creating the transaction");
                Console.WriteLine(buildtransaction.LogFile);
                return "Error while creating the transaction";
            }
            // TODO: Calculate Fees
            CalculateFees(redis, $@"{matxrawfile}",buildtransaction.TxInCount, buildtransaction.TxOutCount,
                countwitness, mainnet, ref buildtransaction, out fees);

            if (fees == 0)
            {
                Console.WriteLine(@"Error while calculating the fees");
                Console.WriteLine(buildtransaction.LogFile);
                return "Error while calculating the fees";
            }

            GlobalFunctions.DeleteFile(matxrawfile);

            b = NewMintAndSendCreateTransactionBuildRaw(
                redis,
                utxopaymentaddress,
                newMintAndSendClass,
                !string.IsNullOrEmpty(metadata) ? metadatafile : "",
                policyscriptfile,
                matxrawfile,
                guid,
                payaddress,
                countwitness,
                mainnet,
                ttl,
                mintandsendminutxo,
                fees,
                utxoPremintedTokenAddress,
                premintedpromotokenaddress,
                sendpremintedtoken,
                ref buildtransaction);

            if (!b)
            {
                Console.WriteLine(@"Error while creating the transaction (2)");
                Console.WriteLine(buildtransaction.LogFile);
                return "Error while creating the transaction (2)";
            }
            var ok = SignAndSubmit(redis, signfiles.ToArray(), policyskeyfile, matxrawfile, matxsignedfile, mainnet, submittransaction,
                               ref buildtransaction);

            GlobalFunctions.DeleteFile(policyscriptfile);
            GlobalFunctions.DeleteFile(metadatafile);
            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(policyskeyfile);

            foreach (var a in signfiles)
                GlobalFunctions.DeleteFile(a);
            return ok;

        }
        public static TxInAddressesClass FilterAllTxInWithTokens(TxInAddressesClass utxopaymentaddress)
        {
            TxInAddressesClass result = utxopaymentaddress;
            result.TxIn = utxopaymentaddress.TxIn.Where(x => x.Tokens == null || !x.Tokens.Any()).ToArray();
            return result;
        }

      

        private static bool NewMintAndSendCreateTransactionBuildRaw(IConnectionMultiplexer redis, TxInAddressesClass utxopaymentaddress,
         NewMintAndSendClass newmintAndSendClass,
         string metadatafile, string policyscriptfile, string matxrawfile, string guid, string payaddress,
         int witnesscount, bool mainnet, long ttl, string mintandsendminutxo, long fees,
         TxInAddressesClass utxoPremintedTokenAddress,
         Premintedpromotokenaddress premintedpromotokenaddress, 
         Nftprojectsendpremintedtoken sendpremintedtoken,
         ref BuildTransactionClass buildtransaction)
        {

            buildtransaction.LockTxIn = utxopaymentaddress.TxIn;


            string command = CliCommandExtensions.GetTransactionBuildRawWithLatestEra()
                .GetFees(fees);

            // This is the First TX in - this can be the Payin Address or the Account Address
            
            GetTxInHashes(utxopaymentaddress, out string com1, out int txincount, out var lovelacesummery, ref buildtransaction);
            command += $" {com1}";

            // Add the preminted token address to the transaction (if any)
            long lovelacesummary2 = 0;
            if (utxoPremintedTokenAddress != null && utxoPremintedTokenAddress.TxIn != null &&
                utxoPremintedTokenAddress.TxIn.Length > 0)
            {
                GetTxInHashes(utxoPremintedTokenAddress, out string com2, out int txincount2, out lovelacesummary2,
                    ref buildtransaction);
                command += $" {com2}";
                txincount += txincount2;
            }

            long rest = lovelacesummery - buildtransaction.Fees;


            string minttoken = "";
            long adatosendout = 0;
            long minutxofinal = 0;
            long totalpremintedtokens = 0;
            foreach (var mas in newmintAndSendClass.MintAndSends)
            {
                var reservations =
                    newmintAndSendClass.NftReservations.Where(x => x.Reservationtoken == mas.Reservationtoken)
                        .ToArray();
                string sendtoken = "";

                // Calculate the Minuxto - all 6 NFT we will Add 2 ADA - i know, it is not correct, but it works
                foreach (var reservation in reservations)
                {
                    long amount = Math.Max(1, reservation.Tc * reservation.Nft.Multiplier);
                    if (!string.IsNullOrEmpty(sendtoken))
                        sendtoken += " + ";
                    sendtoken += $"{amount} {newmintAndSendClass.NftProject.Policyid}.{CreateMintTokenname(newmintAndSendClass.NftProject.Tokennameprefix, reservation.Nft.Name)}";

                    if (!string.IsNullOrEmpty(minttoken))
                        minttoken += " + ";
                    minttoken += $"{amount} {newmintAndSendClass.NftProject.Policyid}.{CreateMintTokenname(newmintAndSendClass.NftProject.Tokennameprefix, reservation.Nft.Name)}";
                }


                if (sendpremintedtoken != null)
                {
                    totalpremintedtokens+= sendpremintedtoken.Countokenstosend;
                    sendtoken += $" + {sendpremintedtoken.Countokenstosend} " + sendpremintedtoken.PolicyidOrCollection + "." + sendpremintedtoken.Tokenname.ToHex();
                }


                // Calculate the REAL minutxo
                if (mintandsendminutxo == nameof(MinUtxoTypes.minutxo) && !string.IsNullOrEmpty(sendtoken))
                {
                    minutxofinal = CalculateRequiredMinUtxo(redis, mas.Receiveraddress, sendtoken, "", guid, mainnet,
                        ref buildtransaction);
                }

                if (minutxofinal == 0)
                    minutxofinal = 2000000;

                if (!string.IsNullOrEmpty(sendtoken))
                    command += $" --tx-out {mas.Receiveraddress.FilterToLetterOrDigit()}+{minutxofinal}+\"{sendtoken}\"";
                buildtransaction.TxOutCount++;
                adatosendout += minutxofinal;
                rest = rest - minutxofinal;
            }


            // Set the change address
            command += $" --tx-out {payaddress.FilterToLetterOrDigit()}+{rest}";
            buildtransaction.TxOutCount++;

            if (sendpremintedtoken != null)
            {
                command=command.GetTxOut(new []{utxoPremintedTokenAddress}, utxoPremintedTokenAddress.Address, totalpremintedtokens, sendpremintedtoken.PolicyidOrCollection+"."+sendpremintedtoken.Tokenname.ToHex(), 0, 0,
                    ref buildtransaction);
            }



            if (!string.IsNullOrEmpty(metadatafile) && File.Exists(metadatafile))
            {
                buildtransaction.LogFile += File.ReadAllText(metadatafile) + Environment.NewLine;
            }

            command = command.GetMint(minttoken)
                .GetMetadataJsonFile(metadatafile)
                .GetOutFile(matxrawfile)
                .GetTTL(ttl)
                //  .GetWitnessOverride(2)
                .GetMintingScriptFile(policyscriptfile);
              //  .GetNetwork(mainnet);

            // We only save the first receiver here - this is not correct, but we have to change the transactions table to save more than one
            buildtransaction.BuyerTxOut = new TxOutClass
            { Amount = adatosendout, ReceiverAddress = newmintAndSendClass.MintAndSends.First().Receiveraddress };
            buildtransaction.ProjectTxOut = new TxOutClass { Amount = rest, ReceiverAddress = payaddress };


            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }


            return true;
        }



        public static string MintAndSendCip68(IConnectionMultiplexer redis, NewMintAndSendClass newMintAndSendClass,
          string payaddress, string payskey, bool mainnet, bool submittransaction, bool ignoresoldstate, string mintandsendminutxo, 
          ref BuildTransactionClass buildtransaction)
        {
            string guid = GlobalFunctions.GetGuid();

            string metadatafile = $"{GeneralConfigurationClass.TempFilePath}metadata{guid}.json";
            string policyscriptfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.script";
            string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string paymentskeyfile = $"{GeneralConfigurationClass.TempFilePath}payment{guid}.skey";
            string policyskeyfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.skey";

            foreach (var r in newMintAndSendClass.NftReservations)
            {
                if (r.Nft.NftprojectId != newMintAndSendClass.NftProject.Id)
                    return "NFT are not all in the same project (MintAndSendCip68)";

                if (r.Nft.State == "sold" && ignoresoldstate == false)
                    return "Some NFT are already sold (MintAndSendCip68)";
            }

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";

            var utxopaymentaddress =
                GetNewUtxo(payaddress); //, ref buildtransaction, maxtx: 20);
            utxopaymentaddress = FilterAllTxInWithTokens(utxopaymentaddress);

            buildtransaction.LockTxIn = utxopaymentaddress.TxIn;

            List<NftWithMintingAddressClass> nfts = new List<NftWithMintingAddressClass>();
            foreach (var nftreservation in newMintAndSendClass.NftReservations)
            {
                var res = newMintAndSendClass.MintAndSends.FirstOrDefault(x => x.Reservationtoken == nftreservation.Reservationtoken)?.Receiveraddress;
                nfts.Add(new NftWithMintingAddressClass(nftreservation.Nft, res));
            }

            var metadatafiles = GetCip68MetadataFiles(nfts, guid);

            File.WriteAllText(policyscriptfile, newMintAndSendClass.NftProject.Policyscript);
            File.WriteAllText(paymentskeyfile, payskey);

            List<string> signfiles = new List<string> { paymentskeyfile };


            var countwitness = signfiles.Count();

            string polskey = Encryption.DecryptString(newMintAndSendClass.NftProject.Policyskey,
                newMintAndSendClass.NftProject.Password);
            File.WriteAllText(policyskeyfile, polskey);
            countwitness++;

            // END -Create the Sign Keys for Signing and to calculate the witnesses

            var mintingcosts = GlobalFunctions.GetMintingcosts2(newMintAndSendClass.NftProject.Id,
                newMintAndSendClass.NftReservations.Count, 0);

            long ttl = (long)q.Slot + 6000;
            long fees = 0;

            bool b = MintAndSendCreateTransactionBuildRawCip68(
                redis,
                utxopaymentaddress,
                newMintAndSendClass,
                metadatafiles,
                policyscriptfile,
                matxrawfile,
                guid,
                payaddress,
                countwitness,
                mainnet,
                ttl,
                mintandsendminutxo,
                fees,
                ref buildtransaction);

            if (!b)
            {
                Console.WriteLine(@"Error while creating the transaction");
                Console.WriteLine(buildtransaction.LogFile);
                return "Error while creating the transaction";
            }
            // TODO: Calculate Fees
            CalculateFees(redis, $@"{matxrawfile}", buildtransaction.TxInCount, buildtransaction.TxOutCount,
                countwitness, mainnet, ref buildtransaction, out fees);

            if (fees == 0)
            {
                Console.WriteLine(@"Error while calculating the fees");
                Console.WriteLine(buildtransaction.LogFile);
                return "Error while calculating the fees";
            }
            GlobalFunctions.DeleteFile(matxrawfile);
            b = MintAndSendCreateTransactionBuildRawCip68(
                redis,
                utxopaymentaddress,
                newMintAndSendClass,
                metadatafiles,
                policyscriptfile,
                matxrawfile,
                guid,
                payaddress,
                countwitness,
                mainnet,
                ttl,
                mintandsendminutxo,
                fees,
                ref buildtransaction);


            var ok = SignAndSubmit(redis, signfiles.ToArray(), policyskeyfile, matxrawfile, matxsignedfile, mainnet, submittransaction,
                               ref buildtransaction);

            GlobalFunctions.DeleteFile(policyscriptfile);
            GlobalFunctions.DeleteFile(metadatafile);
            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(policyskeyfile);

            foreach (var a in signfiles)
                GlobalFunctions.DeleteFile(a);
            return ok;
        }

    
        private static bool MintAndSendCreateTransactionBuildRawCip68(IConnectionMultiplexer redis, TxInAddressesClass utxopaymentaddress,
  NewMintAndSendClass newmintAndSendClass,
  List<Cip68MetadataFilesClass> metadatafiles, string policyscriptfile, string matxrawfile, string guid, string payaddress,
  int witnesscount, bool mainnet, long ttl, string mintandsendminutxo, long fees,
  ref BuildTransactionClass buildtransaction)
        {


            string command = CliCommandExtensions.GetTransactionBuildRawWithLatestEra()
                .GetFees(fees);
            // This is the First TX in - this can be the Payin Address or the Account Address

            GetTxInHashes(utxopaymentaddress, out string com1, out int txincount, out var lovelacesummery,
                ref buildtransaction);
            command += $" {com1}";

            long rest = lovelacesummery - buildtransaction.Fees;


            string minttoken = "";
            long adatosendout = 0;
            foreach (var mas in newmintAndSendClass.MintAndSends)
            {
                var reservations =
                    newmintAndSendClass.NftReservations.Where(x => x.Reservationtoken == mas.Reservationtoken)
                        .ToArray();
                string sendtokenUser = "";

                // Calculate the Minuxto - all 6 NFT we will Add 2 ADA - i know, it is not correct, but it works
                foreach (var reservation in reservations)
                {
                    if (!string.IsNullOrEmpty(sendtokenUser))
                        sendtokenUser += " + ";
                    long amount = Math.Max(1, reservation.Tc * reservation.Nft.Multiplier);
                    sendtokenUser +=
                        $"{amount} {newmintAndSendClass.NftProject.Policyid}.{CreateMintTokenname(newmintAndSendClass.NftProject.Tokennameprefix, reservation.Nft.Name, Cip68Type.NftUserToken)}";

                    string tokenname = GlobalFunctions.GetTokenname(newmintAndSendClass.NftProject.Tokennameprefix, reservation.Nft.Name);
                    metadatafiles.First(x => x.Tokenname == tokenname).SendToken = $"1 {newmintAndSendClass.NftProject.Policyid}.{CreateMintTokenname(newmintAndSendClass.NftProject.Tokennameprefix, reservation.Nft.Name, Cip68Type.ReferenceToken)}";


                    if (!string.IsNullOrEmpty(minttoken))
                        minttoken += " + ";
                    minttoken +=
                        $"{amount} {newmintAndSendClass.NftProject.Policyid}.{CreateMintTokenname(newmintAndSendClass.NftProject.Tokennameprefix, reservation.Nft.Name, Cip68Type.NftUserToken)}";
                    minttoken +=
                        $"+ 1 {newmintAndSendClass.NftProject.Policyid}.{CreateMintTokenname(newmintAndSendClass.NftProject.Tokennameprefix, reservation.Nft.Name, Cip68Type.ReferenceToken)}";
                }

                long minutxofinalUser = 2000000;
                //  if (mintandsendminutxo == nameof(MinUtxoTypes.minutxo))
                {
                    // Calculate the REAL minutxo
                    minutxofinalUser = CalculateRequiredMinUtxo(redis, mas.Receiveraddress.FilterToLetterOrDigit(),
                        sendtokenUser, "", guid, mainnet,
                        ref buildtransaction);
                }

                buildtransaction.Cip68ReferenceTokenTxOut = new TxOutClass() { Amount = 0, ReceiverAddress = newmintAndSendClass.NftProject.Cip68referenceaddress };

                foreach (var cip68MetadataFilesClass in metadatafiles)
                {
                    cip68MetadataFilesClass.MinUtxo = CalculateRequiredMinUtxo(redis, newmintAndSendClass.NftProject.Cip68referenceaddress.FilterToLetterOrDigit(), cip68MetadataFilesClass.SendToken, cip68MetadataFilesClass.Filename, guid, mainnet, ref buildtransaction);
                    buildtransaction.Cip68ReferenceTokenTxOut.Amount += cip68MetadataFilesClass.MinUtxo;
                    if (cip68MetadataFilesClass.MinUtxo > 3500000)
                    {
                        buildtransaction.LogFile += $"Error: MinUtxo for {cip68MetadataFilesClass.Filename} is too high" + Environment.NewLine;
                        return false;
                    }
                }

                if (minutxofinalUser == 0)
                    minutxofinalUser = 2000000;

                if (!string.IsNullOrEmpty(sendtokenUser))
                {
                    command +=
                        $" --tx-out {mas.Receiveraddress.FilterToLetterOrDigit()}+{minutxofinalUser}+\"{sendtokenUser}\"";
                    buildtransaction.TxOutCount++;
                }

                adatosendout += minutxofinalUser;
                rest = rest - minutxofinalUser;


                // Reference Nft
                foreach (var cip68MetadataFilesClass in metadatafiles)
                {
                    if (!string.IsNullOrEmpty(cip68MetadataFilesClass.SendToken))
                    {
                        command +=
                            $" --tx-out {newmintAndSendClass.NftProject.Cip68referenceaddress.FilterToLetterOrDigit()}+{cip68MetadataFilesClass.MinUtxo}+\"{cip68MetadataFilesClass.SendToken}\"";
                        buildtransaction.LogFile +=
                            File.ReadAllText(cip68MetadataFilesClass.Filename) + Environment.NewLine;
                        buildtransaction.TxOutCount++;
                        command += $" --tx-out-inline-datum-file {cip68MetadataFilesClass.Filename}";
                        adatosendout += cip68MetadataFilesClass.MinUtxo;
                        rest = rest - cip68MetadataFilesClass.MinUtxo;
                        buildtransaction.TxOutCount++;
                    }
                }
            }

            // Change address
            command += $" --tx-out {payaddress.FilterToLetterOrDigit()}+{rest}";
            buildtransaction.TxOutCount++;


            if (!string.IsNullOrEmpty(minttoken))
                command += $" --mint=\"{minttoken}\"";


            command = command.GetOutFile(matxrawfile)
                .GetTTL(ttl)
                .GetMintingScriptFile(policyscriptfile);


            // We only save the first receiver here - this is not correct, but we have to change the transactions table to save more than one
            buildtransaction.BuyerTxOut = new TxOutClass
            { Amount = adatosendout, ReceiverAddress = newmintAndSendClass.MintAndSends.First().Receiveraddress };
            buildtransaction.ProjectTxOut = new TxOutClass { Amount = rest, ReceiverAddress = payaddress };


            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }

            return true;
        }
        public static bool CalculateFees(IConnectionMultiplexer redis, string bodyfile, int txincount, int txoutcount, int witnesscount, bool mainnet,
            ref BuildTransactionClass buildtransaction, out long fee)
        {
            string guid = GlobalFunctions.GetGuid();
            string protocolParamsFile = $"{GeneralConfigurationClass.TempFilePath}protocol{guid}.json";
            GenerateProtocolParamsFile(protocolParamsFile, redis, mainnet, out string errormessage2);
            if (!string.IsNullOrEmpty(errormessage2))
            {
                buildtransaction.LogFile += errormessage2 + Environment.NewLine;
                fee = 0;
                return false;
            }

            return CalculateFees(bodyfile, txincount, txoutcount, witnesscount, mainnet, protocolParamsFile,
                ref buildtransaction, out fee);
        }


        private static uint CalculateBaseFee(string bodyfile, uint? a = null, uint? b = null)
        {
            if (!a.HasValue)
                a = new uint?(44U);
            if (!b.HasValue)
                b = new uint?(155381U);

            var s = File.ReadAllText(bodyfile);
            if (string.IsNullOrEmpty(s))
                return a.Value + b.Value;

            var matx = JsonConvert.DeserializeObject<MatxRawClass>(s);

            return (uint)matx.CborHex.Length * a.Value + b.Value;
        }


        public static bool CalculateFees(string bodyfile, int txincount, int txoutcount, int witnesscount, bool mainnet,
            string protocolparamsfile, ref BuildTransactionClass buildtransaction, out long fee)
        {
            fee = 0;
            string command2 = CliCommandExtensions.GetTransactionCalculateMinFee()
                .GetTxBodyFile(bodyfile)
                .GetTxInCount(txincount)
                .GetTxOutCount(txoutcount)
                .GetWitnessCount(witnesscount)
                .GetNetwork(mainnet)
                .GetProtocolParamsFile(protocolparamsfile);

            buildtransaction.LogFile += command2 + Environment.NewLine;
            string log2 = CardanoCli(command2, out var errormessage3);
            buildtransaction.LogFile += log2 + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage3))
                buildtransaction.LogFile += errormessage3 + Environment.NewLine;

            //   GlobalFunctions.DeleteFile(protocolparamsfile);

            if (log2.Contains("Lovelace"))
            {
                log2 = log2.Replace(" Lovelace", "").Replace("\n", "").Replace("\r", "");
            }
            else
            {
                fee = CalculateBaseFee(bodyfile) + 10000;
                buildtransaction.Fees = fee;
                return true;
            }

            // I add 10000 to the fee - this is a workaround, because since conway the fees will not calculated correctly
            fee = Convert.ToInt64(log2) + 10000;
            buildtransaction.Fees = fee;

            return true;
        }


        public static void GetTxInHashes(Dictionary<string, string> utxopaymentaddress, out string command,
            out int txincount, out long lovelace, ref BuildTransactionClass buildtransaction)
        {

            if (utxopaymentaddress == null)
            {
                command = "";
                lovelace = 0;
                txincount = 0;
                return;
            }

            command = "";
            int tt = 0;
            long ll = 0;
            do
            {
                tt++;
                if (utxopaymentaddress.ContainsKey($"TxHash{tt}") &&
                    utxopaymentaddress.ContainsKey($"TxId{tt}"))
                {
                    utxopaymentaddress.TryGetValue($"TxHash{tt}", out string txhash);
                    utxopaymentaddress.TryGetValue($"TxId{tt}", out string txid);
                    utxopaymentaddress.TryGetValue($"ll{tt}", out string l1);
                    if (txhash == "" || txid == "")
                        break;
                    command += $" --tx-in {txhash}#{txid}";
                    ll += Convert.ToInt64(l1);

                }
                else
                    break;

                // Remove later - to cosolite - if there are too many tx - just take 100
                //  if (tt == 101)
                //      break;


            } while (true);

            if (string.IsNullOrEmpty(command))
            {
                buildtransaction.LogFile += Environment.NewLine;
                foreach (var dict in utxopaymentaddress)
                {
                    buildtransaction.LogFile += $"{dict.Key} - {dict.Value}{Environment.NewLine}";
                }
            }
            else
            {
                buildtransaction.LogFile += $"TX-IN: {command}{Environment.NewLine}";
            }

            txincount = tt - 1;
            lovelace = ll;
        }

        public static void GetTxInHashes(TxInAddressesClass[] utxopaymentaddress, out string command,
            out int txincount, out long lovelace, ref BuildTransactionClass buildtransaction)
        {
            command = "";
            lovelace = 0;
            txincount = 0;

            if (utxopaymentaddress == null)
                return;


            foreach (var addressesClass in utxopaymentaddress)
            {
                if (addressesClass.TxIn == null)
                    continue;

                foreach (var txInClass in addressesClass.TxIn)
                {
                    command += $" --tx-in {txInClass.TxHashId}";
                    lovelace += txInClass.Lovelace;
                    txincount++;
                }
            }

            if (buildtransaction.LogFile == null)
                buildtransaction.LogFile = "";

            if (txincount==0)
            {
                foreach (var addressesClass in utxopaymentaddress)
                {
                    buildtransaction.LogFile += $"No Txin found - {addressesClass.Address}" + Environment.NewLine;
                }
            }
           

            buildtransaction.LogFile += JsonConvert.SerializeObject(utxopaymentaddress) + Environment.NewLine;
            buildtransaction.TxInCount = txincount;
        }
        public static void GetTxInHashes(TxInAddressesClass utxopaymentaddress, out string command,
            out int txincount, out long lovelace, ref BuildTransactionClass buildtransaction)
        {
            GetTxInHashes(new TxInAddressesClass[] { utxopaymentaddress }, out command, out txincount,
                out lovelace, ref buildtransaction);
        }

        public static void GetTxInHashes(TxInClass txin, out string command,
            out int txincount, out long lovelace, ref BuildTransactionClass buildtransaction)
        {
            command = "";
            lovelace = 0;
            txincount = 0;

            if (txin == null)
                return;

            command += $" --tx-in {txin.TxHashId}";
            lovelace += txin.Lovelace;
            txincount++;

            buildtransaction.TxInCount = txincount;
        }

        public static void GetTxInHashesCollateral(TxInAddressesClass[] utxopaymentaddress, out string command)
        {
            command = "";

            if (utxopaymentaddress == null)
                return;

            int i = 0;
            foreach (var addressesClass in utxopaymentaddress)
            {
                if (addressesClass.TxIn == null)
                    continue;

                foreach (var txInClass in addressesClass.TxIn)
                {
                    i++;
                    command += $" --tx-in-collateral {txInClass.TxHashId}";
                    if (i >= 3)
                        return;
                }
            }
        }
        public static void GetTxInHashes(TxInClass[] txins, out string command,
            out int txincount, out long lovelace, ref BuildTransactionClass buildtransaction)
        {
            command = "";
            lovelace = 0;
            txincount = 0;

            if (txins == null)
                return;

            foreach (var txInClass in txins)
            {
                command += $" --tx-in {txInClass.TxHashId}";
                lovelace += txInClass.Lovelace;
                txincount++;
            }
        }

        private static bool CalculateFeesMultipleTokens(IConnectionMultiplexer redis,
            TxInClass txin,
            string receiveraddress, string selleraddress, string mintcostsaddr, string policyid, MultipleTokensClass[] nft,
            string tokennameprefix, string metadatafile, string policyscriptfile, string matxrawfile, string guid,
            int witnesscount, long hastopay, out long fee, bool mainnet,
            Nftprojectsadditionalpayout[] additionalpayoutWallets, PromotionClass promotion, Adminmintandsendaddress paywallet, string refundaddress,
            ref BuildTransactionClass buildtransaction)
        {
            string command = CliCommandExtensions.GetTransactionBuildRawWithLatestEra()
                .GetFees(0);
            fee = 0;

            if (string.IsNullOrEmpty(receiveraddress))
                return false;


            // This is the First TX in - this can be the Payin Address or the Account Address
            int txincount = 0;
            int txoutcount = 0;
            GetTxInHashes(txin, out var com1, out var tt, out var lovelacesummery, ref buildtransaction);
            command += com1;
            txincount += tt;

            // Look here for further TX In and Tx Hashes
            // these can be from the already preminted nfts
            foreach (var nx in nft)
            {
                if (nx.nft.InstockpremintedaddressId != null)
                {
                    com1 = "";
                    txincount = 0;

                    GetTxInHashes(GetNewUtxo(nx.nft.Instockpremintedaddress.Address),
                        out com1,
                        out tt, out lovelacesummery, ref buildtransaction);
                    command += com1;
                    txincount += tt;
                }
            }

            // Pay only with Tokens - Get Txin from adminmintandsendaddress
            if (hastopay == -1)
            {
                var utxopaymentaddress = GetNewUtxo(paywallet.Address);
                utxopaymentaddress = FilterAllTxInWithTokens(utxopaymentaddress);
                GetTxInHashes(utxopaymentaddress, out com1,
                    out tt, out lovelacesummery, ref buildtransaction);
                command += com1;
                txincount += tt;
            }


            List<MultipleTokensClass> minttokens = new List<MultipleTokensClass>();
            List<MultipleTokensClass> sendtokens = new List<MultipleTokensClass>();
            foreach (var nx in nft)
            {
                if (nx.nft.InstockpremintedaddressId != null) // || nx.nft.Fingerprint != null)
                {
                    sendtokens.Add(nx);
                }
                else
                {
                    sendtokens.Add(nx);
                    minttokens.Add(nx);
                }
            }

            string minttoken = "";
            string sendtoken = "";
            int u = 0;
            foreach (var tok in minttokens)
            {
                u++;
                minttoken +=
                    $"{(tok.tokencount * tok.nft.Multiplier)} {policyid}.{CreateMintTokenname(tokennameprefix, tok.nft.Name)}"; // Must be the same as _policyname_ in metadata
                if (u < minttokens.Count())
                    minttoken += " + ";
            }

            u = 0;
            foreach (var tok in sendtokens)
            {
                u++;
                sendtoken +=
                    $"{(tok.tokencount * tok.nft.Multiplier)} {policyid}.{CreateMintTokenname(tokennameprefix, tok.nft.Name)}";
                if (u < sendtokens.Count())
                    sendtoken += " + ";
            }

            if (promotion != null)
            {
                minttoken += $" + {promotion.Token}";
                sendtoken += $" + {promotion.Token}";
            }

            command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+0+\"{sendtoken}\"";
            txoutcount++;


            // Hack for free nfts
            if (hastopay > 2000000)
            {
                // Check for additional Payment Tokens and send them to the customers wallet
                string tokens = GetAdditonalPaymentTokens(txin);
                if (!string.IsNullOrEmpty(tokens))
                    command += $" --tx-out {selleraddress.FilterToLetterOrDigit()}+0+\"{tokens}\"";
                else
                    command += $" --tx-out {selleraddress.FilterToLetterOrDigit()}+0";

                txoutcount++;


                foreach (var nftprojectsadditionalpayout in additionalpayoutWallets)
                {
                    command += $" --tx-out {nftprojectsadditionalpayout.Wallet.Walletaddress.FilterToLetterOrDigit()}+0";
                    txoutcount++;
                }

                if (!string.IsNullOrEmpty(mintcostsaddr))
                {
                    command += $" --tx-out {mintcostsaddr.FilterToLetterOrDigit()}+0";
                    txoutcount++;
                }
                long adaamount1 = txin.Lovelace;
                if (adaamount1 > hastopay)
                {
                    var refund = adaamount1 - hastopay;
                    if (refund > 1500000 && !string.IsNullOrEmpty(refundaddress))
                    {
                        command += $" --tx-out {refundaddress.FilterToLetterOrDigit()}+{refund}";
                        txoutcount++;
                    }
                }

            }
            // Hack for Pay only with tokens 
            if (hastopay == -1)
            {
                string tokens = GetAdditonalPaymentTokens(txin);
                if (!string.IsNullOrEmpty(tokens))
                {
                    command += $" --tx-out {selleraddress.FilterToLetterOrDigit()}+0+\"{tokens}\"";
                    txoutcount++;
                }
                command += $" --tx-out {paywallet.Address.FilterToLetterOrDigit()}+0";
                txoutcount++;
            }

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --mint=\"{minttoken}\"";

            if (!string.IsNullOrEmpty(metadatafile) && File.Exists(metadatafile))
            {
                buildtransaction.LogFile += File.ReadAllText(metadatafile) + Environment.NewLine;
                command += $" --metadata-json-file {metadatafile}";
            }

            command += $" --out-file {matxrawfile}";

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --minting-script-file {policyscriptfile}";


            if (promotion != null)
            {
                string promotionscriptfile =
                    $"{GeneralConfigurationClass.TempFilePath}promotionscriptfile{guid}.script";
                File.WriteAllText(promotionscriptfile, promotion.PolicyScriptfile);
                command += $" --minting-script-file {promotionscriptfile}";
            }

            //   command = command.GetLatestEra(redis, mainnet);

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
                buildtransaction.LogFile += errormessage + Environment.NewLine;

            if (!string.IsNullOrEmpty(log))
            {
                return false;
            }
            return CalculateFees(redis, $@"{matxrawfile}", txincount, txoutcount,
                witnesscount, mainnet,
                ref buildtransaction, out fee);
          /*  return CalculateFees(redis, $@"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw", txincount, txoutcount,
                witnesscount, mainnet,
                ref buildtransaction, out fee);*/
        }



        private static bool CreateTransactionMultipleTokens(IConnectionMultiplexer redis, string senderaddress,
            TxInClass txin, string receiveraddress, string selleraddress,
            string mintcostsaddr, string policyid, MultipleTokensClass[] nft, string tokennameprefix,
            string metadatafile, string policyscriptfile, string matxrawfile, string guid, long mintcosts, long minutxo,
            long stakerewards, long tokenrewards,
            long fee, long ttl, long hastopay, string calculateminutxo, bool mainnet,
            Nftprojectsadditionalpayout[] additionalpayoutWallets, float discount, PromotionClass promotion, Adminmintandsendaddress paywallet, string refundaddress,
            ref BuildTransactionClass buildtransaction)
        {
            buildtransaction.SenderAddress = senderaddress;
            buildtransaction.Fees = fee;

            string command = CliCommandExtensions.GetTransactionBuildRawWithLatestEra()
                .GetFees(fee);

            if (string.IsNullOrEmpty(receiveraddress))
                return false;


            // This is the First TX in - this can be the Payin Address or the Account Address
            GetTxInHashes(txin, out var com1, out var tt, out var lovelacesummery, ref buildtransaction);
            command += com1;

            long adaamountfrompremintedOrpaywallet = 0;

            // Look here for further TX In and Tx Hashes
            // these can be from the already preminted nfts
            foreach (var nx in nft)
            {
                if (nx.nft.InstockpremintedaddressId != null)
                {
                    var utxo = GetNewUtxo(nx.nft.Instockpremintedaddress.Address);
                    GetTxInHashes(utxo, out com1, out tt, out lovelacesummery, ref buildtransaction);
                    command += com1;
                    adaamountfrompremintedOrpaywallet += utxo.LovelaceSummary;
                }
            }

            // Pay only with Tokens - Get Txin from adminmintandsendaddress
            if (hastopay == -1)
            {
                var utxopaymentaddress = GetNewUtxo(paywallet.Address);
                utxopaymentaddress = FilterAllTxInWithTokens(utxopaymentaddress);
                GetTxInHashes(utxopaymentaddress, out com1,
                    out tt, out lovelacesummery, ref buildtransaction);
                command += com1;
                adaamountfrompremintedOrpaywallet += utxopaymentaddress.LovelaceSummary;

                buildtransaction.LockTxIn = utxopaymentaddress.TxIn;
            }



            List<MultipleTokensClass> minttokens = new List<MultipleTokensClass>();
            List<MultipleTokensClass> sendtokens = new List<MultipleTokensClass>();
            foreach (var nx in nft)
            {
                if (nx.nft.InstockpremintedaddressId != null) //|| nx.nft.Fingerprint != null)
                {
                    sendtokens.Add(nx);
                }
                else
                {
                    sendtokens.Add(nx);
                    minttokens.Add(nx);
                }
            }

            string minttoken = "";
            string sendtoken = "";
            int u = 0;
            foreach (var tok in minttokens)
            {
                u++;
                minttoken +=
                    $"{(tok.tokencount * tok.nft.Multiplier)} {policyid}.{CreateMintTokenname(tokennameprefix, tok.nft.Name)}";
                if (u < minttokens.Count())
                    minttoken += " + ";
            }

            u = 0;
            foreach (var tok in sendtokens)
            {
                u++;
                sendtoken +=
                    $"{(tok.tokencount * tok.nft.Multiplier)} {policyid}.{CreateMintTokenname(tokennameprefix, tok.nft.Name)}";
                if (u < sendtokens.Count())
                    sendtoken += " + ";
            }

            if (promotion != null)
            {
                minttoken += $" + {promotion.Token}";
                sendtoken += $" + {promotion.Token}";
            }


            // Calculate the Minuxto - all 6 NFT we will Add 2 ADA - i know, it is not correct, but it works
            long minutxofinal = 0;
            long sendbackToUser = minutxo;
            int ux = 0;

            if (string.IsNullOrEmpty(calculateminutxo) || calculateminutxo == nameof(MinUtxoTypes.twoadaeverynft))
            {
                sendbackToUser = minutxo * nft.Length;
            }

            if (calculateminutxo == nameof(MinUtxoTypes.twoadaall5nft))
            {
                foreach (var tok in nft)
                {
                    ux++;
                    if (ux >= 5)
                    {
                        ux = 0;
                        sendbackToUser += minutxo;
                    }
                }
            }

            if (calculateminutxo == nameof(MinUtxoTypes.minutxo))
            {
                sendbackToUser =
                    CalculateRequiredMinUtxo(redis, receiveraddress, sendtoken, "", guid, mainnet, ref buildtransaction);
            }

            minutxofinal = sendbackToUser;

            long adaamount1 = txin.Lovelace;
            long refund = 0;

            if (hastopay > 2000000)
            {

                // If the customer paid to much, send the amount back - or to the refund address
                if (adaamount1 > hastopay)
                {
                    refund = adaamount1 - hastopay;
                    if (!string.IsNullOrEmpty(refundaddress) && refund > 1500000)
                    {
                        command += $" --tx-out {refundaddress.FilterToLetterOrDigit()}+{refund}";
                    }
                    else
                        minutxofinal += (adaamount1 - hastopay);
                }

                // Stakerewards
                if (mintcosts - stakerewards - tokenrewards >= 1000000)
                {
                    minutxofinal += stakerewards;
                    mintcosts -= stakerewards;
                    minutxofinal += tokenrewards;
                    mintcosts -= tokenrewards;
                    buildtransaction.StakeRewards = stakerewards;
                    buildtransaction.TokenRewards = tokenrewards;
                }

                if (discount > 0)
                {
                    long d = (long)((hastopay - sendbackToUser) / 100 * discount);
                    minutxofinal += d;
                    buildtransaction.Discount = d;
                }

                long rest = (adaamount1 + adaamountfrompremintedOrpaywallet) - fee - mintcosts - minutxofinal - refund;


                List<TxOutClass> txout = new List<TxOutClass>();
                buildtransaction.BuyerTxOut = new TxOutClass
                { Amount = minutxofinal, ReceiverAddress = receiveraddress };
                buildtransaction.ProjectTxOut = new TxOutClass { Amount = rest, ReceiverAddress = selleraddress, Tokens = txin.Tokens.ToArray() };

                command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+{minutxofinal}+\"{sendtoken}\"";


                if (!string.IsNullOrEmpty(mintcostsaddr) && mintcosts != 0)
                {
                    buildtransaction.MintingcostsTxOut = new TxOutClass
                    { Amount = mintcosts, ReceiverAddress = mintcostsaddr };
                    command += $" --tx-out {mintcostsaddr.FilterToLetterOrDigit()}+{mintcosts}";
                }

                foreach (var nftprojectsadditionalpayout in additionalpayoutWallets.OrEmptyIfNull())
                {
                    long addvalue = GetAdditionalPayoutwalletsValue(nftprojectsadditionalpayout,
                        hastopay - buildtransaction.Fees - mintcosts - minutxofinal, nft.Length);
                    buildtransaction.LogFile +=
                        $"HasToPay: {hastopay} + Fees: {buildtransaction.Fees} + Mintcosts: {mintcosts} + MinutxoFinal: {minutxofinal} (incl. Discount: {buildtransaction.Discount ?? 0}) + Add.Wallets: {addvalue}" +
                        Environment.NewLine;

                    if (addvalue <= 0) continue;
                    command += $" --tx-out {nftprojectsadditionalpayout.Wallet.Walletaddress.FilterToLetterOrDigit()}+{addvalue}";
                    rest -= addvalue;
                    nftprojectsadditionalpayout.Valuetotal = addvalue;
                }
                buildtransaction.AdditionalPayouts = additionalpayoutWallets;

                // Check for additional Payment Tokens and send them to the customers wallet
                string tokens = GetAdditonalPaymentTokens(txin);
                if (!string.IsNullOrEmpty(tokens))
                    command += $" --tx-out {selleraddress.FilterToLetterOrDigit()}+{rest}+\"{tokens}\"";
                else
                    command += $" --tx-out {selleraddress.FilterToLetterOrDigit()}+{rest}";

                if (txin.Tokens != null)
                    buildtransaction.PriceInTokens = txin.Tokens.FirstOrDefault();

                if (rest < 0)
                {
                    return false;
                }

            }
            if (hastopay <= 2000000 && hastopay >= 0)
            {
                // Free Tokens
                command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+{(hastopay - fee)}+\"{sendtoken}\"";
                buildtransaction.BuyerTxOut = new TxOutClass
                { Amount = minutxofinal, ReceiverAddress = receiveraddress };
                buildtransaction.ProjectTxOut = new TxOutClass { Amount = 0, ReceiverAddress = selleraddress };
            }

            // Pay only with tokens
            if (hastopay == -1)
            {
                // Send Token to User
                command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+{minutxofinal}+\"{sendtoken}\"";

                // Send Payment Tokens to the Seller
                long minutxofinal2 = 0;
                string tokens = GetAdditonalPaymentTokens(txin);
                if (!string.IsNullOrEmpty(tokens))
                {
                    minutxofinal2 = CalculateRequiredMinUtxo(redis, selleraddress, tokens, "", guid, mainnet, ref buildtransaction);
                    command += $" --tx-out {selleraddress.FilterToLetterOrDigit()}+{minutxofinal2}+\"{tokens}\"";
                }

                long rest = (adaamount1 + adaamountfrompremintedOrpaywallet) - fee - mintcosts - minutxofinal - minutxofinal2;

                // Send the rest to the Paywallet
                command += $" --tx-out {paywallet.Address.FilterToLetterOrDigit()}+{rest}";

                buildtransaction.NmkrCosts = adaamountfrompremintedOrpaywallet - rest;
                buildtransaction.BuyerTxOut = new TxOutClass
                { Amount = minutxofinal, ReceiverAddress = receiveraddress };
                buildtransaction.ProjectTxOut = new TxOutClass { Amount = minutxofinal2, ReceiverAddress = selleraddress, Tokens = txin.Tokens.ToArray() };
                if (txin.Tokens != null)
                    buildtransaction.PriceInTokens = txin.Tokens.FirstOrDefault();
            }


            if (!string.IsNullOrEmpty(minttoken))
                command += $" --mint=\"{minttoken}\"";

            if (!string.IsNullOrEmpty(metadatafile) && File.Exists(metadatafile))
            {
                buildtransaction.LogFile += File.ReadAllText(metadatafile) + Environment.NewLine;
                command += $" --metadata-json-file {metadatafile}";
            }

            command += $" --out-file {matxrawfile}";

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --minting-script-file {policyscriptfile}";


            if (promotion != null)
            {
                string promotionscriptfile =
                    $"{GeneralConfigurationClass.TempFilePath}promotionscriptfile{guid}.script";
                File.WriteAllText(promotionscriptfile, promotion.PolicyScriptfile);
                command += $" --minting-script-file {promotionscriptfile}";
            }


            /*  command = command.GetLatestEra(redis, mainnet)
                  .GetTTL(ttl);*/

            command = command.GetTTL(ttl);

            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.ErrorMessage = errormessage;
                buildtransaction.LogFile += errormessage + Environment.NewLine;
            }

            if (!string.IsNullOrEmpty(log))
            {
                return false;
            }

            return true;
        }


        private static bool CalculateFeesMultipleTokensCip68(IConnectionMultiplexer redis,
        TxInClass txin,
        string receiveraddress, string txoutaddr, string mintcostsaddr, string referenceaddress, string policyid, MultipleTokensClass[] nft,
        string tokennameprefix, List<Cip68MetadataFilesClass> metadatafiles, string policyscriptfile, string matxrawfile, string guid,
        int witnesscount, long hastopay, out long fee, bool mainnet,
        Nftprojectsadditionalpayout[] additionalpayoutWallets, PromotionClass promotion,
        ref BuildTransactionClass buildtransaction)
        {
            string command = CliCommandExtensions.GetTransactionBuildRawWithLatestEra()
                .GetFees(0);
            fee = 0;

            if (string.IsNullOrEmpty(receiveraddress))
                return false;


            // This is the First TX in - this can be the Payin Address or the Account Address
            int txincount = 0;
            int txoutcount = 0;
            GetTxInHashes(txin, out var com1, out var tt, out var lovelacesummery, ref buildtransaction);
            command += com1;
            txincount += tt;

            // Look here for further TX In and Tx Hashes
            // these can be from the already preminted nfts
            foreach (var nx in nft)
            {
                if (nx.nft.InstockpremintedaddressId != null)
                {
                    com1 = "";
                    txincount = 0;

                    GetTxInHashes(GetNewUtxo(nx.nft.Instockpremintedaddress.Address),
                        out com1,
                        out tt, out lovelacesummery, ref buildtransaction);
                    command += com1;
                    txincount += tt;
                }
            }

            List<MultipleTokensClass> minttokens = new List<MultipleTokensClass>();
            List<MultipleTokensClass> sendtokens = new List<MultipleTokensClass>();
            foreach (var nx in nft)
            {
                if (nx.nft.InstockpremintedaddressId != null) // || nx.nft.Fingerprint != null)
                {
                    sendtokens.Add(nx);
                }
                else
                {
                    sendtokens.Add(nx);
                    minttokens.Add(nx);
                }
            }

            string minttoken = "";
            string sendtokenUser = "";


            int u = 0;
            foreach (var tok in minttokens)
            {
                u++;
                minttoken +=
                    $"{(tok.tokencount * tok.nft.Multiplier)} {policyid}.{CreateMintTokenname(tokennameprefix, tok.nft.Name, Cip68Type.NftUserToken)} + {(tok.tokencount * tok.nft.Multiplier)} {policyid}.{CreateMintTokenname(tokennameprefix, tok.nft.Name, Cip68Type.ReferenceToken)}"; // Must be the same as _policyname_ in metadata

                if (u < minttokens.Count())
                {
                    minttoken += " + ";
                }
            }

            u = 0;
            foreach (var tok in sendtokens)
            {
                u++;
                sendtokenUser +=
                    $"{(tok.tokencount * tok.nft.Multiplier)} {policyid}.{CreateMintTokenname(tokennameprefix, tok.nft.Name, Cip68Type.NftUserToken)}";
                string tokenname = GlobalFunctions.GetTokenname(tok.nft.Nftproject.Tokennameprefix, tok.nft.Name);
                metadatafiles.First(x => x.Tokenname == tokenname).SendToken = $"{(tok.tokencount * tok.nft.Multiplier)} {policyid}.{CreateMintTokenname(tokennameprefix, tok.nft.Name, Cip68Type.ReferenceToken)}";
                if (u < sendtokens.Count())
                {
                    sendtokenUser += " + ";
                }
            }

            if (promotion != null)
            {
                // TODO: this is not correct - we have to look if the promotion project is a cip68 project or not. If not, we have to add regular metadata to the transaction
                // TODO: the promotion token must be send to an other reference address
                minttoken += $" + {CreateMintTokenname("", promotion.Token, Cip68Type.NftUserToken)} + {CreateMintTokenname("", promotion.Token, Cip68Type.ReferenceToken)}";
                sendtokenUser += $" + {CreateMintTokenname("", promotion.Token, Cip68Type.NftUserToken)}";
                //minttoken += $" + {CreateMintTokenname("", promotion.Token, 1)}";
            }

            command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+0+\"{sendtokenUser}\"";
            txoutcount++;

            foreach (var filesClass in metadatafiles)
            {
                command += $" --tx-out {referenceaddress.FilterToLetterOrDigit()}+0+\"{filesClass.SendToken}\"";
                buildtransaction.LogFile += File.ReadAllText(filesClass.Filename) + Environment.NewLine;
                command += $" --tx-out-inline-datum-file {filesClass.Filename}";
                txoutcount++;
            }



            // Hack for free nfts
            if (hastopay > 2000000)
            {
                // Check for additional Payment Tokens and send them to the customers wallet
                string tokens = GetAdditonalPaymentTokens(txin);
                if (!string.IsNullOrEmpty(tokens))
                    command += $" --tx-out {txoutaddr.FilterToLetterOrDigit()}+0+\"{tokens}\"";
                else
                    command += $" --tx-out {txoutaddr.FilterToLetterOrDigit()}+0";

                txoutcount++;


                foreach (var nftprojectsadditionalpayout in additionalpayoutWallets)
                {
                    command += $" --tx-out {nftprojectsadditionalpayout.Wallet.Walletaddress.FilterToLetterOrDigit()}+0";
                    txoutcount++;
                }

                if (!string.IsNullOrEmpty(mintcostsaddr))
                {
                    command += $" --tx-out {mintcostsaddr.FilterToLetterOrDigit()}+0";
                    txoutcount++;
                }
            }

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --mint=\"{minttoken}\"";

            //  command += " --json-metadata-detailed-schema";

            command += $" --out-file {matxrawfile}";

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --minting-script-file {policyscriptfile}";


            if (promotion != null)
            {
                string promotionscriptfile =
                    $"{GeneralConfigurationClass.TempFilePath}promotionscriptfile{guid}.script";
                File.WriteAllText(promotionscriptfile, promotion.PolicyScriptfile);
                command += $" --minting-script-file {promotionscriptfile}";
            }

            //   command = command.GetLatestEra(redis, mainnet);

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
                buildtransaction.LogFile += errormessage + Environment.NewLine;

            if (!string.IsNullOrEmpty(log))
            {
                return false;
            }

            return CalculateFees(redis, $@"{matxrawfile}", txincount, txoutcount,
                witnesscount, mainnet,
                ref buildtransaction, out fee);
        }
        private static bool CreateTransactionMultipleTokensCip68(IConnectionMultiplexer redis, string senderaddress,
         TxInClass txin, string receiveraddress, string txoutaddr,
         string mintcostsaddr, string referenceaddress, string policyid, MultipleTokensClass[] nft, string tokennameprefix,
         List<Cip68MetadataFilesClass> metadatafiles, string policyscriptfile, string matxrawfile, string guid, long mintcosts, long minutxo,
         long stakerewards, long tokenrewards,
         long fee, long ttl, long hastopay, string calculateminutxo, bool mainnet,
         Nftprojectsadditionalpayout[] additionalpayoutWallets, float discount, PromotionClass promotion,
         ref BuildTransactionClass buildtransaction)
        {
            buildtransaction.SenderAddress = senderaddress;
            buildtransaction.Fees = fee;

            string command = CliCommandExtensions.GetTransactionBuildRawWithLatestEra()
                .GetFees(fee);

            if (string.IsNullOrEmpty(receiveraddress))
                return false;


            // This is the First TX in - this can be the Payin Address or the Account Address
            GetTxInHashes(txin, out var com1, out var tt, out var lovelacesummery, ref buildtransaction);
            command += com1;

            long adaamountfrompreminted = 0;

            // Look here for further TX In and Tx Hashes
            // these can be from the already preminted nfts
            foreach (var nx in nft)
            {
                if (nx.nft.InstockpremintedaddressId != null)
                {
                    var utxo = GetNewUtxo(nx.nft.Instockpremintedaddress.Address);
                    GetTxInHashes(utxo, out com1, out tt, out lovelacesummery, ref buildtransaction);
                    command += com1;
                    adaamountfrompreminted += utxo.LovelaceSummary;
                }
            }

            List<MultipleTokensClass> minttokens = new List<MultipleTokensClass>();
            List<MultipleTokensClass> sendtokens = new List<MultipleTokensClass>();
            foreach (var nx in nft)
            {
                if (nx.nft.InstockpremintedaddressId != null) //|| nx.nft.Fingerprint != null)
                {
                    sendtokens.Add(nx);
                }
                else
                {
                    sendtokens.Add(nx);
                    minttokens.Add(nx);
                }
            }

            string minttoken = "";
            string sendtokenUser = "";

            int u = 0;
            foreach (var tok in minttokens)
            {
                u++;
                minttoken +=
                    $"{(tok.tokencount * tok.nft.Multiplier)} {policyid}.{CreateMintTokenname(tokennameprefix, tok.nft.Name, Cip68Type.NftUserToken)} + {(tok.tokencount * tok.nft.Multiplier)} {policyid}.{CreateMintTokenname(tokennameprefix, tok.nft.Name, Cip68Type.ReferenceToken)}"; // Must be the same as _policyname_ in metadata

                if (u < minttokens.Count())
                {
                    minttoken += " + ";
                }
            }

            u = 0;
            foreach (var tok in sendtokens)
            {
                u++;
                sendtokenUser +=
                    $"{(tok.tokencount * tok.nft.Multiplier)} {policyid}.{CreateMintTokenname(tokennameprefix, tok.nft.Name, Cip68Type.NftUserToken)}";
                string tokenname = GlobalFunctions.GetTokenname(tok.nft.Nftproject.Tokennameprefix, tok.nft.Name);
                metadatafiles.First(x => x.Tokenname == tokenname).SendToken = $"{(tok.tokencount * tok.nft.Multiplier)} {policyid}.{CreateMintTokenname(tokennameprefix, tok.nft.Name, Cip68Type.ReferenceToken)}";
                if (u < sendtokens.Count())
                {
                    sendtokenUser += " + ";
                }
            }

            if (promotion != null)
            {
                minttoken += $" + {CreateMintTokenname("", promotion.Token, Cip68Type.NftUserToken)} + {CreateMintTokenname("", promotion.Token, Cip68Type.ReferenceToken)}";
                sendtokenUser += $" + {CreateMintTokenname("", promotion.Token, Cip68Type.ReferenceToken)}";
                //minttoken += $" + {CreateMintTokenname("", promotion.Token, 1)}";
            }



            // Calculate the Minuxto - all 6 NFT we will Add 2 ADA - i know, it is not correct, but it works
            long minutxofinal = 0;
            long sendbackToUser = minutxo;
            int ux = 0;

            if (string.IsNullOrEmpty(calculateminutxo) || calculateminutxo == nameof(MinUtxoTypes.twoadaeverynft))
            {
                sendbackToUser = minutxo * nft.Length;
            }

            if (calculateminutxo == nameof(MinUtxoTypes.twoadaall5nft))
            {
                foreach (var tok in nft)
                {
                    ux++;
                    if (ux >= 5)
                    {
                        ux = 0;
                        sendbackToUser += minutxo;
                    }
                }
            }

            if (calculateminutxo == nameof(MinUtxoTypes.minutxo))
            {
                sendbackToUser =
                    CalculateRequiredMinUtxo(redis, receiveraddress, sendtokenUser, "", guid, mainnet, ref buildtransaction);
            }

            buildtransaction.Cip68ReferenceTokenTxOut = new TxOutClass { Amount = 0, ReceiverAddress = referenceaddress };
            foreach (var cip68MetadataFilesClass in metadatafiles)
            {
                cip68MetadataFilesClass.MinUtxo = CalculateRequiredMinUtxo(redis, referenceaddress, cip68MetadataFilesClass.SendToken, cip68MetadataFilesClass.Filename, guid, mainnet, ref buildtransaction);
                buildtransaction.Cip68ReferenceTokenTxOut.Amount += cip68MetadataFilesClass.MinUtxo;
            }


            minutxofinal = sendbackToUser;

            long adaamount1 = txin.Lovelace;

            if (hastopay > 2000000)
            {
                // If the customer paid to much, send the amount back
                if (adaamount1 > hastopay)
                {
                    minutxofinal += (adaamount1 - hastopay);
                }

                // Stakerewards
                if (mintcosts - stakerewards - tokenrewards >= 1000000)
                {
                    minutxofinal += stakerewards;
                    mintcosts -= stakerewards;
                    minutxofinal += tokenrewards;
                    mintcosts -= tokenrewards;
                    buildtransaction.StakeRewards = stakerewards;
                    buildtransaction.TokenRewards = tokenrewards;
                }

                if (discount > 0)
                {
                    long d = (long)((hastopay - sendbackToUser) / 100 * discount);
                    minutxofinal += d;
                    buildtransaction.Discount = d;
                }

                long rest = (adaamount1 + adaamountfrompreminted) - fee - mintcosts - minutxofinal;


                List<TxOutClass> txout = new List<TxOutClass>();
                buildtransaction.BuyerTxOut = new TxOutClass
                { Amount = minutxofinal, ReceiverAddress = receiveraddress };
                buildtransaction.ProjectTxOut = new TxOutClass { Amount = rest, ReceiverAddress = txoutaddr };


                command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+{minutxofinal}+\"{sendtokenUser}\"";


                if (!string.IsNullOrEmpty(mintcostsaddr) && mintcosts != 0)
                {
                    buildtransaction.MintingcostsTxOut = new TxOutClass
                    { Amount = mintcosts, ReceiverAddress = mintcostsaddr };
                    command += $" --tx-out {mintcostsaddr.FilterToLetterOrDigit()}+{mintcosts}";
                }


                foreach (var nftprojectsadditionalpayout in additionalpayoutWallets.OrEmptyIfNull())
                {
                    long addvalue = GetAdditionalPayoutwalletsValue(nftprojectsadditionalpayout,
                        hastopay - buildtransaction.Fees - mintcosts - minutxofinal, nft.Length);
                    buildtransaction.LogFile +=
                        $"HasToPay: {hastopay} + Fees: {buildtransaction.Fees} + Mintcosts: {mintcosts} + MinutxoFinal: {minutxofinal} (incl. Discount: {buildtransaction.Discount ?? 0}) + Add.Wallets: {addvalue}" +
                        Environment.NewLine;

                    if (addvalue <= 0) continue;
                    command += $" --tx-out {nftprojectsadditionalpayout.Wallet.Walletaddress.FilterToLetterOrDigit()}+{addvalue}";
                    rest -= addvalue;
                    nftprojectsadditionalpayout.Valuetotal = addvalue;
                }
                buildtransaction.AdditionalPayouts = additionalpayoutWallets;

                rest -= metadatafiles.Sum(x => x.MinUtxo);

                // Check for additional Payment Tokens and send them to the customers wallet
                string tokens = GetAdditonalPaymentTokens(txin);
                if (!string.IsNullOrEmpty(tokens))
                    command += $" --tx-out {txoutaddr.FilterToLetterOrDigit()}+{rest}+\"{tokens}\"";
                else
                    command += $" --tx-out {txoutaddr.FilterToLetterOrDigit()}+{rest}";



                if (rest < 0)
                {
                    return false;
                }
            }
            else
            {
                // Free Tokens
                command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+{(hastopay - fee - metadatafiles.Sum(x => x.MinUtxo))}+\"{sendtokenUser}\"";
                buildtransaction.BuyerTxOut = new TxOutClass
                { Amount = minutxofinal, ReceiverAddress = receiveraddress };
                buildtransaction.ProjectTxOut = new TxOutClass { Amount = 0, ReceiverAddress = txoutaddr };
            }
            // Reference Nft
            foreach (var cip68MetadataFilesClass in metadatafiles)
            {
                command +=
                    $" --tx-out {referenceaddress.FilterToLetterOrDigit()}+{cip68MetadataFilesClass.MinUtxo}+\"{cip68MetadataFilesClass.SendToken}\"";

                buildtransaction.LogFile +=
                    File.ReadAllText(cip68MetadataFilesClass.Filename) + Environment.NewLine;
                command += $" --tx-out-inline-datum-file {cip68MetadataFilesClass.Filename}";
            }

            //  command += " --json-metadata-detailed-schema";

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --mint=\"{minttoken}\"";



            command += $" --out-file {matxrawfile}";

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --minting-script-file {policyscriptfile}";


            if (promotion != null)
            {
                string promotionscriptfile =
                    $"{GeneralConfigurationClass.TempFilePath}promotionscriptfile{guid}.script";
                File.WriteAllText(promotionscriptfile, promotion.PolicyScriptfile);
                command += $" --minting-script-file {promotionscriptfile}";
            }


            //command += $" --ttl {ttl}";
            command = command.GetTTL(ttl);

            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.ErrorMessage = errormessage;
                buildtransaction.LogFile += errormessage + Environment.NewLine;
            }

            if (!string.IsNullOrEmpty(log))
            {
                return false;
            }

            return true;
        }


        private static bool CalculateFees(IConnectionMultiplexer redis, TxInAddressesClass utxopaymentaddress,
            string receiveraddress,
            string txoutaddr, string mintcostsaddr, string policyid, Nft[] nft, string tokennameprefix,
            string metadatafile, string policyscriptfile, string matxrawfile, string guid, int witnesscount,
            long hastopay,
            out long fee, string nodeversion, bool mainnet, Nftprojectsadditionalpayout[] additionalpayoutWallets,
            float discount, ref BuildTransactionClass buildtransaction,
            bool ignoresoldflag = false)
        {
            string command = "latest transaction build-raw --fee 0";
            fee = 0;

            if (string.IsNullOrEmpty(receiveraddress))
                return false;


            // This is the First TX in - this can be the Payin Address or the Account Address
            int txoutcount = 0;
            int txincount = 0;
            GetTxInHashes(utxopaymentaddress, out var com1, out var tt, out var lovelacesummery, ref buildtransaction);
            command += com1;

            txincount = txincount + tt;

            // Look here for further TX In and Tx Hashes
            // these can be from the already preminted nfts
            foreach (var nx in nft)
            {
                if (nx.InstockpremintedaddressId != null)
                {
                    GetTxInHashes(GetNewUtxo(nx.Instockpremintedaddress.Address), out com1,
                        out tt, out lovelacesummery, ref buildtransaction);
                    command += com1;

                    txincount = txincount + tt;
                }
            }

            List<string> minttokens = new List<string>();
            List<string> sendtokens = new List<string>();
            foreach (var nx in nft)
            {
                if (nx.InstockpremintedaddressId != null || nx.Fingerprint != null)
                {
                    if (ignoresoldflag == true)
                        minttokens.Add(nx.Name);
                    sendtokens.Add(nx.Name);
                }
                else
                {
                    sendtokens.Add(nx.Name);
                    minttokens.Add(nx.Name);
                }
            }

            string minttoken = "";
            string sendtoken = "";
            int u = 0;
            foreach (var tok in minttokens)
            {
                u++;
                if (string.IsNullOrEmpty(tok))
                    minttoken += $"1 {policyid}"; // This is for royalty tokens only
                else
                    minttoken +=
                        $"1 {policyid}.{CreateMintTokenname(tokennameprefix, tok, 0)}";
                if (u < minttokens.Count())
                    minttoken += " + ";
            }

            u = 0;
            foreach (var tok in sendtokens)
            {
                u++;
                if (string.IsNullOrEmpty(tok))
                    sendtoken += $"1 {policyid}"; // For Roayalty Tokens
                else
                    sendtoken +=
                        $"1 {policyid}.{CreateMintTokenname(tokennameprefix, tok, 0)}";

                if (u < sendtokens.Count())
                    sendtoken += " + ";
            }



            command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+0+\"{sendtoken}\"";
            txoutcount++;

            if (hastopay > 2000000) // Hack for free NFTS
            {

                // Check for additional Payment Tokens and send them to the customers wallet
                string tokens = GetAdditonalPaymentTokens(utxopaymentaddress);
                if (!string.IsNullOrEmpty(tokens))
                    command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+0+\"{tokens}\"";
                else
                    command += $" --tx-out {txoutaddr.FilterToLetterOrDigit()}+0";


                txoutcount++;

                foreach (var nftprojectsadditionalpayout in additionalpayoutWallets)
                {
                    command += $" --tx-out {nftprojectsadditionalpayout.Wallet.Walletaddress.FilterToLetterOrDigit()}+0";
                    txoutcount++;
                }

                if (!string.IsNullOrEmpty(mintcostsaddr))
                {
                    command += $" --tx-out {mintcostsaddr.FilterToLetterOrDigit()}+0";
                    txoutcount++;
                }
            }

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --mint=\"{minttoken}\"";

            if (!string.IsNullOrEmpty(metadatafile) && File.Exists(metadatafile))
            {
                buildtransaction.LogFile += File.ReadAllText(metadatafile) + Environment.NewLine;
                command += $" --metadata-json-file {metadatafile}";
            }

            command += $" --out-file {matxrawfile}";

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --minting-script-file {policyscriptfile}";

            buildtransaction.LogFile += command + Environment.NewLine;
            string log;
            try
            {
                log = CardanoCli(command, out var errormessage);

                if (!string.IsNullOrEmpty(errormessage))
                {
                    buildtransaction.LogFile += errormessage + Environment.NewLine;
                    buildtransaction.ErrorMessage = errormessage;
                }

                if (!string.IsNullOrEmpty(log))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return CalculateFees(redis, $@"{matxrawfile}", txincount, txoutcount,
                witnesscount, mainnet,
                ref buildtransaction, out fee);
        }




        private static string GetAdditonalPaymentTokens(TxInClass txin)
        {
            string tokens = "";
            foreach (var txinToken in txin.Tokens.OrEmptyIfNull())
            {
                if (!string.IsNullOrEmpty(tokens))
                    tokens += " + ";
                tokens += txinToken.TokenHex();
            }
            return tokens;
        }
        private static string GetAdditonalPaymentTokens(TxInAddressesClass txinAddress)
        {
            string tokens = "";
            foreach (var txin in txinAddress.TxIn)
            {
                string st = GetAdditonalPaymentTokens(txin);
                if (!string.IsNullOrEmpty(st))
                {
                    if (!string.IsNullOrEmpty(tokens))
                        tokens += " + ";
                    tokens += st;
                }
            }

            return tokens;
        }

        /// <summary>
        /// This version is ONLY for manually minting. We are using Spaces in the Tokenname and so on. It will always used the NMKR Studio Wallet for payment.
        /// And we dont care about already minted or not.
        /// </summary>
        /// <param name="utxopaymentaddress"></param>
        /// <param name="receiveraddress"></param>
        /// <param name="txoutaddr"></param>
        /// <param name="mintcostsaddr"></param>
        /// <param name="policyid"></param>
        /// <param name="tokenname"></param>
        /// <param name="tokennameprefix"></param>
        /// <param name="metadatafile"></param>
        /// <param name="policyscriptfile"></param>
        /// <param name="matxrawfile"></param>
        /// <param name="guid"></param>
        /// <param name="witnesscount"></param>
        /// <param name="fee"></param>
        /// <param name="nodeversion"></param>
        /// <param name="mainnet"></param>
        /// <param name="buildtransaction"></param>
        /// <returns></returns>
        private static bool CalculateFees(IConnectionMultiplexer redis, TxInAddressesClass utxopaymentaddress,
            string receiveraddress,
            string txoutaddr, string mintcostsaddr, string policyid, string tokenname, string tokennameprefix,
            string metadatafile, string policyscriptfile, string matxrawfile, string guid, int witnesscount,
            out long fee, bool mainnet, ref BuildTransactionClass buildtransaction)
        {
            string command = "latest transaction build-raw --fee 0";
            fee = 0;

            if (string.IsNullOrEmpty(receiveraddress))
                return false;


            // This is the First TX in - this can be the Payin Address or the Account Address
            int tt = 0;
            string com1 = "";
            int txincount = 0;
            GetTxInHashes(utxopaymentaddress, out com1, out txincount, out var lovelacesummery, ref buildtransaction);
            command += com1;

            tt = tt + txincount;



            List<string> minttokens = new List<string>();
            List<string> sendtokens = new List<string>();
            sendtokens.Add(tokenname);
            minttokens.Add(tokenname);

            string minttoken = "";
            string sendtoken = "";
            int u = 0;
            foreach (var tok in minttokens)
            {
                u++;
                if (string.IsNullOrEmpty(tok))
                    minttoken += $"1 {policyid}"; // This is for royalty tokens only
                else
                    minttoken +=
                        $"1 {policyid}.{CreateMintTokenname(tokennameprefix, tok, 0)}";
                if (u < minttokens.Count())
                    minttoken += " + ";
            }

            u = 0;
            foreach (var tok in sendtokens)
            {
                u++;
                if (string.IsNullOrEmpty(tok))
                    sendtoken += $"1 {policyid}"; // For Roayalty Tokens
                else
                    sendtoken +=
                        $"1 {policyid}.{CreateMintTokenname(tokennameprefix, tok, 0)}";

                if (u < sendtokens.Count())
                    sendtoken += " + ";
            }



            command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+0+\"{sendtoken}\"";
            command += $" --tx-out {txoutaddr.FilterToLetterOrDigit()}+0";

            if (!string.IsNullOrEmpty(mintcostsaddr))
                command += $" --tx-out {mintcostsaddr.FilterToLetterOrDigit()}+0";

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --mint=\"{minttoken}\"";

            if (!string.IsNullOrEmpty(metadatafile) && File.Exists(metadatafile))
            {
                buildtransaction.LogFile += File.ReadAllText(metadatafile) + Environment.NewLine;
                command += $" --metadata-json-file {metadatafile}";
            }

            command += $" --out-file {matxrawfile}";

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --minting-script-file {policyscriptfile}";

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
                buildtransaction.LogFile += errormessage + Environment.NewLine;

            if (!string.IsNullOrEmpty(log))
            {
                return false;
            }

            int txoutcount = 2;
            if (!string.IsNullOrEmpty(mintcostsaddr))
                txoutcount = 3;

            return CalculateFees(redis, $@"{matxrawfile}", tt, txoutcount,
                witnesscount, mainnet,
                ref buildtransaction, out fee);

        }


        private static bool CreateTransaction(IConnectionMultiplexer redis, string senderaddress, TxInAddressesClass utxopaymentaddress,
            string receiveraddress, string txoutaddr, string mintcostsaddr, string policyid, Nft[] nft,
            string tokennameprefix, string metadatafile, string policyscriptfile, string matxrawfile, string guid,
            long mintcosts, long minutxo, long fee, long stakerewards, long tokenrewards, long ttl, long hastopay, string calculateminutxo,
            string nodeversion,
            bool mainnet, Nftprojectsadditionalpayout[] additionalpayoutWallets, float discount,
            ref BuildTransactionClass buildtransaction, bool ignoresoldflag = false)
        {
            buildtransaction.SenderAddress = senderaddress;
            buildtransaction.Fees = fee;

            if (string.IsNullOrEmpty(receiveraddress))
                return false;

            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra();

            // Free Tokens always with raw - because we have not enough ada for the change address
            if (hastopay == 2000000)
            {
                command += $"-raw --fee {buildtransaction.Fees} ";
            }
            else command += " ";

            // This is the First TX in - this can be the Payin Address or the Account Address
            GetTxInHashes(utxopaymentaddress, out var com1, out var tt, out var lovelacesummery, ref buildtransaction);
            command += com1;

            long adaamountfrompreminted = 0;

            buildtransaction.LogFile += $"Command: {com1}{Environment.NewLine}";
            // Look here for further TX In and Tx Hashes
            // these can be from the already preminted nfts
            foreach (var nx in nft)
            {
                if (nx.InstockpremintedaddressId != null)
                {
                    var utxo = GetNewUtxo(nx.Instockpremintedaddress.Address);
                    GetTxInHashes(utxo, out com1, out tt, out lovelacesummery, ref buildtransaction);
                    command += com1;
                    adaamountfrompreminted += utxo.LovelaceSummary;
                }
            }

            List<string> minttokens = new List<string>();
            List<string> sendtokens = new List<string>();
            foreach (var nx in nft)
            {
                if (nx.InstockpremintedaddressId != null || nx.Fingerprint != null)
                {
                    if (ignoresoldflag == true)
                        minttokens.Add(nx.Name);
                    sendtokens.Add(nx.Name);
                }
                else
                {
                    sendtokens.Add(nx.Name);
                    minttokens.Add(nx.Name);
                }
            }

            string minttoken = "";
            string sendtoken = "";
            int u = 0;
            foreach (var tok in minttokens)
            {
                u++;
                if (string.IsNullOrEmpty(tok))
                    minttoken += $"1 {policyid}"; // For royalty tokens only
                else
                    minttoken +=
                        $"1 {policyid}.{CreateMintTokenname(tokennameprefix, tok, 0)}";
                if (u < minttokens.Count())
                    minttoken += " + ";
            }

            u = 0;
            foreach (var tok in sendtokens)
            {
                u++;
                if (string.IsNullOrEmpty(tok))
                    sendtoken += $"1 {policyid}"; // For Royalty Tokens only
                else
                    sendtoken +=
                        $"1 {policyid}.{CreateMintTokenname(tokennameprefix, tok, 0)}";
                if (u < sendtokens.Count())
                    sendtoken += " + ";
            }




            // Calculate the Minuxto - all 6 NFT we will Add 2 ADA - i know, it is not correct, but it works
            long minutxofinal = minutxo;
            int ux = 0;

            if (string.IsNullOrEmpty(calculateminutxo) || calculateminutxo == nameof(MinUtxoTypes.twoadaeverynft))
            {
                minutxofinal = minutxo * nft.Length;
            }

            if (calculateminutxo == nameof(MinUtxoTypes.twoadaall5nft))
            {
                foreach (var tok in nft)
                {
                    ux++;
                    if (ux >= 5)
                    {
                        ux = 0;
                        minutxofinal += minutxo;
                    }
                }
            }

            if (calculateminutxo == nameof(MinUtxoTypes.minutxo))
            {
                minutxofinal =
                    CalculateRequiredMinUtxo(redis, receiveraddress, sendtoken, "", guid, mainnet, ref buildtransaction);
            }

            long adaamount1 = utxopaymentaddress.LovelaceSummary;
            buildtransaction.LogFile += $"Adaamount: {adaamount1}{Environment.NewLine}";
            // If the customer paid to much, send the amount back

            if (hastopay != 2000000) // 2 ADA is for free NFTS
            {
                if (hastopay > 0 && adaamount1 > hastopay)
                    minutxofinal += (adaamount1 - hastopay);

                // Stakerewards
                if (mintcosts - stakerewards - tokenrewards >= 1000000)
                {
                    minutxofinal = minutxofinal + stakerewards;
                    mintcosts = mintcosts - stakerewards;
                    buildtransaction.StakeRewards = stakerewards;
                    minutxofinal = minutxofinal + tokenrewards;
                    mintcosts = mintcosts - tokenrewards;
                    buildtransaction.TokenRewards = tokenrewards;
                }

                if (discount > 0)
                {
                    long d = (long)(hastopay / 100 * discount);
                    minutxofinal += d;
                    buildtransaction.Discount = d;
                }


                long rest = (adaamount1 + adaamountfrompreminted) - fee - mintcosts - minutxofinal;
                buildtransaction.LogFile += $"Rest: {rest}{Environment.NewLine}";

                buildtransaction.BuyerTxOut = new TxOutClass
                { Amount = minutxofinal, ReceiverAddress = receiveraddress };
                buildtransaction.ProjectTxOut = new TxOutClass { Amount = rest, ReceiverAddress = txoutaddr };

                command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+{minutxofinal}+\"{sendtoken}\""; // Sendback To User


                if (!string.IsNullOrEmpty(mintcostsaddr) && mintcosts != 0)
                {
                    buildtransaction.MintingcostsTxOut = new TxOutClass
                    { Amount = mintcosts, ReceiverAddress = mintcostsaddr };
                    command += $" --tx-out {mintcostsaddr.FilterToLetterOrDigit()}+{mintcosts}"; // Mintingcosts
                }

                foreach (var nftprojectsadditionalpayout in additionalpayoutWallets.OrEmptyIfNull())
                {
                    long addvalue = GetAdditionalPayoutwalletsValue(nftprojectsadditionalpayout,
                        hastopay - buildtransaction.Fees - mintcosts - minutxofinal, nft.Length);

                    buildtransaction.LogFile +=
                        $"HasToPay: {hastopay} + Fees: {buildtransaction.Fees} + Mintcosts: {mintcosts} + MinutxoFinal: {minutxofinal} (incl. Discount: {buildtransaction.Discount??0}) + Add.Wallets: {addvalue}" +
                        Environment.NewLine;

                    if (addvalue > 0)
                    {
                        command +=
                            $" --tx-out {nftprojectsadditionalpayout.Wallet.Walletaddress.FilterToLetterOrDigit()}+{addvalue}";
                        rest = rest - addvalue;
                    }

                    buildtransaction.LogFile +=
                        $"Additional Payout: {nftprojectsadditionalpayout.Wallet.Walletaddress}+{addvalue}{Environment.NewLine}";
                    nftprojectsadditionalpayout.Valuetotal = addvalue;
                }

                buildtransaction.AdditionalPayouts = additionalpayoutWallets;


                if (rest < 0)
                {
                    buildtransaction.LogFile += $"Rest is smaller than zero - return {rest}{Environment.NewLine}";
                    buildtransaction.LogFile += $"Command so far: {command}{Environment.NewLine}";

                    return false;
                }

                // This is for additional payment tokens - because it is only one payment token, 2 ada should be enough
                string tokens = GetAdditonalPaymentTokens(utxopaymentaddress);
                if (!string.IsNullOrEmpty(tokens))
                    command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+2000000+\"{tokens}\"";


                command += $" --change-address {txoutaddr}"; // Rest off ADA
            }
            else
            {
                // Free Tokens - only the Fee will deducted
                command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+{(hastopay - fee)}+\"{sendtoken}\""; // Sendback To User
            }



            if (!string.IsNullOrEmpty(minttoken))
                command += $" --mint=\"{minttoken}\"";

            if (!string.IsNullOrEmpty(metadatafile) && File.Exists(metadatafile))
            {
                buildtransaction.LogFile += File.ReadAllText(metadatafile) + Environment.NewLine;
                command += $" --metadata-json-file {metadatafile}";
            }


            if (!string.IsNullOrEmpty(minttoken))
                command += $" --minting-script-file {policyscriptfile}";

            if (hastopay != 2000000)
            {
                command = command.GetWitnessOverride(2)
                    .GetTTL(ttl)
                    .GetNetwork(mainnet)
                    .GetOutFile(matxrawfile);
            }
            else
            {
                command = command.GetTTL(ttl)
                    .GetOutFile(matxrawfile);
            }

            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.ErrorMessage = errormessage;
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                if (hastopay == 2000000)
                    return false;
            }

            if (hastopay == 2000000)
            {
                return true;
            }

            if (!log.Contains("Estimated transaction fee"))
            {
                return false;
            }
            else
            {
                buildtransaction.Fees = GetEstimatedFees(log);
                if (buildtransaction.Fees == 0)
                    return false;
            }

            return true;
        }

        public static long CalculateRequiredMinUtxo(IConnectionMultiplexer redis, string receiveraddress, string sendtoken, string inlinedatumfile, string guid, bool mainnet,
            ref BuildTransactionClass buildtransaction)
        {
            buildtransaction.RequiredMinUtxo = 0;
            string protocolparamsfile = $"{GeneralConfigurationClass.TempFilePath}protocol{guid}.json";
            SaveProtocolParamsFile(redis, protocolparamsfile, mainnet, ref buildtransaction);
            if (!File.Exists(protocolparamsfile))
                return 0;

            string command = CliCommandExtensions.GetTransactionCalculateMinRequiredUtxoLatestEra()
                .GetProtocolParamsFile(protocolparamsfile)
                .GetTxOut(receiveraddress, sendtoken)
                .GetTxOutInlineDatumFile(inlinedatumfile);


            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                string s = File.ReadAllText(protocolparamsfile);
                buildtransaction.LogFile += "Protocol params file:" + s + Environment.NewLine;

                buildtransaction.ErrorMessage = errormessage;
                buildtransaction.LogFile += errormessage + Environment.NewLine;
            }

            buildtransaction.RequiredMinUtxo = GetRequiredMinutxo(log);

            GlobalFunctions.DeleteFile(protocolparamsfile);

            if (buildtransaction.RequiredMinUtxo == 0)
            {
                buildtransaction.RequiredMinUtxo = 2000000;
                return buildtransaction.RequiredMinUtxo;
            }

            // We are adding 0.25 ADA because of the error in Babbage era with minutxo
            return GlobalFunctions.RoundUpToThousand(buildtransaction.RequiredMinUtxo + 250000);
        }

        private static long GetRequiredMinutxo(string log)
        {
            // Lovelace 176325
            if (log.Contains("Lovelace "))
            {
                log = log.Replace("Lovelace ", "").Replace("\n", "").Replace("\r", "");
                try
                {
                    return Convert.ToInt64(log);
                }
                catch
                {
                }
            }
            if (log.Contains("Coin "))
            {
                log = log.Replace("Coin ", "").Replace("\n", "").Replace("\r", "");
                try
                {
                    return Convert.ToInt64(log);
                }
                catch
                {
                }
            }
            return 0;
        }

        public static long GetAdditionalPayoutwalletsValue(Nftprojectsadditionalpayout nftprojectsadditionalpayout,
            long hastopay, long nftcount)
        {
            if (nftprojectsadditionalpayout.Valuetotal != null && nftprojectsadditionalpayout.Valuetotal >= 1000000)
                return (long)nftprojectsadditionalpayout.Valuetotal * nftcount;

            if (nftprojectsadditionalpayout.Valuepercent == null ||
                !(nftprojectsadditionalpayout.Valuepercent > 0)) return 0;
            var v = hastopay / 100 * nftprojectsadditionalpayout.Valuepercent;
            long v1 = Convert.ToInt64(v);
            v1 = Math.Max(1000000, v1);
            if (!nftprojectsadditionalpayout.Wallet.Walletaddress.ToLower().StartsWith("addr"))
                v1 = Math.Max(2000000, v1);
            return v1;
        }
      

        /// <summary>
        /// This version is ONLY for manuall minting. We are using Spaces in the Tokenname and so on. It will always used the NMKR Studio Wallet for payment.
        /// We dont care about already minted. So use carefully!
        /// </summary>
        /// <param name="senderaddress"></param>
        /// <param name="utxopaymentaddress"></param>
        /// <param name="receiveraddress"></param>
        /// <param name="txoutaddr"></param>
        /// <param name="mintcostsaddr"></param>
        /// <param name="policyid"></param>
        /// <param name="tokenname"></param>
        /// <param name="tokennameprefix"></param>
        /// <param name="metadatafile"></param>
        /// <param name="policyscriptfile"></param>
        /// <param name="matxrawfile"></param>
        /// <param name="guid"></param>
        /// <param name="mintcosts"></param>
        /// <param name="minutxo"></param>
        /// <param name="fee"></param>
        /// <param name="ttl"></param>
        /// <param name="hastopay"></param>
        /// <param name="nodeversion"></param>
        /// <param name="buildtransaction"></param>
        /// <returns></returns>
        private static bool CreateTransaction(string senderaddress, TxInAddressesClass utxopaymentaddress,
            string receiveraddress, string txoutaddr, string mintcostsaddr, string policyid, string tokenname,
            string tokennameprefix, string metadatafile, string policyscriptfile, string matxrawfile,
            long mintcosts, long minutxo, long fee, long ttl, long hastopay,
            ref BuildTransactionClass buildtransaction)
        {
            buildtransaction.SenderAddress = senderaddress;
            buildtransaction.Fees = fee;

            string command = $@"latest transaction build-raw --fee {fee}";

            if (string.IsNullOrEmpty(receiveraddress))
                return false;


            // This is the First TX in - this can be the Payin Address or the Account Address
            int tt = 0;
            string com1 = "";
            int txincount = 0;
            GetTxInHashes(utxopaymentaddress, out com1, out txincount, out var lovelacesummery, ref buildtransaction);
            command += com1;

            tt = tt + txincount;

            long adaamountfrompreminted = 0;


            List<string> minttokens = new List<string>();
            List<string> sendtokens = new List<string>();

            sendtokens.Add(tokenname);
            minttokens.Add(tokenname);

            string minttoken = "";
            string sendtoken = "";
            int u = 0;
            foreach (var tok in minttokens)
            {
                u++;
                if (string.IsNullOrEmpty(tok))
                    minttoken += $"1 {policyid}"; // For royalty tokens only
                else
                    minttoken +=
                        $"1 {policyid}.{CreateMintTokenname(tokennameprefix, tok, 0)}";
                if (u < minttokens.Count())
                    minttoken += " + ";
            }

            u = 0;
            foreach (var tok in sendtokens)
            {
                u++;
                if (string.IsNullOrEmpty(tok))
                    sendtoken += $"1 {policyid}"; // For Royalty Tokens only
                else
                    sendtoken +=
                        $"1 {policyid}.{CreateMintTokenname(tokennameprefix, tok, 0)}";
                if (u < sendtokens.Count())
                    sendtoken += " + ";
            }




            // Calculate the Minuxto - all 6 NFT we will Add 2 ADA - i know, it is not correct, but it works
            long minutxofinal = minutxo;
            minutxofinal = minutxo;

            long adaamount1 = utxopaymentaddress.LovelaceSummary;

            // If the customer paid to much, send the amount back
            if (hastopay > 0 && adaamount1 > hastopay)
            {
                minutxofinal += (adaamount1 - hastopay);
            }


            long rest = (adaamount1 + adaamountfrompreminted) - fee - mintcosts - minutxofinal;
            if (rest < 0)
            {
                return false;
            }

            List<TxOutClass> txout = new List<TxOutClass>();
            buildtransaction.BuyerTxOut = new TxOutClass { Amount = minutxofinal, ReceiverAddress = receiveraddress };
            buildtransaction.ProjectTxOut = new TxOutClass { Amount = rest, ReceiverAddress = txoutaddr };

            command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+{minutxofinal}+\"{sendtoken}\"";
            command += $" --tx-out {txoutaddr.FilterToLetterOrDigit()}+{rest}";

            if (!string.IsNullOrEmpty(mintcostsaddr) && mintcosts != 0)
            {
                buildtransaction.MintingcostsTxOut = new TxOutClass
                { Amount = mintcosts, ReceiverAddress = mintcostsaddr };
                command += $" --tx-out {mintcostsaddr.FilterToLetterOrDigit()}+{mintcosts}";
            }

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --mint=\"{minttoken}\"";

            if (!string.IsNullOrEmpty(metadatafile) && File.Exists(metadatafile))
            {
                buildtransaction.LogFile += File.ReadAllText(metadatafile) + Environment.NewLine;
                command += $" --metadata-json-file {metadatafile}";
            }

            command += $" --out-file {matxrawfile}";

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --minting-script-file {policyscriptfile}";

            command += $" --ttl {ttl}";
            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
                buildtransaction.LogFile += errormessage + Environment.NewLine;

            if (!string.IsNullOrEmpty(log))
            {
                return false;
            }

            return true;
        }



        public static bool LegacyAuctionTransactionBid(IConnectionMultiplexer redis, SmartContractAuctionsParameterClass scapc, bool mainnet,
            ref BuildTransactionClass buildtransaction)
        {
            buildtransaction.BuyerTxOut = new TxOutClass() { ReceiverAddress = scapc.changeaddress };

            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra()
                .GetNetwork(mainnet)
                .GetTxIn(scapc.utxopaymentaddress, ref buildtransaction)
                .GetTxOut(scapc.legacyaddress, scapc.bidamount)
                .GetTxOut(scapc.utxopaymentaddress, scapc.changeaddress, null, "", scapc.bidamount, 3000000,
                    ref buildtransaction)
                .GetChangeAddress(scapc.changeaddress)
                .GetProtocolParamsFile(scapc.protocolParamsFile)
                .GetWitnessOverride(2)
                .GetOutFile(scapc.matxrawfile);

            buildtransaction.Command = command;


            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }

            buildtransaction.Fees = GetEstimatedFees(log);
            if (buildtransaction.Fees == 0)
                return false;

            return true;
        }

        public static bool SmartContractsAuctionTransactionBid(IConnectionMultiplexer redis, SmartContractAuctionsParameterClass scapc, bool mainnet,
            ref BuildTransactionClass buildtransaction)
        {
            buildtransaction.BuyerTxOut = new TxOutClass() { ReceiverAddress = scapc.changeaddress };

            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra()
                .GetNetwork(mainnet)
                .GetTxIn(scapc.utxopaymentaddress, ref buildtransaction)
                .GetTxIn(scapc.utxoScript)
                .GetScriptValid()
                .GetTxInScriptFile(scapc.scriptfile)
                .GetTxInDatumFile(scapc.olddatumfile)
                .GetTxInRedeemerFile(scapc.redeemerfile)
                .GetRequiredSignerHash(scapc.signerhash)
                .GetTxInCollateral(scapc.utxopaymentaddress, scapc.collateraltxin)
                .GetTxOutCollateral(scapc.changeaddress, 1500000, scapc.utxopaymentaddress, scapc.collateraltxin)
                .GetTotalCollateral(1500000, scapc.collateraltxin)
                .GetTxOutScripthash(scapc.scripthash, 2000000, scapc.tokencount ?? 1, scapc.policyidAndTokenname,
                    scapc.bidamount) // TX-OUT to the Smart Contract
                .GetTxOutDatumHash(scapc.scriptDatumHash) // The new Datum hash from this action
                .GetTxOutDatumEmbedFile(scapc.newdatumfile)
                .GetTxOut(scapc.utxopaymentaddress, scapc.changeaddress, scapc.tokencount, scapc.policyidAndTokenname,
                    2000000 + scapc.bidamount, 3000000, ref buildtransaction)
                .GetChangeAddress(scapc.changeaddress)
                .GetTxOut(scapc.receiver.ToArray()) // The old bidder - if there was one
                .GetProtocolParamsFile(scapc.protocolParamsFile)
                .GetWitnessOverride(4)
                .GetCddlFormat()
                .GetOutFile(scapc.matxrawfile);


            buildtransaction.Command = command;


            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }

            buildtransaction.Fees = GetEstimatedFees(log);
            if (buildtransaction.Fees == 0)
                return false;

            return true;
        }



        public static bool SmartContractsDirectSale(IConnectionMultiplexer redis, SmartContractAuctionsParameterClass scapc, bool mainnet,
            ref BuildTransactionClass buildtransaction)
        {
            buildtransaction.BuyerTxOut = new TxOutClass() { ReceiverAddress = scapc.changeaddress };

            var b = (scapc.smartcontractmemvalue == null)
                ? CreateCliCommandSmartContractsDirectSale(redis, scapc, mainnet, ref buildtransaction)
                : CreateCliCommandSmartContractsDirectSaleBuildRaw(redis, scapc, mainnet, ref buildtransaction);



            if (buildtransaction.Fees == 0)
                return false;

            return b;
        }


        public static bool SmartContractsDirectSaleOffer(IConnectionMultiplexer redis, SmartContractAuctionsParameterClass scapc, bool mainnet,
            ref BuildTransactionClass buildtransaction)
        {
            buildtransaction.BuyerTxOut = new TxOutClass() { ReceiverAddress = scapc.changeaddress };

            var b = (scapc.smartcontractmemvalue == null)
                ? CreateCliCommandSmartContractsDirectSaleOffer(redis, scapc, mainnet, ref buildtransaction)
                : CreateCliCommandSmartContractsDirectSaleOfferBuildRaw(redis, scapc, mainnet, ref buildtransaction);



            if (buildtransaction.Fees == 0)
                return false;

            return b;
        }
        private static bool CreateCliCommandSmartContractsDirectSaleOffer(IConnectionMultiplexer redis,
        SmartContractAuctionsParameterClass scapc, bool mainnet, ref BuildTransactionClass buildtransaction)
        {
            long rest = (scapc.utxopaymentaddress.Sum(x => x.LovelaceSummary));
            long ll = 2000000;
            var rec = scapc.receiver.FirstOrDefault(x => x.receivertype == ReceiverTypes.seller);
            if (rec != null)
                ll = rec.lovelace;


            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra()
                .GetNetwork(mainnet)
                .GetTxIn(scapc.utxopaymentaddress, ref buildtransaction)
                .GetTxIn(scapc.utxoScript)
                .GetTxInScriptFile(scapc.scriptfile)
                .GetTxInDatumFile(scapc.olddatumfile)
                .GetTxInRedeemerFile(scapc.redeemerfile)
                .GetRequiredSignerHash(scapc.signerhash)
                .GetTxInCollateral(scapc.utxopaymentaddress, scapc.collateraltxin)
                .GetTxOutCollateral(scapc.changeaddress, 1500000, scapc.utxopaymentaddress, scapc.collateraltxin)
                .GetTotalCollateral(1500000, scapc.collateraltxin)
                .GetTxOut(scapc.receiver.Where(x => x.receivertype != ReceiverTypes.seller).ToArray())
                .GetTxOutRestWithTokens(redis, scapc.utxopaymentaddress, scapc.changeaddress, scapc.tokencount, scapc.policyidAndTokenname, ll,
                    ref buildtransaction)

                .GetChangeAddress(scapc.changeaddress)
                .GetWitnessOverride(4)
                .GetCddlFormat()
                .GetProtocolParamsFile(scapc.protocolParamsFile)
                .GetOutFile(scapc.matxrawfile);

            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }
            buildtransaction.Fees = GetEstimatedFees(log);
            if (buildtransaction.Fees == 0)
                return false;

            return true;
        }

        public static bool CreateCliCommandSmartContractsDirectSaleOfferBuildRaw(IConnectionMultiplexer redis,
            SmartContractAuctionsParameterClass scapc, bool mainnet, ref BuildTransactionClass buildtransaction)
        {
            if (CalcluatePlutusCostsOffers(redis, scapc, mainnet,
                    ref buildtransaction))
            {
                scapc.smartcontractmemvalue = buildtransaction.smartcontractmemvalue;
                scapc.smartcontracttimevalue = buildtransaction.smartcontracttimevalue;

                return CreateCliCommandSmartContractsDirectSaleOfferBuildRawCommand(redis, scapc, mainnet,
                    buildtransaction.Fees,
                    ref buildtransaction);
            }

            return false;
        }

        private static bool CalcluatePlutusCostsOffers(IConnectionMultiplexer redis, SmartContractAuctionsParameterClass scapc, bool mainnet, ref BuildTransactionClass buildtransaction)
        {

            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra()
                .GetNetwork(mainnet)
                .GetTxIn(scapc.utxopaymentaddress, ref buildtransaction)
                .GetTxIn(scapc.utxoScript)
                .GetTxInScriptFile(scapc.scriptfile)
                .GetTxInDatumFile(scapc.olddatumfile)
                .GetTxInRedeemerFile(scapc.redeemerfile)
                .GetRequiredSignerHash(scapc.signerhash)
                .GetTxInCollateral(scapc.utxopaymentaddress, scapc.collateraltxin)
                .GetTxOutCollateral(scapc.changeaddress, 1500000, scapc.utxopaymentaddress, scapc.collateraltxin)
                .GetTotalCollateral(1500000, scapc.collateraltxin)
                .GetTxOut(scapc.receiver.Where(x => x.receivertype != ReceiverTypes.seller).ToArray())
                .GetTxOutWithTokensMinutxo(redis, scapc.utxopaymentaddress, scapc.changeaddress, scapc.tokencount, scapc.policyidAndTokenname,
                    ref buildtransaction)
                .GetChangeAddress(scapc.changeaddress)
                .GetWitnessOverride(4)
                .GetCddlFormat()
                .GetProtocolParamsFile(scapc.protocolParamsFile)
                .GetCalculatePlutusScriptCost(scapc.costsfile);

            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;


            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }
            buildtransaction.Fees = GetEstimatedFees(log);
            if (buildtransaction.Fees == 0)
                return false;


            if (!File.Exists(scapc.costsfile))
                return false;

            var costs = JsonConvert.DeserializeObject<PlutusCostsClass[]>(File.ReadAllText(scapc.costsfile));
            if (costs == null || !costs.Any())
                return false;

            buildtransaction.smartcontractmemvalue = costs.First().ExecutionUnits.Memory;
            buildtransaction.smartcontracttimevalue = costs.First().ExecutionUnits.Steps;


            return true;
        }

        private static bool CreateCliCommandSmartContractsDirectSaleOfferBuildRawCommand(IConnectionMultiplexer redis,
           SmartContractAuctionsParameterClass scapc, bool mainnet, long fee,
           ref BuildTransactionClass buildtransaction)
        {
            long rest = (scapc.utxopaymentaddress.Sum(x => x.LovelaceSummary) - fee);
            long ll = 0;
            var rec = scapc.receiver.FirstOrDefault(x => x.receivertype == ReceiverTypes.seller);
            if (rec != null)
                ll = rec.lovelace;

            string command = CliCommandExtensions.GetTransactionBuildRawWithLatestEra()
                .GetFees(fee)
                .GetTxIn(scapc.utxopaymentaddress, ref buildtransaction)
                .GetTxIn(scapc.utxoScript)
                .GetTxInScriptFile(scapc.scriptfile)
                .GetTxInDatumFile(scapc.olddatumfile)
                .GetTxInRedeemerFile(scapc.redeemerfile)
                .GetTxInExecutionUnits(scapc.smartcontracttimevalue ?? 0, scapc.smartcontractmemvalue ?? 0)
                .GetTxInCollateral(scapc.utxopaymentaddress, scapc.collateraltxin)
                .GetTxOutCollateral(scapc.changeaddress, 1500000, scapc.utxopaymentaddress, scapc.collateraltxin)
                .GetTotalCollateral(1500000, scapc.collateraltxin)
                .GetTxOut(scapc.receiver.Where(x => x.receivertype != ReceiverTypes.seller).ToArray())
                .GetTxOutRestWithTokens(redis, scapc.utxopaymentaddress, scapc.changeaddress, scapc.tokencount, scapc.policyidAndTokenname, rest + ll,
                    ref buildtransaction)
                .GetCddlFormat()
                .GetProtocolParamsFile(scapc.protocolParamsFile)
                .GetOutFile(scapc.matxrawfile);



            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }

            return true;
        }

        private static bool CreateCliCommandSmartContractsDirectSaleBuildRaw(IConnectionMultiplexer redis,
           SmartContractAuctionsParameterClass scapc, bool mainnet, ref BuildTransactionClass buildtransaction)
        {
            if (CalcluatePlutusCosts(redis, scapc, mainnet,
                    ref buildtransaction))
            {
                scapc.smartcontractmemvalue = buildtransaction.smartcontractmemvalue;
                scapc.smartcontracttimevalue = buildtransaction.smartcontracttimevalue;

                return CreateCliCommandSmartContractsDirectSaleBuildRawCommand(redis, scapc, mainnet,
                    buildtransaction.Fees,
                    ref buildtransaction);
            }

            return false;
        }

        private static bool CalcluatePlutusCosts(IConnectionMultiplexer redis, SmartContractAuctionsParameterClass scapc, bool mainnet, ref BuildTransactionClass buildtransaction)
        {

            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra()
                .GetNetwork(mainnet)
                .GetTxIn(scapc.utxopaymentaddress, ref buildtransaction)
                .GetTxIn(scapc.utxoScript)
                .GetTxInScriptFile(scapc.scriptfile)
                .GetTxInDatumFile(scapc.olddatumfile)
                .GetTxInRedeemerFile(scapc.redeemerfile)
                .GetRequiredSignerHash(scapc.signerhash)
                .GetTxInCollateral(scapc.utxopaymentaddress, scapc.collateraltxin)
                .GetTxOutCollateral(scapc.changeaddress, 1500000, scapc.utxopaymentaddress, scapc.collateraltxin)
                .GetTotalCollateral(1500000, scapc.collateraltxin)
                .GetTxOut(scapc.receiver.ToArray())
                .GetTxOutWithTokensMinutxo(redis, scapc.utxopaymentaddress, scapc.changeaddress, null, null,
                    ref buildtransaction)
                .GetChangeAddress(scapc.changeaddress)
                .GetWitnessOverride(4)
                .GetCddlFormat()
                .GetProtocolParamsFile(scapc.protocolParamsFile)
                .GetCalculatePlutusScriptCost(scapc.costsfile);

            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;


            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }
            buildtransaction.Fees = GetEstimatedFees(log);
            if (buildtransaction.Fees == 0)
                return false;


            if (!File.Exists(scapc.costsfile))
                return false;

            var costs = JsonConvert.DeserializeObject<PlutusCostsClass[]>(File.ReadAllText(scapc.costsfile));
            if (costs == null || !costs.Any())
                return false;

            buildtransaction.smartcontractmemvalue = costs.First().ExecutionUnits.Memory;
            buildtransaction.smartcontracttimevalue = costs.First().ExecutionUnits.Steps;

            GlobalFunctions.DeleteFile(scapc.costsfile);

            return true;
        }

        private static bool CreateCliCommandSmartContractsDirectSaleBuildRawCommand(IConnectionMultiplexer redis,
            SmartContractAuctionsParameterClass scapc, bool mainnet, long fee,
            ref BuildTransactionClass buildtransaction)
        {
            long rest = (scapc.utxopaymentaddress.Sum(x => x.LovelaceSummary) + scapc.lockamount) -
                        (scapc.receiver.Sum(x => x.lovelace) + fee);

            string command = CliCommandExtensions.GetTransactionBuildRawWithLatestEra()
                .GetFees(fee)
                .GetTxIn(scapc.utxopaymentaddress, ref buildtransaction)
                .GetTxIn(scapc.utxoScript)
                .GetTxInScriptFile(scapc.scriptfile)
                .GetTxInDatumFile(scapc.olddatumfile)
                .GetTxInRedeemerFile(scapc.redeemerfile)
                .GetTxInExecutionUnits(scapc.smartcontracttimevalue ?? 0, scapc.smartcontractmemvalue ?? 0)
                .GetTxInCollateral(scapc.utxopaymentaddress, scapc.collateraltxin)
                .GetTxOutCollateral(scapc.changeaddress, 1500000, scapc.utxopaymentaddress, scapc.collateraltxin)
                .GetTotalCollateral(1500000, scapc.collateraltxin)
                .GetTxOut(scapc.receiver.ToArray())
                .GetTxOutRestWithTokens(redis, scapc.utxopaymentaddress, scapc.changeaddress, null, null, rest,
                    ref buildtransaction)
                .GetCddlFormat()
                .GetProtocolParamsFile(scapc.protocolParamsFile)
                .GetOutFile(scapc.matxrawfile);



            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }

            return true;
        }

        private static bool CreateCliCommandSmartContractsDirectSale(IConnectionMultiplexer redis,
            SmartContractAuctionsParameterClass scapc, bool mainnet, ref BuildTransactionClass buildtransaction)
        {
            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra()
                .GetNetwork(mainnet)
                .GetTxIn(scapc.utxopaymentaddress, ref buildtransaction)
                .GetTxIn(scapc.utxoScript)
                .GetTxInScriptFile(scapc.scriptfile)
                .GetTxInDatumFile(scapc.olddatumfile)
                .GetTxInRedeemerFile(scapc.redeemerfile)
                .GetRequiredSignerHash(scapc.signerhash)
                .GetTxInCollateral(scapc.utxopaymentaddress, scapc.collateraltxin)
                .GetTxOutCollateral(scapc.changeaddress, 1500000, scapc.utxopaymentaddress, scapc.collateraltxin)
                .GetTotalCollateral(1500000, scapc.collateraltxin)
                .GetTxOut(scapc.receiver.ToArray())
                .GetTxOutWithTokensMinutxo(redis, scapc.utxopaymentaddress, scapc.changeaddress, null, null,
                    ref buildtransaction)
                .GetChangeAddress(scapc.changeaddress)
                .GetWitnessOverride(4)
                .GetCddlFormat()
                .GetProtocolParamsFile(scapc.protocolParamsFile)
                .GetOutFile(scapc.matxrawfile);

            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }
            buildtransaction.Fees = GetEstimatedFees(log);
            if (buildtransaction.Fees == 0)
                return false;

            return true;
        }

        private static long GetEstimatedFees(string log)
        {
            // Estimated transaction fee: Lovelace 176325
            if (log.Contains("Estimated transaction fee: Lovelace "))
            {
                log = log.Replace("Estimated transaction fee: Lovelace ", "").Replace("\n", "").Replace("\r", "");
                try
                {
                    return Convert.ToInt64(log);
                }
                catch
                {
                }
            }
           
            if (log.Contains("Estimated transaction fee: Coin "))
            {
                log = log.Replace("Estimated transaction fee: Coin ", "").Replace("\n", "").Replace("\r", "");
                try
                {
                    return Convert.ToInt64(log);
                }
                catch
                {
                }
            }
            // Estimated transaction fee: 211877 Lovelace - new in version 10
            if (log.Contains("Estimated transaction fee: "))
            {
                log = log.Replace("Estimated transaction fee: ", "");
                log = log.Replace(" Lovelace", "").Replace("\n", "").Replace("\r", "");
                try
                {
                    return Convert.ToInt64(log);
                }
                catch
                {
                }
            }
            return 0;
        }

        public static bool SmartContractsAuctionTransactionClose(IConnectionMultiplexer redis, SmartContractAuctionsParameterClass scapc,
            bool mainnet, ref BuildTransactionClass buildtransaction)
        {
            buildtransaction.BuyerTxOut = new TxOutClass() { ReceiverAddress = scapc.changeaddress };

            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra()
                .GetNetwork(mainnet)
                .GetTxIn(scapc.utxopaymentaddress, ref buildtransaction)
                .GetTxIn(scapc.utxoScript)
                .GetTxInScriptFile(scapc.scriptfile)
                .GetTxInDatumFile(scapc.olddatumfile)
                .GetTxInRedeemerFile(scapc.redeemerfile)
                .GetRequiredSignerHash(scapc.signerhash)
                .GetTxInCollateral(scapc.collateraltxin)
                .GetTxOut(scapc.receiver.ToArray())
                .GetChangeAddress(scapc.changeaddress)
                .GetInvalidBefore(scapc.startslot)
                .GetInvalidHereAfter(scapc.next10slots)
                .GetProtocolParamsFile(scapc.protocolParamsFile)
                //     .GetWitnessOverride(2)
                .GetOutFile(scapc.matxrawfile);


            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }

            buildtransaction.Fees = GetEstimatedFees(log);
            if (buildtransaction.Fees == 0)
                return false;

            return true;
        }

        public static bool LegacyTransactionLockNft(IConnectionMultiplexer redis, TxInAddressesClass[] utxofinal, string changeaddress,
            long tokencount, string policyidAndTokenname, string legacyaddress, string protocolParamsFile,
            string matxrawfile, bool mainnet, ref BuildTransactionClass buildtransaction)
        {
            buildtransaction.SenderAddress = changeaddress;


            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra()
                .GetNetwork(mainnet)
                .GetTxIn(utxofinal, ref buildtransaction) // TX-IN from the Seller - with the Token
                .GetTxOut(legacyaddress, 2000000, $"{tokencount} {policyidAndTokenname}")
                .GetTxOut(utxofinal, changeaddress, tokencount, policyidAndTokenname, 2000000, 3000000,
                    ref buildtransaction) // Rest of the TX-IN minus the Token and with minimum of 2 ADA
                .GetChangeAddress(changeaddress) // Rest of the ADA from the TX-IN
                .GetProtocolParamsFile(protocolParamsFile)
                .GetWitnessOverride(2)
                .GetOutFile(matxrawfile);


            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }

            buildtransaction.Fees = GetEstimatedFees(log);
            if (buildtransaction.Fees == 0)
                return false;

            return true;
        }



        public static bool CreateCliCommandLockTransactionSmartcontract(IConnectionMultiplexer redis, TxInAddressesClass[] utxofinal, string changeaddress,
            long tokencount, string policyidAndTokenname, string scripthash, string scriptDatumHash, string scriptdatumfile,
            string protocolParamsFile,
            string matxrawfile, string metadatafile, bool mainnet, ref BuildTransactionClass buildtransaction)
        {
            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra()
                .GetNetwork(mainnet)
                .GetTxIn(utxofinal, ref buildtransaction) // TX-IN from the Seller - with the Token
                .GetTxOutScripthash(scripthash, 2000000, tokencount,
                    policyidAndTokenname) // TX-OUT to the Smart Contract - scripthash is the sm address
                                          //       .GetTxOutDatumHash(scriptDatumHash) // Datum Hash for the Smart Contract
                .GetTxOutDatumEmbedFile(scriptdatumfile)
                .GetTxOutWithTokensMinutxo(redis, utxofinal, changeaddress, tokencount, policyidAndTokenname, ref buildtransaction)
                .GetChangeAddress(changeaddress)
                //  .GetJsonMetadataNoSchema(metadatafile)
                .GetMetadataJsonFile(metadatafile)
                .GetWitnessOverride(6)
                .GetProtocolParamsFile(protocolParamsFile)
                .GetCddlFormat()
                .GetOutFile(matxrawfile);


            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }
            buildtransaction.Fees = GetEstimatedFees(log);
            if (buildtransaction.Fees == 0)
                return false;
            return true;
        }




        public static bool CheckIfAddressIsValid(EasynftprojectsContext db, string address, bool mainnet,
            out string outaddress, out Blockchain blockchain, bool acceptadahandle = false,
            bool acceptstakeaddress = false)
        {
            blockchain = Blockchain.Unknown;
            if (address.Length >= 32 && address.Length <= 44 && !address.StartsWith("$") && 
                !address.ToLower().StartsWith("tb") && !address.ToLower().StartsWith("bc") && 
                !address.ToLower().StartsWith("addr") && !address.ToLower().StartsWith("stake"))
            {
                if (SolanaFunctions.IsValidSolanaPublicKey(address))
                {
                    blockchain = Blockchain.Solana;
                    outaddress = address;
                    return true;
                }
            }


            // Aptos
            if (address.StartsWith("0x") && address.Length == 66)
            {
                IBlockchainFunctions aptos = new AptosBlockchainFunctions();
                if (aptos.CheckForValidAddress(address, mainnet))
                {
                    outaddress = address;
                    blockchain = Blockchain.Aptos;
                    return true;
                }
            }



            if (address.ToLower().StartsWith("addr") || address.ToLower().StartsWith("stake"))
                blockchain = Blockchain.Cardano;


            // Check for Bitcoin
            if (blockchain == Blockchain.Unknown)
            {
                IBlockchainFunctions bitcoin = new BitcoinBlockchainFunctions();
                if (bitcoin.CheckForValidAddress(address, mainnet))
                {
                    blockchain = Blockchain.Bitcoin;
                    outaddress = address;
                    return true;
                }
            }


            db ??= new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            address = address.ToLower();
            outaddress = "";
            if (string.IsNullOrEmpty(address))
                return false;

            address = address.Trim();

            if (address.StartsWith("stake") && acceptstakeaddress == false)
                return false;

            if (address.StartsWith("stake"))
            {
                var adrs = BlockfrostFunctions.GetAllAddressesWithThisStakeAddress(null, address);
                if (adrs == null || !adrs.Any())
                    return false;
                address = adrs.First().Address;
            }

            if (acceptadahandle)
            {
                var a1 = address;
                address = GetAddressFromAdaHandle(db, address, mainnet);
                outaddress = address;
                if (a1!=address)
                    blockchain = Blockchain.Cardano;
            }
            else
            {
                outaddress = address;
            }

            return IsValidCardanoAddress(address, mainnet);
        }

        public static string GetAddressFromAdaHandle(EasynftprojectsContext db, string address, bool mainnet)
        {
            if (string.IsNullOrEmpty(address))
                return address;

            var adahandles = (from a in db.Adahandles
                              select a).AsNoTracking().ToList();

            foreach (var adahandle in adahandles)
            {
                if (address.StartsWith(adahandle.Prefix))
                {
                    address = GetAdaHandle(address, adahandle, mainnet);
                    break;
                }
            }

            return address;
        }

        private static string GetAdaHandle(string address, Adahandle adahandle, bool mainnet)
        {
            var res = KoiosFunctions.GetFirstAssetAddressList(adahandle.Policyid,
                GlobalFunctions.ToHexString(address.ToLower().Substring(1)));

            // Check if CIP68 Handle
            if (string.IsNullOrEmpty(res))
                res = KoiosFunctions.GetFirstAssetAddressList(adahandle.Policyid, "000de140" +
                    GlobalFunctions.ToHexString(address.ToLower().Substring(1)));

            return res;
        }


        private static CardanoTransactionClass CalculateFeesForMintAndSign(IConnectionMultiplexer redis,
            CardanoTransactionClass ctc,
            string nodeversion, bool mainnet)
        {
            string command = "latest transaction build-raw --fee 0";
            ctc.Fee = 0;


            if (string.IsNullOrEmpty(ctc.ReceiverAddressNft))
            {
                ctc.Result = false;
                return ctc;
            }


            // This is the First TX in - this can be the Payin Address or the Account Address
            int txoutcount = 0;
            BuildTransactionClass buildtransaction = new BuildTransactionClass();
            GetTxInHashes(ctc.UtxoPaymentAddress, out var com1, out var txincount, out var lovelacesummery,
                ref buildtransaction);
            command += com1;

            // Look here for further TX In and Tx Hashes
            // these can be from the already preminted nfts
            foreach (var nx in ctc.nft)
            {
                if (nx.InstockpremintedaddressId != null)
                {
                    com1 = "";
                    GetTxInHashes(GetNewUtxo(nx.Instockpremintedaddress.Address), out com1,
                        out var txincount1, out lovelacesummery, ref buildtransaction);
                    ctc.buildtransaction = buildtransaction;
                    command += com1;

                    txincount += txincount1;
                }
            }

            List<string> minttokens = new List<string>();
            List<string> sendtokens = new List<string>();
            foreach (var nx in ctc.nft)
            {
                if (nx.InstockpremintedaddressId != null || nx.Fingerprint != null)
                {
                    sendtokens.Add(nx.Name);
                }
                else
                {
                    sendtokens.Add(nx.Name);
                    minttokens.Add(nx.Name);
                }
            }

            string txintokens = ctc.UtxoPaymentAddress.GetTxInTokens();

            string minttoken = "";
            string sendtoken = "";
            int u = 0;
            foreach (var tok in minttokens)
            {
                u++;
                minttoken += $"1 {ctc.Policyid}.{CreateMintTokenname(ctc.TokennamePrefix, tok)}";
                if (u < minttokens.Count())
                    minttoken += " + ";
            }

            u = 0;
            foreach (var tok in sendtokens)
            {
                u++;
                sendtoken += $"1 {ctc.Policyid}.{CreateMintTokenname(ctc.TokennamePrefix, tok)}";
                if (u < sendtokens.Count())
                    sendtoken += " + ";
            }



            command += $" --tx-out {ctc.ReceiverAddressNft.FilterToLetterOrDigit()}+0+\"{sendtoken}\"";
            txoutcount++;
            command += $" --tx-out {ctc.ReceiverAddressProject.FilterToLetterOrDigit()}+0";
            txoutcount++;
            if (string.IsNullOrEmpty(txintokens))
            {
                command += $" --tx-out {ctc.SenderAddress.FilterToLetterOrDigit()}+0";
                txoutcount++;
            }
            else
            {
                command += $" --tx-out {ctc.SenderAddress.FilterToLetterOrDigit()}+0+\"{txintokens}\"";
                txoutcount++;
            }


            if (!string.IsNullOrEmpty(ctc.ReceiverAddressMinting) && ctc.MintingFees > 0)
            {
                command += $" --tx-out {ctc.ReceiverAddressMinting.FilterToLetterOrDigit()}+0";
                txoutcount++;
            }

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --mint=\"{minttoken}\"";

            if (!string.IsNullOrEmpty(ctc.MetadataFilename) && File.Exists(ctc.MetadataFilename))
            {
                ctc.buildtransaction.LogFile += File.ReadAllText(ctc.MetadataFilename) + Environment.NewLine;
                command += $" --metadata-json-file {ctc.MetadataFilename}";
            }

            command += $" --out-file {ctc.MatxrawFilename}";

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --minting-script-file {ctc.PolicyscriptFilename}";


            ctc.buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            ctc.buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
                ctc.buildtransaction.LogFile += errormessage + Environment.NewLine;

            if (!string.IsNullOrEmpty(log))
            {
                ctc.Result = false;
                return ctc;
            }

            ctc.Result = CalculateFees(redis, ctc.MatxrawFilename, txincount, txoutcount, ctc.CountWitness, mainnet,
                ref buildtransaction, out var fee);
            ctc.Fee = fee;
            return ctc;
        }



        private static CardanoTransactionClass CreateTransactionForMintAndSign(IConnectionMultiplexer redis,
            CardanoTransactionClass ctc,
            string nodeversion, bool mainnet)
        {
            string command = $"latest transaction build-raw --fee {ctc.Fee}";

            if (string.IsNullOrEmpty(ctc.ReceiverAddressNft))
            {
                ctc.Result = false;
                return ctc;
            }


            // This is the First TX in - this can be the Payin Address or the Account Address
            int txincount = 0;
            int txoutcount = 0;
            string com1 = "";
            BuildTransactionClass buildtransaction = new BuildTransactionClass();
            GetTxInHashes(ctc.UtxoPaymentAddress, out com1, out txincount, out var lovelacesummery,
                ref buildtransaction);
            command += com1;

            txincount = txincount + txincount;
            // Look here for further TX In and Tx Hashes
            // these can be from the already preminted nfts
            foreach (var nx in ctc.nft)
            {
                if (nx.InstockpremintedaddressId != null)
                {
                    com1 = "";
                    txincount = 0;

                    GetTxInHashes(GetNewUtxo(nx.Instockpremintedaddress.Address), out com1,
                        out txincount, out lovelacesummery, ref ctc.buildtransaction);
                    ctc.buildtransaction = buildtransaction;
                    command += com1;

                    txincount = txincount + txincount;
                }
            }

            List<string> minttokens = new List<string>();
            List<string> sendtokens = new List<string>();
            foreach (var nx in ctc.nft)
            {
                if (nx.InstockpremintedaddressId != null || nx.Fingerprint != null)
                {
                    sendtokens.Add(nx.Name);
                }
                else
                {
                    sendtokens.Add(nx.Name);
                    minttokens.Add(nx.Name);
                }
            }

            string txintokens = "";
            ctc.UtxoPaymentAddress.GetTxInTokens();

            string minttoken = "";
            string sendtoken = "";
            int u = 0;
            foreach (var tok in minttokens)
            {
                u++;
                minttoken += $"1 {ctc.Policyid}.{CreateMintTokenname(ctc.TokennamePrefix, tok)}";
                if (u < minttokens.Count())
                    minttoken += " + ";
            }

            u = 0;
            foreach (var tok in sendtokens)
            {
                u++;
                sendtoken +=
                    $"1 {ctc.Policyid}.{CreateMintTokenname(ctc.TokennamePrefix, tok)}";
                if (u < sendtokens.Count())
                    sendtoken += " + ";

                if (ctc.SenderAddress == ctc.ReceiverAddressNft)
                {
                    if (!string.IsNullOrEmpty(txintokens))
                        txintokens += " + ";
                    txintokens += $"1 {ctc.Policyid}.{CreateMintTokenname(ctc.TokennamePrefix, tok)}";
                }
            }



            long minutxofee = 0; // TODO - berechnen, wenn wir diese Routine noch anderweitig einsetzen wollen
            long returnfee = lovelacesummery - ctc.Fee - ctc.MintingFees - ctc.NftCosts - minutxofee;



            if (ctc.SenderAddress != ctc.ReceiverAddressNft)
            {
                // TODO: Das ist noch falsch - wird aber aktuell noch nicht benutzt
                command += $" --tx-out {ctc.ReceiverAddressNft.FilterToLetterOrDigit()}+0+\"{sendtoken}\"";
                txoutcount++;
            }

            command += $" --tx-out {ctc.ReceiverAddressProject.FilterToLetterOrDigit()}+{ctc.NftCosts}";
            txoutcount++;
            if (string.IsNullOrEmpty(txintokens))
            {
                command += $" --tx-out {ctc.SenderAddress.FilterToLetterOrDigit()}+{returnfee}";
                txoutcount++;
            }
            else
            {
                command += $" --tx-out {ctc.SenderAddress.FilterToLetterOrDigit()}+{returnfee}+\"{txintokens}\"";
                txoutcount++;
            }


            if (!string.IsNullOrEmpty(ctc.ReceiverAddressMinting) && ctc.MintingFees > 0)
            {
                command += $" --tx-out {ctc.ReceiverAddressMinting.FilterToLetterOrDigit()}+{ctc.MintingFees}";
                txoutcount++;
            }

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --mint=\"{minttoken}\"";

            if (!string.IsNullOrEmpty(ctc.MetadataFilename) && File.Exists(ctc.MetadataFilename))
            {
                ctc.buildtransaction.LogFile += File.ReadAllText(ctc.MetadataFilename) + Environment.NewLine;
                command += $" --metadata-json-file {ctc.MetadataFilename}";
            }

            command += $" --out-file {ctc.MatxrawFilename}";

            if (!string.IsNullOrEmpty(minttoken))
                command += $" --minting-script-file {ctc.PolicyscriptFilename}";

            ctc.buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            ctc.buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
                ctc.buildtransaction.LogFile += errormessage + Environment.NewLine;

            if (!string.IsNullOrEmpty(log))
            {
                ctc.Result = false;
                return ctc;
            }

            ctc.Result = CalculateFees(redis, ctc.MatxrawFilename, txincount, txoutcount, ctc.CountWitness, mainnet,
                ref buildtransaction, out var fee);
            ctc.Fee = fee;

            return ctc;
        }


        private static bool CalculateFeesBurnAllAdaAndTokens(IConnectionMultiplexer redis,
            TxInClass txin,
            string receiveraddress, string matxrawfile, string policyfile, string guid, int witnesscount, bool mainnet,
            out long fee,
            ref BuildTransactionClass buildtransaction)
        {
            string command = "latest transaction build-raw --fee 0";
            fee = 0;

            if (string.IsNullOrEmpty(receiveraddress))
                return false;

            int tt = 0;
            string com1 = "";
            int txincount = 0;
            GetTxInHashes(txin, out com1, out txincount, out var lovelacesummery, ref buildtransaction);
            command += com1;

            tt = tt + txincount;

            string tokens2 = "";
            if (txin.Tokens != null)
            {
                foreach (var txinToken in txin.Tokens)
                {
                    if (!string.IsNullOrEmpty(tokens2))
                        tokens2 += " + ";
                    string tok = txinToken.Quantity + " " + txinToken.PolicyId + "." + txinToken.TokennameHex;
                    tokens2 += $"-{tok}";
                }
            }


            if (!string.IsNullOrEmpty(tokens2))
            {
                command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+0"; // +\"" + tokens + "\"";
                command += $" --mint=\"{tokens2}\"";
            }
            else return false;

            command += $" --minting-script-file {policyfile}";
            command += $" --out-file {matxrawfile}";

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
                buildtransaction.LogFile += errormessage + Environment.NewLine;

            if (!string.IsNullOrEmpty(log))
            {
                return false;
            }

            return CalculateFees(redis, $@"{matxrawfile}", tt, 1, witnesscount,
                mainnet,
                ref buildtransaction, out fee);
        }


        private static bool CreateTransactionBurnAllAdaAndTokens(TxInClass txin,
            string receiveraddress, string matxrawfile, string policyfile, string guid, long fee, long ttl,
            bool mainnet,
            ref BuildTransactionClass buildtransaction)
        {
            buildtransaction.Fees = fee;
            buildtransaction.SenderAddress = "";

            string command = $@"latest transaction build-raw --fee {fee}";

            if (string.IsNullOrEmpty(receiveraddress))
                return false;


            int tt = 0;
            string com1 = "";
            int txincount = 0;
            GetTxInHashes(txin, out com1, out txincount, out var lovelacesummery, ref buildtransaction);
            command += com1;

            tt = tt + txincount;

            string tokens2 = "";
            if (txin.Tokens != null)
            {
                foreach (var txinToken in txin.Tokens)
                {
                    if (!string.IsNullOrEmpty(tokens2))
                        tokens2 += " + ";
                    string tok = txinToken.Quantity + " " + txinToken.PolicyId + "." + txinToken.TokennameHex;
                    tokens2 += $"-{tok}";
                }
            }

            long adaamount = txin.Lovelace;
            long rest = adaamount - fee;
            if (rest < 0)
            {
                return false;
            }

            List<TxOutClass> txout = new List<TxOutClass>();
            buildtransaction.BuyerTxOut = new TxOutClass { Amount = rest, ReceiverAddress = receiveraddress };

            if (!string.IsNullOrEmpty(tokens2))
            {
                command += $" --tx-out {receiveraddress.FilterToLetterOrDigit()}+{rest}"; // + "+\"" + tokens + "\"";
                command += $" --mint=\"{tokens2}\"";
            }
            else return false;

            command += $" --minting-script-file {policyfile}";
            command += $" --out-file {matxrawfile}";
            command += $" --ttl {ttl}";

            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
                buildtransaction.LogFile += errormessage + Environment.NewLine;

            if (!string.IsNullOrEmpty(log))
            {
                return false;
            }

            return true;
        }



        public static string BurnTokens(EasynftprojectsContext db, IConnectionMultiplexer redis, string burningaddress, string burningfeesreceiver,
            string policyskey,
            string policypassword, string policyscript, string senderskey, string senderskeysalt, bool mainnet, TxInClass txin,
            out CardanoTransactionClass ctc, bool submittransaction = true, int maxtx = 1)
        {
            string guid = GlobalFunctions.GetGuid();

            string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string paymentskeyfile = $"{GeneralConfigurationClass.TempFilePath}payment{guid}.skey";
            string policyscriptfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.script";
            string policyskeyfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.skey";

            // TODO: Umbauen auf ctc
            ctc = new CardanoTransactionClass
            {
                SenderAddress = burningaddress,
                ReceiverAddressNft = burningfeesreceiver,
                Fee = 0,
                Guid = guid,
                MatxSignedFilename = matxsignedfile,
                MatxrawFilename = matxrawfile,
                PolicyscriptFilename = policyscriptfile,
            };

            File.WriteAllText(policyscriptfile, policyscript);


            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";


            //   long fees;

            long fees;
            bool b = CalculateFeesBurnAllAdaAndTokens(redis,
                txin,
                burningfeesreceiver,
                matxrawfile,
                policyscriptfile,
                guid,
                2, // Address + Policy
                mainnet,
                out fees,
                ref ctc.buildtransaction);

            ctc.buildtransaction.Fees = fees;

            if (!b || fees <= 0)
                return "Error while calculating the costs";

            long ttl = (long)q.Slot + 6000;
            b = CreateTransactionBurnAllAdaAndTokens(
                txin,
                burningfeesreceiver,
                matxrawfile,
                policyscriptfile,
                guid,
                fees,
                ttl,
                mainnet,
                ref ctc.buildtransaction);
            if (!b)
                return "Error while creating the transaction";

            string payskey = Encryption.DecryptString(senderskey, senderskeysalt + GeneralConfigurationClass.Masterpassword);
            File.WriteAllText(paymentskeyfile, payskey);

            string polskey = Encryption.DecryptString(policyskey, policypassword);
            File.WriteAllText(policyskeyfile, polskey);

            ctc.SignFiles.Add(policyskeyfile);
            ctc.SignFiles.Add(paymentskeyfile);


            var ok = SignAndSubmit(redis, ctc.SignFiles.ToArray(), "", ctc.MatxrawFilename, ctc.MatxSignedFilename, mainnet, submittransaction,
                ref ctc.buildtransaction);

            GlobalFunctions.DeleteFile(policyskeyfile);
            GlobalFunctions.DeleteFile(policyscriptfile);
            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(paymentskeyfile);

            return ok;

        }







        public static string MintAndSendMultipleTokensFromProjectAddress(EasynftprojectsContext db,
            IConnectionMultiplexer redis,
            TxInClass txin, MultipleTokensClass[] nft, string receiveraddress,
            Nftproject project, string nodeversion, bool mainnet, long hastopay,
            Nftprojectsadditionalpayout[] additionalpayoutWallets, float discount, long stakerewards, long tokenrewards,
            PromotionClass promotion,
            ref BuildTransactionClass buildtransaction, bool submittransaction = true, bool nomintcosts = false,
            int maxtx = 0, string changeaddress = null)
        {
            string guid = GlobalFunctions.GetGuid();

            string metadatafile = $"{GeneralConfigurationClass.TempFilePath}metadata{guid}.json";
            string policyscriptfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.script";
            string alternativepayinskeyfile = $"{GeneralConfigurationClass.TempFilePath}altskey{guid}.skey";
            string matxrawfile_withoutfee = $"{GeneralConfigurationClass.TempFilePath}matx_nofee{guid}.raw";
            string matxrawfile_withfee = $"{GeneralConfigurationClass.TempFilePath}matx_fee{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string policyskeyfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.skey";

            string promotionskeyfile = $"{GeneralConfigurationClass.TempFilePath}promotion{guid}.skey";

            string restofadaaddress = project.CustomerwalletId == null
                ? project.Customer.Adaaddress
                : project.Customerwallet.Walletaddress;



            // TODO: Calcluate the minutxo for the seller tx and calcluate the rest for the changeaddress in the Transaction
            string changeaddress1 = restofadaaddress;

            if (!string.IsNullOrEmpty(changeaddress))
                changeaddress1 = changeaddress;
            // TODO END




            if (!nft.Any())
                return "No NFT specified";

            foreach (var r in nft)
            {
                if (r.nft.NftprojectId != project.Id)
                    return "NFT are not all in the same project (MintAndSendMultipleTokensFromProjectAddress)";

                if (r.nft.State == "sold")
                    return "Some NFT are already sold (MintAndSendMultipleTokensFromProjectAddress)";
            }

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";

            List<string> signfiles = new List<string>();
            List<NftWithMintingAddressClass> n1List = new List<NftWithMintingAddressClass>();

            if (promotion != null)
            {
                File.WriteAllText(promotionskeyfile, promotion.SKey);
                signfiles.Add(promotionskeyfile);
                n1List.Add(new NftWithMintingAddressClass(promotion.PromotionNft, ""));
            }

            foreach (var tokensClass in nft)
            {
                n1List.Add(new NftWithMintingAddressClass(tokensClass.nft, receiveraddress));
            }

            string metadata = CreateMetadataNew(n1List.ToArray(), true);


            if (!string.IsNullOrEmpty(metadata))
                File.WriteAllText(metadatafile, metadata);

            File.WriteAllText(policyscriptfile, project.Policyscript);

            // Create the Sign Keys for Signind and to calculate the witnesses


            int countwitness = 0;
            int z = 0;
            foreach (var nx in nft)
            {
                if (nx.nft.InstockpremintedaddressId != null)
                {
                    z++;
                    string premintedfile = $"{GeneralConfigurationClass.TempFilePath}preminted{guid}_{z}.skey";
                    string password = nx.nft.Instockpremintedaddress.Salt + GeneralConfigurationClass.Masterpassword;
                    string premintedpaykey =
                        Encryption.DecryptString(nx.nft.Instockpremintedaddress.Privateskey, password);
                    File.WriteAllText(premintedfile, premintedpaykey);
                    signfiles.Add(premintedfile);
                }
            }

            if (!string.IsNullOrEmpty(project.Alternativeaddress))
            {
                string altpayinskey = Encryption.DecryptString(project.Alternativepayskey, project.Password);
                File.WriteAllText(alternativepayinskeyfile, altpayinskey);
                signfiles.Add(alternativepayinskeyfile);
            }


            countwitness = signfiles.Count();

            string polskey =
                Encryption.DecryptString(project.Policyskey, project.Password);
            File.WriteAllText(policyskeyfile, polskey);
            countwitness++;

            // END -Create the Sign Keys for Signind and to calculate the witnesses

            long fees;
            var mintingcosts = GlobalFunctions.GetMintingcosts2(nft.First().nft.NftprojectId, nft.Length, hastopay);

            bool b = CalculateFeesMultipleTokens(redis,
                txin,
                receiveraddress,
                restofadaaddress, // Rest of ADA here
                mintingcosts.Mintingcostsreceiver, // Minting Fees here
                nft.First().nft.Nftproject.Policyid,
                nft,
                nft.First().nft.Nftproject.Tokennameprefix,
                !string.IsNullOrEmpty(metadata) ? metadatafile : "",
                policyscriptfile,
                matxrawfile_withoutfee,
                guid,
                countwitness,
                hastopay,
                out fees,
                mainnet,
                additionalpayoutWallets,
                promotion,
                null,
                null,
                ref buildtransaction);

            if (!b || fees <= 0)
                return "Error while calculating the costs";

            long ttl = (long)q.Slot + 6000;
            GlobalFunctions.DeleteFile(matxrawfile_withoutfee);
            int c = 0;
            do
            {
                c++;
                b = CreateTransactionMultipleTokens(
                    redis,
                    nft.First().nft.Nftproject.Customer.Adaaddress,
                    txin,
                    receiveraddress,
                    restofadaaddress, // Rest of ADA here
                    mintingcosts.Mintingcostsreceiver, // Minting Fees here
                    nft.First().nft.Nftproject.Policyid,
                    nft,
                    nft.First().nft.Nftproject.Tokennameprefix,
                    !string.IsNullOrEmpty(metadata) ? metadatafile : "",
                    policyscriptfile,
                    matxrawfile_withfee,
                    guid,
                    mintingcosts.Costs,
                    mintingcosts.MinUtxo,
                    stakerewards,
                    tokenrewards,
                    fees,
                    ttl,
                    hastopay,
                    nft.First().nft.Nftproject.Minutxo,
                    mainnet,
                    additionalpayoutWallets,
                    discount,
                    promotion,
                    null,
                    null,
                    ref buildtransaction);

                if (b)
                    break;

                if (buildtransaction.ErrorMessage.Contains("The following tx input(s) were not present in the UTxO") &&
                    c < 5)
                {
                    // When we have the txhash after 4 tried not in the utxo, we will try it later
                    if (c == 4)
                        return "CHECK LATER";

                    // Otherwise we will wait 20 seconds and try again
                    Console.WriteLine($@"Error:{buildtransaction.ErrorMessage}");
                    buildtransaction.LogFile += $"{DateTime.Now}Wait 10 seconds{Environment.NewLine}";
                    Console.WriteLine(@"Wait 10 seconds");
                    Thread.Sleep(10000);
                    Console.WriteLine(@"Try again");
                    buildtransaction.LogFile += $"{DateTime.Now}Try again{Environment.NewLine}";
                }
                else break;

            } while (true);

            if (!b)
                return "Error while creating the transaction";


            var ok = SignAndSubmit(redis, signfiles.ToArray(), policyskeyfile, matxrawfile_withfee, matxsignedfile, mainnet, submittransaction,
                ref buildtransaction);


            // And delete all created Files
            GlobalFunctions.DeleteFile(policyscriptfile);
            GlobalFunctions.DeleteFile(metadatafile);
            GlobalFunctions.DeleteFile(matxrawfile_withfee);
            GlobalFunctions.DeleteFile(matxrawfile_withoutfee);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(policyskeyfile);
            foreach (var a in signfiles)
                GlobalFunctions.DeleteFile(a);

            return ok;
        }

        public static string SignAndSubmit(IConnectionMultiplexer redis, string[] privateskeyfilename, string policyskeyfilename,
            string matxrawfilename, string matxsignedfilename, bool mainnet, bool submittransaction, ref BuildTransactionClass buildtransaction)
        {
            // Sign the Transaction
            var b = SignTransaction(privateskeyfilename, policyskeyfilename, matxrawfilename, matxsignedfilename, mainnet,
                ref buildtransaction);
            if (!b || !File.Exists(matxsignedfilename))
                return "Error while signing the transaction";


            buildtransaction.SignedTransaction = File.ReadAllText(matxsignedfilename);

            if (!submittransaction)
            {
                return "OK";
            }

            // Submit the Transaction
            var submissionresult = SubmitTransactionWithFallback(matxsignedfilename, buildtransaction);

            buildtransaction = submissionresult.Buildtransaction ?? buildtransaction;
            if (submissionresult.Success)
            {
                //  GlobalFunctions.LogMessage(db, "Successfully submitted transaction", buildtransaction.LogFile);
                buildtransaction.TxHash = submissionresult.TxHash;
                LockTxinAddresses(redis, buildtransaction);
            }
            else
                return "Error while submitting transaction";

            return string.IsNullOrEmpty(submissionresult.TxHash) ? "Error while submitting transaction" : "OK";
        }

        private static void LockTxinAddresses(IConnectionMultiplexer redis, BuildTransactionClass buildtransaction)
        {
            foreach (var txin in buildtransaction.LockTxIn.OrEmptyIfNull())
            {
                GlobalFunctions.SaveStringToRedis(redis, $"UsedTxin_{txin.TxHashId}", "1", 1200);
            }
        }


        public static string MintAndSendMultipleTokensFromProjectAddressCip68(
        IConnectionMultiplexer redis,
        TxInClass txin, MultipleTokensClass[] nft, string receiveraddress,
        Nftproject project, string nodeversion, bool mainnet, long hastopay,
        Nftprojectsadditionalpayout[] additionalpayoutWallets, float discount, long stakerewards, long tokenrewards,
        PromotionClass promotion,
        ref BuildTransactionClass buildtransaction, bool submittransaction = true, bool nomintcosts = false,
        int maxtx = 0, string changeaddress = null)
        {
            string guid = GlobalFunctions.GetGuid();

            string policyscriptfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.script";
            string alternativepayinskeyfile = $"{GeneralConfigurationClass.TempFilePath}altskey{guid}.skey";
            string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string policyskeyfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.skey";

            string promotionskeyfile = $"{GeneralConfigurationClass.TempFilePath}promotion{guid}.skey";

            string restofadaaddress = project.CustomerwalletId == null
                ? project.Customer.Adaaddress
                : project.Customerwallet.Walletaddress;


            if (!nft.Any())
                return "No NFT specified";

            foreach (var r in nft)
            {
                if (r.nft.NftprojectId != project.Id)
                    return "NFT are not all in the same project (MintAndSendMultipleTokensFromProjectAddressCip68)";

                if (r.nft.State == "sold")
                    return "Some NFT are already sold (MintAndSendMultipleTokensFromProjectAddressCip68)";
            }

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";

            List<string> signfiles = new List<string>();
            List<NftWithMintingAddressClass> n1List = new List<NftWithMintingAddressClass>();

            if (promotion != null)
            {
                File.WriteAllText(promotionskeyfile, promotion.SKey);
                signfiles.Add(promotionskeyfile);
                n1List.Add(new NftWithMintingAddressClass(promotion.PromotionNft, ""));
            }

            foreach (var tokensClass in nft)
            {
                n1List.Add(new NftWithMintingAddressClass(tokensClass.nft, receiveraddress));
            }

            var metadatafiles = GetCip68MetadataFiles(n1List, guid);


            File.WriteAllText(policyscriptfile, project.Policyscript);

            // Create the Sign Keys for Signind and to calculate the witnesses


            int countwitness = 0;
            int z = 0;
            foreach (var nx in nft)
            {
                if (nx.nft.InstockpremintedaddressId != null)
                {
                    z++;
                    string premintedfile = $"{GeneralConfigurationClass.TempFilePath}preminted{guid}_{z}.skey";
                    string password = nx.nft.Instockpremintedaddress.Salt + GeneralConfigurationClass.Masterpassword;
                    string premintedpaykey =
                        Encryption.DecryptString(nx.nft.Instockpremintedaddress.Privateskey, password);
                    File.WriteAllText(premintedfile, premintedpaykey);
                    signfiles.Add(premintedfile);
                }
            }

            if (!string.IsNullOrEmpty(project.Alternativeaddress))
            {
                string altpayinskey = Encryption.DecryptString(project.Alternativepayskey, project.Password);
                File.WriteAllText(alternativepayinskeyfile, altpayinskey);
                signfiles.Add(alternativepayinskeyfile);
            }


            countwitness = signfiles.Count();

            string polskey =
                Encryption.DecryptString(project.Policyskey, project.Password);
            File.WriteAllText(policyskeyfile, polskey);
            countwitness++;

            // END -Create the Sign Keys for Signind and to calculate the witnesses

            long fees;
            var mintingcosts = GlobalFunctions.GetMintingcosts2(project.Id, nft.Length, hastopay);

            bool b = CalculateFeesMultipleTokensCip68(redis,
                txin,
                receiveraddress,
                restofadaaddress, // Rest of ADA here
                mintingcosts.Mintingcostsreceiver, // Minting Fees here
                project.Cip68referenceaddress,
                project.Policyid,
                nft,
                project.Tokennameprefix,
                metadatafiles,
                policyscriptfile,
                matxrawfile,
                guid,
                countwitness,
                hastopay,
                out fees,
                mainnet,
                additionalpayoutWallets,
                promotion,
                ref buildtransaction);

            if (!b || fees <= 0)
                return "Error while calculating the costs";

            long ttl = (long)q.Slot + 6000;
            GlobalFunctions.DeleteFile(matxrawfile);
            int c = 0;
            do
            {
                c++;
                b = CreateTransactionMultipleTokensCip68(redis,
                    project.Customer.Adaaddress,
                    txin,
                    receiveraddress,
                    restofadaaddress, // Rest of ADA here
                    mintingcosts.Mintingcostsreceiver, // Minting Fees here
                    project.Cip68referenceaddress,
                    project.Policyid,
                    nft,
                    project.Tokennameprefix,
                    metadatafiles,
                    policyscriptfile,
                    matxrawfile,
                    guid,
                    mintingcosts.Costs,
                    mintingcosts.MinUtxo,
                    stakerewards,
                    tokenrewards,
                    fees,
                    ttl,
                    hastopay,
                    nft.First().nft.Nftproject.Minutxo,
                    mainnet,
                    additionalpayoutWallets,
                    discount,
                    promotion,
                    ref buildtransaction);

                if (b)
                    break;

                if (buildtransaction.ErrorMessage.Contains("The following tx input(s) were not present in the UTxO") &&
                    c < 5)
                {
                    // When we have the txhash after 4 tried not in the utxo, we will try it later
                    if (c == 4)
                        return "CHECK LATER";

                    // Otherwise we will wait 20 seconds and try again
                    Console.WriteLine($@"Error:{buildtransaction.ErrorMessage}");
                    buildtransaction.LogFile += $"{DateTime.Now}Wait 20 seconds{Environment.NewLine}";
                    Console.WriteLine(@"Wait 20 seconds");
                    Thread.Sleep(20000);
                    Console.WriteLine(@"Try again");
                    buildtransaction.LogFile += $"{DateTime.Now}Try again{Environment.NewLine}";
                }
                else break;

            } while (true);

            if (!b)
                return "Error while creating the transaction";

            var ok = SignAndSubmit(redis, signfiles.ToArray(), policyskeyfile, matxrawfile, matxsignedfile, mainnet, submittransaction,
                ref buildtransaction);

            // And delete all created Files
            GlobalFunctions.DeleteFile(policyscriptfile);
            foreach (var filesClass in metadatafiles)
            {
                GlobalFunctions.DeleteFile(filesClass.Filename);
            }
            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(policyskeyfile);

            foreach (var a in signfiles)
                GlobalFunctions.DeleteFile(a);
            return ok;
        }




        public static string CheckMetadata(EasynftprojectsContext db, IConnectionMultiplexer redis, Nft nft,
            string senderaddress, string receiveraddress,
            string metadataoverride, string nodeversion, bool mainnet,
            out BuildTransactionClass buildtransaction)
        {
            buildtransaction = new BuildTransactionClass();

            string guid = GlobalFunctions.GetGuid();

            string metadatafile = $"{GeneralConfigurationClass.TempFilePath}metadata{guid}.json";
            string policyscriptfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.script";
            string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string policyskeyfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.skey";

            if (nft == null)
                return "No NFT specified";

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";

            var utxopaymentaddress =
                GetNewUtxo(senderaddress);

            string metadata = "";
            if (!string.IsNullOrEmpty(metadataoverride))
            {
                File.WriteAllText(metadatafile, metadataoverride);
                metadata = metadataoverride;
            }
            else
            {
                List<NftWithMintingAddressClass> nfts = new List<NftWithMintingAddressClass>
                {
                    new NftWithMintingAddressClass(nft, receiveraddress)
                };

                metadata = CreateMetadataNew(nfts, true);
                if (!string.IsNullOrEmpty(metadata))
                    File.WriteAllText(metadatafile, metadata);
                else
                {
                    return "Error while creating the metadata";
                }
            }


            if (!metadata.Contains("version"))
            {
                buildtransaction.ErrorMessage = "Version not found in metadata";
                return "Version not found in metadata";
            }

            if (!metadata.Contains(nft.Nftproject.Policyid))
            {
                buildtransaction.ErrorMessage = "Policyid not found in metadata";
                return "Policyid not found in metadata";
            }

            if (!metadata.Contains(nft.Name) && !metadata.Contains(nft.Name.ToHex()))
            {
                buildtransaction.ErrorMessage = "Tokenname not found in metadata";
                return "Tokenname not found in metadata";
            }


            buildtransaction.Metadata = metadata;

            File.WriteAllText(policyscriptfile, nft.Nftproject.Policyscript);

            // Create the Sign Keys for Signind and to calculate the witnesses

            int countwitness = 1;

            policyskeyfile = "";

            // END -Create the Sign Keys for Signind and to calculate the witnesses

            long fees;
            //  var mintingcosts = GlobalFunctions.GetMintingcosts2(db, nft.First().NftprojectId, nft.Length);
            bool b = CalculateFees(redis,
                utxopaymentaddress,
                receiveraddress,
                nft.Nftproject.Customer.Adaaddress, // Rest of ADA here
                "",
                nft.Nftproject.Policyid,
                new[] { nft },
                nft.Nftproject.Tokennameprefix,
                !string.IsNullOrEmpty(metadata) ? metadatafile : "",
                policyscriptfile,
                matxrawfile,
                guid,
                countwitness,
                20000000,
                out fees,
                nodeversion,
                mainnet,
                new Nftprojectsadditionalpayout[] { },
                0,
                ref buildtransaction,
                true);

            if (!b || fees <= 0)
                return "Error while calculating the costs";

            GlobalFunctions.DeleteFile(policyscriptfile);
            GlobalFunctions.DeleteFile(metadatafile);
            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(policyskeyfile);
            return "OK";
        }

        public static string CheckMetadataCip68(EasynftprojectsContext db, IConnectionMultiplexer redis, Nft nft,
       string senderaddress, string receiveraddress,
       string metadataoverride, string nodeversion, bool mainnet,
       out BuildTransactionClass buildtransaction)
        {
            buildtransaction = new BuildTransactionClass();

            string guid = GlobalFunctions.GetGuid();

            string metadatafile = $"{GeneralConfigurationClass.TempFilePath}metadata{guid}.json";
            string policyscriptfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.script";
            string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string policyskeyfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.skey";

            if (nft == null)
                return "No NFT specified";

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";

            var utxopaymentaddress = GetNewUtxo(senderaddress);
            List<Cip68MetadataFilesClass> metadatafiles = new();
            if (string.IsNullOrEmpty(metadataoverride))
            {
                List<NftWithMintingAddressClass> nfts = new List<NftWithMintingAddressClass> { new NftWithMintingAddressClass(nft, receiveraddress) };
                metadatafiles = GetCip68MetadataFiles(nfts, guid);
            }
            else
            {
                metadatafiles = GetCip68MetadataFiles(metadataoverride, nft, guid);
            }

            File.WriteAllText(policyscriptfile, nft.Nftproject.Policyscript);

            // Create the Sign Keys for Signind and to calculate the witnesses

            int countwitness = 1;

            policyskeyfile = "";

            // END -Create the Sign Keys for Signind and to calculate the witnesses

            long fees;

            var project = nft.Nftproject;
            try
            {
                var b = CalculateFeesMultipleTokensCip68(redis,
                    utxopaymentaddress.TxIn.First(),
                    receiveraddress,
                    nft.Nftproject.Customer.Adaaddress, // Rest of ADA here
                    "",
                    project.Cip68referenceaddress,
                    project.Policyid,
                    new MultipleTokensClass[] { new MultipleTokensClass() { nft = nft, Multiplier = 1, tokencount = 1 } },
                    project.Tokennameprefix,
                    metadatafiles,
                    policyscriptfile,
                    matxrawfile,
                    guid,
                    countwitness,
                    20000000,
                    out fees,
                    mainnet,
                    new Nftprojectsadditionalpayout[] { },
                    null,
                    ref buildtransaction);

                if (!b || fees <= 0)
                    return "Error while calculating the costs";
            }
            catch
            {
                return "Error while calculating the costs";
            }

            GlobalFunctions.DeleteFile(policyscriptfile);
            GlobalFunctions.DeleteFile(metadatafile);
            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(policyskeyfile);
            return "OK";
        }
        public static string MassMintAndSend(EasynftprojectsContext db, IConnectionMultiplexer redis, Nft[] nft,
            string receiveraddress,
            long hastopay, string nodeversion, bool mainnet, string senderaddress, string senderskey,
            string minutxoversion,
            out BuildTransactionClass buildtransaction, bool submittransaction = true, bool ignoresoldflag = false,
            bool nomintcosts = false, int maxtx = 0)
        {
            buildtransaction = new BuildTransactionClass();

            string guid = GlobalFunctions.GetGuid();

            string metadatafile = $"{GeneralConfigurationClass.TempFilePath}metadata{guid}.json";
            string policyscriptfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.script";
            string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string paymentskeyfile = $"{GeneralConfigurationClass.TempFilePath}payment{guid}.skey";
            string policyskeyfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.skey";

            if (!nft.Any())
                return "No NFT specified";

            int projectid = nft.First().NftprojectId;
            foreach (var r in nft)
            {
                if (r.NftprojectId != projectid)
                    return "NFT are not all in the same project (MassMintAndSend)";

                if (r.State == "sold" && ignoresoldflag == false)
                    return "Some NFT are already sold (MassMintAndSend)";
            }

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";

            var utxopaymentaddress = GetNewUtxo(senderaddress);
            List<NftWithMintingAddressClass> nfts = new List<NftWithMintingAddressClass>();
            foreach (var r in nft)
            {
                nfts.Add(new NftWithMintingAddressClass(r, receiveraddress));
            }

            string metadata = CreateMetadataNew(nfts, ignoresoldflag);


            if (!string.IsNullOrEmpty(metadata))
                File.WriteAllText(metadatafile, metadata);

            File.WriteAllText(policyscriptfile, nft.First().Nftproject.Policyscript);

            // Create the Sign Keys for Signind and to calculate the witnesses

            string payskey = senderskey;
            File.WriteAllText(paymentskeyfile, payskey);

            int countwitness = 1;
            List<string> signfiles = new List<string>();
            signfiles.Add(paymentskeyfile);

            bool needpolicyfile = false;
            int z = 0;
            foreach (var nx in nft)
            {
                if (nx.InstockpremintedaddressId != null)
                {
                    z++;
                    string premintedfile = $"{GeneralConfigurationClass.TempFilePath}preminted{guid}_{z}.skey";
                    string password = nx.Instockpremintedaddress.Salt + GeneralConfigurationClass.Masterpassword;
                    string premintedpaykey = Encryption.DecryptString(nx.Instockpremintedaddress.Privateskey, password);
                    File.WriteAllText(premintedfile, premintedpaykey);
                    signfiles.Add(premintedfile);
                }
                else needpolicyfile = true;
            }

            countwitness = signfiles.Count();

            if (needpolicyfile)
            {
                string polskey =
                    Encryption.DecryptString(nft.First().Nftproject.Policyskey, nft.First().Nftproject.Password);
                File.WriteAllText(policyskeyfile, polskey);
                countwitness++;
            }
            else policyskeyfile = "";

            // END -Create the Sign Keys for Signind and to calculate the witnesses

            long fees;
            var mintingcosts = GlobalFunctions.GetMintingcosts2(nft.First().NftprojectId, nft.Length, hastopay);
            bool b = CalculateFees(redis,
                utxopaymentaddress,
                receiveraddress,
                senderaddress, // Rest of ADA here
                nomintcosts ? "" : mintingcosts.Mintingcostsreceiver, // Minting Fees here
                nft.First().Nftproject.Policyid,
                nft,
                nft.First().Nftproject.Tokennameprefix,
                !string.IsNullOrEmpty(metadata) ? metadatafile : "",
                policyscriptfile,
                matxrawfile,
                guid,
                countwitness,
                hastopay,
                out fees,
                nodeversion,
                mainnet,
                new Nftprojectsadditionalpayout[] { },
                0,
                ref buildtransaction,
                ignoresoldflag);

            if (!b || fees <= 0)
                return "Error while calculating the costs";

            long ttl = (long)q.Slot + 6000;

            GlobalFunctions.DeleteFile(matxrawfile);

            b = CreateTransaction(
                redis,
                senderaddress,
                utxopaymentaddress,
                receiveraddress,
                senderaddress, // Rest of ADA here
                nomintcosts ? "" : mintingcosts.Mintingcostsreceiver, // Minting Fees here
                nft.First().Nftproject.Policyid,
                nft,
                nft.First().Nftproject.Tokennameprefix,
                !string.IsNullOrEmpty(metadata) ? metadatafile : "",
                policyscriptfile,
                matxrawfile,
                guid,
                nomintcosts ? 0 : mintingcosts.Costs,
                mintingcosts.MinUtxo,
                fees,
                0, // Stakerewards
                0, // Tokenrewards
                ttl,
                0, // This must be null
                minutxoversion,
                nodeversion,
                mainnet,
                new Nftprojectsadditionalpayout[] { },
                0,
                ref buildtransaction,
                ignoresoldflag);
            if (!b)
                return "Error while creating the transaction";


            var ok = SignAndSubmit(redis, signfiles.ToArray(), policyskeyfile, matxrawfile, matxsignedfile, mainnet,
                submittransaction,
                ref buildtransaction);

            GlobalFunctions.DeleteFile(policyscriptfile);
            GlobalFunctions.DeleteFile(metadatafile);
            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(policyskeyfile);
            foreach (var a in signfiles)
                GlobalFunctions.DeleteFile(a);

            return ok;
        }


        public static string MintAndSendMultipleTokensFromApiCip68(IConnectionMultiplexer redis,
         MultipleTokensClass[] nft,
         Nftaddress address, string receiveraddress, string restofadaaddress,
         Nftprojectsadditionalpayout[] additionalpayoutWallets, float discount, long stakerewards, long tokenrewards,
        bool mainnet, TxInClass txin, PromotionClass promotion, Nftproject project, Adminmintandsendaddress paywallet,
         ref BuildTransactionClass buildtransaction)
        {
            string guid = GlobalFunctions.GetGuid();

            string policyscriptfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.script";
            string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string paymentskeyfile = $"{GeneralConfigurationClass.TempFilePath}payment{guid}.skey";
            string policyskeyfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.skey";

            string promotionskeyfile = $"{GeneralConfigurationClass.TempFilePath}promotion{guid}.skey";


            if (!nft.Any())
                return "No NFT specified";

            int projectid = project.Id;
            foreach (var r in nft)
            {
                if (r.nft.NftprojectId != projectid)
                    return "NFT are not all in the same project (MintAndSendMultipleTokensFromApiCip68)";
                if (r.nft.State == "sold")
                    return "Some NFT are already sold (MintAndSendMultipleTokensFromApiCip68)";
            }

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";

            List<NftWithMintingAddressClass> n1List = new List<NftWithMintingAddressClass>();
            foreach (var tokensClass in nft)
            {
                n1List.Add(new NftWithMintingAddressClass(tokensClass.nft, receiveraddress));
            }

            List<string> signfiles = new List<string>();

            if (promotion != null)
            {
                File.WriteAllText(promotionskeyfile, promotion.SKey);
                signfiles.Add(promotionskeyfile);
                n1List.Add(new NftWithMintingAddressClass(promotion.PromotionNft, ""));
            }

            var metadatafiles = GetCip68MetadataFiles(n1List, guid);


            File.WriteAllText(policyscriptfile, nft.First().nft.Nftproject.Policyscript);

            // Create The Signing Keys and Calculate the witnesses
            int countwitness = 0;


            signfiles.Add(paymentskeyfile);


            bool needpolicyfile = false;
            int z = 0;
            foreach (var nx in nft)
            {
                if (nx.nft.InstockpremintedaddressId != null)
                {
                    z++;
                    string premintedfile = $"{GeneralConfigurationClass.TempFilePath}preminted{guid}_{z}.skey";
                    string password = nx.nft.Instockpremintedaddress.Salt + GeneralConfigurationClass.Masterpassword;
                    string premintedpaykey =
                        Encryption.DecryptString(nx.nft.Instockpremintedaddress.Privateskey, password);
                    File.WriteAllText(premintedfile, premintedpaykey);
                    signfiles.Add(premintedfile);
                }
                else
                {
                    needpolicyfile = true;
                }
            }

            countwitness = signfiles.Count();

            if (needpolicyfile)
            {
                string polskey = Encryption.DecryptString(project.Policyskey,
                    project.Password);
                File.WriteAllText(policyskeyfile, polskey);
                countwitness++;
            }
            else
            {
                policyskeyfile = "";
            }

            // END Create the Signing Keys

            long hastopay = 0;
            if (address.Price != null) hastopay = (long)address.Price;

            long fees;
            var mintingcosts = GlobalFunctions.GetMintingcosts2(project.Id, nft.Length, hastopay);
            bool b = false;
            try
            {
                b = CalculateFeesMultipleTokensCip68(redis,
                   txin,
                   receiveraddress,
                   restofadaaddress, // Rest of ADA here (eg. internal customer wallet or external customer wallet)
                   mintingcosts.Mintingcostsreceiver, // Minting Fees here
                   project.Cip68referenceaddress,
                   project.Policyid,
                   nft,
                   project.Tokennameprefix,
                   metadatafiles,
                   policyscriptfile,
                   matxrawfile,
                   guid,
                   countwitness,
                   hastopay,
                   out fees,
                   mainnet,
                   additionalpayoutWallets,
                   promotion,
                   ref buildtransaction);

                if (!b || fees <= 0)
                    return "Error while calculating the costs";
            }
            catch
            {
                return "Error while calculating the costs";
            }

            long ttl = (long)q.Slot + 6000;

            int count = 0;
            GlobalFunctions.DeleteFile(matxrawfile);
            count++;
            b = CreateTransactionMultipleTokensCip68(
                redis,
                address.Address,
                txin,
                receiveraddress,
                restofadaaddress, // Rest - eg internal project wallet or external customer wallet
                mintingcosts.Mintingcostsreceiver, // Minting Fees here
                project.Cip68referenceaddress,
                project.Policyid,
                nft,
                project.Tokennameprefix,
                metadatafiles,
                policyscriptfile,
                matxrawfile,
                guid,
                mintingcosts.Costs,
                mintingcosts.MinUtxo,
                stakerewards,
                tokenrewards,
                fees,
                ttl,
                hastopay,
                project.Minutxo,
                mainnet,
                additionalpayoutWallets,
                discount,
                promotion,
                ref buildtransaction);
            if (!b)
                return "Error while creating the transaction";

            if (string.IsNullOrEmpty(address.Salt))
            {
                string payskey = Encryption.DecryptString(address.Privateskey, address.Nftproject.Password);
                File.WriteAllText(paymentskeyfile, payskey);
            }
            else
            {
                string salt = address.Salt;
                string password = salt + GeneralConfigurationClass.Masterpassword;
                string payskey = Encryption.DecryptString(address.Privateskey, password);
                File.WriteAllText(paymentskeyfile, payskey);
            }


            var ok = SignAndSubmit(redis, signfiles.ToArray(), policyskeyfile, matxrawfile, matxsignedfile, mainnet, true,
                ref buildtransaction);

            GlobalFunctions.DeleteFile(policyscriptfile);
            foreach (var filesClass in metadatafiles)
            {
                GlobalFunctions.DeleteFile(filesClass.Filename);
            }
            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(policyskeyfile);

            foreach (var a in signfiles)
                GlobalFunctions.DeleteFile(a);

            return ok;
        }

        private static List<Cip68MetadataFilesClass> GetCip68MetadataFiles(List<NftWithMintingAddressClass> n1List, string guid)
        {
            List<Cip68MetadataFilesClass> metadatafiles = new List<Cip68MetadataFilesClass>();

            int i = 0;
            foreach (var nft1 in n1List)
            {
                i++;
                string metadata = CreateMetadataCip68(nft1, true);
                Cip68MetadataFilesClass m = new Cip68MetadataFilesClass()
                { Filename = $"{GeneralConfigurationClass.TempFilePath}metadata{guid}_{i}.json", Tokenname = GlobalFunctions.GetTokenname(nft1.Nft.Nftproject.Tokennameprefix, nft1.Nft.Name) };
                metadatafiles.Add(m);
                File.WriteAllText(m.Filename, metadata);
            }

            return metadatafiles;
        }

        private static List<Cip68MetadataFilesClass> GetCip68MetadataFiles(string metadataoverride, Nft nft, string guid)
        {
            List<Cip68MetadataFilesClass> metadatafiles = new List<Cip68MetadataFilesClass>();
            int i = 1;
            string metadata = metadataoverride;
            Cip68MetadataFilesClass m = new Cip68MetadataFilesClass()
            {
                Filename = $"{GeneralConfigurationClass.TempFilePath}metadata{guid}_{i}.json",
                Tokenname = GlobalFunctions.GetTokenname(nft.Nftproject.Tokennameprefix, nft.Name)
            };
            metadatafiles.Add(m);
            File.WriteAllText(m.Filename, metadata);

            return metadatafiles;
        }

        public static string  MintAndSendMultipleTokensFromApi(EasynftprojectsContext db, IConnectionMultiplexer redis,
            MultipleTokensClass[] nft,
            Nftaddress address, string receiveraddress, string restofadaaddress,
            Nftprojectsadditionalpayout[] additionalpayoutWallets, float discount, long stakerewards, long tokenrewards,
            bool mainnet, TxInClass txin, PromotionClass promotion, Adminmintandsendaddress paywallet,
            ref BuildTransactionClass buildtransaction)
        {
            string guid = GlobalFunctions.GetGuid();

            string metadatafile = $"{GeneralConfigurationClass.TempFilePath}metadata{guid}.json";
            string policyscriptfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.script";
            string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string paymentskeyfile = $"{GeneralConfigurationClass.TempFilePath}payment{guid}.skey";
            string policyskeyfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.skey";
            string mintandsendskeyfile = $"{GeneralConfigurationClass.TempFilePath}mintandsend{guid}.skey";
            string promotionskeyfile = $"{GeneralConfigurationClass.TempFilePath}promotion{guid}.skey";


            if (!nft.Any())
            {
                return "No NFT specified";
            }

            int projectid = nft.First().nft.NftprojectId;
            foreach (var r in nft)
            {
                if (r.nft.NftprojectId != projectid)
                {
                    return "NFT are not all in the same project (MintAndSendMultipleTokensFromApi)";
                }

                if (r.nft.State == "sold")
                {
                    return "Some NFT are already sold (MintAndSendMultipleTokensFromApi) " + r.nft.State + " - " +
                           r.nft.Id + " - " + r.nft.Name;
                }
            }

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";

            List<NftWithMintingAddressClass> n1List = new List<NftWithMintingAddressClass>();
            foreach (var tokensClass in nft)
            {
                n1List.Add(new NftWithMintingAddressClass(tokensClass.nft, receiveraddress));
            }

            List<string> signfiles = new List<string>();

            if (promotion != null)
            {
                File.WriteAllText(promotionskeyfile, promotion.SKey);
                signfiles.Add(promotionskeyfile);
                n1List.Add(new NftWithMintingAddressClass(promotion.PromotionNft, ""));
            }

            string metadata = CreateMetadataNew(n1List, true);

            if (!string.IsNullOrEmpty(metadata))
                File.WriteAllText(metadatafile, metadata);

            File.WriteAllText(policyscriptfile, nft.First().nft.Nftproject.Policyscript);

            // Create The Signing Keys and Calculate the witnesses
            signfiles.Add(paymentskeyfile);

            bool needpolicyfile = false;
            int z = 0;
            foreach (var nx in nft)
            {
                if (nx.nft.InstockpremintedaddressId != null)
                {
                    z++;
                    string premintedfile = $"{GeneralConfigurationClass.TempFilePath}preminted{guid}_{z}.skey";
                    string password = nx.nft.Instockpremintedaddress.Salt + GeneralConfigurationClass.Masterpassword;
                    string premintedpaykey =
                        Encryption.DecryptString(nx.nft.Instockpremintedaddress.Privateskey, password);
                    File.WriteAllText(premintedfile, premintedpaykey);
                    signfiles.Add(premintedfile);
                }
                else
                {
                    needpolicyfile = true;
                }
            }


            var countwitness = signfiles.Count();
            if (needpolicyfile)
            {
                string polskey = Encryption.DecryptString(nft.First().nft.Nftproject.Policyskey,
                    nft.First().nft.Nftproject.Password);
                File.WriteAllText(policyskeyfile, polskey);
                countwitness++;
            }
            else
            {
                policyskeyfile = "";
            }

            // END Create the Signing Keys

            long hastopay = 0;
            if (address.Price != null) hastopay = (long)address.Price;

            long fees;
            var mintingcosts = GlobalFunctions.GetMintingcosts2(nft.First().nft.NftprojectId, nft.Length, hastopay);

            // Mintingcosts -1 means that the user can send any amount of ada. but we are using the adminmintandsendaddresses for the fees and minutxo
            if (hastopay == -1)
            {
                mintingcosts.Costs = 0;
                string payskey = Encryption.DecryptString(paywallet.Privateskey,
                    GeneralConfigurationClass.Masterpassword + paywallet.Salt);
                File.WriteAllText(mintandsendskeyfile, payskey);
                signfiles.Add(mintandsendskeyfile);
                countwitness++;
            }



            bool b;
            try
            {
                b = CalculateFeesMultipleTokens(
                    redis,
                    txin,
                    receiveraddress,
                    restofadaaddress, // Rest of ADA here (eg. internal customer wallet or external customer wallet)
                    mintingcosts.Mintingcostsreceiver, // Minting Fees here
                    nft.First().nft.Nftproject.Policyid,
                    nft,
                    nft.First().nft.Nftproject.Tokennameprefix,
                    metadatafile,
                    policyscriptfile,
                    matxrawfile,
                    guid,
                    countwitness,
                    hastopay,
                    out fees,
                    mainnet,
                    additionalpayoutWallets,
                    promotion,
                    paywallet,
                    address.Refundreceiveraddress,
                    ref buildtransaction);

                if (!b || fees <= 0)
                    return "Error while calculating the costs";
            }
            catch
            {
                buildtransaction.LogFile += "Exception Error while calculating the costs" + Environment.NewLine;
                return "Error while calculating the costs";
            }

            long ttl = (long)(q.Slot ?? 0) + 6000;
            GlobalFunctions.DeleteFile(matxrawfile);
            b = CreateTransactionMultipleTokens(
                redis,
                address.Address,
                txin,
                receiveraddress,
                restofadaaddress, // Rest - eg internal project wallet or external customer wallet
                mintingcosts.Mintingcostsreceiver, // Minting Fees here
                nft.First().nft.Nftproject.Policyid,
                nft,
                nft.First().nft.Nftproject.Tokennameprefix,
                metadatafile,
                policyscriptfile,
                matxrawfile,
                guid,
                mintingcosts.Costs,
                mintingcosts.MinUtxo,
                stakerewards,
                tokenrewards,
                fees,
                ttl,
                hastopay,
                address.Nftproject.Minutxo,
                mainnet,
                additionalpayoutWallets,
                discount,
                promotion,
                paywallet,
                address.Refundreceiveraddress,
                ref buildtransaction);
            if (!b)
            {
                buildtransaction.LogFile += "Exception Error while creating the transaction" + Environment.NewLine;
                return "Error while creating the transaction";
            }

            if (string.IsNullOrEmpty(address.Salt))
            {
                string payskey = Encryption.DecryptString(address.Privateskey, address.Nftproject.Password);
                File.WriteAllText(paymentskeyfile, payskey);
            }
            else
            {
                string salt = address.Salt;
                string password = salt + GeneralConfigurationClass.Masterpassword;
                string payskey = Encryption.DecryptString(address.Privateskey, password);
                File.WriteAllText(paymentskeyfile, payskey);
            }

            var ok = SignAndSubmit(redis, signfiles.ToArray(), policyskeyfile, matxrawfile, matxsignedfile, mainnet, true,
                ref buildtransaction);

            GlobalFunctions.DeleteFile(policyscriptfile);
            GlobalFunctions.DeleteFile(metadatafile);
            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(policyskeyfile);
            GlobalFunctions.DeleteFile(paymentskeyfile);
            GlobalFunctions.DeleteFile(mintandsendskeyfile);

            foreach (var a in signfiles)
                GlobalFunctions.DeleteFile(a);

            return ok;
        }


        public static bool GetTxId(string matxsignedfile, ref BuildTransactionClass buildtransaction)
        {
            string command2 = $"latest transaction txid --tx-file {matxsignedfile}";
            buildtransaction.LogFile += command2 + Environment.NewLine;
            var z = CardanoCli(command2, out var errormessage);
            // buildtransaction.SubmissionResult = z;
            buildtransaction.LogFile += z + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
                buildtransaction.LogFile += errormessage + Environment.NewLine;

            if (z.Contains("failed"))
                return false;

            // If there was no TX ID - wait 2 second and try again
            if (string.IsNullOrEmpty(z.Trim()))
            {
                Task.Delay(2000).Wait();
                buildtransaction.LogFile += command2 + Environment.NewLine;
                z = CardanoCli(command2, out var errormessage2);
                //   buildtransaction.SubmissionResult = z;
                buildtransaction.LogFile += z + Environment.NewLine;
                if (!string.IsNullOrEmpty(errormessage2))
                    buildtransaction.LogFile += errormessage2 + Environment.NewLine;

            }

            if (z.Contains("failed") || string.IsNullOrEmpty(z.Trim()))
                return false;

            buildtransaction.TxHash = z.Replace("\n", "").Replace("\r", "");
            return true;
        }

        public static SubmissionResultClass SubmitTransaction(string matxsignedfile, bool mainnet,
            ref BuildTransactionClass buildtransaction)
        {
            try
            {
                string command2 = CliCommandExtensions.GetTransactionSubmit()
                    .GetTxFile(matxsignedfile)
                    .GetNetwork(mainnet);

                buildtransaction.LogFile += command2 + Environment.NewLine;
                var z = CardanoCli(command2, out var errormessage);
                buildtransaction.LogFile += z + Environment.NewLine;
                if (!string.IsNullOrEmpty(errormessage))
                {
                    buildtransaction.ErrorMessage = errormessage;
                    buildtransaction.LogFile += errormessage + Environment.NewLine;
                }

                buildtransaction.SubmissionResult = z;
                if (z.ToLower().Contains("error"))
                    return new SubmissionResultClass()
                    { ErrorMessage = z, Success = false, Buildtransaction = buildtransaction };
                if (z.ToLower().Contains("successfully") || string.IsNullOrEmpty(z.Trim()))
                    return new SubmissionResultClass()
                    { Success = true, Buildtransaction = buildtransaction, TxHash = buildtransaction.TxHash };
            }
            catch (Exception e)
            {
                buildtransaction.LogFile += e.Message + Environment.NewLine + e.InnerException?.StackTrace +
                                            Environment.NewLine;
                return new SubmissionResultClass()
                { ErrorMessage = e.Message, Success = false, Buildtransaction = buildtransaction };
            }
            return new SubmissionResultClass()
            { Success = false, Buildtransaction = buildtransaction };
        }

        public static SubmissionResultClass SubmitTransactionWithFallback(
            string matxsignedfile, BuildTransactionClass bt)
        {
            var res = Task.Run(async () => await SubmitTransactionWithFallbackAsync(matxsignedfile, bt));
            return res.Result;
        }

        public static async Task<SubmissionResultClass> SubmitTransactionWithFallbackAsync(
            string matxsignedfile, BuildTransactionClass bt)
        {
            SubmissionResultClass res = new SubmissionResultClass() { ErrorMessage = "", Buildtransaction = bt };
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);

            // Submit first to Meastro
            var maestroresult = await MaestroFunctions.SubmitTurboTransactionFileAsync(matxsignedfile);
            if (!maestroresult.Success || string.IsNullOrEmpty(maestroresult.TxHash))
            {
                bt.LogFile += Environment.NewLine + "Submit via Meastro failed - Try again with Blockfrost" +
                              Environment.NewLine + maestroresult.ErrorMessage + Environment.NewLine;
                await GlobalFunctions.LogMessageAsync(db, "Submit via Maestro failed - Try again with Blockfrost",
                    matxsignedfile + Environment.NewLine + bt.LogFile);
            }
            else
            {
                bt.LogFile += Environment.NewLine + "Submit via Maestro SUCCESS" + Environment.NewLine + "TX-Hash: " + maestroresult.TxHash + Environment.NewLine;
                await GlobalFunctions.LogMessageAsync(db, "Submit via Maestro SUCCESS",
                    matxsignedfile);
                if (!string.IsNullOrEmpty(maestroresult.TxHash))
                {
                    res.TxHash = maestroresult.TxHash.Replace("\"", "");
                    res.Success = true;
                }
            }


            if (maestroresult.Success) return res;

            // Submit to Blockfrost
            var blockfrostresult = await BlockfrostFunctions.SubmitTransactionFileAsync(matxsignedfile);
            if (!blockfrostresult.Success)
            {
                bt.LogFile += Environment.NewLine + "Submit via Blockfrost failed - Try again with CLI" +
                              Environment.NewLine + blockfrostresult.ErrorMessage + Environment.NewLine;
                await GlobalFunctions.LogMessageAsync(db, "Submit via Blockfrost failed - Try again with CLI",
                    matxsignedfile + Environment.NewLine + bt.LogFile);
            }
            else
            {
                bt.LogFile += Environment.NewLine + "Submit via Blockfrost SUCCESS" + Environment.NewLine + "TX-Hash: " + blockfrostresult.TxHash + Environment.NewLine;
                await GlobalFunctions.LogMessageAsync(db, "Submit via Blockfrost SUCCESS",
                    matxsignedfile);
                res.TxHash = blockfrostresult.TxHash.Replace("\"", "");
                res.Success = true;
            }

            if (maestroresult.Success || blockfrostresult.Success) return res;

            var cliresult = ConsoleCommand.SubmitTransaction(matxsignedfile, GlobalFunctions.IsMainnet(), ref bt);
            if (cliresult.Success)
            {
                bt.LogFile += Environment.NewLine + "Submit via CLI successful" + Environment.NewLine;
                ConsoleCommand.GetTxId(matxsignedfile, ref bt);
                bt.LogFile += Environment.NewLine + "TX-Hash: " + bt.TxHash + Environment.NewLine;
                await GlobalFunctions.LogMessageAsync(db, "Submit via CLI successful " + bt.TxHash,
                    matxsignedfile + Environment.NewLine + bt.LogFile);
                res.TxHash = bt.TxHash.Replace("\"", "");
                res.Success = true;
            }
            else
            {
                bt.LogFile += Environment.NewLine + "Submit via CLI failed";

                await GlobalFunctions.LogMessageAsync(db, "Submit via CLI failed",
                    matxsignedfile + Environment.NewLine + bt.LogFile);
                res.ErrorMessage += Environment.NewLine + bt.LogFile;
                res.Success = false;
            }

            res.Buildtransaction = bt;

            return res;
        }


        public static bool SignTransaction(string[] privateskeyfilename, string policyskeyfilename,
            string matxrawfilename, string matxsignedfilename, bool mainnet, ref BuildTransactionClass buildtransaction)
        {
            string command = CliCommandExtensions.GetTransactionSign()
                .GetSigningKeyFile(policyskeyfilename)
                .GetSigningKeyFile(privateskeyfilename)
                .GetTxBodyFile(matxrawfilename)
                .GetOutFile(matxsignedfilename)
                .GetNetwork(mainnet);

            buildtransaction.LogFile += command + Environment.NewLine;
            string st = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += st + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
                buildtransaction.LogFile += errormessage + Environment.NewLine;

            if (!string.IsNullOrEmpty(st))
                return false;

            if (!File.Exists(matxsignedfilename))
                return false;

            return true;
        }


        public static string CreateMetadataNew(IEnumerable<NftWithMintingAddressClass> nft, bool takeAlsoPremintedNfts = false)
        {
            if (!nft.Any())
                return "";
            GetMetadataClass gmc = new GetMetadataClass((from a in nft select new NftIdWithMintingAddressClass() { MintingAddress = a.MintingAddress, NftId = a.Nft.Id }).ToArray(), takeAlsoPremintedNfts);
            return gmc.MetadataResult().Metadata;
        }

        public static string CreateMetadataCip68(NftWithMintingAddressClass nft, bool takeAlsoPremintedNfts = false)
        {
            GetMetadataClass gmc = new GetMetadataClass(new NftIdWithMintingAddressClass(nft), takeAlsoPremintedNfts);
            return gmc.MetadataResult().MetadataCip68;
        }
        public static string CreateMetadataCip68(int nftId, string mintingaddress, bool takeAlsoPremintedNfts = false)
        {
            GetMetadataClass gmc = new GetMetadataClass(nftId,mintingaddress, takeAlsoPremintedNfts);
            return gmc.MetadataResult().MetadataCip68;
        }
        public static void SaveProtocolParamsFile(IConnectionMultiplexer redis, string protocolparamsfile, bool mainnet,
            ref BuildTransactionClass buildtransaction)
        {
            var prot = GlobalFunctions.GetStringFromRedis(redis, "ProtocolParameter" + mainnet);
            if (string.IsNullOrEmpty(prot))
            {
                string command = CliCommandExtensions.GetQueryProtocolParameter()
                    .GetOutFile(protocolparamsfile)
                    .GetNetwork(mainnet);

                buildtransaction.LogFile += command + Environment.NewLine;

                CardanoCli(command, out var errormessage);

                if (!string.IsNullOrEmpty(errormessage))
                {
                    buildtransaction.ErrorMessage = errormessage;
                    buildtransaction.LogFile += errormessage + Environment.NewLine;
                }
                else
                {
                    GlobalFunctions.SaveStringToRedis(redis, "ProtocolParameter" + mainnet, File.ReadAllText(protocolparamsfile), 1000);
                }
            }
            else
            {
                File.WriteAllText(protocolparamsfile, prot);
            }
        }



        public static void CreateSendbackMessageMetadata(string sendbackmessagefile, string sendbackmessage)
        {
            CardanoMessageClass cmc = new CardanoMessageClass() { The674 = new The674() { Msg = new[] { sendbackmessage } } };
            string message = JsonConvert.SerializeObject(cmc);
            File.WriteAllText(sendbackmessagefile, message);
        }

        public static string MintManually(EasynftprojectsContext db, IConnectionMultiplexer redis, Nftproject project, MintManuallyClass mmc,
             bool mainnet, long mintingcosts, string mintingcostsaddress,
            out BuildTransactionClass buildtransaction)
        {
            buildtransaction = new BuildTransactionClass();

            string guid = GlobalFunctions.GetGuid();

            string metadatafile = $"{GeneralConfigurationClass.TempFilePath}metadata{guid}.json";
            string policyscriptfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.script";
            string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string paymentskeyfile = $"{GeneralConfigurationClass.TempFilePath}payment{guid}.skey";
            string policyskeyfile = $"{GeneralConfigurationClass.TempFilePath}policy{guid}.skey";


            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";


            var utxopaymentaddress = ConsoleCommand.GetNewUtxo(mmc.SenderAddress);
            utxopaymentaddress = FilterAllTxInWithTokens(utxopaymentaddress);


            /* var test1 = GetNewUtxo(mmc.SenderAddress, Dataproviders.Blockfrost);
              var test2 = GetNewUtxo(mmc.SenderAddress, Dataproviders.Maestro);
              var test3 = GetNewUtxo(mmc.SenderAddress, Dataproviders.Koios);
              var test4 = GetNewUtxo(mmc.SenderAddress, Dataproviders.Cli);

              buildtransaction.LogFile+="Blockfrost: "+ JsonConvert.SerializeObject(test1, Formatting.Indented)+Environment.NewLine;
              buildtransaction.LogFile+="Maestro: "+ JsonConvert.SerializeObject(test2, Formatting.Indented)+Environment.NewLine;
              buildtransaction.LogFile+="Koios: "+ JsonConvert.SerializeObject(test3, Formatting.Indented)+Environment.NewLine;
              buildtransaction.LogFile+="CLI: "+ JsonConvert.SerializeObject(test4, Formatting.Indented)+Environment.NewLine;

              */

            if (utxopaymentaddress == null || utxopaymentaddress.TxIn == null)
                return "UTXO can not be determined";

            if (utxopaymentaddress.LovelaceSummary < 3500000)
            {
                return "Less than 3.5 ADA on Token Wallet available";
            }



            File.WriteAllText(policyscriptfile, project.Policyscript);
            File.WriteAllText(metadatafile, mmc.Metadata);

            // Create the Sign Keys for Signind and to calculate the witnesses


            File.WriteAllText(paymentskeyfile, mmc.SenderSKey);

            int countwitness = 1;
            List<string> signfiles = new List<string>();
            signfiles.Add(paymentskeyfile);

            countwitness = signfiles.Count();

            string polskey =
                Encryption.DecryptString(project.Policyskey, project.Password);
            File.WriteAllText(policyskeyfile, polskey);
            countwitness++;

            // END -Create the Sign Keys for Signind and to calculate the witnesses

            long fees;
            bool b = CalculateFees(redis,
                utxopaymentaddress,
                mmc.ReceiverAddress,
                mmc.SenderAddress, // Rest of ADA here
                mintingcostsaddress,
                mmc.PolicyId,
                mmc.Tokenname,
                mmc.Prefix,
                metadatafile,
                policyscriptfile,
                matxrawfile,
                guid,
                countwitness,
                out fees,
                mainnet,
                ref buildtransaction);

            if (!b || fees <= 0)
                return "Error while calculating the costs";

            long ttl = (long)q.Slot + 6000;

            GlobalFunctions.DeleteFile(matxrawfile);

            b = CreateTransaction(
                mmc.SenderAddress,
                utxopaymentaddress,
                mmc.ReceiverAddress,
                mmc.SenderAddress, // Rest of ADA here
                mintingcostsaddress,
                mmc.PolicyId,
                mmc.Tokenname,
                mmc.Prefix,
                metadatafile,
                policyscriptfile,
                matxrawfile,
                mintingcosts,
                2000000,
                fees,
                ttl,
                0,
                ref buildtransaction);
            if (!b)
                return "Error while creating the transaction";

            var ok = SignAndSubmit(redis, signfiles.ToArray(), policyskeyfile, matxrawfile, matxsignedfile, mainnet, true,
                ref buildtransaction);


            GlobalFunctions.DeleteFile(policyscriptfile);
            GlobalFunctions.DeleteFile(metadatafile);
            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(policyskeyfile);

            foreach (var a in signfiles)
                GlobalFunctions.DeleteFile(a);
            return ok;
        }



        public static void GenerateProtocolParamsFile(string protocolParamsFile, IConnectionMultiplexer redis,
            bool mainnet, out string errormessage)
        {
            BuildTransactionClass bt = new BuildTransactionClass();
            SaveProtocolParamsFile(redis, protocolParamsFile, mainnet, ref bt);
            errormessage = bt.ErrorMessage;
        }

        public static string GetPkh(string decryptString)
        {
            string t = GlobalFunctions.GetGuid();
            string vkeyfile = $"{GeneralConfigurationClass.TempFilePath}{t}.vkey";
            File.WriteAllText(vkeyfile, decryptString);

            string command = CliCommandExtensions.GetAddressKeyHash()
                .GetPaymentVerifictionFile(vkeyfile);

            string s = CardanoCli(command, out string errormessage);
            GlobalFunctions.DeleteFile(vkeyfile);
            if (!string.IsNullOrEmpty(errormessage))
                return "";
            return s.Replace("\n", "").Replace("\r", "");
        }


        public static string SignTx(string matxrawfile, string skey)
        {
            BuildTransactionClass bt = new BuildTransactionClass();
            string guid = GlobalFunctions.GetGuid();
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string signfile = $"{GeneralConfigurationClass.TempFilePath}sign{guid}.key";
            File.WriteAllText(signfile, skey);
            string res = "";
            SignTransaction(new string[] { }, signfile, matxrawfile, matxsignedfile, GlobalFunctions.IsMainnet(),
                ref bt);
            if (File.Exists(matxsignedfile))
            {
                res = File.ReadAllText(matxsignedfile);
            }

            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(signfile);
            return res;
        }

        public static string GetCbor(string rawtx)
        {
            if (string.IsNullOrEmpty(rawtx))
                return "";
            try
            {
                var mrc = JsonConvert.DeserializeObject<MatxRawClass>(rawtx);
                if (mrc == null)
                    return "";
                return mrc.CborHex;
            }
            catch
            {
                return "";
            }
        }

        public static async Task<CreateDecentralPaymentByCslResultClass> CreateDecentralPaymentByCsl(
      CreateMintAndSendParametersClass cmaspc, EasynftprojectsContext db, IConnectionMultiplexer redis,
      bool mainnet)
        {
            CreateDecentralPaymentByCslResultClass result = new CreateDecentralPaymentByCslResultClass();
            var cctc = CreateCslTransactionClass(cmaspc, db, redis, mainnet);


            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), GeneralConfigurationClass.CslBuildCbor);
            request.Headers.TryAddWithoutValidation("accept", "*/*");

            string jsonString = JsonConvert.SerializeObject(cctc);
            request.Content = new StringContent(jsonString);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            result.CreatedJson = jsonString;

            var response = await httpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {

                result.CslError = response.ReasonPhrase;
                return result;
            }

            string rep = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(rep))
                return result;

            result.CslResult = rep;

            return result;
        }

        private static long GetSendbackToReceiver(EasynftprojectsContext db, IConnectionMultiplexer redis, CreateMintAndSendParametersClass cmaspc)
        {
            return GlobalFunctions.CalculateSendbackToUser(db, redis, cmaspc.MintTokens.Length, cmaspc.project.Id);
        }

        private static void SetTxOutsForPromotions(CreateMintAndSendParametersClass cmaspc, CslCreateTransactionClass cctc)
        {
            if (cmaspc.Promotion != null)
            {
                var the721 = GetThe721Content(cmaspc.Promotion.Metadata);
                if (the721.Contains(cmaspc.Promotion.PolicyId))
                {
                    cctc.Metadata = cctc.Metadata.Concat(new[]
                    {
                        new Metadatum()
                        {
                            Key = "721",
                            Json = GetThe721Content(cmaspc.Promotion.Metadata)
                        }
                    }).ToArray();
                }

                var the20 = GetThe20Content(cmaspc.Promotion.Metadata);
                if (!string.IsNullOrEmpty(the20) && the20.Contains(cmaspc.Promotion.PolicyId))
                {
                    cctc.Metadata = cctc.Metadata.Concat(new[]
                    {
                        new Metadatum()
                        {
                            Key = "20",
                            Json = the20
                        }
                    }).ToArray();
                }

                cctc.Mints = cctc.Mints.Concat(new[]
                {
                    new Token()
                    {
                        TokenName = cmaspc.Promotion.TokennameHex, Count = cmaspc.Promotion.Tokencount,
                        PolicyId = cmaspc.Promotion.PolicyId,
                        PolicyScriptJson = cmaspc.Promotion.PolicyScriptfile
                    }
                }).ToArray();
            }
        }

        private static long SetTxOutsForRoyaltyToken(CreateMintAndSendParametersClass cmaspc, CslCreateTransactionClass cctc,
            long costs)
        {
            if (!string.IsNullOrEmpty(cmaspc.Createroyaltytokenaddress) && !string.IsNullOrEmpty(cmaspc.Burningaddress))
            {
                cctc.Metadata = cctc.Metadata.Concat(new[]
                {
                    new CardanoSerialisationLibClasses.Metadatum()
                    {
                        Key = "777",
                        Json = GetThe777Content(ConsoleCommand.CreateRoyaltyTokenJson(cmaspc.Createroyaltytokenaddress,
                            cmaspc.Createroyaltytokenpercentage ?? 0))
                    }
                }).ToArray();

                cctc.Mints = cctc.Mints.Concat(new[]
                {
                    new Token()
                    {
                        TokenName = "", Count = 1, PolicyId = cmaspc.project.Policyid,
                        PolicyScriptJson = cmaspc.project.Policyscript
                    }
                }).ToArray();

                cctc.TxOuts = cctc.TxOuts.GetTxOuts(cmaspc.Burningaddress, 2000000,
                    new[]
                    {
                        new Token()
                        {
                            Count = 1, PolicyId = cmaspc.project.Policyid, TokenName = "",
                            PolicyScriptJson = cmaspc.project.Policyscript
                        }
                    }, ref costs);
            }

            return costs;
        }


        public static async Task<CreateDecentralPaymentByCslResultClass> CreateDecentralPaymentCip68(
           CreateMintAndSendParametersClass cmaspc, EasynftprojectsContext db, IConnectionMultiplexer redis,
            bool mainnet)
        {
            CslCreateTransactionClass cctc = new()
            {
                Ttl = cmaspc.ttl,
                Fees = cmaspc.Fees,
                IncludeMetadataHashOnly = cmaspc.IncludeMetadataHashOnly,
                ChangeAddressBech32 = cmaspc.BuyerChangeAddress,
                TxIns = GetTxInHashes(cmaspc.utxofinal),
                Mints = cmaspc.MintTokens,
                MetadataString = cmaspc.MetadataResult,
                ReferenceAddress = cmaspc.ReferenceAddress
            };

            long costs = 0;

            // Additional Payout wallets
            if (cmaspc.BuyerHasToPayInLovelace > cmaspc.Mintingcosts)
                cctc.TxOuts = cctc.TxOuts.GetTxOuts(cmaspc.AdditionalPayouts, cmaspc.BuyerHasToPayInLovelace,
                    cmaspc.selectedreservations.Count, ref costs);

            // Minting costs for NFT-Maker
            cctc.TxOuts = cctc.TxOuts.GetTxOuts(cmaspc.MintingcostsAddress, cmaspc.Mintingcosts, null, ref costs);

            Token[] additionaltokens = GetSellerAdditionalTokens(cmaspc.utxofinal, cmaspc.AdditionalPriceInTokens, out TxInAddressesClass[] newUtxoBuyer);



            // Minting Tokens and other tokens from the buyers txin 
            if (string.IsNullOrEmpty(cmaspc.OptionalReceiverAddress) ||
                cmaspc.OptionalReceiverAddress == cmaspc.BuyerChangeAddress)
            {
                cctc.TxOuts = cctc.TxOuts.GetTxOuts(newUtxoBuyer, cmaspc.BuyerChangeAddress, null, null,
                    cmaspc.MintTokens, cmaspc.Fees ?? 0, cmaspc.Promotion, cmaspc.BuyerHasToPayInLovelace);
            }
            else
            {
                long minutxo = GetSendbackToReceiver(db, redis, cmaspc);
                costs += minutxo;
                cctc.TxOuts = cctc.TxOuts.GetTxOuts(cmaspc.OptionalReceiverAddress, cmaspc.MintTokens, minutxo);
                cctc.TxOuts = cctc.TxOuts.GetTxOuts(newUtxoBuyer, cmaspc.BuyerChangeAddress, null, null,
                    null, cmaspc.Fees ?? 0, cmaspc.Promotion, cmaspc.BuyerHasToPayInLovelace);
            }

            /*
            foreach(var mt in cmaspc.MintTokens.OrEmptyIfNull())
            {
                foreach(var txout in cctc.TxOuts.OrEmptyIfNull())
                {
                    foreach(var token in txout.Tokens.OrEmptyIfNull())
                    {
                        if (mt.PolicyIdAndTokenname==token.PolicyIdAndTokenname)
                        {
                            string metadata = CreateMetadataCip68(token.id,cmaspc.ReferenceAddress, true);
                            token.Datum = metadata;
                        }
                    }
                }
            }
            */



            // The seller
            if (cmaspc.BuyerHasToPayInLovelace > cmaspc.Mintingcosts)
                cctc.TxOuts = cctc.TxOuts.GetTxOuts(cmaspc.SellerAddress, cmaspc.BuyerHasToPayInLovelace - costs, additionaltokens, ref costs);
            cctc.TxOuts.Last().ReduceHereTheMintingcosts = true;

            return CreateCslTransactionCip68(cctc, redis, mainnet);
        }

        private static CreateDecentralPaymentByCslResultClass CreateCslTransactionCip68(CslCreateTransactionClass cctc,
            IConnectionMultiplexer redis, bool mainnet)
        {
            string guid = GlobalFunctions.GetGuid();
            string filename = $"{GeneralConfigurationClass.TempFilePath}Outfile_{guid}.cbor";

            string command = CliCommandExtensions.GetTransactionBuildRawWithLatestEra()
                .GetTxIn(cctc)
                .GetFees(cctc)
                .GetTxOutCip68(cctc.TxOuts, cctc.ReferenceAddress, redis)
                .GetMintCip68(cctc.Mints)
                .GetMintingScriptFile(cctc.TxOuts)
                .GetTTL(cctc.Ttl)
               // .GetCddlFormat()
                .GetOutFile(filename);

            CreateDecentralPaymentByCslResultClass res = new CreateDecentralPaymentByCslResultClass();
            res.CreatedJson = command;

            string log = CardanoCli(command, out var errormessage);
            if (!string.IsNullOrEmpty(errormessage))
            {
                res.CslError = errormessage;
                res.CslResult = "";
                return res;
            }

            if (!string.IsNullOrEmpty(log))
            {
                res.CslError = log;
                res.CslResult = "";
                return res;
            }

            res.CslResult = File.ReadAllText(filename);
            return res;
        }

        /// <summary>
        /// This fuctions returns the additional payment tokens for the seller and returns the final buyer utxo over the out variable. The utxoBuyerIn must contain all addresses and all txins from the buyers wallet
        /// </summary>
        /// <param name="utxoBuyerIn"></param>
        /// <param name="AdditionalPriceInTokens"></param>
        /// <param name="utxoBuyerOut"></param>
        /// <returns></returns>
        private static Token[] GetSellerAdditionalTokens(TxInAddressesClass[] utxoBuyerIn, IEnumerable<PreparedpaymenttransactionsTokenprice> AdditionalPriceInTokens, out TxInAddressesClass[] utxoBuyerOut)
        {
            string json = JsonConvert.SerializeObject(utxoBuyerIn);
            utxoBuyerOut = JsonConvert.DeserializeObject<TxInAddressesClass[]>(json);

            if (AdditionalPriceInTokens == null || !AdditionalPriceInTokens.Any())
                return null;

            List<Token> tokens = new List<Token>();
            long totalneededtokens = AdditionalPriceInTokens.Sum(x => x.Tokencount);

            foreach (var priceInToken in AdditionalPriceInTokens)
            {
                foreach (var addressesClass in utxoBuyerIn)
                {
                    foreach (var txInClass in addressesClass.TxIn)
                    {
                        var u = txInClass.Tokens.FirstOrDefault(x =>
                            x.PolicyId == priceInToken.Policyid && (string.IsNullOrEmpty(priceInToken.Assetname) || x.TokennameHex.ToLower() == GlobalFunctions.ToHexString(priceInToken.Assetname)));

                        if (u != null)
                        {
                            if (u.Quantity > priceInToken.Tokencount)
                            {
                                tokens.Add(new Token() { TokenName = u.TokennameHex.ToLower(), PolicyId = u.PolicyId, Count = priceInToken.Totalcount });

                                var u1 = utxoBuyerOut.First(x => x.Address == addressesClass.Address).TxIn
                                    .First(x => x.TxHashId == txInClass.TxHashId).Tokens
                                    .First(x => x.TokenHex().ToLower() == u.TokenHex().ToLower());
                                u1.ChangeQuantity(-priceInToken.Totalcount);

                                totalneededtokens -= priceInToken.Totalcount;
                            }
                            else
                            {
                                tokens.Add(new Token() { TokenName = u.TokennameHex.ToLower(), PolicyId = u.PolicyId, Count = u.Quantity });
                                totalneededtokens -= u.Quantity;

                                utxoBuyerOut.First(x => x.Address == addressesClass.Address).TxIn
                                    .First(x => x.TxHashId == txInClass.TxHashId).Tokens.Remove(u);
                            }
                        }

                        if (totalneededtokens == 0)
                            break;
                    }

                    if (totalneededtokens == 0)
                        break;
                }

                if (totalneededtokens == 0)
                    break;
            }


            return tokens.ToArray();
        }


        private static string GetThe777Content(string createRoyaltyTokenJson)
        {
            if (string.IsNullOrEmpty(createRoyaltyTokenJson))
                return null;
            var json = JsonConvert.DeserializeObject<The777Class>(createRoyaltyTokenJson);
            if (json == null)
                return null;

            var json1 = JsonConvert.SerializeObject(json.The777);
            return string.IsNullOrEmpty(json1) ? null : json1;
        }
        public static string GetThe721Content(string createRoyaltyTokenJson)
        {
            if (string.IsNullOrEmpty(createRoyaltyTokenJson))
                return null;
            var json = JsonConvert.DeserializeObject<The721Class>(createRoyaltyTokenJson);
            if (json == null)
                return null;

            var json1 = JsonConvert.SerializeObject(json.The721);
            return string.IsNullOrEmpty(json1) ? null : json1;
        }
        public static string GetThe20Content(string createRoyaltyTokenJson)
        {
            if (string.IsNullOrEmpty(createRoyaltyTokenJson))
                return null;
            try
            {
                var json = JsonConvert.DeserializeObject<The20Class>(createRoyaltyTokenJson);
                if (json == null)
                    return null;

                var json1 = JsonConvert.SerializeObject(json.The20);
                return string.IsNullOrEmpty(json1) ? null : json1;
            }
            catch
            {
                return null;
            }
        }
        public static string CreateRoyaltyTokenJson(string createroyaltytokenaddress,
            double createroyaltytokenpercent)
        {
            string meta1 = "{\r\n\t\"777\": {\r\n\t\t\"rate\": \"{rate}\",\r\n\t\t\"addr\": \"{addr}\"\r\n\t}\r\n}";
            string meta2 =
                "{\r\n\t\"777\": {\r\n\t\t\"rate\": \"{rate}\",\r\n\t\t\"addr\": [\r\n\t\t\t\"{addr1}\",\r\n\t\t\t\"{addr2}\"\r\n\t\t]\r\n\t}\r\n}";
            string meta;

            if (createroyaltytokenaddress.Length > 64)
            {
                meta = meta2;
                meta = meta.Replace("{addr1}", createroyaltytokenaddress.Substring(0, 64));
                meta = meta.Replace("{addr2}", createroyaltytokenaddress.Substring(64));
            }
            else
            {
                meta = meta1;
                meta = meta.Replace("{addr}", createroyaltytokenaddress);
            }

            meta = meta.Replace("{rate}", ((createroyaltytokenpercent / 100).ToString().Replace(",", ".")));
            meta = meta.Replace("{pct}", ((createroyaltytokenpercent / 100).ToString().Replace(",", ".")));

            return meta;
        }


        public static string CreateCollectionTokenJson(string royaltytokenaddress, double royaltytokenpercent, string policyid, string ipfshashidentity, string identityprovider)
        {
            string collectiontoken = "{*royaltytoken**comma**identitytoken*}";

            if (string.IsNullOrEmpty(royaltytokenaddress))
            {
                collectiontoken = collectiontoken.Replace("*royaltytoken*", "");
                collectiontoken = collectiontoken.Replace("*comma*", "");
            }
            else
            {
                string s = CreateRoyaltyTokenJson(royaltytokenaddress, royaltytokenpercent);
                s = s.Substring(1, s.Length - 2); // Remove the first { and last }
                collectiontoken = collectiontoken.Replace("*royaltytoken*", s);
            }

            if (string.IsNullOrEmpty(ipfshashidentity))
            {
                collectiontoken = collectiontoken.Replace("*identitytoken*", "");
                collectiontoken = collectiontoken.Replace("*comma*", "");
            }
            else
            {
                string s = CreateIdentityTokenJson(policyid, ipfshashidentity, identityprovider);
                s = s.Substring(1, s.Length - 2); // Remove the first { and last }
                collectiontoken = collectiontoken.Replace("*identitytoken*", s);
            }
            collectiontoken = collectiontoken.Replace("*comma*", ",");

            return JsonFormatter.FormatJson(collectiontoken);
        }

        private static string CreateIdentityTokenJson(string policyid, string ipfshashidentity, string identityprovider)
        {
            string identity = "{\"725\": {\"" + policyid +
                              "\": {\"@context\": \"https://github.com/IAMXID/did-method-iamx\"," +
                              "\"type\": \"Ed25519VerificationKey2020\",\"files\": [{\"name\":\"CIP-0066_NMKR_" +
                              identityprovider + "\", " +
                              " \"mediaType\": \"application/ld+json\",\"src\": \"ipfs://" + ipfshashidentity + "\"" +
                              "}] }, " +
                              " \"version\": \"1.0\"}}";

            return identity;
        }


        public static string GetCborJson(string cbor)
        {
            // Already in envelope
            if (cbor.Contains("Unwitnessed"))
                return cbor;

            var cborx = JsonConvert.DeserializeObject<Cbor>(cbor);
            var matxraw = new MatxRawClass() { CborHex = cborx.cbor, Type = "Unwitnessed Tx ConwayEra", Description = "Ledger Cddl Format" };
            var matxrawText = JsonConvert.SerializeObject(matxraw);
            return matxrawText;
        }
        private static TxOut[] GetTxOuts(this TxOut[] txouts, string receiveraddress, Token[] minttokens, long lovelace)
        {
            List<TxOut> txouts1 = new List<TxOut>();
            List<Token> sendtokens = new List<Token>();
            if (txouts != null)
                txouts1.AddRange(txouts);

            foreach (var mt in minttokens.OrEmptyIfNull())
            {
                sendtokens.AddSendToken(mt);
            }

            txouts1.Add(new TxOut()
            { Lovelace = lovelace, AddressBech32 = receiveraddress, Tokens = sendtokens.ToArray() });

            return txouts1.ToArray();
        }


        private static TxOut[] GetTxOuts(this TxOut[] txouts, TxInAddressesClass[] utxo, string receiveraddress, long? donotincludeQuantity, string donotincludeTokenname, Token[] minttokens, long fee, PromotionClass promotion, long costs)
        {
            long quant = 0;

            List<TxOut> txouts1 = new List<TxOut>();
            List<Token> sendtokens = new List<Token>();
            if (txouts != null)
                txouts1.AddRange(txouts);

            if (utxo == null)
                return txouts1.ToArray();

            foreach (var adr in utxo.OrEmptyIfNull())
            {
                foreach (var txInClass in adr.TxIn.OrEmptyIfNull())
                {
                    foreach (var token in txInClass.Tokens.OrEmptyIfNull())
                    {
                        long quant1;
                        if (donotincludeQuantity == null || string.IsNullOrEmpty(donotincludeTokenname))
                        {
                            quant1 = token.Quantity;
                        }
                        else
                        {

                            if (donotincludeTokenname == $"{token.PolicyId}.{token.TokennameHex}")
                            {
                                if (donotincludeQuantity - quant >= token.Quantity)
                                {
                                    quant += token.Quantity;
                                    continue;
                                }

                                quant1 = token.Quantity - ((long)donotincludeQuantity - quant);
                            }
                            else quant1 = token.Quantity;
                        }

                        sendtokens.Add(new Token { Count = quant1, PolicyId = token.PolicyId, TokenName = token.TokennameHex });
                    }
                }
            }

            foreach (var mt in minttokens.OrEmptyIfNull())
            {
                sendtokens.AddSendToken(mt);
            }

            if (promotion != null)
            {
                var promotiontoken = new Token
                {
                    Count = promotion.Tokencount,
                    PolicyId = promotion.PolicyId,
                    TokenName = promotion.TokennameHex,
                    PolicyScriptJson = promotion.PolicyScriptfile
                };
                sendtokens.AddSendToken(promotiontoken);
            }



            BuildTransactionClass bt = new BuildTransactionClass();
            GetTxInHashes(utxo, out var com1, out var txincount, out var lovelacesummery, ref bt);

            long rest = lovelacesummery - fee - costs;
            txouts1.Add(new TxOut()
            { Lovelace = rest, AddressBech32 = receiveraddress, Tokens = sendtokens.ToArray() });

            return txouts1.ToArray();
        }

        private static void AddSendToken(this List<Token> sendtokens, Token mt)
        {
            var f = sendtokens.FirstOrDefault(x => x.PolicyId == mt.PolicyId && x.TokenName == mt.TokenName.ToLower());
            if (f != null)
            {
                f.Count += mt.Count;
                f.PolicyScriptJson = mt.PolicyScriptJson;
            }
            else
            {
                sendtokens.Add(mt);
            }
        }

        private static TxOut[] GetTxOuts(this TxOut[] txouts, Nftprojectsadditionalpayout[] additionalpayouts, long hastopay, long nftcount, ref long costs)
        {
            if (additionalpayouts == null)
                return txouts;
            if (!additionalpayouts.Any())
                return txouts;

            List<TxOut> txouts1 = new List<TxOut>();

            if (txouts != null)
                txouts1 = txouts.ToList();

            foreach (var nftprojectsadditionalpayout in additionalpayouts)
            {
                long addvalue = ConsoleCommand.GetAdditionalPayoutwalletsValue(nftprojectsadditionalpayout, hastopay, nftcount);
                if (addvalue > 0)
                {
                    costs += addvalue;
                    txouts1.Add(new TxOut() { Lovelace = addvalue, Tokens = null, AddressBech32 = nftprojectsadditionalpayout.Wallet.Walletaddress });
                }
            }

            return txouts1.ToArray();
        }

        private static TxOut[] GetTxOuts(this TxOut[] txouts, string address, long lovelace, Token[] tokens, ref long costs)
        {
            if (lovelace <= 0)
                return txouts;

            List<TxOut> txouts1 = new List<TxOut>();

            if (txouts != null)
                txouts1 = txouts.ToList();

            costs += lovelace;
            txouts1.Add(new TxOut() { Lovelace = lovelace, Tokens = tokens, AddressBech32 = address });

            return txouts1.ToArray();
        }

        public static TxIn[] GetTxInHashes(TxInAddressesClass[] utxopaymentaddress)
        {
            if (utxopaymentaddress == null)
                return null;

            List<TxIn> txins = new List<TxIn>();

            foreach (var addressesClass in utxopaymentaddress)
            {
                if (addressesClass.TxIn == null)
                    continue;

                foreach (var txInClass in addressesClass.TxIn)
                {
                    txins.Add(new TxIn() { AddressBech32 = addressesClass.Address, Lovelace = txInClass.Lovelace, TransactionHashAndIndex = txInClass.TxHashId, Tokens = GetTxInTokens(txInClass.Tokens.ToArray()) });
                }
            }

            return txins.ToArray();
        }

        private static Token[] GetTxInTokens(TxInTokensClass[] tokens)
        {
            if (tokens == null)
                return null;

            var t = (from a in tokens
                     select new Token() { Count = a.Quantity, PolicyId = a.PolicyId, TokenName = a.TokennameHex }).ToArray();
            return t;
        }


        public static ProtocolParameters GetProtocolParameters(IConnectionMultiplexer redis, bool mainnet)
        {
            string rediskey = "protocolParameters_" + GeneralConfigurationClass.EnvironmentName;
            try
            {
                var cachedData = RedisFunctions.GetData<ProtocolParameters>(redis, rediskey);
                if (cachedData is { MinFeeB: { } })
                    return cachedData;

                string filename =
                    $"{GeneralConfigurationClass.TempFilePath}protocolParameters_{GlobalFunctions.GetGuid()}.json";

                var command = CliCommandExtensions.GetQueryProtocolParameter()
                    .GetNetwork(mainnet)
                    .GetOutFile(filename);

                CardanoCli(command, out var errormessage);
                if (!string.IsNullOrEmpty(errormessage))
                {
                    GlobalFunctions.LogException(null, "GetProtocolParameters: " + errormessage, command);
                    return null;
                }

                var consoleProtocolParameters =
                    JsonConvert.DeserializeObject<ConsoleProtocolParametersClass>(File.ReadAllText(filename));

                ProtocolParameters pp = new ProtocolParameters()
                {
                    CoinsPerUtxoWord = consoleProtocolParameters.UtxoCostPerWord,
                    CoinsPerUtxoByte = consoleProtocolParameters.UtxoCostPerByte,
                    KeyDeposit = consoleProtocolParameters.StakeAddressDeposit ?? 0,
                    PoolDeposit = consoleProtocolParameters.StakePoolDeposit ?? 0,
                    MaxTxSize = consoleProtocolParameters.MaxTxSize,
                    MaxValueSize = consoleProtocolParameters.MaxValueSize,
                    MinFeeA = consoleProtocolParameters.TxFeePerByte,
                    MinFeeB = consoleProtocolParameters.TxFeeFixed,
                    PriceMemory = consoleProtocolParameters.ExecutionUnitPrices.PriceMemory,
                    PriceStep = consoleProtocolParameters.ExecutionUnitPrices.PriceSteps,
                    MaxTxExMem = consoleProtocolParameters.MaxTxExecutionUnits.Memory,
                    MaxTxExSteps = consoleProtocolParameters.MaxTxExecutionUnits.Steps
                };
                RedisFunctions.SetData(redis, rediskey, pp, 300);
                return pp;
            }
            catch (Exception e)
            {
                GlobalFunctions.LogException(null, "GetProtocolParameters: " + e.Message + " - Sending out default parameters",
                    e.InnerException?.Message);

                return new ProtocolParameters()
                {
                    CoinsPerUtxoByte = 4310,
                    CoinsPerUtxoWord = 4310,
                    KeyDeposit = 2000000,
                    MaxTxExMem = 14000000,
                    MaxTxExSteps = 10000000000,
                    MaxTxSize = 16384,
                    MaxValueSize = 5000,
                    MinFeeA = 44,
                    MinFeeB = 155381,
                    PoolDeposit = 500000000,
                    PriceStep = 7.21E-05,
                    PriceMemory = 0.0577,
                };
            }
        }


        public static string FinishLegacyAuction(EasynftprojectsContext db, IConnectionMultiplexer redis, Legacyauction legacyauction, List<TxOutClass> txouts, string selleraddress, TxInClass utxo, string sendbackmessage, bool mainnet, ref BuildTransactionClass buildtransaction)
        {
            string guid = GlobalFunctions.GetGuid();

            string sendbackmessagefile = $"{GeneralConfigurationClass.TempFilePath}metadata{guid}.json";
            string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string signingkeyfile = $"{GeneralConfigurationClass.TempFilePath}signing{guid}.skey";

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";
            long ttl = (long)q.Slot + 50000;

            if (!string.IsNullOrEmpty(sendbackmessage))
            {
                CreateSendbackMessageMetadata(sendbackmessagefile, sendbackmessage);
            }
            else sendbackmessagefile = "";

            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra()
                .GetNetwork(mainnet)
                .GetTxIn(new[] { utxo }, ref buildtransaction)
                .GetChangeAddress(selleraddress)
                .GetTxOut(txouts.ToArray()) // Buyer, Marketplace and Royaltie
                .GetMetadataJsonFile(sendbackmessagefile)
                .GetOutFile(matxrawfile)
                .GetTTL(ttl)
                .GetWitnessOverride(2);

            File.WriteAllText(signingkeyfile, Encryption.DecryptString(legacyauction.Skey, legacyauction.Salt + GeneralConfigurationClass.Masterpassword));
            var res = SendGetFeesSignAndSubmit(db, redis, command, signingkeyfile, matxrawfile, matxsignedfile, mainnet, ref buildtransaction);

            GlobalFunctions.DeleteFile(sendbackmessagefile);
            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(signingkeyfile);
            return res;
        }

        public static string FinishLegacyDirectsale(EasynftprojectsContext db, IConnectionMultiplexer redis, Legacydirectsale direktsaleaddress, List<TxOutClass> txouts, string selleraddress, TxInClass utxo, string sendbackmessage, bool mainnet, ref BuildTransactionClass buildtransaction)
        {
            string guid = GlobalFunctions.GetGuid();

            string sendbackmessagefile = $"{GeneralConfigurationClass.TempFilePath}metadata{guid}.json";
            string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string signingkeyfile = $"{GeneralConfigurationClass.TempFilePath}signing{guid}.skey";

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";
            long ttl = (long)q.Slot + 50000;

            if (!string.IsNullOrEmpty(sendbackmessage))
            {
                CreateSendbackMessageMetadata(sendbackmessagefile, sendbackmessage);
            }
            else sendbackmessagefile = "";

            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra()
                .GetNetwork(mainnet)
                .GetTxIn(new[] { utxo }, ref buildtransaction)
                .GetChangeAddress(selleraddress)
                .GetTxOut(txouts.ToArray()) // Buyer, Marketplace and Royaltie
                .GetMetadataJsonFile(sendbackmessagefile)
                .GetOutFile(matxrawfile)
                .GetTTL(ttl)
                .GetWitnessOverride(2);

            File.WriteAllText(signingkeyfile, Encryption.DecryptString(direktsaleaddress.Skey, direktsaleaddress.Salt + GeneralConfigurationClass.Masterpassword));
            var res = SendGetFeesSignAndSubmit(db, redis, command, signingkeyfile, matxrawfile, matxsignedfile, mainnet, ref buildtransaction);

            GlobalFunctions.DeleteFile(sendbackmessagefile);
            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(signingkeyfile);
            return res;
        }

        public static async Task<bool> CancelSmartLegacyDirectSaleAsync(EasynftprojectsContext db, IConnectionMultiplexer redis, Legacydirectsale directsales, bool isMainnet)
        {
            BuildTransactionClass buildtransaction = new BuildTransactionClass();
            var utxo = await GetNewUtxoAsync(directsales.Address);
            if (utxo == null)
                return false;
            if (utxo.TxIn == null || !utxo.TxIn.Any())
                return false;

            var senderaddress = await GetSenderAsync(directsales.Locknftstxinhashid);

            var s = CardanoSharpFunctions.SendAllAdaAndTokens(db, redis, directsales.Address, directsales.Skey, directsales.Vkey,
                directsales.Salt + GeneralConfigurationClass.Masterpassword, senderaddress,
                isMainnet, ref buildtransaction, directsales.Locknftstxinhashid, 1, 0, "");
            if (s == "OK")
            {
                return true;
            }

            return false;
        }

        public static bool SignWitness(string txbodyfilename, string witnessfile, string policyskey, ref BuildTransactionClass buildtransaction)
        {
            string command = CliCommandExtensions.GetTransactionWitnessLatestEra()
                .GetSigningKeyFile(policyskey)
                .GetTxBodyFile(txbodyfilename)
                .GetOutFile(witnessfile);

            buildtransaction.Command = command;
            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                buildtransaction.ErrorMessage = errormessage;
                return false;
            }

            return true;
        }



        public static bool AssembleFiles(string txbodyfilename, string finalassemblesfile, string[] signfiles, ref BuildTransactionClass buildtransaction)
        {
            string command = CliCommandExtensions.GetTransactionAssemble()
                .GetTxBodyFile(txbodyfilename)
                .GetWitnessFile(signfiles)
                .GetOutFile(finalassemblesfile);

            buildtransaction.Command = command;
            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                buildtransaction.ErrorMessage = errormessage;
                return false;
            }

            return true;
        }



        public static string SendAllAdaRoyalitySplit(EasynftprojectsContext db, IConnectionMultiplexer redis, TxInAddressesClass utxo, Splitroyaltyaddress address, bool mainnet, int maxTxIn, out List<TxOutClass> txouts, ref BuildTransactionClass buildtransaction)
        {
            string guid = GlobalFunctions.GetGuid();
            txouts = new List<TxOutClass>();

            string matxrawfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.raw";
            string matxsignedfile = $"{GeneralConfigurationClass.TempFilePath}matx{guid}.signed";
            string signingkeyfile = $"{GeneralConfigurationClass.TempFilePath}signing{guid}.skey";

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return "Error while getting Query Tip";
            long ttl = (long)q.Slot + 50000;

            string selleraddress = address.Splitroyaltyaddressessplits.FirstOrDefault(x => x.IsMainReceiver == true)!.Address;
            utxo = CardanoSharpFunctions.GetMaxTx(utxo, maxTxIn);

            foreach (var splitroyaltyaddressessplit in address.Splitroyaltyaddressessplits)
            {
                if (splitroyaltyaddressessplit.IsMainReceiver == true)
                    continue;

                if (splitroyaltyaddressessplit.State != "active")
                    continue;

                if (splitroyaltyaddressessplit.Activefrom != null && splitroyaltyaddressessplit.Activefrom > DateTime.Now)
                    continue;

                if (splitroyaltyaddressessplit.Activeto != null && splitroyaltyaddressessplit.Activeto < DateTime.Now)
                    continue;

                long lovelace = Math.Max(1000000, utxo.LovelaceSummary / 100 * (splitroyaltyaddressessplit.Percentage / 100));
                txouts.Add(new TxOutClass() { Amount = lovelace, ReceiverAddress = splitroyaltyaddressessplit.Address, Percentage = splitroyaltyaddressessplit.Percentage });
            }


            // Commision for NMKR
            var txouts1 = new List<TxOutClass>();

            /*  long lovelacenmkr = address.Customer.Defaultsettings.Mintingcosts;
              if (lovelacenmkr!=0)
                 txouts1.Add(new TxOutClass() { Lovelace = lovelacenmkr, ReceiverAddress = address.Customer.Defaultsettings.Mintingaddress });
            */

            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra()
                .GetNetwork(mainnet)
                .GetTxIn(new[] { utxo }, ref buildtransaction)
                .GetChangeAddress(selleraddress)
                .GetTxOut(txouts?.ToArray()) // Splits
                .GetTxOut(txouts1?.ToArray()) // NMKR Fees
                .GetOutFile(matxrawfile)
                .GetTTL(ttl)
                .GetWitnessOverride(2);


            File.WriteAllText(signingkeyfile, Encryption.DecryptString(address.Skey, address.Salt));
            var res = SendGetFeesSignAndSubmit(db, redis, command, signingkeyfile, matxrawfile, matxsignedfile, mainnet, ref buildtransaction);

            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(signingkeyfile);
            return res;

        }

        public static string SendGetFeesSignAndSubmit(EasynftprojectsContext db, IConnectionMultiplexer redis, string command, string signingkeyfile, string matxrawfile, string matxsignedfile, bool mainnet, ref BuildTransactionClass buildtransaction)
        {
            buildtransaction.Command = command;
            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return "Error while creating Transaction";
            }

            buildtransaction.Fees = GetEstimatedFees(log);
            if (buildtransaction.Fees == 0)
                return "Error while creating the Transaction";

            var ok = SignAndSubmit(redis, null, signingkeyfile, matxrawfile, matxsignedfile, mainnet, true,
                ref buildtransaction);

            return ok;
        }

        public static async Task<FrankenAddressProtectionClass> GetFrankenAddressProtectionAddress(EasynftprojectsContext db, IConnectionMultiplexer redis, string receiveraddress)
        {
            FrankenAddressProtectionClass f = new FrankenAddressProtectionClass() { Address = receiveraddress, OriginatorAddress = receiveraddress, StakeAddress = "" };
            var stakeaddress = Bech32Engine.GetStakeFromAddress(receiveraddress);
            if (string.IsNullOrEmpty(stakeaddress))
            {
                return f;
            }

            f.StakeAddress = stakeaddress;
            GetAddressesFromStakeClass[] addresses;
            try
            {
                addresses = await BlockfrostFunctions.GetAllAddressesWithThisStakeAddressAsync(redis, stakeaddress);
            }
            catch
            {
                addresses = (await KoiosFunctions.GetAllAddressesWithThisStakeAddressAsync(redis, stakeaddress)).ToGetAddressesFromStakeClass();
            }


            if (addresses != null && addresses.Any())
            {
                var a = await GetFirstNonScriptAddressAsync(db, redis, addresses);
                f.Address = a ?? receiveraddress;
            }
            return f;
        }


        public static async Task<string> GetTransactionIdAsync(string address)
        {
            try
            {
                var res = await BlockfrostFunctions.GetTransactionIdAsync(address);
                if (string.IsNullOrEmpty(res))
                    return await KoiosFunctions.GetTransactionIdAsync(address);
                return res;
            }
            catch
            {
                return await KoiosFunctions.GetTransactionIdAsync(address);
            }

        }

        private static async Task<string> GetFirstNonScriptAddressAsync(EasynftprojectsContext db, IConnectionMultiplexer redis, GetAddressesFromStakeClass[] addresses)
        {
            List<CheckFrankenAddressClass> adr = new List<CheckFrankenAddressClass>();

            foreach (var address in addresses)
            {
                var a = await KoiosFunctions.GetAddressTransactionsCachedAsync(redis, address.Address);


                BlockfrostAddressInformationClass c1;
                try
                {
                    c1 = await BlockfrostFunctions.GetAddressInformationAsync(address.Address);
                }
                catch
                {
                    c1 = await KoiosFunctions.GetAddressInformationAsync(address.Address);
                }


                if (c1 is { Script: false } && a != null)
                    adr.Add(new CheckFrankenAddressClass() { Address = address.Address, AddressTransactions = a });
            }

            try
            {
                if (!adr.Any())
                    return null;

                var z = adr.MinBy(x => (x.AddressTransactions?.BlockTime ?? 0));
                if (z != null)
                    return z.Address;
            }
            catch (Exception e)
            {
                await GlobalFunctions.LogExceptionAsync(db, "GetFirstNonScriptAddress - " + e.Message, addresses.First().Address + Environment.NewLine + JsonConvert.SerializeObject(adr) + Environment.NewLine + e.InnerException?.Message);

            }

            return null;
        }


        public static string CheckMetadataSimple(string metadata, Nft nft)
        {
            var ret = "";
            if (!metadata.Contains("\"721\""))
            {
                ret += "Missing the 721 Section";
            }

            if (!metadata.Contains(nft.Nftproject.Policyid))
            {
                if (!string.IsNullOrEmpty(ret))
                    ret += Environment.NewLine;
                ret += "Missing the policy id - " + nft.Nftproject.Policyid;
            }

            if ((nft.Nftproject.Tokennameprefix + nft.Name).Length > 32)
            {
                if (!string.IsNullOrEmpty(ret))
                    ret += Environment.NewLine;
                ret += "Tokenname is too long";
            }

            if (!metadata.Contains("\"name\":"))
            {
                if (!string.IsNullOrEmpty(ret))
                    ret += Environment.NewLine;
                ret += "Missing the name property";
            }
            if (!metadata.Contains("\"image\":"))
            {
                if (!string.IsNullOrEmpty(ret))
                    ret += Environment.NewLine;
                ret += "Missing the image property";
            }


            var json = JToken.Parse(metadata);
            var fieldsCollector = new JsonFieldsCollector(json);
            var fields = fieldsCollector.GetAllFields();

            foreach (var field in fields)
            {
                if (field.Value.ToString(CultureInfo.InvariantCulture).Length > 64)
                {
                    if (!string.IsNullOrEmpty(ret))
                        ret += Environment.NewLine;
                    ret += "Some fields exceed the max. length of 64 characters";
                    break;
                }
            }

            try
            {
                var z = JsonConvert.DeserializeObject(metadata);
                var z1 = JsonConvert.SerializeObject(z, Formatting.None);
                var s2 = GlobalFunctions.HasSpecialChars(z1);
                if (!string.IsNullOrEmpty(s2))
                {
                    if (!string.IsNullOrEmpty(ret))
                        ret += Environment.NewLine;
                    ret += $"Your metadata contains some special characters. ({s2}). But that doesn't mean the metadata is wrong.";
                }
                if (!z1.Contains("\"" + nft.Nftproject.Tokennameprefix + nft.Name + "\":{"))
                {
                    if (!string.IsNullOrEmpty(ret))
                        ret += Environment.NewLine;
                    ret += "Missing the tokenname -" + nft.Nftproject.Tokennameprefix + nft.Name;
                }
            }
            catch
            {
                if (!string.IsNullOrEmpty(ret))
                    ret += Environment.NewLine;
                ret += "Metadata could not be deserialized";
            }


            return string.IsNullOrEmpty(ret) ? "OK" : ret;
        }


        public static TxInAddressesClass RemoveAdaHandles(TxInAddressesClass utxo, List<Adahandle> adahandles)
        {
            var result = utxo;
            foreach (var adahandle in adahandles)
            {
                foreach (var txins in utxo.TxIn)
                {
                    if (txins.Tokens != null && txins.Tokens.FirstOrDefault(x => x.PolicyId == adahandle.Policyid) != null)
                    {
                        result.TxIn = result.TxIn.RemoveFromArray(txins);
                    }
                }
            }

            return result;
        }

        public static string GetMetadatafileForDatum(string ptsjJson, string filename)
        {
            var datumcbor = CSLServiceFunctions.JsonToPlutusDataCbor(ptsjJson);
            if (string.IsNullOrEmpty(datumcbor.CborHex))
                return "";

            var datumstring = datumcbor.CborHex.Split(64);
            if (datumstring.Length == 0)
                return "";

            var json = "{\"30\": \"5\"";
            int i = 50;
            foreach (var s in datumstring)
            {
                json += $",\"{i}\": \"{s}\"";
                i++;
            }
            json += "}";
            File.WriteAllText(filename, json);
            return filename;
        }

        public static bool CreateCliCommandLockAdaTransactionSmartcontract(IConnectionMultiplexer redis,
            TxInAddressesClass[] utxofinal, string changeaddress, string smartcontractsAddress, long offer,
            string scriptDatumHash, string scriptdatumfile, string protocolParamsFile, string matxrawfile, string metadatafile,
            bool mainnet, ref BuildTransactionClass buildtransaction)
        {
            string command = CliCommandExtensions.GetTransactionBuildWithLatestEra()
                .GetNetwork(mainnet)
                .GetTxIn(utxofinal, ref buildtransaction) // TX-IN from the Seller - with the Token
                .GetTxOutScripthash(smartcontractsAddress, offer)
                .GetTxOutDatumEmbedFile(scriptdatumfile)
                .GetTxOutWithTokensMinutxo(redis, utxofinal, changeaddress, null, null, ref buildtransaction) // The rest with tokens - can not made by changeaddress
                .GetChangeAddress(changeaddress)
                .GetMetadataJsonFile(metadatafile)
                .GetWitnessOverride(6)
                .GetProtocolParamsFile(protocolParamsFile)
                .GetCddlFormat()
                .GetOutFile(matxrawfile);


            buildtransaction.Command = command;

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
            {
                buildtransaction.LogFile += errormessage + Environment.NewLine;
                return false;
            }

            buildtransaction.Fees = GetEstimatedFees(log);
            if (buildtransaction.Fees == 0)
                return false;
            return true;
        }

        public static async Task<long?> GetSlotAsync()
        {
            try
            {
                var res = await BlockfrostFunctions.GetSlotAsync();
                if (res == null)
                    return await KoiosFunctions.GetSlotAsync();
                else
                    return res;
            }
            catch
            {
                return await KoiosFunctions.GetSlotAsync();
            }
        }

        public static Querytip GetQueryTip()
        {
            var res = Task.Run(async () => await GetQueryTipAsync());
            return res.Result;
        }

        public static async Task<Querytip> GetQueryTipAsync()
        {
            var qt = await BlockfrostFunctions.GetQueryTipAsync() ?? await KoiosFunctions.GetQueryTipAsync();
            return qt;
        }

        public static BlockfrostAssetClass? GetAssetFromBlockchain(Nft nft1, Nftproject project, string assetnameinhex = "")
        {
            var res = Task.Run(async () => await GetAssetFromBlockchainAsync(nft1,project, assetnameinhex));
            return res.Result;
        }

        public static async Task<BlockfrostAssetClass?> GetAssetFromBlockchainAsync(Nft nft1, Nftproject project, string assetnameinhex = "")
        {
            BlockfrostAssetClass? as1 = null;
            if (project.Enabledcoins.Contains(Coin.ADA.ToString()) == true)
            {
               as1=await GetAssetFromCardanoBlockchainAsync(project.Policyid, project.Tokennameprefix, nft1.Name);
               if (as1 != null)
               {
                    return as1;
               }
            }

            if (project.Enabledcoins.Contains(Coin.SOL.ToString()) == true && !string.IsNullOrEmpty(nft1.Solanatokenhash))
            {
                as1 = SolanaFunctions.GetAssetFromSolanaBlockchain(project, nft1.Solanatokenhash);
                if (as1 != null)
                {
                    return as1;
                }
            }

            return as1;

        }

        public static async Task<BlockfrostAssetClass?> GetAssetFromCardanoBlockchainAsync(string policyid, string tokennameprefix, string assetname)
        {
            string assetnameInHex = string.IsNullOrEmpty(tokennameprefix) ? assetname.ToHex() : (tokennameprefix + assetname).ToHex();

            BlockfrostAssetClass as1 = null;
            var assetid1 = GlobalFunctions.GetAssetId(policyid, tokennameprefix, assetname, true);
            var assetid2 = GlobalFunctions.GetAssetId(policyid, tokennameprefix, assetname, false);

          /*  if (!string.IsNullOrEmpty(assetnameInHex))
            {
                assetid1 = policyid + assetnameInHex;
                assetid2 = "";
            }*/

            var assetid1cip68 = GlobalFunctions.GetCip68ReferenceToken(assetid1);

            // Check on which Tokenname we have minted the NFT
            as1 = await BlockfrostFunctions.GetAssetInformationAsync(assetid1);
            if (as1 != null)
                return as1;
            // Then check if the Asset is CIP68
            as1 = await BlockfrostFunctions.GetAssetInformationAsync(assetid1cip68);
            if (as1 != null)
                return as1;

            // Check with different asset id (if there are spaces)
            if (!string.IsNullOrEmpty(assetid2) && assetid1 != assetid2)
            {
                var assetid2cip68 = GlobalFunctions.GetCip68ReferenceToken(assetid2);
                as1 = await BlockfrostFunctions.GetAssetInformationAsync(assetid2);
                if (as1 != null)
                    return as1;
                as1 = await BlockfrostFunctions.GetAssetInformationAsync(assetid2cip68);
                if (as1 != null)
                    return as1;
            }

            // Alternative look on Koios - if we run into the limit of blockfrost or if blockfrost is down
      /*      as1 = (await KoiosFunctions.GetAssetInformationAsync(policyid,
                    assetnameInHex))
                .ToBlockfrostAssetClass();
            if (as1 != null)
                return as1;

            // Also look for CIP68
            as1 = (await KoiosFunctions.GetAssetInformationAsync(policyid,
                    "000643b0" + assetnameInHex)).ToBlockfrostAssetClass();*/
            return as1;
        }

        public static string CreateSimpleScriptAddress(PolicyScript ps)
        {
            var guid = Guid.NewGuid().ToString("N");
            var psjson = JsonConvert.SerializeObject(ps);
            string filenamescript = $"{GeneralConfigurationClass.TempFilePath}script_{guid}.json";
            string filenameout = $"{GeneralConfigurationClass.TempFilePath}script_{guid}.addr";

            File.WriteAllText(filenamescript, psjson);

            var command = CliCommandExtensions.GetAddressBuild()
                .GetNetwork(GlobalFunctions.IsMainnet())
                .GetPaymentScriptFile(filenamescript)
                .GetOutFile(filenameout);

            string address = null;
            string log = CardanoCli(command, out var errormessage);
            if (string.IsNullOrEmpty(errormessage))
            {
                if (File.Exists(filenameout))
                {
                    address = File.ReadAllText(filenameout);
                }
            }

            GlobalFunctions.DeleteFile(filenameout);
            GlobalFunctions.DeleteFile(filenamescript);
            return address;
        }

        public static string AssembleTx(string txCbor, string[] signatures, ref BuildTransactionClass bt)
        {
            string guid = GlobalFunctions.GetGuid();
            string finalassemblesfile = $"{GeneralConfigurationClass.TempFilePath}txbody_{guid}.signed";


            // Save TX Body with Envelope
            string txbodyfile = $"{GeneralConfigurationClass.TempFilePath}txbody_{guid}.cbor";
            var matxraw = new MatxRawClass() { CborHex = txCbor, Type = "Unwitnessed Tx BabbageEra", Description = "Ledger Cddl Format" };
            var matxrawText = JsonConvert.SerializeObject(matxraw);
            File.WriteAllText(txbodyfile, matxrawText);


            // Save Witness / Signature Files
            List<string> signaturefiles = new List<string>();
            int i = 0;
            foreach (var s in signatures)
            {
                i++;
                string filename = $"{GeneralConfigurationClass.TempFilePath}txsignature_{guid}_{i}.cbor";
                signaturefiles.Add(filename);
                MatxRawClass mrc = new()
                { CborHex = TrimSignatureFromDappWallets(s), Description = "Key Witness ShelleyEra", Type = "TxWitness BabbageEra" };
                System.IO.File.WriteAllText(filename, JsonConvert.SerializeObject(mrc));
            }

            AssembleFiles(txbodyfile, finalassemblesfile, signaturefiles.ToArray(), ref bt);
            if (!File.Exists(finalassemblesfile))
                return null;

            string txsigned = File.ReadAllText(finalassemblesfile);


            // Clean up
            GlobalFunctions.DeleteFile(finalassemblesfile);
            GlobalFunctions.DeleteFile(txbodyfile);
            foreach (var s in signaturefiles)
            {
                GlobalFunctions.DeleteFile(s);
            }

            return GetCbor(txsigned);
        }

        private static string TrimSignatureFromDappWallets(string s)
        {
            s = s.Substring(s.IndexOf("825820"));
            if (s.Length > 202)
                s = s.Substring(0, 202);
            return s;
        }

        public static string CreateUnvestTransaction(IConnectionMultiplexer redis, Lockedasset lockedAsset, long fee, ref BuildTransactionClass buildtransaction)
        {
            string guid = GlobalFunctions.GetGuid();
            string txbodyfile = $"{GeneralConfigurationClass.TempFilePath}txbody_{guid}.cbor";
            string scriptfile = $"{GeneralConfigurationClass.TempFilePath}script_{guid}.json";
            File.WriteAllText(scriptfile, lockedAsset.Policyscript);

            var q = ConsoleCommand.GetQueryTip();
            if (q == null)
                return null;

            long ttl = (long)q.Slot + 6000;

            string command = CliCommandExtensions.GetTransactionBuildRawWithLatestEra()
                .GetFees(fee)
                .GetTxIn(lockedAsset.Locktxid + "#0")
                .GetTxInScriptFile(scriptfile)
                .GetTxOut(lockedAsset, fee)
                .GetInvalidBefore(lockedAsset.Lockslot)
                .GetOutFile(txbodyfile)
                .GetTTL(ttl)
                .GetCddlFormat();

            buildtransaction.LogFile += command + Environment.NewLine;
            string log = CardanoCli(command, out var errormessage);
            buildtransaction.LogFile += log + Environment.NewLine;
            if (!string.IsNullOrEmpty(errormessage))
                buildtransaction.LogFile += errormessage + Environment.NewLine;

            if (!string.IsNullOrEmpty(log))
            {
                return null;
            }

            if (fee != 0)
            {
                if (!File.Exists(txbodyfile))
                    return null;

                string txbody = File.ReadAllText(txbodyfile);
                string cbor = GetCbor(txbody);
                GlobalFunctions.DeleteFile(txbodyfile);
                GlobalFunctions.DeleteFile(scriptfile);
                return cbor;
            }

            var b = CalculateFees(redis, txbodyfile, 2, 1,
                1, GlobalFunctions.IsMainnet(),
                ref buildtransaction, out fee);

            GlobalFunctions.DeleteFile(txbodyfile);
            GlobalFunctions.DeleteFile(scriptfile);

            if (b == false)
                return null;

            return CreateUnvestTransaction(redis, lockedAsset, fee, ref buildtransaction);
        }

        public static async Task<Utxo[]> GetUtxosFromWalletAsync(string ChangeAddress)
        {
            var stakeAddress = Bech32Engine.GetStakeFromAddress(ChangeAddress);
            var adrs = await BlockfrostFunctions.GetAllAddressesWithThisStakeAddressAsync(null, stakeAddress);
            var utxos = GetNewUtxo(adrs.Select(x => x.Address).ToArray()).ToCardanosharpUtxos();
            return utxos.ToArray();
        }

        internal static string GetCip68Extrafield(Nftproject project, string mintaddress)
        {
            return project.Cip68extrafield switch
            {
                null => project.Cip68extrafield,
                "" => "",
                "<pkh_user_token>" => GlobalFunctions.GetPkhFromAddress(mintaddress),
                "<pkh_policy_address>" => GlobalFunctions.GetPkhFromAddress(project.Policyaddress),
                _ => project.Cip68extrafield
            };
        }


        public static async Task<CreateDecentralPaymentByCslResultClass> CreateDecentralPaymentByCli(
    CreateMintAndSendParametersClass cmaspc, EasynftprojectsContext db, IConnectionMultiplexer redis,
    bool mainnet)
        {
            CreateDecentralPaymentByCslResultClass result = new CreateDecentralPaymentByCslResultClass();
            var cctc = CreateCslTransactionClass(cmaspc, db, redis, mainnet);

            string matxrawfile = GeneralConfigurationClass.TempFilePath + "matxrawfile" + GlobalFunctions.GetGuid() + ".cbor";
            string command = CliCommandExtensions.GetTransactionBuildRawWithLatestEra()
                .GetFees(cctc)
                .GetTxIn(cctc)
                .GetTxOut(cctc)
                .GetMint(cctc)
                .GetMetadataJsonFile(cctc)
                .GetOutFile(matxrawfile)
                .GetTTL(cctc)
                .GetMintingScriptFile(cctc);

            string log = CardanoCli(command, out var errormessage);
            if (!string.IsNullOrEmpty(errormessage))
            {
                result.CslError = errormessage;
                return result;
            }

            var st=File.ReadAllText(matxrawfile);
         //   result.CslResult=GetCbor(st);
         result.CslResult = st;

            return result;
        }
     
        internal static CslCreateTransactionClass CreateCslTransactionClass(CreateMintAndSendParametersClass cmaspc,
            EasynftprojectsContext db, IConnectionMultiplexer redis, bool mainnet)
        {

            var the721content = GetThe721Content(cmaspc.MetadataResult);
            var the20content = GetThe20Content(cmaspc.MetadataResult);

            List<Metadatum> meta = new List<Metadatum>();

            if (!string.IsNullOrEmpty(the721content) && the721content.Contains(cmaspc.project.Policyid))
            {
                meta.Add(new Metadatum() { Key = "721", Json = the721content });
            }
            if (!string.IsNullOrEmpty(the20content) && the20content.Contains(cmaspc.project.Policyid))
            {
                meta.Add(new Metadatum() { Key = "20", Json = the20content });
            }

            CslCreateTransactionClass cctc = new()
            {
                Ttl = cmaspc.ttl,
                Fees = cmaspc.Fees,
                IncludeMetadataHashOnly = cmaspc.IncludeMetadataHashOnly,
                ChangeAddressBech32 = cmaspc.BuyerChangeAddress,
                ProtocolParameters = GetProtocolParameters(redis, mainnet),
                TxIns = GetTxInHashes(cmaspc.utxofinal),
                Mints = cmaspc.MintTokens,
            };

            cctc.Metadata = meta.ToArray();

            long costs = 0;


            // Additional Payout wallets
            if (cmaspc.BuyerHasToPayInLovelace > cmaspc.Mintingcosts)
                cctc.TxOuts = cctc.TxOuts.GetTxOuts(cmaspc.AdditionalPayouts, cmaspc.BuyerHasToPayInLovelace,
                    cmaspc.selectedreservations.Count, ref costs);

            // Minting costs for NMKR
            cctc.TxOuts = cctc.TxOuts.GetTxOuts(cmaspc.MintingcostsAddress, cmaspc.Mintingcosts, null, ref costs);
            // Create Royalty Tokens
            costs = SetTxOutsForRoyaltyToken(cmaspc, cctc, costs);
            // Promotions
            SetTxOutsForPromotions(cmaspc, cctc);


            Token[] additionaltokens = GetSellerAdditionalTokens(cmaspc.utxofinal, cmaspc.AdditionalPriceInTokens, out TxInAddressesClass[] newUtxoBuyer);

            // The seller
            if (cmaspc.BuyerHasToPayInLovelace > cmaspc.Mintingcosts)
                cctc.TxOuts = cctc.TxOuts.GetTxOuts(cmaspc.SellerAddress, cmaspc.BuyerHasToPayInLovelace - costs, additionaltokens, ref costs);


            // Minting Tokens and other tokens from the buyers txin 
            if (string.IsNullOrEmpty(cmaspc.OptionalReceiverAddress) ||
                cmaspc.OptionalReceiverAddress == cmaspc.BuyerChangeAddress)
            {
                cctc.TxOuts = cctc.TxOuts.GetTxOuts(newUtxoBuyer, cmaspc.BuyerChangeAddress, null, null,
                    cmaspc.MintTokens, cmaspc.Fees ?? 0, cmaspc.Promotion, costs);
            }
            else
            {
                long minutxo = GetSendbackToReceiver(db, redis, cmaspc);
                costs += minutxo;
                cctc.TxOuts = cctc.TxOuts.GetTxOuts(cmaspc.OptionalReceiverAddress, cmaspc.MintTokens, minutxo);
                cctc.TxOuts = cctc.TxOuts.GetTxOuts(newUtxoBuyer, cmaspc.BuyerChangeAddress, null, null,
                    null, cmaspc.Fees ?? 0, cmaspc.Promotion, costs);

            }

            return cctc;
        }
    }
}

