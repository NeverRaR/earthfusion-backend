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
            // conn to use
            OracleConnection conn = OracleHelpers.GetOracleConnection(username, passwd, false);

            // List to return
            List<WktWithName> contents = new List<WktWithName>();

            string testQueryString = ("select SDO_GEOMETRY.get_wkt(geom) from nemo." + tableName + " where rownum < " + (rowCount + 1).ToString()).ToString();
            
            // has geom?
            bool hasGeom = OracleHelpers.IsColumnNameExistsInTableName(conn, tableName, "GEOM".ToString());
            if (!hasGeom)
            {
                return contents;
            }

            // has name?
            bool hasName = OracleHelpers.IsColumnNameExistsInTableName(conn, tableName, "NAME".ToString());
            if (hasName)
            {
                // also grab the name.
                testQueryString = ("select SDO_GEOMETRY.get_wkt(geom), NAME from nemo." + tableName + " where rownum < " + (rowCount + 1).ToString()).ToString();
            }

            Logging.Info("WktPullTest", "Constructed query: " + testQueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(testQueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    // Console.WriteLine(reader.GetString(0));
                    string wkt = reader.GetString(0);
                    string name = null;
                    if (hasName)
                    {
                        // some data still has null string even there is name column
                        // also, this proves to be useful when we didn't select name from the table.
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