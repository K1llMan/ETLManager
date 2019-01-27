using System.Data;
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

        public override IDbConnection GetConnection()
        {
            return new SqlConnection(connectionStr);
        }

        #endregion Основные функции
    }
}
