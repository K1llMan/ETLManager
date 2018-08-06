using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Linq;

using Dapper;

using Npgsql;

namespace ETLCommon
{
    public class Database
    {
        #region Поля

        private string connectionStr;
        private IDbConnection connection;
        private IDbTransaction transaction;

        #endregion Поля

        #region Свойства

        public DBTable this[string tableName]
        {
            get
            {
                dynamic result = Query(
                    "select exists (" +
                    " select 1" +
                    " from information_schema.tables" +
                    $" where table_name = '{tableName}')")?.FirstOrDefault();

                // Если не существует, то создаём новую
                if (result == null || !result.exists)
                    return null;

                return new DBTable(this, tableName);
            }
        }

        /// <summary>
        /// Тип базы
        /// </summary>
        public DBType DatabaseType { get; private set; }

        #endregion Свойства

        #region Вспомогательные функции

        private string GetConnectionString(string parameters)
        {
            string[] properties = parameters.Split(';', StringSplitOptions.RemoveEmptyEntries);

            Uri uri = new Uri(properties[0]);

            string typeStr = uri.Scheme;

            if (!Enum.TryParse(typeof(DBType), typeStr, true, out var type))
                return string.Empty;

            DatabaseType = (DBType)type;

            Dictionary<string, string> paramDict = new Dictionary<string, string> {
                { "User ID", uri.UserInfo.Split(':')[0] },
                { "Password", uri.UserInfo.Split(':')[1] },
            };

            List<string> segments = uri.Segments.ToList();
            segments.Remove("/");

            // У Postgre другой набор параметров подключения
            if (DatabaseType == DBType.PostgreSql)
            {
                paramDict.Add("Host", uri.Host);
                paramDict.Add("Port", uri.Port.ToString());

                if (segments.Count > 0 && !paramDict.ContainsKey("Database"))
                    paramDict.Add("Database", segments.Last());
            }
            else
                paramDict.Add("Data Source", DatabaseType == DBType.Oracle 
                    ? uri.Host
                    : uri.Host + "\\" + uri.Segments.Last());

            foreach (string property in properties.Skip(1))
            {
                string key = property.GetMatches(@".+(?=\=)").First().Trim();
                string value = property.GetMatches(@"(?<=\=).+").First().Trim();

                if (string.IsNullOrEmpty(key) || paramDict.ContainsKey(key) )
                    continue;

                paramDict.Add(key, value);
            }

            return string.Join("; ", paramDict.Select(p => $"{p.Key}={p.Value}"));
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public Database(string connection)
        {
            connectionStr = connection;
        }

        /// <summary>
        /// Соединения с базой (формат [dbType]://[user]:[password]@[serverName[:portNumber][/instanceName]][;property=value[;property=value]])
        /// postgresql://sysdba:masterkey@localhost:5432/db
        /// </summary>
        public void Connect()
        {
            connection?.Close();
            connection?.Dispose();

            if (string.IsNullOrEmpty(connectionStr))
                return;

            string connectionString = GetConnectionString(connectionStr);

            switch (DatabaseType)
            {
                case DBType.PostgreSql:
                    connection = new NpgsqlConnection(connectionString);
                    break;

                case DBType.SqlServer:
                    connection = new SqlConnection(connectionString);
                    break;

                case DBType.Oracle:
                    connection = new OracleConnection(connectionString);
                    break;
                default:
                    return;
            }

            connection.Open();
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

        #endregion Основные функции
    }
}
