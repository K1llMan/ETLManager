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
            int count = UpdateManager.Updates.Count;

            foreach (ETLUpdateRecord rec in UpdateManager.Updates.Values)
            {
                // Обновление невозможно применить при запущенном процессе закачки
                ETLProcess prc = Pumps.FirstOrDefault(p => p.ProgramID == rec.ProgramID);
                if (prc != null && prc.IsExecuting)
                    continue;

                UpdateManager.ApplyUpdate(rec, Context.History);
            }

            return count - UpdateManager.Updates.Count;
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
            if (prc.IsExecuting)
                return -1;

            // Для запуска процесса версия БД должна быть выше
            if (prc.Version > Context.Version)
                throw new Exception("Версия системы ниже необходимой для запуска.");

            decimal sessNo = Context.History.AddRecord(prc.ProgramID, Context.Version, prc.Version, "user", config);

            // При запуске добавляем в список запущенных
            prc.OnStart += async (s, a) => {
                await Broadcast.Send(new ETLBroadcastAction {
                    Action = "startPump",
                    Data = new Dictionary<string, object> {
                        { "id", id }
                    }
                });
            };

            // При закрытии процесса удаляем его из запущенных
            prc.OnExit += async (s, a) => {
                await Broadcast.Send(new ETLBroadcastAction {
                    Action = "endPump",
                    Data = new Dictionary<string, object> {
                        { "id", id },
                        { "status", ((ETLProcess)s).LastStatus.ToString() }
                    }
                });
            };

            prc.Start(sessNo);
            return sessNo;
        }

        /// <summary>
        /// Аварийное завершение закачки
        /// </summary>
        public void Terminate(string id)
        {
            ETLProcess pump = Pumps.FirstOrDefault(p => p.IsExecuting && p.ProgramID == id);
            pump?.Terminate();
        }

        public List<ETLLogRecord> GetLog(decimal sessNo)
        {
            dynamic record = Context.History[sessNo];
            if (record == null)
                return null;

            string path = Path.Combine(Context.Settings.Registry.LogsPath, record.programid, sessNo + ".txt");
            return ETLLogParser.Parse(path);
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
        }

        #endregion Основные функции

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}
