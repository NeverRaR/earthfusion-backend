using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OracleTest
{
    public class RedisSetKeyExpireTimeResult
    {
        public DateTime Date { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string KeyName { get; set; }
        public int ExpiryTimeInSeconds { get; set; }
        public DateTime ExpectedExpiryDate { get; set; }
    }
}
