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

        public override void Connect()
        {
            base.Connect();
            connection = new OracleConnection(connectionStr);
            connection.Open();
        }

        #endregion Основные функции
    }
}
