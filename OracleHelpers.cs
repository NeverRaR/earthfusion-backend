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
            return conn;
        }
    }
}