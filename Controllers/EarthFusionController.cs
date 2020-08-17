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
    //路由设置
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EarthFusionController : ControllerBase
    {
        [HttpGet]
        public Session GetLoginSession(string username, string password)
        {
            Session httpResponse = new Session();
            Logging.Info("request", "Received request for GetLoginSession");
            Logging.Info("GetLoginSession", "username: " + username);
            httpResponse.Date = DateTime.Now;
            int statusCode = (int)HttpStatusCode.OK;
            Logging.Info("GetLoginSession", "Begins.");
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
                    sessionId = GenericHelpers.CreateMD5(random.Next(114514, 1919810).ToString());
                    string sessionKeyName = "EARTH_FUSION_SESSION_" + sessionId;
                    if (RedisHelpers.GetString(sessionKeyName) == null)
                    {
                        RedisHelpers.SetString(sessionKeyName, userInformation.userId.ToString());
                        // three hour
                        RedisHelpers.SetKeyExpireTime(sessionKeyName, 3 * 60 * 60);
                        break;
                    }
                }
                httpResponse.sessionId = sessionId;
            }
            httpResponse.StatusCode = statusCode;
            this.HttpContext.Response.StatusCode = statusCode;
            Logging.Info("request", "Reponse returned for GetLoginSession");
            return httpResponse;
        }

        [HttpGet]
        public UserInformationHttp GetSession(string sessionId)
        {
            UserInformationHttp httpResponse = new UserInformationHttp();
            Logging.Info("request", "Received request for GetSession");
            Logging.Info("GetSession", "sessionId: " + sessionId);
            httpResponse.Date = DateTime.Now;
            int statusCode = (int)HttpStatusCode.OK;
            Logging.Info("GetSession", "Begins.");
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
        public RegisterResult RegisterAccount(string username, string password)
        {
            RegisterResult httpResponse = new RegisterResult();
            httpResponse.Date = DateTime.Now;
            int statusCode = (int)HttpStatusCode.OK;
            Logging.Info("request", "Received request for RegisterAccount");
            if (username == null || password == null)
            {
                httpResponse.Message = "Register failed. Something you provided is null.";
                Logging.Info("RegisterAccount", httpResponse.Message);
                statusCode = (int)HttpStatusCode.InternalServerError;
            }
            else
            {
                Logging.Info("RegisterAccount", "username: " + username);
                Logging.Info("RegisterAccount", "Begins.");
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
        public RegisterResult RegisterAccountAlt(RegisterRequest httpRequest)
        {
            string username = httpRequest.Username;
            string password = httpRequest.Password;
            RegisterResult httpResponse = new RegisterResult();
            Logging.Info("request", "Received request for RegisterAccount");
            Logging.Info("RegisterAccountAlt", "username: " + username);
            httpResponse.Date = DateTime.Now;
            int statusCode = (int)HttpStatusCode.OK;
            Logging.Info("RegisterAccountAlt", "Begins.");
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
    }
}