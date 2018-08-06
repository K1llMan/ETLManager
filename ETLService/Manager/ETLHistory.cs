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

        public decimal AddRecord(string programID, Version systemVersion, Version programVersion, string userName, string config)
        {
            try
            {
                decimal id = db["etl_history"].GetNextVal();
                string query = 
                    "insert into etl_history (id, programid, systemversion, programversion, username, config)" +
                    $" values ({id}, '{programID}', '{systemVersion}', '{programVersion}', '{userName}', '{config}')";

                db.Execute(query);

                return id;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public dynamic GetRecord(decimal sessNo)
        {
            try
            {
                return db.Query(
                    "select *" + 
                    " from etl_history" + 
                    $" where id = {sessNo}").FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ETLHistory(Database db)
        {
            this.db = db;
        }

        #endregion Основные функции
    }
}
