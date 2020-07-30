using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace Utils
{
    class OracleTests
    {
        public static string ConnectionTest(string username, string passwd, bool connectAsDba)
        {
            OracleConnection conn = OracleHelpers.GetOracleConnection(username, passwd, connectAsDba);
            conn.Open();
            string testResult = "Connection state: " + conn.State;
            conn.Close();
            return testResult;
        }
        public static List<string> PullTest(string username, string passwd, string tableName, int rowCount)
        {
            OracleConnection conn = OracleHelpers.GetOracleConnection(username, passwd, false);
            string testQueryString = ("select SDO_GEOMETRY.get_wkt(geom) from nemo." + tableName + " where rownum < " + (rowCount + 1).ToString()).ToString();
            Logging.Info("PullTest", "Constructed query: " + testQueryString);
            OracleCommand command = new OracleCommand(testQueryString, conn);
            conn.Open();
            OracleDataReader reader = command.ExecuteReader();
            List<string> wkts = new List<string>();
            try
            {
                while (reader.Read())
                {
                    // Console.WriteLine(reader.GetString(0));
                    string wkt = reader.GetString(0);
                    wkts.Add(wkt);
                }
            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            return wkts;
        }
    }
}