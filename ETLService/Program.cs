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
                .Build();
    }
}
