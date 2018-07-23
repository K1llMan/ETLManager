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

        public Database DB { get; }

        public string Path { get; }

        public decimal Version { get; private set; }

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
                try
                {
                    List<Migration> m = JsonCommon.Load<List<Migration>>(file.FullName);

                    ((List<Migration>)migrations).AddRange(m);
                }
                catch (Exception ex)
                {
                    Logger.WriteToTrace($"Ошибка при загрузке миграции из файла \"{file.Name}\": {ex}", TraceMessageKind.Error);
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
                    " where table_name = 'etlparams')").First();

                // При отсутствии таблицы мигрируем базу до нулевой версии
                if (!result.exists)
                    UpTo(0);
                else
                {
                    result = DB.Query(
                        "select value " +
                        " from ETLParams" +
                        " where name = 'Version'").Single();

                    if (result != null)
                        Version = Convert.ToDecimal(result.value);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToTrace($"Ошибка при получении версии базы данных: {ex}", TraceMessageKind.Error);
            }
        }

        private bool CheckMigrationNumber(decimal num)
        {
            if (num < 0)
            {
                Logger.WriteToTrace("Номер миграции должен быть положительным.", TraceMessageKind.Error);
                return false;
            }

            return Version != num;
        }

        private bool ApplyMigrations(decimal num, string direction)
        {
            if (!CheckMigrationNumber(num))
                return false;

            // Список обрабатываемых миграций
            var list = migrations.Where(m => m.Version <= Math.Max(Version, num) && m.Version > Math.Min(Version, num));
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
                        throw new Exception($"{m.Version} до версии {num}: {ex}");
                    }

                // Если вся миграция прошла успешно, то обновляем версии
                // В базе
                DB.Execute(
                    "update ETLParams" +
                    $" set value = {num}" +
                    " where name = 'Version'");

                // В объекте
                Version = num;

                DB.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteToTrace($"Ошибка при выполнении миграции {ex}", TraceMessageKind.Error);

                DB.Rollback();
                return false;
            }
        }

        #endregion Вспомогательные функции

        #region Основные функции

        /// <summary>
        /// Миграция вверх
        /// </summary>
        public bool UpTo(decimal num)
        {
            return ApplyMigrations(num, "up");
        }

        /// <summary>
        /// Миграция вниз
        /// </summary>
        public bool DownTo(decimal num)
        {
            return ApplyMigrations(num, "down");
        }

        public Migrator(Database db, string path)
        {
            DB = db;
            Path = path;
            Version = -1;

            GetMigrationsList();
            GetDBVersion();
        }

        #endregion Основные функции
    }
}
