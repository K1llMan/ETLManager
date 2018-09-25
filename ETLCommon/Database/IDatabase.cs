using System;
using System.Collections.Generic;
using System.Data;

namespace ETLCommon
{
    /// <summary>
    /// Интерфейс для базы данных
    /// </summary>
    public interface IDatabase
    {
        #region Свойства

        /// <summary>
        /// Индексатор по таблицам
        /// </summary>
        DBTable this[string tableName] { get; }

        /// <summary>
        /// Тип базы данных
        /// </summary>
        DBType DatabaseType { get; }

        #endregion Свойства

        #region Методы

        /// <summary>
        /// Соединение
        /// </summary>
        void Connect();

        /// <summary>
        /// Отключение
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Создание транзакции
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Фиксация транзакции
        /// </summary>
        void Commit();

        /// <summary>
        /// Откат транзакции
        /// </summary>
        void Rollback();

        /// <summary>
        /// Параметризированный запрос, возвращающий количество строк
        /// </summary>
        object Execute(string query, object param = null, CommandType? type = null);

        /// <summary>
        /// Параметризированный запрос, возвращающий значение
        /// </summary>
        object ExecuteScalar(string query, object param = null, CommandType? type = null);

        /// <summary>
        /// Запрос, возвращающий результат
        /// </summary>
        IEnumerable<dynamic> Query(string query, object param = null, CommandType? type = null);

        /// <summary>
        /// Преобразование из типа базы данных
        /// </summary>
        Type FromDBType(string type);

        /// <summary>
        /// Преобразование в тип базы данных
        /// </summary>
        string ToDBType(Type type, int length = 0);

        #endregion Методы
    }
}
