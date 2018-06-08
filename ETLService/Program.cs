using System;

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
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            Logger.Initialize("ETLManager", baseDir, true);

            Manager = new ELTManager();

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(options => {
                    options.Limits.MaxConcurrentConnections = 100;
                    options.Limits.MaxConcurrentUpgradedConnections = 100;
                })
                // Установка папки с сайтом
                //.UseWebRoot("static")
                .Build();
    }
}
