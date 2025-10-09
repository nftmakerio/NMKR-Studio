using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace NMKR.Shared.Mailerlite
{
    public class AddSubscriberToMailerliteClass
    {

        public async Task<bool> AddSubscriberToMailerlite(int customerid)
        {
            if (string.IsNullOrEmpty(GeneralConfigurationClass.MailerliteUrl))
                return false;

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var customer = await (from a in db.Customers
                    .Include(a=>a.Country)
                where a.Id==customerid
                select a).FirstOrDefaultAsync();

            if (customer == null)
                return false;

            MailerliteSubscriberClass mlsc = new MailerliteSubscriberClass()
            {
                Email = customer.Email,
                Fields = new Fields()
                {
                    City = customer.City, Company = customer.Company, LastName = customer.Lastname,
                    Name = customer.Firstname, ZIP = customer.Zip, Country = customer.Country.Nicename,
                    State = customer.State
                },
                Groups = new[] {$"{GeneralConfigurationClass.MailerliteGroupId}"}
            };


            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("POST"), $"{GeneralConfigurationClass.MailerliteUrl}/subscribers");
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {GeneralConfigurationClass.MailerliteKey}");

            request.Content = new StringContent(JsonConvert.SerializeObject(mlsc));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

    }
}
