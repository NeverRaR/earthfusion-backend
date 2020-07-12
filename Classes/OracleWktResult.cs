using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace OracleTest
{
    public class OracleWktResponse
    {
        public OracleWktResponse()
        {
            Contents = new List<string>();
        }
        public DateTime Date { get; set; }
        public int StatusCode { get; set; }
        public string TableName { get; set; }
        public string Message { get; set; }
        public List<string> Contents { get; set; }
    }
}
