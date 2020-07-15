using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Utils;

namespace OracleTest.Controllers
{
    //路由设置
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OracleTestController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<OracleConnectionTestResult> DatabaseConnectionTest(OracleConnectionTestRequest request)
        {
            string message;
            int statusCode;
            try
            {
                message = OracleTests.ConnectionTest(request.Username, request.Password, false);
                statusCode = (int)HttpStatusCode.OK;
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException e)
            {
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
                RowCount = RowCount + 1
            };
            OracleWktResponse httpResponse = new OracleWktResponse();
            string message = "Okay....";
            int statusCode;
            httpResponse.TableName = request.Tablename;
            httpResponse.Date = DateTime.Now;
            List<string> result;
            Logging.Info("request", "Received request for PullFromTableWithNum");
            Logging.Info("PullFromTableWithNum", "Tablename: " + request.Tablename);
            Logging.Info("PullFromTableWithNum", "Row count to pull from: " + request.RowCount.ToString());
            Logging.Info("PullFromTableWithNum", "Pulling process begins.");
            try
            {
                result = OracleTests.PullTest(request.Username, request.Password, request.Tablename, request.RowCount);
                statusCode = (int)HttpStatusCode.OK;
            }
            catch (Oracle.ManagedDataAccess.Client.OracleException e)
            {
                message = e.ToString().Split(new[] { '\r', '\n' }).FirstOrDefault();
                statusCode = (int)HttpStatusCode.InternalServerError;
                result = new List<string>();
            }
            Logging.Info("PullFromTableWithNum", "Pulling process ends.");
            this.HttpContext.Response.StatusCode = statusCode;
            foreach (string content in result)
            {
                httpResponse.Contents.Add(content);
            }
            httpResponse.Message = message;
            httpResponse.StatusCode = statusCode;
            Logging.Info("request", "Reponse returned for PullFromTableWithNum");
            return httpResponse;
        }
    }
}