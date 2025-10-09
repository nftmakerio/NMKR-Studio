using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace NMKR.Shared.Classes
{
    public static class IpfsFunctions
    {
        public static async Task<string> AddFileAsync(string filename)
        {
            string responseString = "";
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new("POST"), GeneralConfigurationClass.IPFSApi + "/add");
            var multipartContent = new MultipartFormDataContent
            {
                { new ByteArrayContent(await File.ReadAllBytesAsync(filename)), "file", Path.GetFileName(filename) }
            };
            request.Content = multipartContent;

            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                responseString = response.Content.ReadAsStringAsync().Result;
            }

            return responseString;
        }

        public static string AddFile(string filename)
        {
            var res = Task.Run(async () => await AddFileAsync(filename));
            return res.Result;
        }



        public static string PinFile(string hash)
        {
            string responseString = "";
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new("POST"), GeneralConfigurationClass.IPFSApi + "/pin/add?arg="+hash.Trim()+"&progress=false");
            try
            {
                var response = httpClient.Send(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    responseString = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return responseString;
        }

        public static async Task<string> PinFileAsync(string hash)
        {
            string responseString = "";
            using var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(15) };
            using var request = new HttpRequestMessage(new("POST"), GeneralConfigurationClass.IPFSApi + "/pin/add?arg=" + hash.Trim() + "&progress=false");

            try
            {
                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    responseString = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           
            return responseString;
        }

        public static string UnPinFile(string hash)
        {
            string responseString = "";
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new("POST"), GeneralConfigurationClass.IPFSApi + "/pin/add?arg=" + hash + "&progress=false");
            try
            {
                var response = httpClient.Send(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    responseString = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch
            {

            }

            return responseString;
        }


    }
}
