using System.Linq;

using Npgsql;

namespace ETLCommon
{
    public class PostgreSql: Database, IDatabase
    {
        #region Свойства

        public override DBTable this[string tableName]
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

                return new DBTable(this, tableName);
            }
        }

        #endregion Свойства

        #region Основные функции

        public PostgreSql(string connection) : base(connection)
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
