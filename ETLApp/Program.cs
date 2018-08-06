using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using ETLCommon;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ETLApp
{
    class Program
    {
        /// <summary>
        /// args[0] - id закачки;
        /// args[1] - тип конфигурации: -s - строка, -f - файл
        /// args[2] - конфигурация: в зависимости от args[1] это строка конфигурации или имя файла
        /// </summary>
        static void Main(string[] args)
        {
            ETLProgram program = null;
            try
            {
                ETLSettings settings = new ETLSettings(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ETLSettings.json"));
                decimal sessNo = Convert.ToDecimal(args[0]);

                JObject data = args[1] == "-f" 
                    ? JsonCommon.Load(Path.Combine(settings.Registry.ProgramsPath, args[2]))
                    : (JObject)JsonConvert.DeserializeObject(args[2]);

                string id = data["id"].ToString();

                Logger.Initialize($"{sessNo}", Path.Combine(settings.Registry.LogsPath, id), true);
                Logger.WriteToTrace($"Инициализация закачки \"{id}\"...", TraceMessageKind.Information);
            
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

                program.SessNo = sessNo;
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
