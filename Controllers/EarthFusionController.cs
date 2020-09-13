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
                else if (userInformation.accountStatus == "disabled")
                {
                    statusCode = (int)HttpStatusCode.Forbidden;
                    httpResponse.Message = "Your account is disabled. Please Consult an Administrator.";
                }
                else
                {
                    httpResponse.Message = "good";
                    httpResponse.username = userInformation.userName;
                    Random random = new System.Random();
                    string sessionId;
                    while (true)
                    {
                        // random hex string
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
        [HttpGet]
        public BussinessDistrictReport AnalysisBussinessDistrictReport(string sessionId, double log, double lat)
        {
            UserInformation user = SessionHelpers.Validate(sessionId);
            if (user == null) return null;
            BussinessDistrictReport report = new BussinessDistrictReport();
            report.userId = user.userId;
            report.longitude = log;
            report.latitude = lat;
            report.reportId = -1;
            report.trafficAccessibility = OracleHelpers.TrafficAccessibilityPointQuery(log, lat, 1000) * 5 + OracleHelpers.TrafficAccessibilityPointQuery(log, lat, 2000);
            report.competitiveness = OracleHelpers.CompetitivenessPointQuery(log, lat, 1000) * 5 + OracleHelpers.CompetitivenessPointQuery(log, lat, 2000);
            report.busAccessibility = OracleHelpers.BusAccessibilityPointQuery(log, lat, 1000) * 5 + OracleHelpers.BusAccessibilityPointQuery(log, lat, 2000);
            report.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OracleHelpers.InsertBussinessDistrictReport(report);
            return report;
        }

        [HttpPost]
        public ReportTag UploadBussinessDistrictReport(string sessionId, double longitude, double latitude, String date, int trafficAccessibility, int busAccessibility, int competitiveness)
        {
            UserInformation user = SessionHelpers.Validate(sessionId);
            if (user == null) return null;
            BussinessDistrictReport report = new BussinessDistrictReport();
            report.userId = user.userId;
            report.longitude = longitude;
            report.latitude = latitude;
            report.date = date;
            report.trafficAccessibility = trafficAccessibility;
            report.busAccessibility = busAccessibility;
            report.competitiveness = competitiveness;
            ReportTag tag = new ReportTag();
            tag.reportId = OracleHelpers.InsertBussinessDistrictReport(report);
            tag.date = date;
            return tag;
        }

        [HttpPost]
        public ReportTag UploadBussinessVitalityReport(string sessionId, double ulLongitude, double ulLatitude, double lrLongitude, double lrLatitude, String date, String trafficAccessibility, String busAccessibility, String competitiveness, int rowNum, int colNum)
        {
            UserInformation user = SessionHelpers.Validate(sessionId);
            if (user == null) return null;
            BussinessVitalityReport report = new BussinessVitalityReport();
            report.userId = user.userId;
            report.ulLongitude = ulLongitude;
            report.ulLatitude = ulLatitude;
            report.lrLongitude = lrLongitude;
            report.lrLatitude = lrLatitude;
            report.date = date;
            report.rowNum = rowNum;
            report.colNum = colNum;
            report.trafficAccessibility = trafficAccessibility;
            report.busAccessibility = busAccessibility;
            report.competitiveness = competitiveness;
            ReportTag tag = new ReportTag();
            tag.reportId = OracleHelpers.InsertBussinessVitalityReport(report);
            tag.date = date;
            return tag;
        }
        [HttpGet]
        public BussinessVitalityReport AnalysisBussinessVitalityReport(string sessionId, double ullog, double ullat, double lrlog, double lrlat, int rowNum, int colNum)
        {
            UserInformation user = SessionHelpers.Validate(sessionId);
            if (user == null) return null;
            if (rowNum * colNum > 200) return null;
            BussinessVitalityReport report = new BussinessVitalityReport();
            report.userId = user.userId;
            report.rowNum = rowNum;
            report.colNum = colNum;
            report.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            report.ulLongitude = ullog;
            report.ulLatitude = ullat;
            report.lrLongitude = lrlog;
            report.lrLatitude = lrlat;
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
                    int cbusAccessibility = OracleHelpers.BusAccessibilityRegionQuery(cullog, cullat, clrlog, clrlat) * 80
                                                    + OracleHelpers.BusAccessibilityRegionQuery(cullog - 4.5 * dellog, cullat + 4.5 * dellog, clrlog + 4.5 * dellog, clrlat - 4.5 * dellog);
                    report.trafficAccessibility += ctrafficAccessibilit + ",";
                    report.competitiveness += ccompetitiveness + ",";
                    report.busAccessibility += cbusAccessibility + ",";
                }
            }
            return report;
        }

        [HttpGet]
        public BussinessDistrictReport GetBussinessDistrictReportByReportID(string sessionId, int reportId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            if (user == null) return null;
            BussinessDistrictReport report = new BussinessDistrictReport();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string QueryString = "select * from nemo.BussinessDistrictReport where user_id=" + user.userId + " AND bd_report_id=" + reportId;
            Logging.Info("GetBussinessDistrictReportByReportID", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            if (reader.RowSize == 0) return null;
            try
            {
                while (reader.Read())
                {
                    report.userId = reader.GetInt32(0);
                    report.reportId = reader.GetInt32(1);
                    report.longitude = reader.GetFloat(2);
                    report.latitude = reader.GetFloat(3);
                    report.date = reader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss");
                    report.competitiveness = reader.GetInt32(5);
                    report.trafficAccessibility = reader.GetInt32(6);
                    report.busAccessibility = reader.GetInt32(7);
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
            if (user == null) return null;
            BussinessVitalityReport report = new BussinessVitalityReport();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string QueryString = "select * from nemo.BussinessVitalityReport where user_id=" + user.userId + " AND bv_report_id=" + reportId;
            Logging.Info("GetBussinessVitalityReportByReportID", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            if (reader.RowSize == 0) return null;
            try
            {
                while (reader.Read())
                {
                    report.userId = reader.GetInt32(0);
                    report.reportId = reader.GetInt32(1);
                    report.ulLongitude = reader.GetFloat(2);
                    report.ulLatitude = reader.GetFloat(3);
                    report.lrLongitude = reader.GetInt32(4);
                    report.lrLatitude = reader.GetInt32(5);
                    report.date = reader.GetDateTime(6).ToString("yyyy-MM-dd HH:mm:ss");
                    report.competitiveness = reader.GetString(7);
                    report.rowNum = reader.GetInt32(8);
                    report.colNum = reader.GetInt32(9);
                    report.trafficAccessibility = reader.GetString(10);
                    report.busAccessibility = reader.GetString(11);
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
        public UserIDWithAllReport GetAllBussinessVitalityReport(string sessionId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            if (user == null) return null;
            BussinessVitalityReport report = new BussinessVitalityReport();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string QueryString = "select bv_report_id,bv_report_time from nemo.BussinessVitalityReport where user_id=" + user.userId;
            Logging.Info("GetAllBussinessVitalityReport", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();
            UserIDWithAllReport result = new UserIDWithAllReport();
            result.userId = user.userId;
            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    ReportTag temp = new ReportTag();
                    temp.reportId = reader.GetInt32(0);
                    temp.date = reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm:ss");
                    result.allReports.Add(temp);
                }
            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            return result;
        }

        [HttpGet]
        public UserIDWithAllReport GetAllBussinessDistrictReport(string sessionId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            if (user == null) return null;
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string QueryString = "select bd_report_id,bd_report_time from nemo.BussinessDistrictReport where user_id=" + user.userId;
            Logging.Info("GetAllBussinessDistrictReport", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();
            UserIDWithAllReport result = new UserIDWithAllReport();
            result.userId = user.userId;
            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    ReportTag temp = new ReportTag();
                    temp.reportId = reader.GetInt32(0);
                    temp.date = reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm:ss");
                    result.allReports.Add(temp);
                }
            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            return result;
        }

        [HttpDelete]
        public CommonResponse DeleteBussinessDistrictReport(String sessionId, int reportId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            CommonResponse response = new CommonResponse();
            response.statusCode = 403;
            response.message = "sessionId is invalid.";
            response.date = DateTime.Now;
            if (user == null) return response;
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string DeleteString = "delete from nemo.BussinessDistrictReport where user_id=" + user.userId + " AND bd_report_id=" + reportId;
            Logging.Info("DeleteBussinessDistrictReport", "Constructed delete: " + DeleteString);

            // constructs command from string
            OracleCommand command = new OracleCommand(DeleteString, conn);

            // open db connection
            conn.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Logging.Warning("DeleteBussinessDistrictReport", "an exception " + e.ToString());
            }
            finally
            {
                conn.Close();
            }
            response.statusCode = 200;
            response.message = "OK";
            return response;
        }

        [HttpDelete]
        public CommonResponse DeleteBussinessVitalityReport(String sessionId, int reportId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            CommonResponse response = new CommonResponse();
            response.statusCode = 403;
            response.message = "sessionId is invalid.";
            response.date = DateTime.Now;
            if (user == null) return response;
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string DeleteString = "delete from nemo.BussinessVitalityReport where user_id=" + user.userId + " AND bv_report_id=" + reportId;
            Logging.Info("DeleteBussinessVitalityReport", "Constructed delete: " + DeleteString);

            // constructs command from string
            OracleCommand command = new OracleCommand(DeleteString, conn);

            // open db connection
            conn.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Logging.Warning("DeleteBussinessVitalityReport", "an exception " + e.ToString());
            }
            finally
            {
                conn.Close();
            }
            response.statusCode = 200;
            response.message = "OK";
            return response;
        }

        [HttpGet]
        public ShopSearchResponse SearchShopByExactName(string sessionId, string query)
        {
            ShopSearchResponse httpResponse = new ShopSearchResponse();
            Logging.Info("request", "Received request for ShopSearchExact");
            httpResponse.Date = DateTime.Now;
            int statusCode = (int)HttpStatusCode.OK;
            UserInformation currentUser = SessionHelpers.Validate(sessionId);
            if (currentUser == null)
            {
                httpResponse.Message = "No such user.";
                statusCode = (int)HttpStatusCode.Forbidden;
            }
            else
            {
                httpResponse.Message = "Okay..";
                List<ShopSearchResult> result = ShopSearchExact.Search(query);
                httpResponse.Contents = result;
            }
            httpResponse.StatusCode = statusCode;
            this.HttpContext.Response.StatusCode = statusCode;
            Logging.Info("request", "Reponse returned for ShopSearchExact");
            return httpResponse;
        }

        [HttpPatch]
        public AltAccountResponse AltAccountStatusWithUserId(string sessionId, string userId, string operation)
        {
            AltAccountResponse httpResponse = new AltAccountResponse();
            Logging.Info("request", "Received request for AltAccountStatusWithUserId");
            httpResponse.Date = DateTime.Now;
            int statusCode = (int)HttpStatusCode.OK;
            UserInformation currentUser = SessionHelpers.Validate(sessionId);
            if (currentUser == null)
            {
                httpResponse.Message = "No such user.";
                statusCode = (int)HttpStatusCode.Forbidden;
            }
            else
            {
                httpResponse.Message = "Okay..";
                httpResponse.Result = AccountHelpers.AltAccountStatus(sessionId, Int32.Parse(userId), operation);
            }
            httpResponse.StatusCode = statusCode;
            statusCode = httpResponse.Result.BoolResult ? (int)HttpStatusCode.OK : (int)HttpStatusCode.Forbidden;
            this.HttpContext.Response.StatusCode = statusCode;
            Logging.Info("request", "Reponse returned for AltAccountStatusWithUserId");
            return httpResponse;
        }

        [HttpPatch]
        public AltAccountResponse AltAccountPasswordWithNewPassword(string sessionId, string userId, string password)
        {
            AltAccountResponse httpResponse = new AltAccountResponse();
            Logging.Info("request", "Received request for AltAccountPasswordWithNewPassword");
            httpResponse.Date = DateTime.Now;
            int statusCode = (int)HttpStatusCode.OK;
            UserInformation currentUser = SessionHelpers.Validate(sessionId);
            if (currentUser == null)
            {
                httpResponse.Message = "No such user.";
                statusCode = (int)HttpStatusCode.Forbidden;
            }
            else
            {
                httpResponse.Message = "Okay..";
                httpResponse.Result = AccountHelpers.AltAccountPassword(sessionId, Int32.Parse(userId), password);
            }
            httpResponse.StatusCode = statusCode;
            statusCode = httpResponse.Result.BoolResult ? (int)HttpStatusCode.OK : (int)HttpStatusCode.Forbidden;
            this.HttpContext.Response.StatusCode = statusCode;
            Logging.Info("request", "Reponse returned for AltAccountPasswordWithNewPassword");
            return httpResponse;
        }

        [HttpGet]
        public UserInformationQueryResponse GetAllAccountData(string sessionId)
        {
            UserInformationQueryResponse httpResponse = new UserInformationQueryResponse();
            Logging.Info("request", "Received request for GetAllAccountData");
            httpResponse.Date = DateTime.Now;
            int statusCode = (int)HttpStatusCode.OK;
            UserInformation currentUser = SessionHelpers.Validate(sessionId);
            if (currentUser == null)
            {
                httpResponse.Message = "No such user.";
                statusCode = (int)HttpStatusCode.Forbidden;
            }
            else if (currentUser.role != "administrator")
            {
                httpResponse.Message = "You are not the admin.";
                statusCode = (int)HttpStatusCode.Forbidden;
            }
            else
            {
                httpResponse.Message = "Okay..";
                List<UserInformation> result = AccountHelpers.GetAllUserInformation(sessionId);
                httpResponse.Contents = result;
            }
            httpResponse.StatusCode = statusCode;
            this.HttpContext.Response.StatusCode = statusCode;
            Logging.Info("request", "Reponse returned for GetAllAccountData");
            return httpResponse;
        }


    }
}