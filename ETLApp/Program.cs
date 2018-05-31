using System;
using System.IO;

using ETLCommon;

using Newtonsoft.Json.Linq;

namespace ETLApp
{
    class Program
    {
        public static ETLSettings Settings { get; set; }

        static void Main(string[] args)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            Logger.Initialize($"{args[0]}.txt", string.Empty, false);

            Settings = new ETLSettings(Path.Combine(baseDir, "ETLSettings.json"));

            Logger.WriteToTrace($"Запущена закачка {args[0]}.");
            while (true)
            {
                
            }
            Logger.CloseLogFile();
        }
    }
}
