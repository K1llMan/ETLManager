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
        #region Поля

        private static ETLProgram program;
        private static ETLContext context;
        private static dynamic historyRecord;

        #endregion Поля

        #region Вспомогательные функции

        private static void Init(decimal sessNo, string id, string moduleName)
        {
            Logger.Initialize($"{sessNo}", Path.Combine(context.Settings.Registry.LogsPath, id), true);
            Logger.WriteToTrace($"Инициализация закачки \"{id}\"...", TraceMessageKind.Information);

            // Получаем описание закачки и загружаем модуль с кодом
            string module = Path.Combine(context.Settings.Registry.ModulesPath, moduleName);
            if (!File.Exists(module))
                throw new Exception($"Не обнаружен программный модуль \"{moduleName}\".");

            Assembly assembly = Assembly.LoadFile(module);
            Type type = assembly.GetTypes().FirstOrDefault(t => typeof(ETLProgram).IsAssignableFrom(t));

            if (type == null)
                throw new Exception($"В модуле \"{moduleName}\" отсутствуют типы, реализующие программу закачки.");

            program = (ETLProgram)assembly.CreateInstance(type.FullName, false, BindingFlags.CreateInstance, null,
                null, CultureInfo.CurrentCulture, null);
        }

        private static void Exec(decimal sessNo, JObject config)
        {
            program.SessNo = sessNo;
            program.Context = context;
            program.Initialize(config);
            program.Exec();

            if (historyRecord == null)
                return;

            // Обновление статуса закачки после выполнения
            historyRecord.status = program.Status.ToString();
            historyRecord.pumpfinishdate = (object)DateTime.Now;
            context.History[sessNo] = historyRecord;
        }

        #endregion Вспомогательные функции

        #region Основные функции

        /// <summary>
        /// args[0] - id закачки;
        /// args[1] - тип конфигурации: -h - история, -s - строка -f - файл
        /// args[2] - конфигурация: в зависимости от args[1] это строка конфигурации или имя файла
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                context = new ETLContext(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "ETLSettings.json"));
                context.Initialize();
                context.DB.Connect();

                decimal sessNo = Convert.ToDecimal(args[0]);
                historyRecord = context.History[sessNo];

                JObject config = null;
                switch (args[1]) {
                    case "-f":
                        config = JsonCommon.Load(Path.Combine(context.Settings.Registry.ProgramsPath, args[2]));
                        break;
                    case "-h":
                        config = (JObject)JsonConvert.DeserializeObject(historyRecord.config);
                        break;
                    case "-s":
                        config = (JObject)JsonConvert.DeserializeObject(args[2]);
                        break;
                }

                if (config == null)
                    return;

                // Инициализация
                Init(sessNo, config["id"].ToString(), config["module"].ToString());

                // Выполнение
                Exec(sessNo, config);
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

        #endregion Основные функции
    }
}
