using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Utils
{
    class RedisHelpers
    {
        public static bool SetString(string keyName, string value)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            IDatabase db = redis.GetDatabase();
            db.StringSet(keyName, value);
            return true;
        }
        public static string GetString(string keyName)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            IDatabase db = redis.GetDatabase();
            string value = db.StringGet(keyName);
            return value;
        }
    }
}