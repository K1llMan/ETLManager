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

        public Dictionary<string, DBAttribute> Attributes { get; private set; }

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
                Attributes = new Dictionary<string, DBAttribute>();

                dynamic result = DB.Query(
                    "select (column_name) as Name, (data_type) as Type, (column_default) as DefaultValue, (is_nullable) as Nullable, (character_maximum_length) as Size" +
                    " from information_schema.columns" +
                    $" where table_name = '{Name}'");

                foreach (dynamic field in result)
                {
                    string name = field.name;
                    string nullable = field.nullable;

                    Attributes.Add(name, new DBAttribute{
                        Default = field.defaultvalue,
                        Name = name,
                        Nullable = nullable.IsMatch("yes"),
                        Size = field.size == null ? 0 : field.size,
                        Type = DB.FromDBType(field.type)                    
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToTrace($"Ошибка формирования атрибутов: {ex}", TraceMessageKind.Error);
                throw;
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

                return -1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

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

                return -1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        internal DBTable(Database db, string name)
        {
            DB = db;
            Name = name;

            GetAttrList();
        }

        #endregion Основные функции
    }
}
