using System;
using System.Collections.Generic;
using Utils;
using Oracle.ManagedDataAccess.Client;

namespace EarthFusion
{
    class ShopSearchExact
    {
        public static List<ShopSearchResult> Search(string query)
        {
            string oracleUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_DB_USERNAME"];
            string oraclePassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_DB_PASSWORD"];

            // conn to use
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleUsername, oraclePassword, false);

            // List to return
            List<ShopSearchResult> contents = new List<ShopSearchResult>();

            string testQueryString = "SELECT DISTINCT name, address, lat, lon, area,telephone,studio_id from nemo.SHANGHAI_SHOPS where lower(name) LIKE '%" + query.ToLower() + "%'";

            Logging.Info("ShopSearchExact.Search", "Constructed query: " + testQueryString);

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
                    ShopSearchResult temp = new ShopSearchResult();
                    temp.ShopName = reader.GetString(0);
                    temp.ShopAddress = reader.GetString(1);
                    temp.Latitude = reader.GetDouble(2);
                    temp.Longitude = reader.GetDouble(3);
                    if(!reader.IsDBNull(4))temp.District = reader.GetString(4);
                    if(!reader.IsDBNull(5))temp.telephone=reader.GetString(5);
                    temp.id=reader.GetInt32(6);
                    // commented this out. see description in ShopSearchResult.cs
                    // now ok.
                    temp.ShopClass = reader.GetString(5);
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