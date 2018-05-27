﻿using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json.Linq;
using ETLCommon;

namespace ETLService.Manager
{
    public class ELTManager
    {
        #region Поля

        private FileSystemWatcher watcher;

        #endregion Поля

        #region Свойства

        public ETLManagerSettings Settings { get; set; }

        public Dictionary<string, string> Pumps { get; set; }

        public Dictionary<string, string> ExecutingPumps { get; set; }

        #endregion Свойства

        #region Вспомогательные функции

        private void WatcherHandler(object sender, FileSystemEventArgs e)
        {
            if (!Path.GetExtension(e.Name).IsMatch("json|dll"))
                return;

            Console.Write("olololo!");
        }

        private void InitWatcher()
        {
            watcher = new FileSystemWatcher
            {
                Path = Settings.Registry.UpdatesPath.FullName,
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
            Pumps = new Dictionary<string, string>();

            FileInfo[] pumpConfigs = Settings.Registry.ProgramsPath.GetFiles();
            foreach (FileInfo pumpConfig in pumpConfigs)
                try
                {
                    JObject data = JsonCommon.Load(pumpConfig.FullName);
                    // Если уже существует закачка с таким ID
                    if (Pumps.ContainsKey(data["id"].ToString()))
                    {

                        continue;                        
                    }

                    // Сохраняется конфиг закачки с описанием
                    Pumps.Add(data["id"].ToString(), string.Empty);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
        }

        #endregion Вспомогательные функции

        #region Основные функции

        /// <summary>
        /// Функция запуска закачки
        /// </summary>
        public void Execute(string id)
        {
            // Проверка наличия в реестре
            if (!Pumps.ContainsKey(id))
                return;

            // Проверка среди запущенных
            if (ExecutingPumps.ContainsKey(id))
            {
                return;
            }

            ExecutingPumps.Add(id, string.Empty);
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public ELTManager(FileInfo settings)
        {
            // Инициализация настроек
            Settings = new ETLManagerSettings(settings);

            // Формирование списка закачек
            InitPumpsList();

            // Включение слежения за обновлениями
            InitWatcher();
        }

        #endregion Основные функции
    }
}
