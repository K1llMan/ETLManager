using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ETLCommon;

using Newtonsoft.Json.Linq;

namespace ETLApp
{
    public class ETLProgram
    {
        #region Свойства

        public string ID { get; }

        public ETLSettings Settings { get; set; }

        public List<Stage> Stages { get; set; }

        #endregion Свойства

        #region Основные функции

        /// <summary>
        /// Запуск программы на выполнение
        /// </summary>
        public void Exec()
        {
            Logger.WriteToTrace($"Запуск закачки \"{ID}\".", TraceMessageKind.Information);

            // Обход всех этапов закачки
            foreach (Stage stage in Stages.Where(s => s.Enabled))
                stage.Exec();
        }

        /// <summary>
        /// Инициализация программы
        /// </summary>
        public ETLProgram(ETLSettings settings, JObject data)
        {
            try
            {
                Settings = settings;
                ID = data["id"].ToString();

                // Инициализация списка этапов
                Stages = new List<Stage>();
                foreach (JProperty stage in data["stages"])
                    Stages.Add(new Stage(stage));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion Основные функции
    }
}
