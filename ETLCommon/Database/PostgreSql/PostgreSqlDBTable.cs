using System;
using System.Collections.Generic;
using System.Linq;

namespace ETLCommon
{
    /// <summary>
    /// Класс работы с таблицой в базе
    /// </summary>
    public class PostgreSqlDBTable: DBTable
    {
        #region Свойства

        #endregion Свойства

        #region Вспомогательные функции

        protected override void GetAttrList()
        {
            try
            {
                Attributes = new List<DBAttribute>();

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
        public override decimal GetNextVal()
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
        public override decimal GetCurVal()
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

        public override DBTablePage GetPage(DBTablePage page)
        {
            dynamic rows = DB.Query(
                "select *" + 
                $" from {Name}" + 
                (string.IsNullOrEmpty(page.OrderBy) 
                    ? string.Empty 
                    : $" order by {page.OrderBy} { (string.IsNullOrEmpty(page.OrderDir) ? "asc" : page.OrderDir) }") +
                $" limit {page.PageSize} offset {(page.Page - 1) * page.PageSize}");

            decimal count = Count;
            return new DBTablePage {
                Total = count,
                Page = page.Page,
                PageSize = page.PageSize,
                PageCount = (int)(count / page.PageSize) + 1,
                OrderBy = page.OrderBy,
                OrderDir = page.OrderDir,
                Rows = rows
            };
        }

        #region CRUD

        #endregion CRUD

        internal PostgreSqlDBTable(IDatabase db, string name): base(db, name)
        {
            //GetAttrList();
        }

        #endregion Основные функции
    }
}
