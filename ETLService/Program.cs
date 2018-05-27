using System;
using System.IO;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using ETLCommon;
using ETLService.Manager;

namespace ETLService
{
    public class Program
    {
        public static ELTManager Manager;

        public static void Main(string[] args)
        {
            Logger.Initialize("ETLManager.log", AppDomain.CurrentDomain.BaseDirectory, true);

            FileInfo settings = new FileInfo("appsettings.json");
            Manager = new ELTManager(settings);

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options => {
                    options.Limits.MaxConcurrentConnections = 100;
                    options.Limits.MaxConcurrentUpgradedConnections = 100;
                })
                .Build();
    }
}
