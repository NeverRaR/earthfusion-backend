using System.Collections.Generic;
using System;

namespace earthfusion_backend
{
    public static class Globals
    {
        public static IDictionary<String,String> config = new IniLoader("config.ini").data;
    }
}