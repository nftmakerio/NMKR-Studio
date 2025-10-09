using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.CslService;
using Newtonsoft.Json;

namespace NMKR.Shared.Functions.CSLService
{
    public static class CSLServiceFunctions
    {
        public static string PlutusDataCborToJson(string cbor)
        {
            var res = Task.Run(async () => await PlutusDataCborToJsonAsync(cbor));
            return res.Result;
        }

        public static async Task<string> PlutusDataCborToJsonAsync(string cbor)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"),
                $"{GeneralConfigurationClass.CslService}helpers/plutus_data_cbor_to_json");
            request.Headers.TryAddWithoutValidation("accept", "application/json");

            request.Content = new StringContent(
                $"{{\n  \"dataCbor\": \"{cbor}\"\n}}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            string responseString = await response.Content.ReadAsStringAsync();
            var res = JsonConvert.DeserializeObject<CborToJsonClass>(responseString);
            if (res==null)
                return null;
            return res.Json;
        }

        public static CslServiceCborHexClass JsonToPlutusDataCbor(string json)
        {
            var res = Task.Run(async () => await JsonToPlutusDataCborAsync(json));
            return res.Result;
        }
        public static async Task<CslServiceCborHexClass> JsonToPlutusDataCborAsync(string json)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"),
                $"{GeneralConfigurationClass.CslService}helpers/plutus_data_json_to_cbor");
            request.Headers.TryAddWithoutValidation("accept", "application/json");

            request.Content = new StringContent(
                $"{{\n  \"dataJson\": \"{JsonEncodedText.Encode(json)}\"\n}}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return null;

            string responseString = await response.Content.ReadAsStringAsync();

            CslServiceCborHexClass cscb = JsonConvert.DeserializeObject<CslServiceCborHexClass>(responseString);

            return cscb;
        }

    }
}
