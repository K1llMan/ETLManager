using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;

using Newtonsoft.Json.Linq;
using ETLCommon;

using ETLService.Manager.Broadcast;
using ETLService.Manager.Update;

namespace ETLService.Manager
{
    public class ELTManager: IDisposable
    {
        #region Поля

        private FileSystemWatcher watcher;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Настройки
        /// </summary>
        public ETLContext Context { get; set; }

        /// <summary>
        /// Рассылка информации для клиентов
        /// </summary>
        public ETLBroadcast Broadcast { get; set; }

        /// <summary>
        /// Полный список программ
        /// </summary>
        public List<ETLProcess> Pumps { get; set; }

        /// <summary>
        /// Запущенные программы
        /// </summary>
        public Dictionary<string, ETLProcess> ExecutingPumps { get; set; }

        /// <summary>
        /// Набор обновлений
        /// </summary>
        public Dictionary<string, ETLUpdateRecord> Updates { get; private set; }

        /// <summary>
        /// Менеджер обновлений
        /// </summary>
        public ETLUpdateManager UpdateManager { get; private set; }

        /// <summary>
        /// JWT (JSON Web Token) для авторизации пользователей
        /// </summary>
        public JwtControl JWT { get; private set; }

        #endregion Свойства

        #region Вспомогательные функции

        #region Список закачек

        /// <summary>
        /// Инициализация списка закачек
        /// </summary>
        private void InitPumpsList()
        {
            Logger.WriteToTrace("Формирование списка закачек.");

            Pumps = new List<ETLProcess>();            
            ExecutingPumps = new Dictionary<string, ETLProcess>();

            string[] pumpConfigs = Directory.GetFiles(Context.Settings.Registry.ProgramsPath);
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
                    ETLProcess prc = new ETLProcess(pumpConfig, Context.History);

                    // Сохраняется конфиг закачки с описанием
                    Pumps.Add(prc);
                    ids.Add(id);
                }
                catch (Exception ex)
                {
                    Logger.WriteToTrace($"Ошибка при формировании реестра закачек: {ex}.", TraceMessageKind.Error);
                }
        }

        #endregion Список закачек

        #region Обновления

        private void WatcherHandler(object sender, FileSystemEventArgs e)
        {
            if (!Path.GetExtension(e.Name).IsMatch("json|dll"))
                return;

            Logger.WriteToTrace("Доступны обновления реестра.");
            AddUpdateRecord(e.FullPath);
        }

        private void InitWatcher()
        {
            Logger.WriteToTrace($"Включение слежения за обновлениями в директории \"{Context.Settings.Registry.UpdatesPath}\"...");

            watcher = new FileSystemWatcher
            {
                Path = Context.Settings.Registry.UpdatesPath,
                NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName,
                Filter = "*.*",
            };

            watcher.Changed += WatcherHandler;
            watcher.Renamed += WatcherHandler;

            // Включение слежения за директорией
            watcher.EnableRaisingEvents = true;
        }

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
                    ETLUpdateRecord rec = new ETLUpdateRecord
                    {
                        Config = fileName,
                        Module = data["module"].ToString()
                    };
                    Updates[prc == null ? data["id"].ToString() : prc.ProgramID] = rec;
                    break;
                case "dll":
                    // Если записи нет, то добавляем, иначе нужный модуль уже должен быть прописан в обновление
                    if (!Updates.ContainsKey(prc.ProgramID))
                        Updates.Add(prc.ProgramID, new ETLUpdateRecord { Module = fileName });
                    break;
            }
        }

        /// <summary>
        /// Проверка обновлений при запуске сервиса
        /// </summary>
        private void CheckUpdates()
        {
            Updates = new Dictionary<string, ETLUpdateRecord>();

            string[] files = Directory.GetFiles(Context.Settings.Registry.UpdatesPath);

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
        private void ApplyUpdate(string id, ETLUpdateRecord rec)
        {
            // Обновление невозможно применить при запущенном процессе закачки
            if (ExecutingPumps.ContainsKey(id))
                return;

            ETLProcess prc = Pumps.FirstOrDefault(p => p.ProgramID == id);

            // Копирование файлов в необходимые директории
            if (!string.IsNullOrEmpty(rec.Config))
            {
                MoveFile(rec.Config, Context.Settings.Registry.UpdatesPath, Context.Settings.Registry.ProgramsPath);

                // Обноление конфигурации загруженных в реестр закачек или добавление новой
                string configFile = Path.Combine(Context.Settings.Registry.ProgramsPath, rec.Config);
                if (prc == null)
                    Pumps.Add(new ETLProcess(configFile, Context.History));
                else
                    prc.Init(configFile);
            }

            if (!string.IsNullOrEmpty(rec.Module))
            {
                // Модуль
                MoveFile(rec.Module, Context.Settings.Registry.UpdatesPath, Context.Settings.Registry.ModulesPath);

                // Отладочные данные
                MoveFile(rec.Module.Replace("dll", "pdb"), Context.Settings.Registry.UpdatesPath, Context.Settings.Registry.ModulesPath);
            }

            // Удаление применённого обновления из списка доступных
            Updates.Remove(id);
        }

        #endregion Обновления

        #region JWT

        private void InitJWT()
        {
            if (Context.Settings.JWTKey.Length < 16)
                Logger.WriteToTrace("Для корректной работы JWT ключ должен быть не менее 16 символов.", TraceMessageKind.Error);

            // Функция формирование требований после проверки данных пользователя
            CheckUser check = d =>
            {
                return new Claim[] {
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim(ClaimTypes.Role, "Admin")
                };
            };

            JWT = new JwtControl(check, Context.Settings.JWTKey);
        }

        #endregion JWT

        #region Рассылка

        private void InitBroadcast()
        {
            Broadcast = new ETLBroadcast();
        }

        #endregion Рассылка

        #region Обновления

        private void InitUpdateManager()
        {
            Logger.WriteToTrace($"Включение слежения за обновлениями в директории \"{Context.Settings.Registry.UpdatesPath}\"...");
            UpdateManager = new ETLUpdateManager(Pumps, Context.Settings.Registry);

            Logger.WriteToTrace("Проверка наличия обновлений.");
            UpdateManager.CheckUpdates();

            UpdateManager.OnReceiveUpdate += async (s, a) =>{
                await Broadcast.Send(new ETLBroadcastAction {
                    Action = "receiveUpdate",
                    Data = a.UpdateInfo
                });
            };

            UpdateManager.OnUpdate += async (s, a) => {
                await Broadcast.Send(new ETLBroadcastAction
                {
                    Action = "update",
                    Data = a.UpdateInfo
                });
            };
        }

        #endregion Обновления

        #endregion Вспомогательные функции

        #region Основные функции

        public int ApplyUpdates()
        {
            int count = Program.Manager.Updates.Count;

            foreach (KeyValuePair<string, ETLUpdateRecord> pair in Updates)
            {
                // Обновление невозможно применить при запущенном процессе закачки
                if (ExecutingPumps.ContainsKey(pair.Key))
                    continue;

                ApplyUpdate(pair.Key, pair.Value);
            }

            return count - Updates.Count;
        }

        /// <summary>
        /// Функция запуска закачки
        /// </summary>
        public decimal Execute(string id, string config)
        {
            ETLProcess prc = Pumps.FirstOrDefault(p => p.ProgramID == id);

            // Проверка наличия в реестре
            if (prc == null)
                throw new Exception("Закачка с заданным идентификатором отсутствует.");

            // Проверка среди запущенных
            if (!ExecutingPumps.ContainsKey(id))
            {
                // Для запуска процесса версия БД должна быть выше
                if (prc.Version > Context.Version)
                    throw new Exception("Версия системы ниже необходимой для запуска.");

                decimal sessNo = Context.History.AddRecord(prc.ProgramID, Context.Version, prc.Version, "user", config);

                // При запуске добавляем в список запущенных
                prc.OnStart += async (s, a) => {
                    ExecutingPumps.Add(id, (ETLProcess)s);
                    await Broadcast.Send(new ETLBroadcastAction {
                        Action = "startPump",
                        Data = new Dictionary<string, object> {
                            { "id", id }
                        }
                    });
                };

                // При закрытии процесса удаляем его из запущенных
                prc.OnExit += async (s, a) => {
                    ExecutingPumps.Remove(id);
                    await Broadcast.Send(new ETLBroadcastAction {
                        Action = "endPump",
                        Data = new Dictionary<string, object> {
                            { "id", id },
                            { "status", ((ETLProcess)s).LastStatus.ToString() }
                        }
                    });
                };

                prc.Start(sessNo, config);
                return sessNo;
            }

            return -1;
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public ELTManager()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            Context = new ETLContext(Path.Combine(baseDir, "ETLSettings.json"));

            // Соединение с базой
            try
            {
                Context.Initialize();
            }
            catch (Exception ex)
            {
                Logger.WriteToTrace($"Ошибка при подключении к базе: {ex}", TraceMessageKind.Error);
            }

            InitPumpsList();

            InitJWT();
            InitBroadcast();
            InitUpdateManager();

            //CheckUpdates();
            //InitWatcher();
        }

        #endregion Основные функции

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}
