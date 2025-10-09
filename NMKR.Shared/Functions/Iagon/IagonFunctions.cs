using NMKR.Shared.Classes;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using NMKR.Shared.Classes.Iagon;

namespace NMKR.Shared.Functions.Iagon
{
    public class IagonFunctions
    {
        public static async Task<IagonUploadResultClass> AddFileAsync(string file,string directory, string filename)
        {
            if (string.IsNullOrEmpty(file))
                return new IagonUploadResultClass() { Success = false, Message = "File is null or empty" };

            if (!File.Exists(file))
                return new IagonUploadResultClass() { Success = false, Message = "File does not exists" };

            if (string.IsNullOrEmpty(filename))
                filename= Path.GetFileName(file);


            HttpClient client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, GeneralConfigurationClass.IagonConfiguration.ApiUrl + "/storage/upload");

            request.Headers.Add("x-api-key", GeneralConfigurationClass.IagonConfiguration.ApiKey);


            MultipartFormDataContent content = new MultipartFormDataContent
            {
                { new ByteArrayContent(await File.ReadAllBytesAsync(file)), "file", Path.GetFileName(file) },
                { new StringContent(filename), "filename" },
                { new StringContent("public"), "visibility" }
            };
            request.Content = content;

            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string responseString = response.Content.ReadAsStringAsync().Result;
                var res = Newtonsoft.Json.JsonConvert.DeserializeObject<IagonUploadResultClass>(responseString);
                return res;
            }
            string responseString1 = response.Content.ReadAsStringAsync().Result;

            return new IagonUploadResultClass() {Success = false, Message = responseString1};
        }

        public static IagonUploadResultClass AddFile(string file,string directory, string filename)
        {
            var res = Task.Run(async () => await AddFileAsync(file,directory,filename));
            return res.Result;
        }


    }
}
