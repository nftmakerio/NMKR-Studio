using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.NmkrStore;
using Newtonsoft.Json;

namespace NMKR.Shared.Functions.NmkrStoreApi
{
    public static class NmkrStoreApiFunctions
    {
        public static async Task<NmkrStoreApiGetVerifiedCollectionClass> GetVerifiedCollectionAsync(string policyid)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.StoreApiUrl}/verified-collection/api/verifiedcollections/{policyid}");
            request.Headers.TryAddWithoutValidation("accept", "application/json");

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                NmkrStoreApiGetVerifiedCollectionClass res =
                    JsonConvert.DeserializeObject<NmkrStoreApiGetVerifiedCollectionClass>(responseString);
                return res;
            }

            return null;
        }

        public static async Task<bool> RegisterVerifiedCollectionAsync(NmkrStoreApiGetVerifiedCollectionClass collection)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.StoreApiUrl}/verified-collection/api/verifiedcollections");
            request.Headers.TryAddWithoutValidation("accept", "application/json");

            request.Content = new StringContent(JsonConvert.SerializeObject(collection));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> UpdateVerifiedCollectionAsync(NmkrStoreApiGetVerifiedCollectionClass collection)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("PUT"), $"{GeneralConfigurationClass.StoreApiUrl}/verified-collection/api/verifiedcollections/{collection.Id}");
            request.Headers.TryAddWithoutValidation("accept", "*/*");

            request.Content = new StringContent(JsonConvert.SerializeObject(collection));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json-patch+json");

            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> RemoveVerifiedCollectionAsync(string uid)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"{GeneralConfigurationClass.StoreApiUrl}/verified-collection/api/verifiedcollections/{uid}");
            request.Headers.TryAddWithoutValidation("accept", "application/json");

            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
