using System;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using ETLCommon;
using ETLService.Manager;

using Microsoft.Extensions.Configuration;

namespace ETLService
{
    public class Program
    {
        public static ELTManager Manager;

        public static void Main(string[] args)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            Logger.Initialize("ETLManager", baseDir, true);

            Manager = new ELTManager();

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) {
            var configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();
            /*
            var hostUrl = configuration["urls"];
            if (string.IsNullOrEmpty(hostUrl))
                hostUrl = "http://localhost:50828";*/

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options => {
                    options.Limits.MaxConcurrentConnections = 100;
                    options.Limits.MaxConcurrentUpgradedConnections = 100;
                })
                //.UseUrls(hostUrl)
                // Установка папки с сайтом
                //.UseWebRoot("static")
                .Build();
        }
    }
}
