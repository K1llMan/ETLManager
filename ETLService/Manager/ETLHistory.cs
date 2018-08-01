using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ETLCommon;

namespace ETLService.Manager
{
    /// <summary>
    /// Запись истории
    /// </summary>
    public class ETLHistoryRecord
    {
        public decimal ID;
        public string ProgramID;
        public string SystemVersion;
        public string ProgramVersion;
        public DateTime PumpDate;
        public string UserName;
    }

    /// <summary>
    /// Класс для работы с историей
    /// </summary>
    public class ETLHistory
    {
        #region Поля

        private Database db;

        #endregion Поля

        #region Основные функции

        public decimal AddRecord(string programID, Version systemVersion, Version programVersion, string userName)
        {
            try
            {
                string query = "insert into etl_history (programid, systemversion, programversion, username)" +
                               $" values ('{programID}', '{systemVersion}', '{programVersion}', '{userName}')";

                db.Execute(query);

                decimal id = db["etl_history"].GetCurVal();
                return id;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public ETLHistory(Database db)
        {
            this.db = db;
        }

        #endregion Основные функции
    }
}
