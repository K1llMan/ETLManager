using System;

using ETLCommon;

namespace ETLApp
{
    class Program
    {
        public static ETLSettings Settings { get; set; }

        static void Main(string[] args)
        {
            ETLProgram program = null;
            try
            {
                program = new ETLProgram(args[0]);
                program.Exec();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                program?.Dispose();
            }
        }
    }
}
