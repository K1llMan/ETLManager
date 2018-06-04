using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using ETLCommon;

using Newtonsoft.Json.Linq;

namespace ETLApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ETLSettings settings = new ETLSettings(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ETLSettings.json"));
                JObject data = JsonCommon.Load(Path.Combine(settings.Registry.ProgramsPath, args[0]));

                string id = data["id"].ToString();

                Logger.Initialize($"{id}.txt", settings.Registry.LogsPath, false);
                Logger.WriteToTrace($"Инициализация закачки \"{id}\".", TraceMessageKind.Information);
            
                // Получаем описание закачки и загружаем модуль с кодом
                string module = Path.Combine(settings.Registry.LibsPath, data["module"].ToString());
                if (File.Exists(module))
                {
                    Assembly assembly = Assembly.LoadFile(module);
                    Type type = assembly.GetTypes().FirstOrDefault(t => typeof(ETLProgram).IsAssignableFrom(t));
                    ETLProgram program = (ETLProgram)assembly.CreateInstance(type.FullName, false, BindingFlags.CreateInstance, null, 
                        new object[] { settings, data }, CultureInfo.CurrentCulture, null);

                    program?.Exec();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToTrace($"Ошибка при инициализации закачки: {ex}", TraceMessageKind.Error);
                Console.WriteLine(ex);
            }
            finally
            {
                Logger.CloseLogFile();
            }
        }
    }
}
