using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace earthfusion_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }
        
        // public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        //     WebHost.CreateDefaultBuilder(args)
        //     .UseUrls(Variables.listenUrls)
        //     .UseStartup<Startup>();
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).UseKestrel(options =>
            {
                options.Listen(Variables.allIpv4, 8970);
                options.Listen(Variables.allIpv4, 8971, listenOptions =>
                {
                    // here, grab your https certificate.pfx.
                    listenOptions.UseHttps("certificate.pfx", "");
                });
            })
            .UseStartup<Startup>();
    }
}
