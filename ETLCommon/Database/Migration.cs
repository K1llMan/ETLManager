using System;
using System.Collections.Generic;
using System.Linq;

namespace ETLCommon
{
    public class Migration
    {
        #region Поля

        public Version Version;
        public List<Dictionary<string, List<string>>> Up;
        public List<Dictionary<string, List<string>>> Down;

        #endregion Поля

        #region Основные функции

        /// <summary>
        /// Получение запросов для миграции
        /// </summary>
        public List<string> GetQueries(string direction, DBType type)
        {
            try
            {
                List<Dictionary<string, List<string>>> list = new List<Dictionary<string, List<string>>>();
                List<string> queries = new List<string>();

                switch (direction)
                {
                    case "up":
                        list = Up;
                        break;

                    case "down":
                        list = Down;
                        break;
                }

                foreach (Dictionary<string, List<string>> query in list)
                {
                    List<string> strs = query.Where(p => p.Key == type.ToString().ToLower())
                            .Select(p => p.Value)
                            .FirstOrDefault() ??
                        // Общие запросы
                        query.Where(p => p.Key == "common")
                            .Select(p => p.Value)
                            .FirstOrDefault();

                    queries.Add(string.Join("", strs));
                }

                return queries;

            }
            catch
            {
                return new List<string>();
            }
        }

        #endregion Основные функции
    }
}
