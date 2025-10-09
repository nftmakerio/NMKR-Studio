using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Api.Apikey
{
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly IConnectionMultiplexer _redis;
        public ApiKeyValidator(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public bool IsValid(string apiKey)
        {
            var remoteIpAddress = "";
            using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            string apifunction = this.GetType().Name;

            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apiKey, remoteIpAddress ?? string.Empty);
            return result.ResultState == ResultStates.Ok;

        }
    }

   

    public interface IApiKeyValidator
    {
        bool IsValid(string apiKey);
    }
}
