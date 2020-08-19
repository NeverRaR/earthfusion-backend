using Utils;
using Microsoft.Extensions.Configuration.Ini;
using System.Collections.Generic;
using System;

namespace OracleTest
{
    // use 'static' keyword to create 'global' variables
    public static class Globals
    {
        // constructor set to use predefined list to init.
        public static KnownTables knownTables = new KnownTables();
    }
}

namespace earthfusion_backend
{
    public static class Globals
    {
        public static IDictionary<String,String> config = new IniLoader("config.ini").data;
    }
}
