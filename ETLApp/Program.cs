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
        private static ETLSettings settings;
        private static ETLHistory history;
        private static dynamic historyRecord;

        #endregion Поля

        #region Вспомогательные функции

        private static void Init(decimal sessNo, string id, string moduleName)
        {
            settings = new ETLSettings(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ETLSettings.json"));
            history = new ETLHistory(settings.DB);
            historyRecord = history[sessNo];

            Logger.Initialize($"{sessNo}", Path.Combine(settings.Registry.LogsPath, id), true);
            Logger.WriteToTrace($"Инициализация закачки \"{id}\"...", TraceMessageKind.Information);

            // Получаем описание закачки и загружаем модуль с кодом
            string module = Path.Combine(settings.Registry.ModulesPath, moduleName);
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
            program.Settings = settings;
            program.Initialize(config);
            program.Exec();

            // Обновление статуса закачки после выполнения
            historyRecord.status = program.Status.ToString();
            history[sessNo] = historyRecord;
        }

        #endregion Вспомогательные функции

        #region Основные функции

        /// <summary>
        /// args[0] - id закачки;
        /// args[1] - тип конфигурации: -s - строка, -f - файл
        /// args[2] - конфигурация: в зависимости от args[1] это строка конфигурации или имя файла
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                JObject config = args[1] == "-f"
                    ? JsonCommon.Load(Path.Combine(settings.Registry.ProgramsPath, args[2]))
                    : (JObject)JsonConvert.DeserializeObject(args[2]);

                decimal sessNo = Convert.ToDecimal(args[0]);
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
