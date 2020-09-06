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

// Disable ArithmeticExpressionsMustDeclarePrecedence in this file.
// Sometimes those things get so stupid...
#pragma warning disable SA1407 

namespace EarthFusion.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EarthFusionController : ControllerBase
    {
        [HttpGet]
        // public Session Login(string username, string password)
        public Session GetLoginSession(string username, string password)
        {
            Session httpResponse = new Session();
            Logging.Info("request", "Received request for GetLoginSession");
            Logging.Info("Login(GetLoginSession)", "username: " + username);
            httpResponse.Date = DateTime.Now;
            int statusCode = (int)HttpStatusCode.OK;
            Logging.Info("Login(GetLoginSession)", "Begins.");
            if (username == null || password == null)
            {
                httpResponse.Message = "Login failed. Something you provided is null.";
                Logging.Info("Login(GetLoginSession)", httpResponse.Message);
                statusCode = (int)HttpStatusCode.InternalServerError;
            }
            else
            {
                UserInformation userInformation = SessionHelpers.Login(username, password);
                if (userInformation == null)
                {
                    statusCode = (int)HttpStatusCode.Forbidden;
                    httpResponse.Message = "boom";
                }
                else
                {
                    httpResponse.Message = "good";
                    httpResponse.username = userInformation.userName;
                    Random random = new System.Random();
                    string sessionId;
                    while (true)
                    {
                        // fake SHA1
                        sessionId = GenericHelpers.GetRandomHexNumber(40);
                        string sessionKeyName = "EARTH_FUSION_SESSION_" + sessionId;
                        if (RedisHelpers.GetString(sessionKeyName) == null)
                        {
                            RedisHelpers.SetString(sessionKeyName, userInformation.userId.ToString());
                            // expire time: three hour
                            RedisHelpers.SetKeyExpireTime(sessionKeyName, 3 * 60 * 60);
                            break;
                        }
                    }
                    httpResponse.sessionId = sessionId;
                }
            }
            httpResponse.StatusCode = statusCode;
            this.HttpContext.Response.StatusCode = statusCode;
            Logging.Info("request", "Reponse returned for GetLoginSession");
            return httpResponse;
        }

        [HttpGet]
        // public UserInformationHttp Session(string sessionId)
        public UserInformationHttp GetSession(string sessionId)
        {
            UserInformationHttp httpResponse = new UserInformationHttp();
            Logging.Info("request", "Received request for GetSession");
            Logging.Info("Session(GetSession)", "sessionId: " + sessionId);
            httpResponse.Date = DateTime.Now;
            int statusCode = (int)HttpStatusCode.OK;
            Logging.Info("Session(GetSession)", "Begins.");
            UserInformation currentUser = SessionHelpers.Validate(sessionId);
            if (currentUser == null)
            {
                httpResponse.Message = "No such user.";
                statusCode = (int)HttpStatusCode.Forbidden;
            }
            else
            {
                httpResponse.Message = "Okay..";
                httpResponse.userInformation = currentUser;
            }
            httpResponse.StatusCode = statusCode;
            this.HttpContext.Response.StatusCode = statusCode;
            Logging.Info("request", "Reponse returned for GetSession");
            return httpResponse;
        }

        [HttpPost]
        // public RegisterResult Register(string username, string password)
        public RegisterResult RegisterAccount(string username, string password)
        {
            RegisterResult httpResponse = new RegisterResult();
            httpResponse.Date = DateTime.Now;
            int statusCode = (int)HttpStatusCode.OK;
            Logging.Info("request", "Received request for RegisterAccount");
            if (username == null || password == null)
            {
                httpResponse.Message = "Register failed. Something you provided is null.";
                Logging.Info("Register(RegisterAccount)", httpResponse.Message);
                statusCode = (int)HttpStatusCode.InternalServerError;
            }
            else
            {
                Logging.Info("Register(RegisterAccount)", "username: " + username);
                Logging.Info("Register(RegisterAccount)", "Begins.");
                bool result = SessionHelpers.RegisterWithoutEmail(username, password);
                httpResponse.Result = result;
                if (!result)
                {
                    httpResponse.Message = "Register failed. Maybe the username specified has already exists.";
                    statusCode = (int)HttpStatusCode.Forbidden;
                }
                else
                {
                    httpResponse.Message = "Okay..";
                }
            }
            httpResponse.StatusCode = statusCode;
            this.HttpContext.Response.StatusCode = statusCode;
            Logging.Info("request", "Reponse returned for RegisterAccount");
            return httpResponse;
        }

        [HttpPost]
        // public RegisterResult RegisterAlt(RegisterRequest httpRequest)
        public RegisterResult RegisterAccountAlt(RegisterRequest httpRequest)
        {
            string username = httpRequest.Username;
            string password = httpRequest.Password;
            RegisterResult httpResponse = new RegisterResult();
            Logging.Info("request", "Received request for RegisterAccount");
            Logging.Info("RegisterAlt(RegisterAccountAlt)", "username: " + username);
            httpResponse.Date = DateTime.Now;
            int statusCode = (int)HttpStatusCode.OK;
            Logging.Info("RegisterAlt(RegisterAccountAlt)", "Begins.");
            bool result = SessionHelpers.RegisterWithoutEmail(username, password);
            httpResponse.Result = result;
            if (!result)
            {
                httpResponse.Message = "Register failed. Maybe the username specified has already exists.";
                statusCode = (int)HttpStatusCode.Forbidden;
            }
            else
            {
                httpResponse.Message = "Okay..";
            }
            httpResponse.StatusCode = statusCode;
            this.HttpContext.Response.StatusCode = statusCode;
            Logging.Info("request", "Reponse returned for RegisterAccountAlt");
            return httpResponse;
        }

        [HttpPost]
        public BussinessDistrictReport AnalysisBussinessDistricReport(string sessionId, double log, double lat)
        {
            UserInformation user = SessionHelpers.Validate(sessionId);
            if (user == null) return null;
            BussinessDistrictReport report = new BussinessDistrictReport();
            report.userId = user.userId;
            report.longitude = log;
            report.latitude = lat;
            report.reportId = 1;
            report.trafficAccessibility = OracleHelpers.TrafficAccessibilityPointQuery(log, lat, 1000) * 5 + OracleHelpers.TrafficAccessibilityPointQuery(log, lat, 2000);
            report.competitiveness = OracleHelpers.CompetitivenessPointQuery(log, lat);
            report.date = DateTime.Now;
            return report;
        }

        [HttpPost]
        public BussinessVitalityReport AnalysisBussinessVitalityReport(string sessionId, double ullog, double ullat, double lrlog, double lrlat, int rowNum, int colNum)
        {
            UserInformation user = SessionHelpers.Validate(sessionId);
            if (user == null) return null;
            if (rowNum * colNum > 200) return null;
            BussinessVitalityReport report = new BussinessVitalityReport();
            report.userId = user.userId;
            report.rowNum = rowNum;
            report.colNum = colNum;
            report.date = DateTime.Now;
            report.ulLongitude = ullog;
            report.ulLatitude = ullat;
            report.ldLongitude = lrlog;
            report.ldLatitude = lrlat;
            int i, j;
            double dellog = (lrlog - ullog) / colNum;
            double dellat = (ullat - lrlat) / rowNum;
            for (i = 0; i < colNum; i++)
            {
                for (j = 0; j < rowNum; j++)
                {
                    double cullog, cullat, clrlog, clrlat;
                    cullog = ullog + dellog * i;
                    cullat = ullat - dellat * j;
                    clrlog = cullog + dellog;
                    clrlat = cullat - dellat;
                    int ctrafficAccessibilit = OracleHelpers.TrafficAccessibilityRegionQuery(cullog, cullat, clrlog, clrlat) * 80
                                                    + OracleHelpers.TrafficAccessibilityRegionQuery(cullog - 4.5 * dellog, cullat + 4.5 * dellog, clrlog + 4.5 * dellog, clrlat - 4.5 * dellog);
                    int ccompetitiveness = OracleHelpers.CompetitivenessRegionQuery(cullog, cullat, clrlog, clrlat) * 80
                                                    + OracleHelpers.CompetitivenessRegionQuery(cullog - 4.5 * dellog, cullat + 4.5 * dellog, clrlog + 4.5 * dellog, clrlat - 4.5 * dellog);
                    report.trafficAccessibility += ctrafficAccessibilit + ",";
                    report.competitiveness += ccompetitiveness + ",";
                }
            }
            return report;
        }

        [HttpGet]
        public BussinessDistrictReport GetBussinessDistricReportByReportID(string sessionId, int reportId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            if (user == null) return null;
            BussinessDistrictReport report = new BussinessDistrictReport();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = ("select * from nemo.BussinessDistricReport where user_id=" + user.userId).ToString();
            Logging.Info("GetBussinessDistricReportByReportID", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            if (reader.RowSize == 0) return null;
            try
            {
                /*
                   
                    CREATE  TABLE nemo.BussinessDistricReport
                    (
                        user_id int,
                        bd_report_id int,
                        bd_report_log float,
                        bd_report_lat float,
                        bd_report_time date,
                        bd_competitiveness int,
                        bd_traffic_accessibility  int,
                        PRIMARY KEY(bd_report_id)
     
                    )
                */
                while (reader.Read())
                {
                    report.userId = reader.GetInt32(0);
                    report.reportId = reader.GetInt32(1);
                    report.longitude = reader.GetFloat(2);
                    report.latitude = reader.GetFloat(3);
                    report.date = reader.GetDateTime(4);
                    report.competitiveness = reader.GetInt32(5);
                    report.trafficAccessibility = reader.GetInt32(6);
                }
            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            return report;
        }

        [HttpGet]
        public BussinessVitalityReport GetBussinessVitalityReportByReportID(string sessionId, int reportId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            BussinessVitalityReport report = new BussinessVitalityReport();
            report.date = DateTime.Now;
            report.reportId = 1;
            report.userId = 897;
            report.colNum = 2;
            report.rowNum = 2;
            report.trafficAccessibility = "0,20,30,40";
            report.competitiveness = "0,10,23,40";
            return report;
        }

        [HttpGet]
        public UserIDWithAllReport GetAllBussinessVitalityReport(string sessionId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            if (user == null) return null;
            UserIDWithAllReport result = new UserIDWithAllReport();
            ReportTag tag = new ReportTag();
            tag.date = DateTime.Now;
            tag.reportId = 100;
            result.userId = user.userId;
            result.allReports.Add(tag);
            result.allReports.Add(tag);
            return result;
        }

        [HttpGet]
        public UserIDWithAllReport GetAllBussinessDistricReport(string sessionId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            if (user == null) return null;
            UserIDWithAllReport result = new UserIDWithAllReport();
            ReportTag tag = new ReportTag();
            tag.date = DateTime.Now;
            tag.reportId = 100;
            result.userId = user.userId;
            result.allReports.Add(tag);
            result.allReports.Add(tag);
            return result;
        }
    }
}