using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Linq;
using System.Security.Cryptography;
using NMKR.Shared.Functions;
using Microsoft.EntityFrameworkCore;

namespace NMKR.Api
{
    public static class CheckCachedAccess
    {

        public static CachedResultClass? CheckCachedResult(IConnectionMultiplexer redis, string apifunction, string apikey, string parameter)
        {
            return RedisFunctions.GetData<CachedResultClass>(redis, "result_" + apifunction + "_" + apikey + "_" + parameter);
        }

        public static ApiErrorResultClass CheckApikeyOrToken(IConnectionMultiplexer redis, string apifunction, int? nftprojectid, string apikeyortoken, string remoteIpAddress, bool deletetoken=true)
        {
            var result = new ApiErrorResultClass() { ResultState = ResultStates.Ok, ErrorCode = 0, ErrorMessage = "" };
            string redistoken = "apikey_" + apifunction + "_" + apikeyortoken + "_" + nftprojectid + "_" + remoteIpAddress;
            if (!apikeyortoken.StartsWith("token"))
            {
                result = RedisFunctions.GetData<ApiErrorResultClass>(redis,redistoken);
                if (result != null)
                {
                    return result;
                }
                using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
                {
                    result = nftprojectid == null ? CheckApiAccess.CheckApiKey(db, apikeyortoken, remoteIpAddress, out var apikey) : CheckApiAccess.CheckApiKey(db, apikeyortoken, remoteIpAddress, (int) nftprojectid);
                }
                RedisFunctions.SetData<ApiErrorResultClass>(redis,redistoken, result, 60);
            }
            else
            {
                string access = RedisFunctions.GetStringData(redis, apikeyortoken, deletetoken);
                switch (access)
                {
                    case "allowed":
                        return result;
                    case "Mainnet" when GlobalFunctions.IsMainnet():
                        return result;
                    case "Preprod" when !GlobalFunctions.IsMainnet():
                        return result;
                }
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Accesstoken is not valid or already expired";
                result.ErrorCode = 1;
            }
            return result;
        }

        public static int? GetCustomerIdFromApikey(string apikeyortoken)
        {
            if (apikeyortoken.StartsWith("token"))
                return -1;

            using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            string hash = HashClass.GetHash(SHA256.Create(), apikeyortoken);


            var t = (from a in db.Apikeys
                where a.Apikeyhash == hash
                select a).AsNoTracking().FirstOrDefault();
            if (t != null)
                return t.CustomerId;

            return null;
        }


        public static ApiErrorResultClass CheckApikeyOrToken(IConnectionMultiplexer redis, string apifunction,  string projectuid, string apikeyortoken, string remoteIpAddress)
        {
            var result = new ApiErrorResultClass() { ResultState = ResultStates.Ok, ErrorCode = 0, ErrorMessage = "" };

            if (string.IsNullOrEmpty(apikeyortoken))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Apikey/Accesstoken is not valid";
                result.ErrorCode = 1;
                return result;
            }

            string redistoken = "apikey_" + apifunction + "_" + apikeyortoken + "_" + projectuid + "_" + remoteIpAddress;
            if (!apikeyortoken.StartsWith("token"))
            {
                result = RedisFunctions.GetData<ApiErrorResultClass>(redis, redistoken);
                if (result != null)
                {
                    return result;
                }
                using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
                {
                    result = string.IsNullOrEmpty(projectuid) ? 
                        CheckApiAccess.CheckApiKey(db, apikeyortoken, remoteIpAddress.ToString(), out var apikey) : 
                        CheckApiAccess.CheckApiKey(db, apikeyortoken, remoteIpAddress.ToString(), projectuid);
                    db.Database.CloseConnection();
                }
                RedisFunctions.SetData<ApiErrorResultClass>(redis, redistoken, result, 60);
            }
            else
            {
                string access = RedisFunctions.GetStringData(redis, apikeyortoken, false);

                switch (access)
                {
                    case "allowed":
                        return result;
                    case "Mainnet" when GlobalFunctions.IsMainnet():
                        return result;
                    case "Preprod" when !GlobalFunctions.IsMainnet():
                        return result;
                }

                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Accesstoken is not valid or already expired";
                result.ErrorCode = 1;
            }
            return result;
        }

        public static void SetCachedResult(IConnectionMultiplexer redis, string apifunction, string apikey, int statuscode, object result, string parameter, int expiresInSeconds=20)
        {
            IDatabase dbr = redis.GetDatabase();
            CachedResultClass crc = new()
                {ResultString =JsonConvert.SerializeObject(result), Statuscode = statuscode};

            RedisFunctions.SetData<CachedResultClass>(redis,"result_" + apifunction + "_" + apikey + "_" + parameter,crc,expiresInSeconds);
        }
        public static void SetCachedResult(IConnectionMultiplexer redis, CachedApiCallValues apivalues, int statuscode, object result, int expiresInSeconds = 20)
        {
            IDatabase dbr = redis.GetDatabase();
            CachedResultClass crc = new()
                { ResultString = JsonConvert.SerializeObject(result), Statuscode = statuscode };

            RedisFunctions.SetData<CachedResultClass>(redis, "result_" +apivalues.GetRedisString(), crc, expiresInSeconds);
        }
    }
}

