using System;
using System.Net.Http;
using System.Threading.Tasks;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Newtonsoft.Json;

namespace NMKR.Shared.NotificationClasses
{
    public class TestWebhookNotificationClass
    {
        private Nftproject _project { get; }
        public TestWebhookNotificationClass(Nftproject project)
        {
            _project = project;
        }


        public async Task<bool> TestWebhook(string url, string secret)
        {
            try
            {
                var nsc = new NotificationSaleClass()
                {
                    ProjectName = _project.Projectname, ProjectUid = _project.Uid, Price = 10000000, SaleDate = DateTime.Now,
                    SaleType = TransactionTypes.paidonftaddress,
                    EventType = NotificationEventTypes.transactionconfirmed, MintingCosts = 2000000,
                    TxHash = "0016df817c3364daa7f08133385cf6f50a4a7dcaffbbcfa49ecc33d556552c50",
                    DetailResults = "Sample data",
                };

                string payload = JsonConvert.SerializeObject(nsc);
                var hash = GlobalFunctions.GetHMAC(payload, secret);

                var client = new HttpClient();
                client.Timeout = new(0, 1, 0);

                string delimiter = url.Contains("?") ? "&" : "?";

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new($"{url}{delimiter}payloadHash={GlobalFunctions.UrlEncode(hash)}"),

                    //    Content = new StringContent($"\"{payload}\"")
                    Content = new StringContent(payload)
                    {
                        Headers =
                        {
                            ContentType = new("application/json")
                        }
                    }
                };
                using var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return true;
            }
            catch
            {

            }

            return false;
        }
    }
}
