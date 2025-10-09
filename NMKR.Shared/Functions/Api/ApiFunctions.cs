using NMKR.Shared.Classes;
using Google.Authenticator;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NMKR.Shared.Model;
using NMKR.Shared.Classes.CustodialWallets;
using NMKR.Shared.Enums;

namespace NMKR.Shared.Functions.Api
{
    public static class ApiFunctions
    {
        public static async Task<string> GetApiAccessTokenAsync()
        {
            using var httpClient = new HttpClient();
            TwoFactorAuthenticator tfa = new();
            var check = tfa.GetCurrentPIN(GeneralConfigurationClass.ApiTokenPassword);

            string url =
                $"{GeneralConfigurationClass.ApiUrl}/getaccesstoken/{GlobalFunctions.UrlEncode(GeneralConfigurationClass.ApiTokenPassword)}/{check}";
            using var request = new HttpRequestMessage(new("GET"),
                url);
            var response = await httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                GetAccessTokenResultClass gatrc =
                    JsonConvert.DeserializeObject<GetAccessTokenResultClass>(responseString);
                if (gatrc != null)
                    return gatrc.AccessToken;
            }

            return null;
        }

        public static async Task<bool> CallApiRemintAndBurnAsync(string projectuid, string nftuid)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new("GET"),
                GeneralConfigurationClass.ApiUrl + $"/v2/RemintAndBurn/{projectuid}/{nftuid}");
            var token = await GetApiAccessTokenAsync();
            request.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);


            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return true;

            return false;
        }


        public static async Task<PaymentTransactionResultClass> CallCreatePaymentTransactionAsync(CreatePaymentTransactionClass paymenttransaction)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), GeneralConfigurationClass.ApiUrl + "/v2/CreatePaymentTransaction");
            var token = await GetApiAccessTokenAsync();
            request.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);
            request.Headers.TryAddWithoutValidation("accept", "text/plain");


            request.Content = new StringContent(JsonConvert.SerializeObject(paymenttransaction));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                PaymentTransactionResultClass gatrc =
                    JsonConvert.DeserializeObject<PaymentTransactionResultClass>(responseString);
                if (gatrc != null)
                    return gatrc;
            }

            return null;
        }

        public static PaymentTransactionResultClass GetTransactionResult(string uid)
        {
            var res = Task.Run(async () => await GetTransactionResultAsync(uid));
            return res.Result;
        }

        public static async Task<PaymentTransactionResultClass> GetTransactionResultAsync(string uid)
        {
            using var httpClient = new HttpClient();
            string url =
                $"{GeneralConfigurationClass.ApiUrl}/v2/ProceedPaymentTransaction/{uid}/GetTransactionState/";
            using var request = new HttpRequestMessage(new("GET"), url);
            var token = await GetApiAccessTokenAsync();
            request.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);
            request.Headers.TryAddWithoutValidation("accept", "text/plain");

            var response = await httpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK) return null;

            string responseString = response.Content.ReadAsStringAsync().Result;
            PaymentTransactionResultClass gatrc =
                JsonConvert.DeserializeObject<PaymentTransactionResultClass>(responseString);
            return gatrc ?? null;
        }

        public static async Task<GetTransactionsClass[]> CallGetTransactionsAsync(string projectuid, DateTime? dateFrom, DateTime? dateTo, int? customerid)
        {
            using var httpClient = new HttpClient();

            string url = customerid!=null ? $"{GeneralConfigurationClass.ApiUrl}/v2/GetCustomerTransactions/{customerid}?fromdate={dateFrom.NmkrToIsoDateString()}&todate={dateTo.NmkrToIsoDateString()}" :
            $"{GeneralConfigurationClass.ApiUrl}/v2/GetProjectTransactions/{projectuid}?fromdate={dateFrom.NmkrToIsoDateString()}&todate={dateTo.NmkrToIsoDateString()}";

            using var request = new HttpRequestMessage(new("GET"), url);
            var token = await GetApiAccessTokenAsync();
            request.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);
            request.Headers.TryAddWithoutValidation("accept", "text/plain");

            var response = await httpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK) return null;

            string responseString = response.Content.ReadAsStringAsync().Result;
            var gatrc = JsonConvert.DeserializeObject<GetTransactionsClass[]>(responseString);
            return gatrc ?? null;
        }

        public static async Task<GetRefundsClass[]> CallGetRefundsAsync(string projectuid)
        {
            using var httpClient = new HttpClient();
            string url =
                $"{GeneralConfigurationClass.ApiUrl}/v2/GetRefunds/{projectuid}";
            using var request = new HttpRequestMessage(new("GET"), url);
            var token = await GetApiAccessTokenAsync();
            request.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);
            request.Headers.TryAddWithoutValidation("accept", "text/plain");

            var response = await httpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK) return null;

            string responseString = response.Content.ReadAsStringAsync().Result;
            var gatrc = JsonConvert.DeserializeObject<GetRefundsClass[]>(responseString);
            return gatrc ?? null;
        }
        public static async Task<CheckAddressResultClass> CallCheckAddressAsync(string projectuid, string address)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.ApiUrl}/v2/CheckAddress/{projectuid}/{address}");
            var token = await GetApiAccessTokenAsync();

            request.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);

            var response = await httpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK) return null;

            string responseString = response.Content.ReadAsStringAsync().Result;
            CheckAddressResultClass gatrc =
                JsonConvert.DeserializeObject<CheckAddressResultClass>(responseString);
            return gatrc ?? null;
        }

        public static async Task<MintAndSendResultClass> MintAndSendRandomAsync(EasynftprojectsContext db, string projectuid, long nftcount, string receiveraddress, Blockchain blockchain)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.ApiUrl}/v2/MintAndSendRandom/{projectuid}/{nftcount}/{receiveraddress}?blockchain={blockchain.ToString()}");
            var token = await GetApiAccessTokenAsync();
            request.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);

            var response = await httpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string responseStringError = response.Content.ReadAsStringAsync().Result;
                await GlobalFunctions.LogMessageAsync(db, "Api call produced an error - " + response.StatusCode.ToString(),
                    responseStringError);
                return null;
            }


            string responseString = response.Content.ReadAsStringAsync().Result;
            MintAndSendResultClass gatrc = JsonConvert.DeserializeObject<MintAndSendResultClass>(responseString);
            return gatrc ?? null;
        }

        public static async Task<MintAndSendResultClass> MintAndSendSpecificAsync(EasynftprojectsContext db, string projectuid, string nftuid,
            long tokencount, string receiveraddress, Blockchain blockchain)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.ApiUrl}/v2/MintAndSendSpecific/{projectuid}/{nftuid}/{tokencount}/{receiveraddress}?blockchain={blockchain.ToString()}");
            var token = await GetApiAccessTokenAsync();
            request.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);

            var response = await httpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string responseStringError = response.Content.ReadAsStringAsync().Result;
                await GlobalFunctions.LogMessageAsync(db, "Api call produced an error - " + response.StatusCode.ToString(),
                    responseStringError);
                return null;
            }

            string responseString = response.Content.ReadAsStringAsync().Result;
            MintAndSendResultClass gatrc =
                JsonConvert.DeserializeObject<MintAndSendResultClass>(responseString);
            return gatrc ?? null;
        }


        public static async Task<PaymentTransactionResultClass> CreatePaymentTransactionAsync(CreatePaymentTransactionClass cptc)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.ApiUrl}/v2/CreatePaymentTransaction");
            var token = await GetApiAccessTokenAsync();
            request.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);

            string s=JsonConvert.SerializeObject(cptc);
            request.Content = new StringContent(s);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK) return null;

            string responseString = response.Content.ReadAsStringAsync().Result;
            PaymentTransactionResultClass gatrc =
                JsonConvert.DeserializeObject<PaymentTransactionResultClass>(responseString);
            return gatrc ?? null;

        }

        public static async Task<PaymentTransactionResultClass> BuyDirectSaleAsync(string transactionuid, BuyerClass buyer)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.ApiUrl}/v2/ProceedPaymentTransaction/{transactionuid}/BuyDirectsale");
            var token = await GetApiAccessTokenAsync();
            request.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);

            var json = JsonConvert.SerializeObject(buyer);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            string responseString = response.Content.ReadAsStringAsync().Result;
            if (response.IsSuccessStatusCode)
            {
               // string responseString = response.Content.ReadAsStringAsync().Result;
                PaymentTransactionResultClass gatrc =
                    JsonConvert.DeserializeObject<PaymentTransactionResultClass>(responseString);
                return gatrc ?? null;

            }

            return null;
        }

        public static async Task<PaymentTransactionResultClass?> SubmitTransactionAsync(string transactionuid, string signGuid, string signedcbor)
        {
            SubmitTransactionClass stc = new SubmitTransactionClass() {SignGuid = signGuid, SignedCbor = signedcbor};

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.ApiUrl}/v2/ProceedPaymentTransaction/{transactionuid}/SubmitTransaction");
            var token = await GetApiAccessTokenAsync();
            request.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);

            request.Content = new StringContent(JsonConvert.SerializeObject(stc));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                PaymentTransactionResultClass gatrc =
                    JsonConvert.DeserializeObject<PaymentTransactionResultClass>(responseString);
                return gatrc ?? null;
            }

            return null;
        }

        public static async Task<CreateWalletResultClass?> CreateCustodialWalletAsync(int customerid, string walletpassword, bool enterpriseaddress, string walletname)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.ApiUrl}/v2/CreateWallet/{customerid}");
            request.Headers.TryAddWithoutValidation("accept", "text/plain");
            var token = await GetApiAccessTokenAsync();
            request.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);
            CreateManagedWalletClass createManaged = new CreateManagedWalletClass()
            {
                enterpriseaddress = enterpriseaddress, walletname = walletname, walletpassword = walletpassword
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(createManaged));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            string responseString = response.Content.ReadAsStringAsync().Result;
            var gatrc = JsonConvert.DeserializeObject<CreateWalletResultClass>(responseString);
            return gatrc;
        }
        public static async Task<string> GetPaymentAddressAsync(string projectuid, string pricelistCount, Blockchain blockchain=Blockchain.Cardano)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.ApiUrl}/v2/GetPaymentAddressForRandomNftSale/{projectuid}/{pricelistCount}/127.0.0.1?blockchain={blockchain.ToString()}");
            var token = await GetApiAccessTokenAsync();
            request.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);

            var response = await httpClient.SendAsync(request);
          //  if (!response.IsSuccessStatusCode) return null;

            string responseString = response.Content.ReadAsStringAsync().Result;
            return responseString;
        }
        public static async Task<string> CheckPaymentAddressAsync(string projectuid, string address)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.ApiUrl}/v2/CheckAddress/{projectuid}/{address}");
            var token = await GetApiAccessTokenAsync();
            request.Headers.TryAddWithoutValidation("authorization", "Bearer " + token);

            var response = await httpClient.SendAsync(request);
           // if (!response.IsSuccessStatusCode) return null;

            string responseString = response.Content.ReadAsStringAsync().Result;
            return responseString;
        }
    }
}
