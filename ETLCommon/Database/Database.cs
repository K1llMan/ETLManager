using System;
using System.Collections.Generic;
using System.Data;

using Dapper;

namespace ETLCommon
{
    public class Database
    {
        #region Поля

        protected string connectionStr;
        protected IDbConnection connection;
        protected IDbTransaction transaction;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Индексатор по таблицам базы. Для каждой базы получение данных о таблице может отличаться
        /// </summary>
        public virtual DBTable this[string tableName]
        {
            get { return null; }
        }

        /// <summary>
        /// Тип базы
        /// </summary>
        public DBType DatabaseType { get; internal set; }

        #endregion Свойства

        #region Основные функции

        public Database(string connection)
        {
            connectionStr = connection;
            DatabaseType = DBType.Unknown;
        }

        public virtual void Connect()
        {
            if (string.IsNullOrEmpty(connectionStr))
                return;

            connection?.Close();
            connection?.Dispose();
        }

        /// <summary>
        /// Отключение
        /// </summary>
        public void Disconnect()
        {
            connection?.Close();
        }

        /// <summary>
        /// Создание транзакции
        /// </summary>
        public void BeginTransaction()
        {
            if (connection == null)
                return;

            transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        /// <summary>
        /// Фиксация транзакции
        /// </summary>
        public void Commit()
        {
            transaction?.Commit();
        }

        /// <summary>
        /// Откат транзакции
        /// </summary>
        public void Rollback()
        {
            transaction?.Rollback();
        }

        /// <summary>
        /// Параметризированный запрос, возвращающий количество строк
        /// </summary>
        public object Execute(string query, object param = null, CommandType? type = null)
        {
            if (connection?.State == ConnectionState.Closed)
                return -1;
            
            return connection.Execute(query, param, commandType: type, transaction: transaction);
        }

        /// <summary>
        /// Параметризированный запрос, возвращающий значение
        /// </summary>
        public object ExecuteScalar(string query, object param = null, CommandType? type = null)
        {
            return connection?.State == ConnectionState.Closed 
                ? null 
                : connection?.ExecuteScalar(query, param, commandType: type, transaction: transaction);
        }

        /// <summary>
        /// Запрос, возвращающий результат
        /// </summary>
        public IEnumerable<dynamic> Query(string query, object param = null, CommandType? type = null)
        {
            return connection?.State == ConnectionState.Closed 
                ? null 
                : connection?.Query<dynamic>(query, param, commandType: type, transaction: transaction);
        }

        /// <summary>
        /// Типы базы в типы .Net
        /// </summary>
        public Type FromDBType(string type)
        {
            if (type.IsMatch("integer|real|double|bigint|smallint|numeric"))
                return typeof(decimal);

            if (type.IsMatch("character|char|text"))
                return typeof(string);

            if (type.IsMatch("date|time|timestamp"))
                return typeof(DateTime);

            return typeof(object);
        }

        /// <summary>
        /// Типы базы в типы .Net
        /// </summary>
        public string ToDBType(Type type, int length = 0)
        {
            switch (type.Name)
            {
                case "Int16":
                    return "smallint";
                case "Int32":
                    return "int";
                case "Int64":
                    return "bigint";
                case "Decimal":
                    return "decimal";
                case "DateTime":
                    return "date";
                case "Single":
                    return "real";
                case "Double":
                    return "float";
                case "String":
                    return "varchar";
            }

            return "varchar";
        }

        #endregion Основные функции
    }
}
