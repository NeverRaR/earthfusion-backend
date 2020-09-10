using System.Collections.Generic;
using System;

namespace earthfusion_backend
{
    public static class Globals
    {
        public static IDictionary<String,String> config = new IniLoader("config.ini").data;
        public static int httpListenPort = Int32.Parse(config["EARTH_FUSION_LISTEN_HTTP_PORT"]);
        public static int httpsListenPort = Int32.Parse(config["EARTH_FUSION_LISTEN_HTTPS_PORT"]);
    }
}