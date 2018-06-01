using System;
using System.IO;

using ETLCommon;

namespace ETLApp
{
    class ETLProgram: IDisposable
    {
        #region Свойства

        public string ID { get; set; }

        public ETLSettings Settings { get; set; }

        #endregion Свойства

        #region Основные функции

        /// <summary>
        /// Запуск программы на выполнение
        /// </summary>
        public void Exec()
        {
            Logger.WriteToTrace($"Запуск закачки \"{ID}\".", TraceMessageKind.Information);

            try
            {
                // Обход всех этапов закачки
            }
            catch (Exception ex)
            {
                Logger.WriteToTrace($"Ошибка при выполнении закачки \"{ID}\": {ex}.", TraceMessageKind.Error);
            }
        }

        /// <summary>
        /// Инициализация программы
        /// </summary>
        public ETLProgram(string id)
        {
            ID = id;
            Settings = new ETLSettings(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ETLSettings.json"));

            Logger.Initialize($"{id}.txt", Settings.Registry.LogsPath.FullName, false);
            Logger.WriteToTrace($"Инициализация закачки \"{ID}\".", TraceMessageKind.Information);


        }

        #endregion Основные функции

        public void Dispose()
        {
            Logger.CloseLogFile();
        }
    }
}
