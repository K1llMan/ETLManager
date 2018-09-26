﻿using System.Linq;

using Npgsql;

namespace ETLCommon
{
    public class PostgreSqlDB: Database, IDatabase
    {
        #region Свойства

        public override IDBTable this[string tableName]
        {
            get
            {
                dynamic result = Query(
                    "select exists (" +
                    " select 1" +
                    " from information_schema.tables" +
                    $" where table_name = '{tableName}')")?.FirstOrDefault();

                if (result == null || !result.exists)
                    return null;

                return new PostgreSqlDBTable(this, tableName);
            }
        }

        #endregion Свойства

        #region Основные функции

        public PostgreSqlDB(string connection) : base(connection)
        {
            DatabaseType = DBType.PostgreSql;
        }

        public override void Connect()
        {
            base.Connect();
            connection = new NpgsqlConnection(connectionStr);
            connection.Open();
        }

        #endregion Основные функции
    }
}