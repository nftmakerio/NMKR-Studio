using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Crossmint;
using Newtonsoft.Json;

namespace NMKR.Shared.Functions.Crossmint
{
    public static class CrossMintFunctions
    {
        public static async Task<string> RegisterProjectOnCrossmint(RegisterCrossmintProjectsClass projectdata)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"),
                GeneralConfigurationClass.CrossmintSettings.CrossmintApiUrl);
            request.Headers.TryAddWithoutValidation("X-PROJECT-ID",
                GeneralConfigurationClass.CrossmintSettings.CrossmintClientKey);
            request.Headers.TryAddWithoutValidation("X-CLIENT-SECRET",
                GeneralConfigurationClass.CrossmintSettings.CrossmintSecret);
            string st = JsonConvert.SerializeObject(projectdata);
            request.Content = new StringContent(st);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var responsestring = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    var res = JsonConvert.DeserializeObject<CrossmintErrorMessageClass>(responsestring);
                    if (res != null)
                        return "ERROR: " + res.Message;
                }
            }

            if (!response.IsSuccessStatusCode) return null;
            var responsestring2 = await response.Content.ReadAsStringAsync();
            var res2 = JsonConvert.DeserializeObject<CrossmintSuccessClass>(responsestring2);
            if (res2 != null && res2.CollectionId != null)
                return res2.CollectionId;

            return "ERROR: No Crossmint-Id received";
        }
    }
}
