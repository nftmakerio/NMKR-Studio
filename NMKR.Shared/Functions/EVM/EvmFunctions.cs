using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using NMKR.Shared.Classes.EVM;
using Newtonsoft.Json;

namespace NMKR.Shared.Functions.EVM
{
    public static class EvmFunctions
    {
        public static async Task<EvmContractDeployTransactionResultClass> EvmContractDeployTransaction(EvmContractDeployTransactionClass evmContractDeployTransactionClass)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://studio-evmservice.preprod.nmkr.io/v1/contract/deployTransaction");
            request.Headers.TryAddWithoutValidation("accept", "application/json");
            request.Headers.TryAddWithoutValidation("authorization", "test");

            request.Content = new StringContent(JsonConvert.SerializeObject(evmContractDeployTransactionClass));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<EvmContractDeployTransactionResultClass>(responseContent);
        }


        public static async Task<EvmContractLinkProjectResultClass> EvmContractLinkProject(EvmContractLinkProjectClass evmContractLinkProjectClass)
        {
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), "https://studio-evmservice.preprod.nmkr.io/v1/contract/link");
            request.Headers.TryAddWithoutValidation("accept", "application/json");
            request.Headers.TryAddWithoutValidation("authorization", "test");

            request.Content = new StringContent(JsonConvert.SerializeObject(evmContractLinkProjectClass));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<EvmContractLinkProjectResultClass>(responseContent);
        }
    }
}
