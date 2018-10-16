using System.Data;
using System.Data.OracleClient;

namespace ETLCommon
{
    public class OracleDB: Database, IDatabase
    {
        #region Основные функции

        public OracleDB(string connection) : base(connection)
        {
            DatabaseType = DBType.Oracle;
        }

        public override IDbConnection GetConnection()
        {
            return new OracleConnection(connectionStr);
        }

        #endregion Основные функции
    }
}
