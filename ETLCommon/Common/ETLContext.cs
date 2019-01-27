using System;

namespace ETLCommon
{
    /// <summary>
    /// Контекст ETL
    /// </summary>
    public class ETLContext : IDisposable
    {
        #region Свойства

        public IDatabase DB { get; set; }

        public ETLHistory History { get; private set; }

        public Migrator Migrator { get; private set; }

        public ETLSettings Settings { get; }

        public Version Version
        {
            get
            {
                return Migrator?.Version;
            }
        }

        #endregion Свойства

        #region Основные функции

        /// <summary>
        /// Инициализация базы данных и всех модулей, с ней работающих
        /// </summary>
        public void Initialize()
        {
            //DB.Connect();
            History = new ETLHistory(DB);
            Migrator = new Migrator(DB, Settings.Registry.MigrationsPath);
        }

        public ETLContext(string settings)
        {
            Settings = new ETLSettings(settings);

            DB = DatabaseFactory.GetDatabase(Settings.ConnectString);
        }

        #endregion Основные функции

        public void Dispose()
        {
            DB?.Disconnect();
        }
    }
}
