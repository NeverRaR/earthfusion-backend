using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class RedisSetStringResult
    {
        public DateTime Date { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string KeyName { get; set; }
        public string Value { get; set; }
    }
}
