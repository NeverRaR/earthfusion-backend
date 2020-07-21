using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Utils
{
    class RedisHelpers
    {
        private static ConnectionMultiplexer localhost_redis_connection = ConnectionMultiplexer.Connect("localhost");
        private static IDatabase local_redis_database = localhost_redis_connection.GetDatabase();
        public static bool SetString(string keyName, string value)
        {
            bool status = local_redis_database.StringSet(keyName, value);
            return status;
        }
        public static string GetString(string keyName)
        {
            string value = local_redis_database.StringGet(keyName);
            return value;
        }
        public static bool SetKeyExpireTime(string keyName, int expireTimeInSeconds)
        {
            TimeSpan interval = new TimeSpan(0, 0, expireTimeInSeconds);
            bool status = local_redis_database.KeyExpire(keyName, interval);
            return status;
        }
    }
}