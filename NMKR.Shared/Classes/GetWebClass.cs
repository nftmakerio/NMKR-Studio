using System.Net.Http;
using System.Threading.Tasks;

namespace NMKR.Shared.Classes
{
    public static class GetWebClass
    {
        private static readonly HttpClient _httpClient = new();

        public static async Task<string> Get(string queryString)
        {

            // The actual Get method
            using (var result = await _httpClient.GetAsync($"{queryString}"))
            {
                string content = await result.Content.ReadAsStringAsync();
                return content;
            }
        }
    }
}