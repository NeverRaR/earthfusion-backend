using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace Utils
{
    class OracleHelpers
    {
        public static OracleConnection GetOracleConnection(string username, string passwd, bool connectAsDba)
        {
            string passwd_string, username_string;
            // connection string.
            string connString = "Data Source=" +
                                    "(DESCRIPTION=" +
                                        "(ADDRESS=" +
                                            "(PROTOCOL=TCP)" +
                                            "(HOST=localhost)" +
                                            "(PORT=1521)" +
                                        ")" +
                                        "(CONNECT_DATA=" +
                                            "(SID=xe)" +
                                        ")" +
                                    ");" +
                                "User ID=";
            username_string = username;
            passwd_string = passwd;
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = connString + username_string + ";Password=" + passwd_string;
            if (connectAsDba)
            {
                conn.ConnectionString += ";DBA Privilege=SYSDBA;";
            }
            string logMessage = "constructed connection string for username=" + username + ", password=" + passwd;
            if (connectAsDba)
            {
                logMessage += ", WITH SYSDBA privilege.";
            }
            else
            {
                logMessage += ", WITHOUT privilege.";
            }
            Logging.Info("OracleHelpers.GetOracleConnection", logMessage);
            return conn;
        }

        public static bool IsColumnNameExistInTableName(OracleConnection conn, string tableName, string columnName)
        {
            // search all_tab_cols for the result.
            string testQueryString = (
                                        (
                                            "select column_name as found from all_tab_cols where table_name = '"
                                            + tableName.ToUpper()
                                            + "'and column_name = '"
                                            + columnName.ToUpper()
                                            + "'"
                                        )
                                        .ToString()
                                    ).ToString();
            OracleCommand command = new OracleCommand(testQueryString, conn);
            conn.Open();
            OracleDataReader reader = command.ExecuteReader();
            List<string> result = new List<string>();
            try
            {
                while (reader.Read())
                {
                    // Console.WriteLine(reader.GetString(0));
                    string column_name = reader.GetString(0);
                    result.Add(column_name);
                    // should I just returns true at this point?
                }
            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            if (result.Count() >= 1)
            {
                Logging.Info("IsColumnNameExistsInTableName", "Column name " + columnName + " EXISTS in table " + tableName);
                return true;
            }
            Logging.Info("IsColumnNameExistsInTableName", "Column name " + columnName + " DOES NOT EXIST in table " + tableName);
            return false;
        }

        public static bool IsRowExistInColumnInTableName(OracleConnection conn, string query, string tableName, string columnName)
        {
            string testQueryString = "select count(*) from " + tableName + " where " + columnName + " = '" + query + "'";
            OracleCommand command = new OracleCommand(testQueryString, conn);
            conn.Open();
            OracleDataReader reader = command.ExecuteReader();
            int result = 0;
            try
            {
                while (reader.Read())
                {
                    result = reader.GetInt32(0);
                }
            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            if (result > 0)
            {
                return true;
            }
            conn.Close();
            return false;
        }

        public static int CompetitivenessRegionQuery(double ullog, double ullat, double lrlog, double lrlat)
        {
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select count(*)"
                                + "from nemo.SHANGHAI_SHOPS a "
                                + "where MDSYS.sdo_filter(a.geom,SDO_GEOMETRY("
                                + "2003,"
                                + "4326,"
                                + "NULL,"
                                + "SDO_ELEM_INFO_ARRAY(1,1003,3),"
                                + "SDO_ORDINATE_ARRAY(" + ullog + " , " + lrlat + "," + lrlog + " , " + ullat + ")"
                                + ")"
                                + ")='TRUE'";
            Logging.Info("CompetitivenessRegionQuery", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            int ans = 0;
            if (reader.RowSize == 0)
            {
                conn.Close();
                return -1;
            }
            try
            {

                if (reader.Read())
                {
                    ans = reader.GetInt32(0);
                }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            return ans;

        }
        public static int CompetitivenessPointQuery(double log,double lat,double dis)
        {
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select count(*)"

                                +"from nemo.SHANGHAI_SHOPS a "
                                +"where MDSYS.SDO_WITHIN_DISTANCE(a.geom,SDO_GEOMETRY("
                                +"2001,"
                                +"4326,"
                                +"SDO_POINT_TYPE("+log+","+lat+",NULL),"
                                +"NULL,"
                                +"NULL"
                                +"),"
                                +"'DISTANCE="+dis+" UNIT=M'"
                                +")='TRUE'";
            Logging.Info("CompetitivenessPointQuery", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            int ans = 0;
            if (reader.RowSize == 0)
            {
                conn.Close();
                return -1;
            }
            try
            {

                if (reader.Read())
                {
                    ans = reader.GetInt32(0);
                }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            return ans;

        }
        public static int TrafficAccessibilityRegionQuery(double ullog, double ullat, double lrlog, double lrlat)
        {
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select count(*)"
                                + "from nemo.SHANGHAI_ROAD_NETWORK a "
                                + "where MDSYS.sdo_filter(a.geom,SDO_GEOMETRY("
                                + "2003,"
                                + "4326,"
                                + "NULL,"
                                + "SDO_ELEM_INFO_ARRAY(1,1003,3),"
                                + "SDO_ORDINATE_ARRAY(" + ullog + " , " + lrlat + "," + lrlog + " , " + ullat + ")"
                                + ")"
                                + ")='TRUE'";
            Logging.Info("TrafficAccessibilityRegionQuery", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            int ans = 0;
            if (reader.RowSize == 0)
            {
                conn.Close();
                return -1;
            }
            try
            {

                if (reader.Read())
                {
                    ans = reader.GetInt32(0);
                }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            return ans;

        }

        

          public static int BusAccessibilityRegionQuery(double ullog,double ullat,double lrlog,double lrlat)
        {
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select count(*)"
                                +"from nemo.BUS_STATION_POINT a "
                                +"where MDSYS.sdo_filter(a.geom,SDO_GEOMETRY("
                                +"2003,"
                                +"4326,"
                                +"NULL,"
                                +"SDO_ELEM_INFO_ARRAY(1,1003,3),"
                                +"SDO_ORDINATE_ARRAY("+ullog+" , "+ lrlat +","+lrlog+" , "+ullat+")"
                                +")"
                                +")='TRUE'";
            Logging.Info("TrafficAccessibilityRegionQuery", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            int ans=0;
            if(reader.RowSize==0)
            {
                conn.Close();
                return -1;
            }
            try
            {
                
               if(reader.Read())
               {
                   ans=reader.GetInt32(0);
               }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            return ans;
        
        }

        public static int TrafficAccessibilityPointQuery(double log, double lat, float dis)
        {
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select count(*)"
                                + "from nemo.SHANGHAI_ROAD_NETWORK a "
                                + "where MDSYS.SDO_WITHIN_DISTANCE(a.geom,SDO_GEOMETRY("
                                + "2001,"
                                + "4326,"
                                + "SDO_POINT_TYPE(" + log + "," + lat + ",NULL),"
                                + "NULL,"
                                + "NULL"
                                + "),"
                                + "'DISTANCE=" + dis + " UNIT=M'"
                                + ")='TRUE'";
            Logging.Info("TrafficAccessibilityPointQuery", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            int ans = 0;
            if (reader.RowSize == 0)
            {
                conn.Close();
                return -1;
            }
            try
            {

                if (reader.Read())
                {
                    ans = reader.GetInt32(0);
                }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            return ans;
        }

            public static int BusAccessibilityPointQuery(double log,double lat,double dis)
        {
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select count(*)"
                                +"from nemo.BUS_STATION_POINT a "
                                +"where MDSYS.SDO_WITHIN_DISTANCE(a.geom,SDO_GEOMETRY("
                                +"2001,"
                                +"4326,"
                                +"SDO_POINT_TYPE("+log+","+lat+",NULL),"
                                +"NULL,"
                                +"NULL"
                                +"),"
                                +"'DISTANCE="+dis+" UNIT=M'"
                                +")='TRUE'";
            Logging.Info("CompetitivenessPointQuery", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            int ans=0;
            if(reader.RowSize==0)
            {
                conn.Close();
                return -1;
            }
            try
            {
                
               if(reader.Read())
               {
                   ans=reader.GetInt32(0);
               }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            return ans;
        
        }
    }
}