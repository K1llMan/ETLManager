using System;
using System.Collections.Generic;
using System.Text;

using ETLCommon;

using Newtonsoft.Json.Linq;

namespace ETLApp
{
    public class Stage
    {
        #region Свойства

        public string ID { get; }
        public bool Enabled { get; }

        public string Name { get; }

        #endregion Свойства

        #region Основные функции

        /// <summary>
        /// Выполнение этапа закачки
        /// </summary>
        public void Exec()
        {
            try
            {
                Logger.WriteToTrace($"Запуск этапа закачки \"{Name}\".");
            }
            catch (Exception ex)
            {
                Logger.WriteToTrace($"Произошла ошибка: {ex}.", TraceMessageKind.Error);
            }

        }

        public Stage(JProperty stage)
        {
            ID = stage.Name;

            JToken stageDesc = stage.Value;
            Enabled = Convert.ToBoolean(stageDesc["enabled"]);
            Name = stageDesc["name"].ToString();
        }

        #endregion Основные функции
    }
}
