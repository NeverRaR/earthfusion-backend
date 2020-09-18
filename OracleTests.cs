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
        public static List<WktWithName> WktPullTest(string username, string passwd, string tableName, int rowCount,double ullog,double ullat,double lrlog,double lrlat,int isAll)
        {
            // conn to use
            OracleConnection conn = OracleHelpers.GetOracleConnection(username, passwd, false);

            // List to return
            List<WktWithName> contents = new List<WktWithName>();
                        // has geom?
            bool hasGeom = OracleHelpers.IsColumnNameExistInTableName(conn, tableName, "GEOM".ToString());
            if (!hasGeom)
            {
                return contents;
            }

            // has name?
            bool hasName = OracleHelpers.IsColumnNameExistInTableName(conn, tableName, "NAME".ToString());
            bool hasStationName=OracleHelpers.IsColumnNameExistInTableName(conn,tableName,"STATION_NAME");
            string testQueryString = "select SDO_GEOMETRY.get_wkt(geom) ";
            if(hasStationName) testQueryString+=",STATION_NAME,SEQUENCE ";
            else if(hasName) testQueryString+=",NAME ";

            

            testQueryString +=" from nemo." + tableName + " a ";
            if(isAll==0)
            {
                    testQueryString+= " where "
                                   + " MDSYS.sdo_filter(a.geom,SDO_GEOMETRY("
                                   + " 2003,"
                                   + " 4326,"
                                   + " NULL,"
                                   + " SDO_ELEM_INFO_ARRAY(1,1003,3),"
                                   + " SDO_ORDINATE_ARRAY(" + ullog + " , " + lrlat + "," + lrlog + " , " + ullat + ")"
                                   + " )"
                                   + " )='TRUE'";
            }


            Logging.Info("WktPullTest", "Constructed query: " + testQueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(testQueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            int count=0;
            try
            {
                while (reader.Read()&&count<rowCount)
                {
                    // Console.WriteLine(reader.GetString(0));
                    string wkt = reader.GetString(0);
                    string name = null;
                    if (hasName||hasStationName)
                    {
                        // some data still has null string even there is name column
                        // also, this proves to be useful when we didn't select name from the table.
                        try
                        {
                            name = reader.GetString(1);
                            if(hasStationName)name +=reader.GetInt32(2)+"号站";
                        }
                        catch (System.InvalidCastException e)
                        {
                            //Logging.Warning("WktPullTest", "Experienced an InvalidCastException");
                            //if (e.ToString().Split(new[] { '\r', '\n' }).FirstOrDefault() == "System.InvalidCastException: Column contains NULL data")
                            //{
                            //   Logging.Warning("WktPullTest", "Experienced a null string");
                            //}
                            name = null;
                        }
                    }
                    WktWithName temp = new WktWithName();
                    temp.wkt = wkt;
                    temp.name = name;
                    contents.Add(temp);
                    count++;
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

        public static List<WktWithName> WktPullTestWithRange(string username, string passwd, string tableName, int rangeStart, int rangeEnd)
        {
            // conn to use
            OracleConnection conn = OracleHelpers.GetOracleConnection(username, passwd, false);

            // List to return
            List<WktWithName> contents = new List<WktWithName>();

            string testQueryString = "SELECT geom_data from";
            testQueryString += "(";
            testQueryString += "select SDO_GEOMETRY.get_wkt(geom) geom_data, rownum r ";
            testQueryString += "from nemo.";
            testQueryString += tableName;
            testQueryString += ")";
            testQueryString += "where r > " + (rangeStart - 1).ToString() + " and r < " + (rangeEnd + 1).ToString();


            // has geom?
            bool hasGeom = OracleHelpers.IsColumnNameExistInTableName(conn, tableName, "GEOM".ToString());
            if (!hasGeom)
            {
                return contents;
            }

            // has name?
            bool hasName = OracleHelpers.IsColumnNameExistInTableName(conn, tableName, "NAME".ToString());
            if (hasName)
            {
                // also grab the name.
                testQueryString = "SELECT geom_data, real_name from";
                testQueryString += "(";
                testQueryString += "select SDO_GEOMETRY.get_wkt(geom) geom_data, name real_name, rownum r ";
                testQueryString += "from nemo.";
                testQueryString += tableName;
                testQueryString += ")";
                testQueryString += "where r > " + (rangeStart - 1).ToString() + " and r < " + (rangeEnd + 1).ToString();
            }

            Logging.Info("WktPullTestWithRange", "Constructed query: " + testQueryString);

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
                            Logging.Warning("WktPullTestWithRange", "Experienced an InvalidCastException");
                            if (e.ToString().Split(new[] { '\r', '\n' }).FirstOrDefault() == "System.InvalidCastException: Column contains NULL data")
                            {
                                Logging.Warning("WktPullTestWithRange", "Experienced a null string");
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

        public static List<WktWithName> NameSearchTest(string username, string passwd, string tableName, string nameOfPlace)
        {
            // conn to use
            OracleConnection conn = OracleHelpers.GetOracleConnection(username, passwd, false);

            // List to return
            List<WktWithName> contents = new List<WktWithName>();


            // has geom?
            bool hasGeom = OracleHelpers.IsColumnNameExistInTableName(conn, tableName, "GEOM".ToString());
            bool hasName = OracleHelpers.IsColumnNameExistInTableName(conn, tableName, "NAME".ToString());
            if (!hasGeom || !hasName)
            {
                return contents;
            }
            string testQueryString = ("select SDO_GEOMETRY.get_wkt(geom) from nemo." + tableName + " where NAME like '%" + nameOfPlace + "%'").ToString();
            Logging.Info("NameSearchTest", "Constructed query: " + testQueryString);

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