using System;
using System.Collections.Generic;
using System.Linq;

namespace ETLCommon
{
    /// <summary>
    /// Класс работы с таблицой в базе
    /// </summary>
    public class DBTable: IDBTable
    {
        #region Свойства

        public List<DBAttribute> Attributes { get; protected set; }

        /// <summary>
        /// База
        /// </summary>
        public IDatabase DB { get; }

        /// <summary>
        /// Имя таблицы в базе
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Количество записей
        /// </summary>
        public decimal Count {
            get
            {
                return GetCount();
            } 
        }

        #endregion Свойства

        #region Вспомогательные функции

        protected virtual void GetAttrList()
        {
        }

        protected virtual decimal GetCount()
        {
            return Convert.ToDecimal(DB.ExecuteScalar($"select count(*) from {Name}"));
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
                // Автоматически сгенерированные последовательности имеют имя <имя таблицы>_id_seq
                return DB.Query($"select NEXTVAL('{Name}_id_seq') as id").Single().id;
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
                // Автоматически сгенерированные последовательности имеют имя <имя таблицы>_id_seq
                return DB.Query($"select CURRVAL('{Name}_id_seq') as id").Single().id;
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

        internal DBTable(IDatabase db, string name)
        {
            DB = db;
            Name = name;

            GetAttrList();
        }

        #endregion Основные функции
    }
}
