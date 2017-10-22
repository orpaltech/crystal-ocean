using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CrystalOcean.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostingContext, builder) => 
                {
                    var env = hostingContext.HostingEnvironment;
                    if (env.IsDevelopment())
                    {
                        builder.AddUserSecrets<Startup>();
                    }
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    var config = hostingContext.Configuration;
                    logging.AddConfiguration(config.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .UseUrls("http://localhost:5001")
                .Build();
    }
}
