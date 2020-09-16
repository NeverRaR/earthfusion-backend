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
        [HttpPost]
        public ReportTag UploadTrafficAccessibilityReport(string sessionId,double ulLongitude,double ulLatitude,double lrLongitude,double lrLatitude,String date,String trafficAccessibility,String busAccessibility,String competitiveness,int rowNum,int colNum)
        {
            UserInformation user =SessionHelpers.Validate(sessionId);
            if(user==null) return null;
            TrafficAccessibilityReport report=new TrafficAccessibilityReport();
            report.userId=user.userId;
            report.ulLongitude=ulLongitude;
            report.ulLatitude=ulLatitude;
            report.lrLongitude=lrLongitude;
            report.lrLatitude=lrLatitude;
            report.date=date;
            report.rowNum=rowNum;
            report.colNum=colNum;
            report.trafficAccessibility=trafficAccessibility;
            report.busAccessibility=busAccessibility;
            ReportTag tag=new ReportTag();
            tag.reportId=OracleHelpers.InsertTrafficAccessibilityReport(report);
            tag.date=date;
            return tag;            
        }
        [HttpPost]
        public ReportTag UploadGDPReport(string sessionId,String name,String date, String arima, String holtWinters, String holt, int bYear, int eYear)
        {
            UserInformation user = SessionHelpers.Validate(sessionId);
            if (user == null) return null;
            GDPReport report = new GDPReport();
            report.userId = user.userId;
            report.date = date;
            report.arima = arima;
            report.holt = holt;
            report.holtWinters = holtWinters;
            report.bYear = bYear;
            report.eYear = eYear;
            report.name = name;
            ReportTag tag = new ReportTag();
            tag.reportId = OracleHelpers.InsertGDPReport(report);
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
        public TrafficAccessibilityReport AnalysisTrafficAccessibilityReport(string sessionId,double ullog,double ullat,double lrlog,double lrlat,int rowNum,int colNum)
        {
            UserInformation user =SessionHelpers.Validate(sessionId);
            if(user==null) return null;
            if(rowNum*colNum>200) return null;
            TrafficAccessibilityReport report=new TrafficAccessibilityReport();
            report.userId=user.userId;
            report.rowNum=rowNum;
            report.colNum=colNum;
            report.date=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            report.ulLongitude=ullog;
            report.ulLatitude=ullat;
            report.lrLongitude=lrlog;
            report.lrLatitude=lrlat;
            int i,j;
            double dellog=(lrlog-ullog)/colNum;
            double dellat=(ullat-lrlat)/rowNum;
            for(i=0;i<colNum;i++)
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
                    int cbusAccessibility = OracleHelpers.BusAccessibilityRegionQuery(cullog, cullat, clrlog, clrlat) * 80
                                                    + OracleHelpers.BusAccessibilityRegionQuery(cullog - 4.5 * dellog, cullat + 4.5 * dellog, clrlog + 4.5 * dellog, clrlat - 4.5 * dellog);
                    report.trafficAccessibility += ctrafficAccessibilit + ",";
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
        public TrafficAccessibilityReport GetTrafficAccessibilityReportByReportID(string sessionId, int reportId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            if(user==null) return null;
            TrafficAccessibilityReport report=new TrafficAccessibilityReport();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string QueryString = "select * from nemo.TrafficAccessibilityReport where user_id=" + user.userId+" AND ta_report_id="+reportId;
            Logging.Info("GetTrafficAccessibilityReportByReportID", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            if(reader.RowSize==0) return null;
            try
            {
                while (reader.Read())
                {
                    report.userId=reader.GetInt32(0);
                    report.reportId=reader.GetInt32(1);
                    report.ulLongitude=reader.GetFloat(2);
                    report.ulLatitude=reader.GetFloat(3);
                    report.lrLongitude=reader.GetInt32(4);
                    report.lrLatitude=reader.GetInt32(5);
                    report.date=reader.GetDateTime(6).ToString("yyyy-MM-dd HH:mm:ss");
                    report.rowNum=reader.GetInt32(7);
                    report.colNum=reader.GetInt32(8);
                    report.trafficAccessibility=reader.GetString(9);
                    report.busAccessibility=reader.GetString(10);
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
        public GDPReport GetGDPReportByReportID(string sessionId, int reportId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            if (user == null) return null;
            GDPReport report = new GDPReport();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string QueryString = "select * from nemo.GDPReport where user_id=" + user.userId + " AND GDP_report_id=" + reportId;
            Logging.Info("GetGDPReportByReportID", "Constructed query: " + QueryString);

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
                    report.reportId = reader.GetInt32(0);
                    report.userId = reader.GetInt32(1);
                    report.bYear = reader.GetInt32(2);
                    report.eYear = reader.GetInt32(3);
                    report.holtWinters = reader.GetString(4);
                    report.arima = reader.GetString(5);
                    report.holt = reader.GetString(6);
                    report.name = reader.GetString(7);
                    report.date = reader.GetDateTime(8).ToString("yyyy-MM-dd HH:mm:ss");
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
        [HttpGet]
        public UserIDWithAllReport GetAllTrafficAccessibilityReport(string sessionId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            if(user==null) return null;
            TrafficAccessibilityReport report=new TrafficAccessibilityReport();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string QueryString = "select ta_report_id,ta_report_time from nemo.TrafficAccessibilityReport where user_id=" + user.userId;
            Logging.Info("GetAllTrafficAccessibilityReport", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();
            UserIDWithAllReport result=new UserIDWithAllReport();
            result.userId=user.userId;
            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                   ReportTag temp=new ReportTag();
                   temp.reportId=reader.GetInt32(0);
                   temp.date=reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm:ss");  
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
        public UserIDWithAllReport GetAllGDPReport(string sessionId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            if(user==null) return null;
            TrafficAccessibilityReport report=new TrafficAccessibilityReport();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string QueryString = "select GDP_report_id,GDP_report_time from nemo.GDPReport where user_id=" + user.userId;
            Logging.Info("GetAllGDPReport", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();
            UserIDWithAllReport result=new UserIDWithAllReport();
            result.userId=user.userId;
            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                   ReportTag temp=new ReportTag();
                   temp.reportId=reader.GetInt32(0);
                   temp.date=reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm:ss");  
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
        
        [HttpDelete]
        public CommonResponse DeleteTrafficAccessibilityReport(String sessionId,int reportId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            CommonResponse response=new CommonResponse();
            response.statusCode=403;
            response.message="sessionId is invalid.";
            response.date=DateTime.Now;
            if(user==null) return response;
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string DeleteString = "delete from nemo.TrafficAccessibilityReport where user_id=" + user.userId+" AND ta_report_id="+reportId;
            Logging.Info("DeleteTrafficAccessibilityReport", "Constructed delete: " + DeleteString);

            // constructs command from string
            OracleCommand command = new OracleCommand(DeleteString, conn);

            // open db connection
            conn.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Logging.Warning("DeleteTrafficAccessibilityReport","an exception "+e.ToString());
            }
            finally
            {
                conn.Close();
            }
            response.statusCode=200;
            response.message="OK";
            return response;
        }
        [HttpDelete]
        public CommonResponse DeleteGDPReport(String sessionId,int reportId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            CommonResponse response=new CommonResponse();
            response.statusCode=403;
            response.message="sessionId is invalid.";
            response.date=DateTime.Now;
            if(user==null) return response;
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string DeleteString = "delete from nemo.GDPReport where user_id=" + user.userId+" AND GDP_report_id="+reportId;
            Logging.Info("DeleteGDPReport", "Constructed delete: " + DeleteString);

            // constructs command from string
            OracleCommand command = new OracleCommand(DeleteString, conn);

            // open db connection
            conn.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Logging.Warning("DeleteGDPReport","an exception "+e.ToString());
            }
            finally
            {
                conn.Close();
            }
            response.statusCode=200;
            response.message="OK";
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

        [HttpGet]
        public BusLineNum GetBusLineNum()
        {
            BusLineNum response=new BusLineNum();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select count(*)"
                                +" from (select distinct LINE_NAME from nemo.BUS_ROUTE a )";
            Logging.Info("GetBusLineNum", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            if(reader.RowSize==0)
            {
                reader.Close();
                conn.Close();
                return null;
            }
            try
            {
                
               if(reader.Read())
               {
                   response.num=reader.GetInt32(0);
               }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            return response;
        }
        [HttpGet]
        public BusStationNum GetBusStationNum()
        {
            BusStationNum response=new BusStationNum();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select count(*)"
                                +" from nemo.BUS_STATION";
            Logging.Info("GetBusStationNum", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            if(reader.RowSize==0)
            {
                reader.Close();
                conn.Close();
                return null;
            }
            try
            {
                
               if(reader.Read())
               {
                   response.num=reader.GetInt32(0);
               }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            return response;
        }
        [HttpGet]
        public ShopNum GetShopNum()
        {
            ShopNum response=new ShopNum();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select count(*)"
                                +" from nemo.SHANGHAI_SHOPS";
            Logging.Info("GetShopNum", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            if(reader.RowSize==0)
            {
                reader.Close();
                conn.Close();
                return null;
            }
            try
            {
                
               if(reader.Read())
               {
                   response.num=reader.GetInt32(0);
               }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            return response;
        }
        [HttpGet]
        public RegionNum GetRegionNum()
        {
            RegionNum response=new RegionNum();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select count(*)"
                                +" from (select distinct COUNTRY_OR_REGION from nemo.gdp_record a )";
            Logging.Info("GetRegionNum", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            if(reader.RowSize==0)
            {
                reader.Close();
                conn.Close();
                return null;
            }
            try
            {
                
               if(reader.Read())
               {
                   response.num=reader.GetInt32(0);
               }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            return response;
        }
        [HttpGet]
        public BusLineNameList GetBusLineName(int offset)
        {
            BusLineNameList response=new BusLineNameList();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select ln from "
                                +"("
                                +"select ROWNUM as rn,LINE_NAME ln "
                                +"from (select distinct LINE_NAME from nemo.BUS_ROUTE)"
                                +"where ROWNUM<"+(offset+51)
                                +")"
                                +"where rn>"+offset;
            Logging.Info("GetBusLineName", "Constructed query: " + QueryString);
            response.offset=offset;

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                
               while(reader.Read())
               {
                 
                   response.names.Add(reader.GetString(0));
               }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            return response;
        }
        [HttpGet]
        public RegionList GetRegionName(int offset)
        {
            RegionList response=new RegionList();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select cr from "
                                +"("
                                +"select ROWNUM as rn,COUNTRY_OR_REGION cr "
                                +"from (select distinct COUNTRY_OR_REGION from nemo.gdp_record)"
                                +"where ROWNUM<"+(offset+51)
                                +")"
                                +"where rn>"+offset;
            Logging.Info("GetBusLineName", "Constructed query: " + QueryString);
            response.offset=offset;

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                
               while(reader.Read())
               {
                 
                   response.names.Add(reader.GetString(0));
               }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            return response;
        }
        [HttpGet]
        public ShopTagList GetShopTag(int offset)
        {
            ShopTagList response=new ShopTagList();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select sn,lon,lat,studio_id from "
                                +"("
                                +"select ROWNUM as rn,NAME sn,lon,lat,studio_id "
                                +"from nemo.SHANGHAI_SHOPS "
                                +"where ROWNUM<"+(offset+51)
                                +")"
                                +"where rn>"+offset;
            Logging.Info("GetShopName", "Constructed query: " + QueryString);
            response.offset=offset;

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                
               while(reader.Read())
               {
                   ShopTag temp=new ShopTag();
                   temp.name=reader.GetString(0);
                   temp.Longitude=reader.GetFloat(1);
                   temp.Latitude=reader.GetFloat(2);
                   temp.id=reader.GetInt32(3);
                   response.tags.Add(temp);
               }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            return response;
        }
        [HttpGet]
        public ShopSearchResult GetShopDetail(int id)
        {
            string oracleUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_DB_USERNAME"];
            string oraclePassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_DB_PASSWORD"];

            // conn to use
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleUsername, oraclePassword, false);

            // List to return
            ShopSearchResult response = new ShopSearchResult();

            string testQueryString = "SELECT DISTINCT name, address, lat, lon, area,telephone,studio_id from nemo.SHANGHAI_SHOPS where studio_id="+id;

            Logging.Info("ShopSearchExact.Search", "Constructed query: " + testQueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(testQueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                if(reader.Read())
                {
                    response.ShopName = reader.GetString(0);
                    response.ShopAddress = reader.GetString(1);
                    response.Latitude = reader.GetDouble(2);
                    response.Longitude = reader.GetDouble(3);
                    if(!reader.IsDBNull(4))response.District = reader.GetString(4);
                    if(!reader.IsDBNull(5))response.telephone=reader.GetString(5);
                    response.id=reader.GetInt32(6);
                }
            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            conn.Close();
            return response;
        }
        [HttpGet]
        public RegionWithGDP GetRegionWithGDP(String name)
        {
            RegionWithGDP response=new RegionWithGDP();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select COUNTRY_OR_REGION,YEAR,GDP from "
                               +"nemo.gdp_record "
                               +"where COUNTRY_OR_REGION='"+name+"'";
            Logging.Info("GetRegionWithGDP", "Constructed query: " + QueryString);
            response.name=name;
            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                
               while(reader.Read())
               {
                   GDPTag temp=new GDPTag();
                   temp.name=reader.GetString(0);
                   temp.year=reader.GetInt32(1);
                   temp.GDP=reader.GetInt64(2);
                   response.tags.Add(temp);
               }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            return response;
        }
        
        [HttpPatch]
        public CommonResponse ChangeBusLineName(String oldName,String newName,String sessionId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            CommonResponse response=new CommonResponse();
            response.statusCode=403;
            response.message="sessionId is invalid.";
            response.date=DateTime.Now;
            if(user==null) return response;
            if(user.role!="administrator") 
            {
                response.statusCode=401;
                response.message="user is not a admin.";
                return response;
            }
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string UpdateString = "update  nemo.BUS_ROUTE set LINE_NAME='"+newName +"' where LINE_NAME='"+oldName+"'";
            Logging.Info("ChangeBusLineName", "Constructed update: " + UpdateString);

            // constructs command from string
            OracleCommand command = new OracleCommand(UpdateString, conn);

            // open db connection
            conn.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Logging.Warning("ChangeBusLineName","an exception "+e.ToString());
            }
            finally
            {
                conn.Close();
            }
            response.statusCode=200;
            response.message="OK";
            return response;
        }
        [HttpPost]
        public CommonResponse AddGDPTag(String sessionId,String name,int year,Int64 GDP)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            CommonResponse response=new CommonResponse();
            response.statusCode=403;
            response.message="sessionId is invalid.";
            response.date=DateTime.Now;
            if(user==null) return response;
            if(user.role!="administrator") 
            {
                response.statusCode=401;
                response.message="user is not a admin.";
                return response;
            }
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string InsertString = "insert into nemo.gdp_record VALUES('"+name+"',"+year+","+GDP+")";
            Logging.Info("AddGDPTag", "Constructed insert: " + InsertString);

            // constructs command from string
            OracleCommand command = new OracleCommand(InsertString, conn);

            // open db connection
            conn.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Logging.Warning("AddGDPTag","an exception "+e.ToString());
            }
            finally
            {
                conn.Close();
            }
            response.statusCode=200;
            response.message="OK";
            return response;
        }
        [HttpPost]
        public CommonResponse AddShop(String sessionId,String shopName,String shopAddress,double log,double lat,String district,String telephone)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            CommonResponse response=new CommonResponse();
            response.statusCode=403;
            response.message="sessionId is invalid.";
            response.date=DateTime.Now;
            if(user==null) return response;
            if(user.role!="administrator") 
            {
                response.statusCode=401;
                response.message="user is not a admin.";
                return response;
            }
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            int id=0;
            string QueryString = "select max(studio_id) "
                                +" from nemo.SHANGHAI_SHOPS a "; 
            Logging.Info("AddShop", "Constructed query: " + QueryString);
                  
           

            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            OracleDataReader reader = command.ExecuteReader();
            try
            {
                
               if(reader.Read())
               {
                   if(!reader.IsDBNull(0))
                   {
                        id=1+reader.GetInt32(0);
                   }
               }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            string InsertString = "insert into nemo.SHANGHAI_SHOPS VALUES(null,'"+shopName+"','"+shopAddress
                                 +"',"+lat+","+log+",null,null,'"+district+"','"+telephone+"',null,"+id+","
                                 +"SDO_GEOMETRY("
                                 +"2001,"
                                 +"4326,"
                                 +"SDO_POINT_TYPE("+log+","+lat+",NULL),"
                                 +"NULL,"
                                 +"NULL"
                                 +"))";
            Logging.Info("AddShop", "Constructed insert: " + InsertString);
            command=new OracleCommand(InsertString, conn);
            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Logging.Warning("AddShop","an exception "+e.ToString());
            }
            finally
            {
                conn.Close();
            }
            response.statusCode=200;
            response.message="OK";
            return response;
        }
        [HttpPost]
        public CommonResponse AddStationToBusLine(String lineName,String stationName,int sequence,String sessionId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            CommonResponse response=new CommonResponse();
            response.statusCode=403;
            response.message="sessionId is invalid.";
            response.date=DateTime.Now;
            if(user==null) return response;
            if(user.role!="administrator") 
            {
                response.statusCode=401;
                response.message="user is not a admin.";
                return response;
            }
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string InsertString = "insert into nemo.BUS_ROUTE VALUES('"+stationName+"',"+sequence+",'"+lineName+"')";
            Logging.Info("AddStationToBusLine", "Constructed insert: " + InsertString);

            // constructs command from string
            OracleCommand command = new OracleCommand(InsertString, conn);

            // open db connection
            conn.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Logging.Warning("AddStationToBusLine","an exception "+e.ToString());
            }
            finally
            {
                conn.Close();
            }
            response.statusCode=200;
            response.message="OK";
            return response;
        }
        [HttpDelete]
        public CommonResponse RemoveShop(int id,String sessionId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            CommonResponse response=new CommonResponse();
            response.statusCode=403;
            response.message="sessionId is invalid.";
            response.date=DateTime.Now;
            if(user==null) return response;
            if(user.role!="administrator") 
            {
                response.statusCode=401;
                response.message="user is not a admin.";
                return response;
            }
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string DeleteString = "delete from nemo.SHANGHAI_SHOPS where studio_id="+id+"";
            Logging.Info("RemoveShop", "Constructed delete: " + DeleteString);

            // constructs command from string
            OracleCommand command = new OracleCommand(DeleteString, conn);

            // open db connection
            conn.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Logging.Warning("RemoveShop","an exception "+e.ToString());
            }
            finally
            {
                conn.Close();
            }
            response.statusCode=200;
            response.message="OK";
            return response;
        }
        [HttpDelete]
        public CommonResponse RemoveBusLine(String lineName,String sessionId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            CommonResponse response=new CommonResponse();
            response.statusCode=403;
            response.message="sessionId is invalid.";
            response.date=DateTime.Now;
            if(user==null) return response;
            if(user.role!="administrator") 
            {
                response.statusCode=401;
                response.message="user is not a admin.";
                return response;
            }
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string DeleteString = "delete from nemo.BUS_ROUTE where line_name='"+lineName+"'";
            Logging.Info("RemoveBusLine", "Constructed delete: " + DeleteString);

            // constructs command from string
            OracleCommand command = new OracleCommand(DeleteString, conn);

            // open db connection
            conn.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Logging.Warning("RemoveBusLine","an exception "+e.ToString());
            }
            finally
            {
                conn.Close();
            }
            response.statusCode=200;
            response.message="OK";
            return response;
        }
        [HttpDelete]
        public CommonResponse RemoveStationFromBusLine(String lineName,String stationName,int sequence,String sessionId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            CommonResponse response=new CommonResponse();
            response.statusCode=403;
            response.message="sessionId is invalid.";
            response.date=DateTime.Now;
            if(user==null) return response;
            if(user.role!="administrator") 
            {
                response.statusCode=401;
                response.message="user is not a admin.";
                return response;
            }
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string DeleteString = "delete from nemo.BUS_ROUTE where station_name='"+stationName+"' and line_name='"+lineName+"' and sequence="+sequence;
            Logging.Info("AddStationToBusLine", "Constructed delete: " + DeleteString);

            // constructs command from string
            OracleCommand command = new OracleCommand(DeleteString, conn);

            // open db connection
            conn.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Logging.Warning("RemoveStationFromBusLine","an exception "+e.ToString());
            }
            finally
            {
                conn.Close();
            }
            response.statusCode=200;
            response.message="OK";
            return response;
        }
         [HttpDelete]
        public CommonResponse RemoveGDPTag(String name,int year,String sessionId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            CommonResponse response=new CommonResponse();
            response.statusCode=403;
            response.message="sessionId is invalid.";
            response.date=DateTime.Now;
            if(user==null) return response;
            if(user.role!="administrator") 
            {
                response.statusCode=401;
                response.message="user is not a admin.";
                return response;
            }
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string DeleteString = "delete from nemo.GDP_RECORD where COUNTRY_OR_REGION='"+name+"' and year="+year;
            Logging.Info("RemoveGDPTag", "Constructed delete: " + DeleteString);

            // constructs command from string
            OracleCommand command = new OracleCommand(DeleteString, conn);

            // open db connection
            conn.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Logging.Warning("RemoveGDPTag","an exception "+e.ToString());
            }
            finally
            {
                conn.Close();
            }
            response.statusCode=200;
            response.message="OK";
            return response;
        }
        [HttpGet] 
        public BusRoute GetBusRoute(String lineName)
        {
            BusRoute response=new BusRoute();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select station_name,sequence from nemo.bus_route where line_name='"+lineName+"'";
            Logging.Info("GetBusRoute", "Constructed query: " + QueryString);
            response.lineName=lineName;         
            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                
               while(reader.Read())
               {
                 
                   response.allStations.Add(new BusStationTag(reader.GetInt32(1),reader.GetString(0)));
               }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            return response;
        }
        [HttpGet] 
        public StationWithLine GetStationWithLine(String stationName,int sequence)
        {
            StationWithLine response=new StationWithLine();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select line_name from nemo.bus_route where station_name='"+stationName+"' and sequence="+sequence;
            Logging.Info("GetBusRoute", "Constructed query: " + QueryString);
            response.stationName=stationName;
            response.sequence=sequence;         
            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                
               while(reader.Read())
               {
                 
                   response.allLines.Add(reader.GetString(0));
               }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            return response;
        }
        [HttpGet]
        public BusStationTagList GetBusStation(int offset)
        {
            BusStationTagList response=new BusStationTagList();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select sn,seq from "
                                +"("
                                +"select ROWNUM as rn,STATION_NAME sn,SEQUENCE seq "
                                +"from nemo.BUS_STATION "
                                +"where ROWNUM<"+(offset+51)
                                +")"
                                +"where rn>"+offset;
            Logging.Info("GetBusStation", "Constructed query: " + QueryString);
            response.offset=offset;

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();

            // then, executes the data reader
            OracleDataReader reader = command.ExecuteReader();
            try
            {
               while(reader.Read())
               {
                  response.tags.Add(new BusStationTag(reader.GetInt32(1),reader.GetString(0)));
               }

            }
            finally
            {
                // always call Close when done reading.
                reader.Close();
            }
            return response;
        }

        [HttpGet]
        public BusStationList GetAllBusStation()
        {
            BusStationList response=new BusStationList();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select STATION_NAME,SEQUENCE,SDO_GEOMETRY.get_wkt(geom) from nemo.BUS_STATION";
            Logging.Info("GetAllBusStation", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                while(reader.Read())
                {
                    BusStation temp=new BusStation();
                    temp.stationName=reader.GetString(0);
                    temp.sequence=reader.GetInt32(1);
                    temp.wkt=reader.GetString(2);
                    response.stations.Add(temp);
                }
            }
            finally
            {
                conn.Close();
            }
            response.num=response.stations.Count;
            return response;
        }
        [HttpDelete]
        public CommonResponse RemoveBusStation(String stationName,int sequence,String sessionId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            CommonResponse response=new CommonResponse();
            response.statusCode=403;
            response.message="sessionId is invalid.";
            response.date=DateTime.Now;
            if(user==null) return response;
            if(user.role!="administrator")
            {
                response.statusCode=401;
                response.message="user is not a admin.";
                return response;
            }
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string DeleteString = "delete from nemo.BUS_STATION where station_name='"+stationName+"' and sequence="+sequence;
            Logging.Info("RemoveBusStation", "Constructed delete: " + DeleteString);

            // constructs command from string
            OracleCommand command = new OracleCommand(DeleteString, conn);

            // open db connection
            conn.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Logging.Warning("RemoveBusStation","an exception "+e.ToString());
            }
            finally
            {
                conn.Close();
            }
            response.statusCode=200;
            response.message="OK";
            return response;
        }
        [HttpPost]
        public CommonResponse AddBusStation(String stationName,int sequence,double log,double lat,String sessionId)
        {
            UserInformation user = GetSession(sessionId).userInformation;
            CommonResponse response=new CommonResponse();
            response.statusCode=403;
            response.message="sessionId is invalid.";
            response.date=DateTime.Now;
            if(user==null) return response;
            if(user.role!="administrator") 
            {
                response.statusCode=401;
                response.message="user is not a admin.";
                return response;
            }
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);

            string InsertString = "insert into nemo.BUS_STATION VALUES('"+stationName+"',"+sequence+","
                                +"SDO_GEOMETRY("
                                +"2001,"
                                +"4326,"
                                +"SDO_POINT_TYPE("+log+","+lat+",NULL),"
                                +"NULL,"
                                +"NULL"
                                +"))";
            Logging.Info("AddBusStation", "Constructed insert: " + InsertString);

            // constructs command from string
            OracleCommand command = new OracleCommand(InsertString, conn);

            // open db connection
            conn.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Logging.Warning("AddBusStation","an exception "+e.ToString());
            }
            finally
            {
                conn.Close();
            }
            response.statusCode=200;
            response.message="OK";
            return response;
        }
        [HttpGet]
        public BusStation GetBusStationById(string stationName,int sequence)
        {
            BusStation response=new BusStation();
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select STATION_NAME,SEQUENCE,SDO_GEOMETRY.get_wkt(geom) from nemo.BUS_STATION where station_name='"+stationName+"' and sequence="+sequence;
            Logging.Info("GetBusStationById", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                if(reader.Read())
                {
                     response.stationName=reader.GetString(0);
                     response.sequence=reader.GetInt32(1);
                     response.wkt=reader.GetString(2);
                }
            }
            finally
            {
                conn.Close();
            }
            return response;
        }
        [HttpGet]
        public BusStationList SearchBusStation(String key)
        {
            BusStationList response=new BusStationList();
            if(key==null||key.Length<1) return null;
            string oracleSpatialAdminUsername = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_USERNAME"];
            string oracleSpatialAdminPassword = earthfusion_backend.Globals.config["EARTH_FUSION_SPATIAL_ADMIN_DB_PASSWORD"];
            OracleConnection conn = OracleHelpers.GetOracleConnection(oracleSpatialAdminUsername, oracleSpatialAdminPassword, false);
            string QueryString = "select STATION_NAME,SEQUENCE,SDO_GEOMETRY.get_wkt(geom) from nemo.BUS_STATION where station_name like '%"+key+"%'";
            Logging.Info("SearchBusStation", "Constructed query: " + QueryString);

            // constructs command from string
            OracleCommand command = new OracleCommand(QueryString, conn);

            // open db connection
            conn.Open();
            OracleDataReader reader = command.ExecuteReader();
            try
            {
                while(reader.Read())
                {
                    response.stations.Add(new BusStation(reader.GetInt32(1),reader.GetString(0),reader.GetString(2)));
                }
            }
            finally
            {
                conn.Close();
            }
            response.num=response.stations.Count();
            return response;
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
                statusCode = httpResponse.Result.BoolResult ? (int)HttpStatusCode.OK : (int)HttpStatusCode.Forbidden;
            }
            httpResponse.StatusCode = statusCode;
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
                statusCode = httpResponse.Result.BoolResult ? (int)HttpStatusCode.OK : (int)HttpStatusCode.Forbidden;
            }
            httpResponse.StatusCode = statusCode;
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