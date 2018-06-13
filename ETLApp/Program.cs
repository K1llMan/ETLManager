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
            ETLProgram program = null;
            try
            {
                ETLSettings settings = new ETLSettings(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ETLSettings.json"));
                JObject data = JsonCommon.Load(Path.Combine(settings.Registry.ProgramsPath, args[0]));

                string id = data["id"].ToString();

                Logger.Initialize($"{id}", settings.Registry.LogsPath, true);
                Logger.WriteToTrace($"Инициализация закачки \"{id}\".", TraceMessageKind.Information);
            
                // Получаем описание закачки и загружаем модуль с кодом
                string moduleName = data["module"].ToString();
                string module = Path.Combine(settings.Registry.ModulesPath, moduleName);
                if (!File.Exists(module))
                {
                    Logger.WriteToTrace($"Не обнаружен программный модуль \"{moduleName}\".", TraceMessageKind.Error);
                    return;
                }

                Assembly assembly = Assembly.LoadFile(module);
                Type type = assembly.GetTypes().FirstOrDefault(t => typeof(ETLProgram).IsAssignableFrom(t));

                if (type == null)
                {
                    Logger.WriteToTrace($"В модуле \"{moduleName}\" отсутствуют типы, реализующие программу закачки.", TraceMessageKind.Error);
                    return;
                }

                program = (ETLProgram)assembly.CreateInstance(type.FullName, false, BindingFlags.CreateInstance, null, 
                    null, CultureInfo.CurrentCulture, null);

                program.Settings = settings;
                program.Initialize(data);
                program.Exec();
            }
            catch (Exception ex)
            {
                Logger.WriteToTrace($"Критическая ошибка загрузки закачки: {ex}", TraceMessageKind.CriticalError);
            }
            finally
            {
                program?.Dispose();
                Logger.CloseLogFile();
            }
        }
    }
}
