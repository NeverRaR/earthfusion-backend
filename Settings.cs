using System.Net;

namespace earthfusion_backend
{
    class Variables
    {
        // where should the application listen on?
        public static string[] listenUrls = new string[]{
            "http://*:8970".ToString(),
            "https://*:8971".ToString()
        };
        public static IPAddress allIpv4 = IPAddress.Parse("0.0.0.0");
    }
}