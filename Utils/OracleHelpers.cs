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

        public static bool IsColumnNameExistsInTableName(OracleConnection conn, string tableName, string columnName)
        {
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
    }
}