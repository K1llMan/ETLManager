using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Newtonsoft.Json.Linq;
using ETLCommon;

namespace ETLService.Manager
{
    public class ELTManager
    {
        #region Поля

        private FileSystemWatcher watcher;
        private Dictionary<string, string> pumpsFiles;

        #endregion Поля

        #region Свойства

        public ETLSettings Settings { get; set; }

        public Dictionary<string, JObject> Pumps { get; set; }

        public Dictionary<string, Process> ExecutingPumps { get; set; }

        #endregion Свойства

        #region Вспомогательные функции

        private void WatcherHandler(object sender, FileSystemEventArgs e)
        {
            if (!Path.GetExtension(e.Name).IsMatch("json|dll"))
                return;

            Logger.WriteToTrace("Доступны обновления реестра.");
        }

        private void InitWatcher()
        {
            Logger.WriteToTrace($"Включение слежения за обновлениями в директории \"{Settings.Registry.UpdatesPath}\"...");

            watcher = new FileSystemWatcher
            {
                Path = Settings.Registry.UpdatesPath,
                NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName,
                Filter = "*.*",
            };

            watcher.Changed += WatcherHandler;
            watcher.Renamed += WatcherHandler;

            // Включение слежения за директорией
            watcher.EnableRaisingEvents = true;
        }

        private void InitPumpsList()
        {
            Logger.WriteToTrace("Формирование списка закачек.");

            // Словарь соответствия ID закачки файлу, его содержащего
            pumpsFiles = new Dictionary<string, string>();

            Pumps = new Dictionary<string, JObject>();            
            ExecutingPumps = new Dictionary<string, Process>();

            FileInfo[] pumpConfigs = new DirectoryInfo(Settings.Registry.ProgramsPath).GetFiles();
            foreach (FileInfo pumpConfig in pumpConfigs)
                try
                {
                    JObject data = JsonCommon.Load(pumpConfig.FullName);
                    string id = data["id"].ToString();

                    // Если уже существует закачка с таким ID
                    if (Pumps.ContainsKey(id))
                    {
                        Logger.WriteToTrace($"Закачка с ID = \"{id}\" (\"{pumpConfig.Name}\") уже существует.", TraceMessageKind.Warning);
                        continue;                        
                    }

                    // Сохраняется конфиг закачки с описанием
                    Pumps.Add(id, data);
                    pumpsFiles.Add(id, pumpConfig.Name);
                }
                catch (Exception ex)
                {
                    Logger.WriteToTrace($"Ошибка при формировании реестра закачек: {ex}.", TraceMessageKind.Error);
                }
        }

        #endregion Вспомогательные функции

        #region Основные функции

        /// <summary>
        /// Функция запуска закачки
        /// </summary>
        public int Execute(string id)
        {
            // Проверка наличия в реестре
            //if (!Pumps.ContainsKey(id))
            //    return;

            // Проверка среди запущенных
            if (!ExecutingPumps.ContainsKey(id))
            {

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    Arguments = $"ETLApp.dll \"{pumpsFiles[id]}\"",
                    FileName = "dotnet",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                };
                Process prc = Process.Start(psi);
                prc.EnableRaisingEvents = true;

                // При закрытии процесса удаляем его из запущенных
                prc.Exited += (s, a) => {
                    Logger.WriteToTrace($"Процесс закачки \"{id}\" ({prc.Id}) завершён.");
                    ExecutingPumps.Remove(id);
                };

                ExecutingPumps.Add(id, prc);
                return 0;
            }

            return -1;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public ELTManager()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            Logger.Initialize("ETLManager.log", baseDir, true);

            Settings = new ETLSettings(Path.Combine(baseDir, "ETLSettings.json"));

            InitPumpsList();

            InitWatcher();
        }

        #endregion Основные функции
    }
}
