using System;
using System.IO;
using ETLEngineCore.Extensions;
using Newtonsoft.Json.Linq;

namespace ETLService.Manager
{
    public class ELTManager
    {
        #region Поля

        private FileSystemWatcher watcher;

        #endregion Поля

        #region Свойства

        public ETLManagerSettings Settings;

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

        #endregion Вспомогательные функции


        #region Основные функции

        public ELTManager(FileInfo settings)
        {
            // Инициализация настроек
            Settings = new ETLManagerSettings(settings);

            // Включение слежения за обновлениями
            InitWatcher();
        }

        #endregion Основные функции
    }
}
