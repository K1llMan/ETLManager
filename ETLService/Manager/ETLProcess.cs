using System;
using System.Diagnostics;
using System.Linq;

using ETLCommon;

using Newtonsoft.Json.Linq;

namespace ETLService.Manager
{
    /// <summary>
    /// Инициализация процесса, хранение и работа с его данными
    /// </summary>
    public class ETLProcess
    {
        #region Поля

        private ETLHistory history;

        #endregion Поля

        #region Свойства

        public int ProcessID { get; private set; }

        public string ProgramID { get; private set; }

        public string Config { get; private set; }

        public JObject ConfigData { get; private set; }

        public PumpStatus LastStatus { get; private set; }

        public string Module { get; private set; }

        public Version Version { get; private set; }

        #endregion Свойства

        #region События

        public delegate void StartEventHandler(object sender, EventArgs a);
        public event StartEventHandler OnStart;

        public delegate void ExitEventHandler(object sender, EventArgs a);
        public event ExitEventHandler OnExit;

        #endregion События

        #region Вспомогательные функции

        private PumpStatus GetLastStatus()
        {
            dynamic record = history.GetLastRecord(ProgramID);

            if (record == null)
                return PumpStatus.None;

            return Enum.Parse<PumpStatus>(record.status);
        }

        private PumpStatus GetStatus(decimal sessNo)
        {
            dynamic record = history[sessNo];

            if (record == null)
                return PumpStatus.None;

            return Enum.Parse<PumpStatus>(record.status);
        }

        #endregion Вспомогательные функции

        #region Основные функции

        /// <summary>
        /// Запуск процесса закачки
        /// </summary>
        public void Start(decimal sessNo, string config)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                Arguments = $"ETLApp.dll {sessNo} -s \"{config.Replace("\"", "\\\"")}\"",
                FileName = "dotnet",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            Process prc = Process.Start(psi);
            ProcessID = prc.Id;
            prc.EnableRaisingEvents = true;

            Logger.WriteToTrace($"Процесс закачки \"{ProgramID}\" ({ProcessID}) запущен.");

            // Событие запуска
            OnStart?.Invoke(this, EventArgs.Empty);

            prc.Exited += (s, a) => {
                Logger.WriteToTrace($"Процесс закачки \"{ProgramID}\" ({ProcessID}) завершён.");
                // Обновляем статус после окончания закачки
                LastStatus = GetStatus(sessNo);

                OnExit?.Invoke(this, a);
                // После завершения процесса очищаем все обработчики
                OnStart?.GetInvocationList().ToList().ForEach(d => OnStart -= (StartEventHandler)d);
                OnExit?.GetInvocationList().ToList().ForEach(d => OnExit -= (ExitEventHandler)d);
            };
        }

        public void Init(string fileName)
        {
            JObject data = JsonCommon.Load(fileName);

            Config = fileName;
            ConfigData = data;

            ProcessID = -1;
            ProgramID = ConfigData["id"].ToString();
            Module = ConfigData["module"].ToString();
            LastStatus = GetLastStatus();
            Version = new Version(ConfigData["version"]?.ToString() ?? "0.0.0");
        }

        public ETLProcess(string fileName, ETLHistory etlHistory)
        {
            history = etlHistory;
            Init(fileName);
        }

        #endregion Основные функции
    }
}
