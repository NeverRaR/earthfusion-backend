using Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace earthfusion_backend
{
    public class IniLoader
    {
        public IniLoader(string iniFileName)
        {
            data = new ConfigurationBuilder().AddIniFile(iniFileName).Build().Get<Dictionary<string, string>>();
        }
        public IDictionary<String,String> data = null;
    }
}

// public static IniConfigurationProvider config = new 