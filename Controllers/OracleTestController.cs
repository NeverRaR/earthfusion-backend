using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Utils;
using Oracle.ManagedDataAccess.Client;
using OracleTest;

namespace OracleTest.Controllers
{
    //路由设置
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OracleTestController : ControllerBase
    {
        [HttpGet]
        
        // This self-defined request should use Content-Type: applicaion/json. 
        public IEnumerable<OracleConnectionTestResult> DatabaseConnectionTest(OracleConnectionTestRequest request)
        {
            // init variables
            string message;
            int statusCode;
            try
            {
                message = OracleTests.ConnectionTest(request.Username, request.Password, false);
                statusCode = (int)HttpStatusCode.OK;
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException e)
            {
                // truncate to just the first line
                message = e.ToString().Split(new[] { '\r', '\n' }).FirstOrDefault();
                statusCode = (int)HttpStatusCode.Forbidden;
            }
            this.HttpContext.Response.StatusCode = statusCode;
            return Enumerable.Range(1, 1).Select(index => new OracleConnectionTestResult
            {
                Date = DateTime.Now.AddDays(index),
                StatusCode = statusCode,
                Username = request.Username,
                Message = message
            })
            .ToArray();
        }

        [HttpGet]
        public OracleWktResponse PullFromTableWithNum(string Username, string Password, string Tablename, int RowCount)
        {
            OracleConnectionTestPullRequest request = new OracleConnectionTestPullRequest
            {
                Username = Username,
                Password = Password,
                Tablename = Tablename,
                RowCount = RowCount
            };
            OracleWktResponse httpResponse = new OracleWktResponse();
            string message = "Okay....";
            int statusCode;
            httpResponse.TableName = request.Tablename;
            httpResponse.Date = DateTime.Now;
            List<WktWithName> result;

            Logging.Info("request", "Received request for PullFromTableWithNum");
            Logging.Info("PullFromTableWithNum", "Tablename: " + request.Tablename);
            Logging.Info("PullFromTableWithNum", "Row count to pull from: " + request.RowCount.ToString());
            Logging.Info("PullFromTableWithNum", "Pulling process begins.");

            try
            {
                // get result
                result = OracleTests.WktPullTest(request.Username, request.Password, request.Tablename, request.RowCount);
                
                // if no row seleted...
                if (result.Count() == 0)
                {
                    Logging.Warning("PullFromTableWithNum", "Pulling process returned with 0 row when trying to pull from " + Tablename);
                    message = "No row(s) selected. There maybe no geometry data in column GEOM or that column doesn't exist.";
                }
                statusCode = (int)HttpStatusCode.OK;
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException e)
            {
                // if something happened within Oracle
                message = e.ToString().Split(new[] { '\r', '\n' }).FirstOrDefault();
                statusCode = (int)HttpStatusCode.InternalServerError;
                Logging.Warning("PullFromTableWithNum", "Pulling process got something bad when pulling from " + Tablename + " with a message of " + message);
                result = new List<WktWithName>();
            }
            Logging.Info("PullFromTableWithNum", "Pulling process ends.");
            this.HttpContext.Response.StatusCode = statusCode;

            // constructs content
            foreach (WktWithName content in result)
            {
                httpResponse.Contents.Add(content);
            }
            // // or should i just use the result?
            // httpResponse.Contents = result;
            
            // tests shows there are no statical difference by the means of time. 

            // restore table information with predefined dict (Classes/KnownTables.cs)
            try
            {
                GeomTableTypes currentTableType = OracleTest.Globals.knownTables.knownTableType[Tablename];
                httpResponse.GeomTypeId = currentTableType.GeomTypeId;
                httpResponse.LineOrBoundaryTypeId = currentTableType.LineOrBoundaryTypeId;
                httpResponse.EntityTypeId = currentTableType.EntityTypeId;
                Logging.Info("PullFromTableWithNum", "Got information found for table " + Tablename);
            }
            catch (KeyNotFoundException)
            {
                Logging.Warning("PullFromTableWithNum", "No information found for table " + Tablename);
            }

            httpResponse.Message = message;
            httpResponse.StatusCode = statusCode;
            Logging.Info("request", "Reponse returned for PullFromTableWithNum");
            return httpResponse;
        }

        // [HttpGet]
        // // This is for test purpose only
        // public string CheckWhetherColumnNameExistsInTable(string username, string passwd, string tableName, string columnName)
        // {
        //     OracleConnection conn = OracleHelpers.GetOracleConnection(username, passwd, false);
        //     bool test = OracleHelpers.IsColumnNameExistsInTableName(conn, tableName, columnName);
        //     Console.WriteLine(tableName + " " + columnName + " " + "result: " + test);
        //     return test.ToString();
        // }
    }
}