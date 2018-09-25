using System.Data.OracleClient;

namespace ETLCommon
{
    public class Oracle: Database, IDatabase
    {
        #region Основные функции

        public Oracle(string connection) : base(connection)
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
