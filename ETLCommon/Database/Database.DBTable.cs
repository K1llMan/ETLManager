using System;
using System.Collections.Generic;
using System.Linq;

namespace ETLCommon
{
    public class DBAttribute
    {
        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        public object Default { get; internal set; }

        /// <summary>
        /// Имя атрибута
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Может ли принимать пустое значение
        /// </summary>
        public bool Nullable { get; internal set; }

        /// <summary>
        /// Размер поля
        /// </summary>
        public int Size { get; internal set; }

        /// <summary>
        /// Тип
        /// </summary>
        public Type Type { get; internal set; }
    }

    /// <summary>
    /// Класс работы с таблицой в базе
    /// </summary>
    public class DBTable
    {
        #region Свойства

        public List<DBAttribute> Attributes { get; private set; }

        /// <summary>
        /// База
        /// </summary>
        public Database DB { get; }

        /// <summary>
        /// Имя таблицы в базе
        /// </summary>
        public string Name { get; }

        #endregion Свойства

        #region Вспомогательные функции

        private void GetAttrList()
        {
            try
            {
                Attributes = new List<DBAttribute>();

                switch (DB.DatabaseType)
                {
                    case DBType.PostgreSql:
                        dynamic result = DB.Query(
                            "select (column_name) as Name, (data_type) as Type, (column_default) as DefaultValue, (is_nullable) as Nullable, (character_maximum_length) as Size" +
                            " from information_schema.columns" +
                            $" where table_name = '{Name}'");

                        foreach (dynamic field in result)
                        {
                            string name = field.name;
                            string nullable = field.nullable;

                            Attributes.Add(new DBAttribute {
                                Default = field.defaultvalue,
                                Name = name,
                                Nullable = nullable.IsMatch("yes"),
                                Size = field.size == null ? 0 : field.size,
                                Type = DB.FromDBType(field.type)
                            });
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка формирования атрибутов.", ex);
            }
        }

        #endregion Вспомогательные функции

        #region Основные функции

        /// <summary>
        /// Возвращает следующее значение ключа
        /// </summary>
        public decimal GetNextVal()
        {
            try
            {
                switch (DB.DatabaseType)
                {
                    case DBType.PostgreSql:
                        // Автоматически сгенерированные последовательности имею имя <имя таблицы>_id_seq
                        return DB.Query($"select NEXTVAL('{Name}_id_seq') as id").Single().id;
                }
            }
            catch (Exception ex)
            {
            }

            return -1;
        }

        /// <summary>
        /// Возвращает текущее значение
        /// </summary>
        public decimal GetCurVal()
        {
            try
            {
                switch (DB.DatabaseType)
                {
                    case DBType.PostgreSql:
                        // Автоматически сгенерированные последовательности имею имя <имя таблицы>_id_seq
                        return DB.Query($"select CURRVAL('{Name}_id_seq') as id").Single().id;
                }
            }
            catch (Exception ex)
            {
            }

            return -1;
        }

        #region CRUD

        public dynamic Select(string[] fields, string constr = "")
        {
            try {
                // Выбираются только существующие атрибуты
                string[] corrFields = fields.Contains("*") 
                    ? new string[] { "*" }
                    : fields.Intersect(Attributes.Select(a => a.Name)).ToArray();

                string query = 
                    $"select {string.Join(", ", corrFields)}" + 
                    $" from {Name}" + 
                    (string.IsNullOrEmpty(constr) 
                        ? string.Empty
                        : $" where {constr}");

                return DB.Query(query);
            }
            catch (Exception ex) {
            }

            return null;
        }

        public dynamic Select(string constr = "")
        {
            return Select(new string[] { "*" }, constr);
        }

        public int Insert(params dynamic[] rows)
        {
            return 0;
        }

        public int Update(params dynamic[] rows)
        {
            return 0;
        }

        public int Delete(string constr = "")
        {
            return 0;
        }

        #endregion CRUD

        internal DBTable(Database db, string name)
        {
            DB = db;
            Name = name;

            GetAttrList();
        }

        #endregion Основные функции
    }
}
