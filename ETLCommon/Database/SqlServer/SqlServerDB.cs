using System.Data.SqlClient;

namespace ETLCommon
{
    public class SqlServerDB: Database, IDatabase
    {
        #region Основные функции

        public SqlServerDB(string connection) : base(connection)
        {
            DatabaseType = DBType.SqlServer;
        }

        public override void Connect()
        {
            base.Connect();
            connection = new SqlConnection(connectionStr);
            connection.Open();
        }

        #endregion Основные функции
    }
}
