using System;
using System.Linq;

namespace ETLCommon
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

        private IDatabase db;

        #endregion Поля

        #region Свойства

        public dynamic this[decimal sessNo]
        {
            get { return GetRecord(sessNo); }
            set { SetRecord(sessNo, value); }
        }

        #endregion Свойства

        #region Вспомогательные функции

        private dynamic GetRecord(decimal sessNo)
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

        private void SetRecord(decimal sessNo, dynamic record)
        {
            try
            {
                db.Execute(
                    "update etl_history" +
                    " set status = @status, programid = @programid, systemversion = @systemversion, programversion = @programversion," +
                    " pumpstartdate = @pumpstartdate, pumpfinishdate = @pumpfinishdate, username = @username, config = @config" +
                    $" where id = {sessNo}", record);
            }
            catch (Exception ex)
            {
                
            }
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public decimal AddRecord(string programID, Version systemVersion, Version programVersion, string userName, string config)
        {
            try
            {
                decimal id = db["etl_history"].GetNextVal();
                string query = 
                    "insert into etl_history (id, status, programid, systemversion, programversion, username, config)" +
                    $" values ({id}, '{PumpStatus.Terminated.ToString()}', '{programID}', '{systemVersion}', '{programVersion}', '{userName}', '{config}')";

                db.Execute(query);

                return id;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public dynamic GetLastRecord(string programID)
        {
            try
            {
                string query =
                    "select *" + 
                    " from etl_history" + 
                    $" where programid = '{programID}'" +
                    " order by pumpfinishdate desc" +
                    " limit 1";

                return db.Query(query).SingleOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ETLHistory(IDatabase db)
        {
            this.db = db;
        }

        #endregion Основные функции
    }
}
