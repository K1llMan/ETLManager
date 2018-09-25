using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ETLCommon
{
    public class Migrator
    {
        #region Поля

        private IEnumerable<Migration> migrations;

        #endregion Поля

        #region Свойства

        public IDatabase DB { get; }

        public string Path { get; }

        public Version Version { get; private set; }

        #endregion Свойства

        #region Вспомогательные функции

        /// <summary>
        /// Формирование списка миграций
        /// </summary>
        private void GetMigrationsList()
        {
            migrations = new List<Migration>();

            if (string.IsNullOrEmpty(Path))
                return;

            FileInfo[] files = new DirectoryInfo(Path).GetFiles("*.json");
            foreach (FileInfo file in files)
                try {
                    List<Migration> m = JsonCommon.Load<List<Migration>>(file.FullName);

                    ((List<Migration>)migrations).AddRange(m);
                }
                catch {
                }

            migrations = migrations.OrderBy(m => m.Version);
        }

        /// <summary>
        /// Получение версии базы и начальная миграция
        /// </summary>
        private void GetDBVersion()
        {
            try
            {
                dynamic result = DB.Query(
                    "select exists (" +
                    " select 1" +
                    " from information_schema.tables" +
                    " where table_name = 'etl_params')").First();

                // При отсутствии таблицы мигрируем базу до нулевой версии
                if (!result.exists)
                    UpTo("0.0.0");
                else
                {
                    result = DB.Query(
                        "select value " +
                        " from etl_params" +
                        " where name = 'Version'").Single();

                    Version = new Version(result.value);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении версии базы данных.", ex);
            }
        }

        private bool CheckMigrationNumber(Version version)
        {
            return Version != version || Version == new Version("0.0.0");
        }

        private Version Max(Version a, Version b)
        {
            return a > b ? a : b;
        }

        private Version Min(Version a, Version b)
        {
            return a < b ? a : b;
        }

        private bool ApplyMigrations(Version version, string direction)
        {
            if (!CheckMigrationNumber(version))
                return false;

            // Список обрабатываемых миграций
            var list = migrations.Where(m => m.Version <= Max(Version, version) && m.Version >= Min(Version, version));
            // Для понижения версии список обрабатывается в обратном порядке
            list = direction == "down" ? list.Reverse() : list;

            try
            {
                DB.BeginTransaction();

                foreach (Migration m in list)
                    try
                    {
                        foreach (string query in m.GetQueries(direction, DB.DatabaseType))
                            DB.Execute(query);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"{m.Version} до версии {version}: {ex}");
                    }

                // Если вся миграция прошла успешно, то обновляем версии
                // В базе
                DB.Execute(
                    "update etl_params" +
                    $" set value = '{version}'" +
                    " where name = 'Version'");

                // В объекте
                Version = version;

                DB.Commit();
                return true;
            }
            catch (Exception ex)
            {
                DB.Rollback();

                throw new Exception("Ошибка при выполнении миграции", ex);
            }
        }

        #endregion Вспомогательные функции

        #region Основные функции

        /// <summary>
        /// Миграция вверх
        /// </summary>
        public bool UpTo(string num)
        {
            return ApplyMigrations(new Version(num), "up");
        }

        /// <summary>
        /// Миграция вниз
        /// </summary>
        public bool DownTo(string num)
        {
            return ApplyMigrations(new Version(num), "down");
        }

        public Migrator(IDatabase db, string path)
        {
            DB = db;
            Path = path;
            Version = new Version("0.0.0");

            GetMigrationsList();
            GetDBVersion();
        }

        #endregion Основные функции
    }
}
