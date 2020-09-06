using System.Net;

namespace earthfusion_backend
{
    class Variables
    {
        // where should the application listen on?
        public static string[] listenUrls = new string[]{
            "http://*:".ToString() + Globals.httpListenPort.ToString(),
            "https://*:".ToString() + Globals.httpsListenPort.ToString()
        };
        public static IPAddress allIpv4 = IPAddress.Parse("0.0.0.0");
    }
}