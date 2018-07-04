using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;

using Newtonsoft.Json.Linq;
using ETLCommon;

namespace ETLService.Manager
{
    public class ELTManager: IDisposable
    {
        #region Структуры

        public struct UpdateRecord
        {
            public string Module { get; set; }

            public string Config { get; set; }
        }

        #endregion Структуры

        #region Поля

        private FileSystemWatcher watcher;

        #endregion Поля

        #region Свойства

        public ETLSettings Settings { get; set; }

        public List<ETLProcess> Pumps { get; set; }

        public Dictionary<string, ETLProcess> ExecutingPumps { get; set; }

        public Dictionary<string, UpdateRecord> Updates { get; private set; }

        /// <summary>
        /// JWT (JSON Web Token) для авторизации пользователей
        /// </summary>
        public JwtControl JWT { get; }

        #endregion Свойства

        #region Вспомогательные функции

        private void WatcherHandler(object sender, FileSystemEventArgs e)
        {
            if (!Path.GetExtension(e.Name).IsMatch("json|dll"))
                return;

            Logger.WriteToTrace("Доступны обновления реестра.");
            AddUpdateRecord(e.FullPath);
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

            string[] pumpConfigs = Directory.GetFiles(Settings.Registry.ProgramsPath);
            List<string> ids = new List<string>();
            foreach (string pumpConfig in pumpConfigs)
                try
                {
                    string fileName = Path.GetFileName(pumpConfig);
                    JObject data = JsonCommon.Load(pumpConfig);
                    string id = data["id"].ToString();

                    // Если уже существует закачка с таким ID
                    if (ids.Contains(id))
                    {
                        Logger.WriteToTrace($"Закачка с ID = \"{id}\" (\"{fileName}\") уже существует.", TraceMessageKind.Warning);
                        continue;                        
                    }

                    // Процесс инициализируется данными конфигурации
                    ETLProcess prc = new ETLProcess(pumpConfig);

                    // Сохраняется конфиг закачки с описанием
                    Pumps.Add(prc);
                    ids.Add(id);
                }
                catch (Exception ex)
                {
                    Logger.WriteToTrace($"Ошибка при формировании реестра закачек: {ex}.", TraceMessageKind.Error);
                }
        }

        #region Обновления

        private void AddUpdateRecord(string file)
        {
            string extension = Path.GetExtension(file).Replace(".", string.Empty);
            string fileName = Path.GetFileName(file);
            // На программу подразумевается одна конфигурация и один модуль
            ETLProcess prc = Pumps.FirstOrDefault(p => extension.IsMatch("json")
                ? p.Config == fileName
                : p.Module == fileName);

            // В реестре закачка отсутствует, есть только программный модуль
            if (prc == null && extension.IsMatch("dll"))
                return;

            switch (extension)
            {
                case "json":
                    JObject data = JsonCommon.Load(file);
                    UpdateRecord rec = new UpdateRecord
                    {
                        Config = fileName,
                        Module = data["module"].ToString()
                    };
                    Updates[prc == null ? data["id"].ToString() : prc.ProgramID] = rec;
                    break;
                case "dll":
                    // Если записи нет, то добавляем, иначе нужный модуль уже должен быть прописан в обновление
                    if (!Updates.ContainsKey(prc.ProgramID))
                        Updates.Add(prc.ProgramID, new UpdateRecord { Module = fileName });
                    break;
            }
        }

        /// <summary>
        /// Проверка обновлений при запуске сервиса
        /// </summary>
        private void CheckUpdates()
        {
            Logger.WriteToTrace("Проверка наличия обновлений.");
            Updates = new Dictionary<string, UpdateRecord>();

            string[] files = Directory.GetFiles(Settings.Registry.UpdatesPath);

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

        /// <summary>
        /// Применение обновлений
        /// </summary>
        private void ApplyUpdate(string id, UpdateRecord rec)
        {
            // Обновление невозможно применить при запущенном процессе закачки
            if (ExecutingPumps.ContainsKey(id))
                return;

            ETLProcess prc = Pumps.FirstOrDefault(p => p.ProgramID == id);

            // Копирование файлов в необходимые директории
            if (!string.IsNullOrEmpty(rec.Config))
            {
                MoveFile(rec.Config, Settings.Registry.UpdatesPath, Settings.Registry.ProgramsPath);

                // Обноление конфигурации загруженных в реестр закачек или добавление новой
                string configFile = Path.Combine(Settings.Registry.ProgramsPath, rec.Config);
                if (prc == null)
                    Pumps.Add(new ETLProcess(configFile));
                else
                    prc.Init(configFile);
            }

            if (!string.IsNullOrEmpty(rec.Module))
            {
                // Модуль
                MoveFile(rec.Module, Settings.Registry.UpdatesPath, Settings.Registry.ModulesPath);

                // Отладочные данные
                MoveFile(rec.Module.Replace("dll", "pdb"), Settings.Registry.UpdatesPath, Settings.Registry.ModulesPath);
            }

            // Удаление применённого обновления из списка доступных
            Updates.Remove(id);
        }

        #endregion Обновления

        #endregion Вспомогательные функции

        #region Основные функции

        public int ApplyUpdates()
        {
            int count = Program.Manager.Updates.Count;
            Updates.ToList().ForEach(p => ApplyUpdate(p.Key, p.Value));
            return count - Updates.Count;
        }

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

            if (Settings.JWTKey.Length < 16)
                Logger.WriteToTrace("Для корректной работы JWT ключ должен быть не менее 16 символов.", TraceMessageKind.Error);

            // Функция формирование требований после проверки данных пользователя
            CheckUser check = d =>
            {
                return new Claim[] {
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim(ClaimTypes.Role, "Admin")
                };
            };

            JWT = new JwtControl(check, Settings.JWTKey);

            InitPumpsList();
            CheckUpdates();
            InitWatcher();


            DBTable table = Settings.DB["redux_messages"];
        }

        #endregion Основные функции

        public void Dispose()
        {
            Settings?.Dispose();
        }
    }
}
