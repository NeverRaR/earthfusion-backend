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
    [Route("api-for-test/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
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

        [HttpPost]
        public string TestRedisSetString(string keyName, string value)
        {
            if (RedisHelpers.SetString(keyName, value))
            {
                return "Set success.";
            }
            else
            {
                return "Something bad happened";
            }
        }

        [HttpGet]
        public string TestRedisGetString(string keyName)
        {
            return RedisHelpers.GetString(keyName);
        }
    }
}