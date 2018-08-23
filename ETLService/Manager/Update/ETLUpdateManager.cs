using System.Collections.Generic;
using System.IO;
using System.Linq;

using ETLCommon;

using Newtonsoft.Json.Linq;

namespace ETLService.Manager.Update
{
    /// <summary>
    /// Менеджер контроля обновлений
    /// </summary>
    public class ETLUpdateManager
    {
        #region Поля

        private FileSystemWatcher watcher;
        // Настройки реестра
        private ETLRegistrySettings settings;
        // Список существующих закачек
        private List<ETLProcess> pumps;

        #endregion Поля

        #region События

        // Событие получения обновления
        public class ReceiveUpdateEventArgs
        {
            public ETLUpdateRecord UpdateInfo { get; internal set; }
        }

        public delegate void RecieveUpdateEventHandler(object sender, ReceiveUpdateEventArgs e);
        public event RecieveUpdateEventHandler OnReceiveUpdate;

        // Событие обновления
        public class UpdateEventArgs
        {
            public ETLUpdateRecord UpdateInfo { get; internal set; }
        }

        public delegate void UpdateEventHandler(object sender, UpdateEventArgs e);
        public event UpdateEventHandler OnUpdate;

        #endregion События

        #region Свойства

        /// <summary>
        /// Список обновлений
        /// </summary>
        public Dictionary<string, ETLUpdateRecord> Updates { get; }

        #endregion Свойства

        #region Вспомогательные функции

        private void WatcherHandler(object sender, FileSystemEventArgs e)
        {
            if (!Path.GetExtension(e.Name).IsMatch("json|dll"))
                return;

            ETLUpdateRecord rec = AddUpdateRecord(e.FullPath);            
            if (rec == null)
                return;

            // Событие получения обновления
            OnReceiveUpdate?.Invoke(this, new ReceiveUpdateEventArgs{ UpdateInfo = rec });
        }

        private void InitWatcher()
        {
            watcher = new FileSystemWatcher {
                Path = settings.UpdatesPath,
                NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName,
                Filter = "*.*",
            };

            watcher.Changed += WatcherHandler;
            watcher.Renamed += WatcherHandler;

            // Включение слежения за директорией
            watcher.EnableRaisingEvents = true;
        }

        private ETLUpdateRecord AddUpdateRecord(string file)
        {
            string extension = Path.GetExtension(file).Replace(".", string.Empty);
            string fileName = Path.GetFileName(file);
            // На программу подразумевается одна конфигурация и один модуль
            ETLProcess prc = pumps.FirstOrDefault(p => extension.IsMatch("json")
                ? p.Config == fileName
                : p.Module == fileName);

            // В реестре закачка отсутствует, есть только программный модуль
            if (prc == null && extension.IsMatch("dll"))
                return null;

            ETLUpdateRecord rec = null;
            switch (extension)
            {
                case "json":
                    JObject data = JsonCommon.Load(file);
                    string programID = prc == null ? data["id"].ToString() : prc.ProgramID;

                    rec = new ETLUpdateRecord {
                        Config = fileName,
                        Module = data["module"].ToString(),
                        ProgramID = programID
                    };

                    Updates[programID] = rec;
                    break;
                case "dll":
                    // Если записи нет, то добавляем, иначе нужный модуль уже должен быть прописан в обновление
                    if (!Updates.ContainsKey(prc.ProgramID)) {
                        rec = new ETLUpdateRecord {
                            ProgramID = prc.ProgramID,
                            Module = fileName
                        };

                        Updates.Add(prc.ProgramID, rec);
                    }
                    break;
            }

            return rec;
        }

        /// <summary>
        /// Перемещение файла
        /// </summary>
        private void MoveFile(string fileName, string sourceDir, string destDir)
        {
            string sourcePath = Path.Combine(sourceDir, fileName);
            string destPath = Path.Combine(destDir, fileName);

            if (!File.Exists(sourcePath))
                return;

            if (File.Exists(destPath))
                File.Delete(destPath);
            File.Move(sourcePath, destPath);
        }

        #endregion Вспомогательные функции

        #region Основные функции

        /// <summary>
        /// Применение обновлений
        /// </summary>
        public void ApplyUpdate(ETLUpdateRecord rec, ETLHistory history)
        {
            ETLProcess prc = pumps.FirstOrDefault(p => p.ProgramID == rec.ProgramID);

            // Копирование файлов в необходимые директории
            if (!string.IsNullOrEmpty(rec.Config)) {
                MoveFile(rec.Config, settings.UpdatesPath, settings.ProgramsPath);

                // Обноление конфигурации загруженных в реестр закачек или добавление новой
                string configFile = Path.Combine(settings.ProgramsPath, rec.Config);
                if (prc == null)
                    pumps.Add(new ETLProcess(configFile, history));
                else
                    prc.Init(configFile);
            }

            if (!string.IsNullOrEmpty(rec.Module)) {
                // Модуль
                MoveFile(rec.Module, settings.UpdatesPath, settings.ModulesPath);

                // Отладочные данные
                MoveFile(rec.Module.Replace("dll", "pdb"), settings.UpdatesPath, settings.ModulesPath);
            }

            // Удаление применённого обновления из списка доступных
            Updates.Remove(rec.ProgramID);

            // Событие обновления
            OnUpdate?.Invoke(this, new UpdateEventArgs { UpdateInfo = rec });
        }

        /// <summary>
        /// Проверка обновлений
        /// </summary>
        public void CheckUpdates()
        {
            string[] files = Directory.GetFiles(settings.UpdatesPath);

            // Очистка от файлов, не являющихся библиотеками или конфигурациями
            files.Where(f => !Path.GetExtension(f).IsMatch("json|dll|pdb"))
                 .ToList()
                 .ForEach(f => File.Delete(f));

            files = files.Where(f => Path.GetExtension(f).IsMatch("json|dll")).ToArray();

            if (!files.Any())
                return;

            foreach (string file in files)
                AddUpdateRecord(file);
        }

        public ETLUpdateManager(List<ETLProcess> pumpsList, ETLRegistrySettings registrySettings)
        {
            pumps = pumpsList;
            settings = registrySettings;
            Updates = new Dictionary<string, ETLUpdateRecord>();

            InitWatcher();
        }

        #endregion Основные функции
    }
}
