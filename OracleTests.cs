using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Utils;
using OracleTest;

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
        public static List<WktWithName> WktPullTest(string username, string passwd, string tableName, int rowCount)
        {
            OracleConnection conn = OracleHelpers.GetOracleConnection(username, passwd, false);
            string testQueryString = ("select SDO_GEOMETRY.get_wkt(geom) from nemo." + tableName + " where rownum < " + (rowCount + 1).ToString()).ToString();
            Logging.Info("PullTest", "Constructed query: " + testQueryString);
            bool hasName = OracleHelpers.IsColumnNameExistsInTableName(conn, tableName, "NAME".ToString());
            if (hasName)
            {
                testQueryString = ("select SDO_GEOMETRY.get_wkt(geom), NAME from nemo." + tableName + " where rownum < " + (rowCount + 1).ToString()).ToString();
            }
            OracleCommand command = new OracleCommand(testQueryString, conn);
            conn.Open();
            OracleDataReader reader = command.ExecuteReader();
            List<WktWithName> contents = new List<WktWithName>();
            try
            {
                while (reader.Read())
                {
                    // Console.WriteLine(reader.GetString(0));
                    string wkt = reader.GetString(0);
                    string name = null;
                    if (hasName)
                    {
                        try
                        {
                            name = reader.GetString(1);
                        }
                        catch (System.InvalidCastException e)
                        {
                            Logging.Warning("WktPullTest", "Experienced an InvalidCastException");
                            if (e.ToString().Split(new[] { '\r', '\n' }).FirstOrDefault() == "System.InvalidCastException: Column contains NULL data")
                            {
                                Logging.Warning("WktPullTest", "Experienced a null string");
                            }
                            name = null;
                        }
                    }
                    WktWithName temp = new WktWithName();
                    temp.wkt = wkt;
                    temp.name = name;
                    contents.Add(temp);
                }
            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            return contents;
        }
    }
}