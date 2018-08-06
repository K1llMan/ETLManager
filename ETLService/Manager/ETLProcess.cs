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
        #region Свойства

        public int ProcessID { get; private set; }

        public string ProgramID { get; private set; }

        public string Config { get; private set; }

        public JObject ConfigData { get; private set; }

        public string Module { get; private set; }

        public Version Version { get; private set; }

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

            prc.Exited += (s, a) => {
                Logger.WriteToTrace($"Процесс закачки \"{ProgramID}\" ({ProcessID}) завершён.");

                Exit?.Invoke(s, a);
                // После завершения процесса очищаем все обработчики, привязанные с событию завершения
                Exit?.GetInvocationList().ToList().ForEach(d => Exit -= (ProcessEventHandler)d);
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
            Version = new Version(ConfigData["version"]?.ToString() ?? "0.0.0");
        }

        public ETLProcess(string fileName)
        {
            Init(fileName);
        }

        #endregion Основные функции
    }
}
