using System.Collections.Generic;

namespace ETLCommon
{
    public interface IDBTable
    {
        #region Свойства

        List<DBAttribute> Attributes { get; }

        /// <summary>
        /// База
        /// </summary>
        IDatabase DB { get; }

        /// <summary>
        /// Имя таблицы в базе
        /// </summary>
        string Name { get; }

        #endregion Свойства

        #region Методы

        /// <summary>
        /// Возвращает следующее значение ключа
        /// </summary>
        decimal GetNextVal();

        /// <summary>
        /// Возвращает текущее значение
        /// </summary>
        decimal GetCurVal();

        #region CRUD

        dynamic Select(string constr = "");

        int Insert(params dynamic[] rows);

        int Update(params dynamic[] rows);

        int Delete(string constr = "");

        #endregion CRUD

        #endregion Методы
    }
}
