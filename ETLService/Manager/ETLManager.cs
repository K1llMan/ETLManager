using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json.Linq;
using ETLCommon;

namespace ETLService.Manager
{
    public class ELTManager: IDisposable
    {
        #region Поля

        private FileSystemWatcher watcher;

        #endregion Поля

        #region Свойства

        public ETLSettings Settings { get; set; }

        public List<ETLProcess> Pumps { get; set; }

        public Dictionary<string, ETLProcess> ExecutingPumps { get; set; }

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

        /// <summary>
        /// Инициализация списка закачек
        /// </summary>
        private void InitPumpsList()
        {
            Logger.WriteToTrace("Формирование списка закачек.");

            Pumps = new List<ETLProcess>();            
            ExecutingPumps = new Dictionary<string, ETLProcess>();

            FileInfo[] pumpConfigs = new DirectoryInfo(Settings.Registry.ProgramsPath).GetFiles();
            List<string> ids = new List<string>();
            foreach (FileInfo pumpConfig in pumpConfigs)
                try
                {
                    JObject data = JsonCommon.Load(pumpConfig.FullName);
                    string id = data["id"].ToString();

                    // Если уже существует закачка с таким ID
                    if (ids.Contains(id))
                    {
                        Logger.WriteToTrace($"Закачка с ID = \"{id}\" (\"{pumpConfig.Name}\") уже существует.", TraceMessageKind.Warning);
                        continue;                        
                    }

                    // Процесс инициализируется данными конфигурации
                    ETLProcess prc = new ETLProcess(pumpConfig.Name, data);

                    // Сохраняется конфиг закачки с описанием
                    Pumps.Add(prc);
                }
                catch (Exception ex)
                {
                    Logger.WriteToTrace($"Ошибка при формировании реестра закачек: {ex}.", TraceMessageKind.Error);
                }
        }

        /// <summary>
        /// Проверка обновлений при запуске сервиса
        /// </summary>
        private void CheckUpdates()
        {
            Logger.WriteToTrace("Проверка наличия обновлений.");
        }

        #endregion Вспомогательные функции

        #region Основные функции

        /// <summary>
        /// Функция запуска закачки
        /// </summary>
        public int Execute(string id)
        {
            ETLProcess prc = Pumps.FirstOrDefault(p => p.ProgramID == id);

            // Проверка наличия в реестре
            if (prc == null)
                return -1;

            // Проверка среди запущенных
            if (!ExecutingPumps.ContainsKey(id))
            {
                ExecutingPumps.Add(id, prc);

                // При закрытии процесса удаляем его из запущенных
                prc.Exit += (s, a) => {
                    ExecutingPumps.Remove(id);
                };

                prc.Start();
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
            Settings = new ETLSettings(Path.Combine(baseDir, "ETLSettings.json"));

            InitPumpsList();
            CheckUpdates();
            InitWatcher();
        }

        #endregion Основные функции

        public void Dispose()
        {
            Settings?.Dispose();
        }
    }
}
