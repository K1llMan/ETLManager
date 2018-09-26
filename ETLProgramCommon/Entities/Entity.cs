using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using ETLCommon;

namespace ETLProgramCommon
{
    public class Entity: IEnumerable<DataRow>
    {
        #region Поля

        private IDBTable table;
        private DataTable dt;

        #endregion Поля

        #region Вспомогательные функции

        private bool CheckRowAttributes(object[] mapping)
        {
            return true;
        }

        protected void CopyValuesToRow(object[] mapping, DataRow row)
        {
            
        }

        protected DataRow FormDataRow(object[] mapping, bool generateID)
        {
            if (mapping == null)
                return null;

            if (!CheckRowAttributes(mapping))
            {
                // Сообщение об ошибках
                return null;
            }

            DataRow row = dt.NewRow();
            if (generateID)
                row["ID"] = table.GetNextVal();

            CopyValuesToRow(mapping, row);

            return row;
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public Entity(IDBTable dbTable)
        {
            table = dbTable;
            dt = new DataTable();
            dt.Columns.AddRange(table.Attributes.Select(a => new DataColumn(a.Name, a.Type)).ToArray());
        }

        public void Select(string constr = "")
        {
            dynamic rows = table.Select(constr);
            if (rows == null)
                return;

            dt.Clear();

            foreach (IDictionary<string, object> row in rows) {
                DataRow newRow = dt.NewRow();
                foreach (KeyValuePair<string, object> pair in row)
                    newRow[pair.Key] = pair.Value;

                dt.Rows.Add(newRow);
            }

            dt.AcceptChanges();
        }

        public void Update()
        {
            
        }

        public virtual void PumpRow(object[] mapping)
        {
            DataRow row = FormDataRow(mapping, true);
        }

        #endregion Основные функции

        #region IEnumerable

        public IEnumerator<DataRow> GetEnumerator()
        {
            return (IEnumerator<DataRow>)dt.Rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable
    }
}
