using System;
using System.Diagnostics;

using ETLCommon;

namespace ETLService.Manager
{
    /// <summary>
    /// Инициализация процесса, хранение и работа с его данными
    /// </summary>
    public class PumpProcess
    {
        #region Свойства

        public int ID { get; private set; }

        #endregion Свойства

        #region События

        public class WriteEventArgs
        {
            public string Message { get; internal set; }
        }

        public delegate void ProcessEventHandler(object e, EventArgs a);
        public event ProcessEventHandler Exit;

        #endregion События

        #region Основные функции

        /// <summary>
        /// Запуск процесса закачки
        /// </summary>
        public void Start(string id, string module)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                Arguments = $"ETLApp.dll \"{module}\"",
                FileName = "dotnet",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            Process prc = Process.Start(psi);
            prc.EnableRaisingEvents = true;

            Logger.WriteToTrace($"Процесс закачки \"{id}\" ({prc.Id}) запущен.");

            prc.Exited += (s, a) => {
                Logger.WriteToTrace($"Процесс закачки \"{id}\" ({prc.Id}) завершён.");

                Exit?.Invoke(s, a);
            };

            ID = prc.Id;
        }

        #endregion Основные функции
    }
}
