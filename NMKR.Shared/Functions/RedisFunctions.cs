using System;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Shared.Functions
{
    public static class RedisFunctions
    {
        public static void SetData<T>(IConnectionMultiplexer redis, string key, T data, int expireinseconds)
        {
            if (redis == null)
                return;

            IDatabase db = redis.GetDatabase();
            var settings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            db.StringSet(key, JsonConvert.SerializeObject(data,settings), expiry: new TimeSpan(0, 0, expireinseconds));
        }

        public static void SetStringData(IConnectionMultiplexer redis, string key, string data, int expireinseconds)
        {
            if (redis == null)
                return;

            IDatabase db = redis.GetDatabase();
            db.StringSet(key, data, expiry: new TimeSpan(0, 0, expireinseconds));
        }

        public static T GetData<T>(IConnectionMultiplexer redis, string key)
        {
            if (redis == null)
                return default(T); 
            try
            {
                IDatabase db = redis.GetDatabase();
                var res = db.StringGet(key);

                if (res.IsNull)
                    return default(T);
                else
                {
                    var z = JsonConvert.DeserializeObject<T>(res);
                    return z;
                }

            }
            catch
            {
                return default(T);
            }
        }

        public static string GetStringData(IConnectionMultiplexer redis, string key, bool delete)
        {
            if (redis == null)
                return null;
            try
            {
                IDatabase db = redis.GetDatabase();
                //  var res = delete ? db.StringGetDelete(key) : db.StringGet(key);
                var res = db.StringGet(key);
                if (res.IsNull)
                    return null;
                else
                {
                    return res;
                }
            }
            catch
            {
                return null;
            }
        }

        public static void DeleteKey(IConnectionMultiplexer redis, string redisKey)
        {
            if (redis == null)
                return;

            IDatabase db = redis.GetDatabase();
            db.KeyDelete(redisKey);
        }
    }
}
