using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using ETLCommon;

namespace ETLProgramCommon
{
    public class Entity
    {
        #region Поля

        private DBTable table;
        private DataTable dt;

        #endregion Поля

        #region Вспомогательные функции



        #endregion Вспомогательные функции

        #region Основные функции

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
        }

        public Entity(DBTable dbTable)
        {
            table = dbTable;
            dt = new DataTable();
            dt.Columns.AddRange(table.Attributes.Select(a => new DataColumn(a.Name, a.Type)).ToArray());
        }

        #endregion Основные функции
    }
}
