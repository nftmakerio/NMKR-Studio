using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Maestro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace NMKR.Shared.Functions.Meastro
{
    public static class MaestroFunctions
    {
        public static async Task<MeastroAddressInformationClass> GetUtxoAsync(string address)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.MaestroConfiguration.ApiUrl}/addresses/{address}/utxos");
            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("api-key", GeneralConfigurationClass.MaestroConfiguration.ApiKey);

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                var res = JsonConvert.DeserializeObject<MeastroAddressInformationClass>(responseString);
                return res;
            }

            return null;
        }


        public static TxInAddressesClass ToTxInAddresses(this MeastroAddressInformationClass maestro, string address)
        {
            TxInAddressesClass txout = new TxInAddressesClass()
            {
                Address = address,
                DataProvider = Dataproviders.Maestro,
                StakeAddress = Bech32Engine.GetStakeFromAddress(address),
                TxIn = new TxInClass[] { }
            };

            List<TxInClass> txinlist = new List<TxInClass>();

            if (maestro == null)
            {
                return txout;
            }

            foreach (var maestroDatum in maestro.Data.OrEmptyIfNull())
            {
                var txin = new TxInClass
                {
                    TxHash = maestroDatum.TxHash,
                    TxId = maestroDatum.Index ?? 0,
                 //   Slot = maestroDatum.Slot??0
                };

                foreach (var maestroAsset in maestroDatum.Assets.OrEmptyIfNull())
                {
                    if (maestroAsset.Unit == "lovelace")
                    {
                        txin.Lovelace = maestroAsset.Amount ?? 0;
                    }
                    else
                    {
                        txin.AddTokens(maestroAsset.Unit,maestroAsset.Amount??0);
                    }
                }

                txinlist.Add(txin);
            }

            txout.TxIn = txinlist.ToArray();
            return txout;
        }

        public static SubmissionResultClass SubmitTurboTransactionFile(string matxsignedfile)
        {
            var res = Task.Run(async () => await SubmitTurboTransactionFileAsync(matxsignedfile));
            return res.Result;
        }

        public static async Task<SubmissionResultClass> SubmitTurboTransactionFileAsync(string matxsignedfile)
        {
            if (!File.Exists(matxsignedfile))
                return new SubmissionResultClass() { ErrorMessage = matxsignedfile+" does not exists", Success = false };

            string signedTxStr = ConsoleCommand.GetCbor(await File.ReadAllTextAsync(matxsignedfile));
            var res= await SubmitTurboTransactionAsync(Convert.FromHexString(signedTxStr));
            //var res = await SubmitTurboTransactionAsync(signedTxStr);

            if (res.Success)
            {
                BuildTransactionClass bt = new BuildTransactionClass();
                ConsoleCommand.GetTxId(matxsignedfile, ref bt);
                if (string.IsNullOrEmpty(bt.TxHash))
                    return new SubmissionResultClass(){Success = false,ErrorMessage = "TX-Hash not determined", Buildtransaction = bt};
                res.TxHash=bt.TxHash;
            }

            return res;
        }

        public static SubmissionResultClass SubmitTurboTransaction(byte[] signedTx)
        {
            var res = Task.Run(async () => await SubmitTurboTransactionAsync(signedTx));
            return res.Result;
        }

        private static async Task<SubmissionResultClass> SubmitTurboTransactionAsync(byte[] signedTx)
        {
            if (signedTx == null || signedTx.Length == 0)
                return new SubmissionResultClass() { ErrorMessage = "SignedTx is null", Success = false };

            try
            {
                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(new HttpMethod("POST"),
                    $"{GeneralConfigurationClass.MaestroConfiguration.ApiUrl}/txmanager/turbosubmit");
                request.Headers.TryAddWithoutValidation("Accept", "text/plain");
                request.Headers.TryAddWithoutValidation("api-key",
                    GeneralConfigurationClass.MaestroConfiguration.ApiKey);

                await using var ms = new MemoryStream(signedTx);
                request.Content = new StreamContent(ms);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/cbor");

                var response = await httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return new SubmissionResultClass()
                        { ErrorMessage = "Statuscode: " + response.StatusCode, Success = false };
                string responseString = response.Content.ReadAsStringAsync().Result;

                return new SubmissionResultClass() {Success = true, TxHash = responseString};
               
            }
            catch (Exception e)
            {
                return new SubmissionResultClass() { ErrorMessage = e.Message + Environment.NewLine + e.InnerException?.StackTrace + Environment.NewLine, Success = false };
            }
        }
    }
}
