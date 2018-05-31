using System;

using ETLCommon;

namespace ETLApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Initialize($"{args[0]}.txt", string.Empty, false);
            Logger.WriteToTrace($"Запущена закачка {args[0]}.");
            while (true)
            {
                
            }
            Logger.CloseLogFile();
        }
    }
}
