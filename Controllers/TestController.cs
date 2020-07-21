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
    // route
    [Route("api-for-test/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // email
        [HttpPost]
        public string TestSendgrid(string receiver)
        {
            if (!Utils.Utils.IsEmailAddressValid(receiver))
            {
                return ("Illegal email address: " + receiver).ToString();
            }
            SendgridHelpers.TestSend(receiver);
            return ("Sent to " + receiver).ToString();
        }

        // redis
        [HttpPost]
        public RedisSetStringResult TestRedisSetString(string keyName, string value)
        {
            RedisSetStringResult httpResponse = new RedisSetStringResult();
            httpResponse.Date = DateTime.Now;
            httpResponse.KeyName = keyName;
            httpResponse.Value = value;
            int statusCode = (int)HttpStatusCode.OK;
            if (RedisHelpers.SetString(keyName, value))
            {
                httpResponse.Message = "Set success";
            }
            else
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                httpResponse.Message = "Something not good happened...";
            }
            this.HttpContext.Response.StatusCode = statusCode;
            httpResponse.StatusCode = statusCode;
            return httpResponse;
        }

        [HttpGet]
        public RedisGetStringResult TestRedisGetString(string keyName)
        {
            RedisGetStringResult httpResponse = new RedisGetStringResult();
            httpResponse.Date = DateTime.Now;
            httpResponse.KeyName = keyName;
            string tempResult = RedisHelpers.GetString(keyName);
            int statusCode = (int)HttpStatusCode.OK;
            if (tempResult == null)
            {
                statusCode = (int)HttpStatusCode.NoContent;
                return null;
            }
            httpResponse.Message = "Okay..";
            httpResponse.Value = tempResult;
            this.HttpContext.Response.StatusCode = statusCode;
            httpResponse.StatusCode = statusCode;
            return httpResponse;
        }

        [HttpPost]
        public RedisSetKeyExpireTimeResult SetRedisKeyExpireTime(string keyName, int timeSeconds)
        {
            RedisSetKeyExpireTimeResult httpResponse = new RedisSetKeyExpireTimeResult();
            httpResponse.Date = DateTime.Now;
            httpResponse.KeyName = keyName;
            httpResponse.ExpiryTimeInSeconds = timeSeconds;
            int statusCode = (int)HttpStatusCode.OK;
            if (RedisHelpers.SetKeyExpireTime(keyName, timeSeconds))
            {
                httpResponse.Message = "Set expiry time success";
                TimeSpan interval = new TimeSpan(0, 0, timeSeconds);
                httpResponse.ExpectedExpiryDate = DateTime.Now + interval;
            }
            else
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                httpResponse.Message = "Something not good happened...";
            }
            this.HttpContext.Response.StatusCode = statusCode;
            httpResponse.StatusCode = statusCode;
            return httpResponse;
        }
    }
}