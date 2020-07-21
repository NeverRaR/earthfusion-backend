using System.Net;

namespace earthfusion_backend
{
    class Variables
    {
        public static string[] listenUrls = new string[]{
            "http://*:5000".ToString(),
            "https://*:5001".ToString()
        };
        public static IPAddress allIpv6 = IPAddress.Parse("0.0.0.0");
    }
}