using System;
using System.Collections.Generic;
using System.Linq;

using ETLCommon;

using Newtonsoft.Json.Linq;

namespace ETLApp
{
    public class ETLProgram
    {
        #region Свойства

        public string ID { get; private set; }

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

            try
            {
                // Обход всех этапов закачки
                foreach (Stage stage in Stages.Where(s => s.Enabled))
                    stage.Exec();
            }
            catch (Exception ex)
            {
                Logger.WriteToTrace($"Ошибка при выполнении закачки: {ex}", TraceMessageKind.Error);
            }
        }

        /// <summary>
        /// Инициализация программы
        /// </summary>
        public void Initialize(JObject data)
        {
            try
            {
                ID = data["id"].ToString();

                // Инициализация списка этапов
                Stages = new List<Stage>();
                foreach (JProperty stage in data["stages"])
                    Stages.Add(new Stage(stage, this));
            }
            catch (Exception ex)
            {
                Logger.WriteToTrace($"Ошибка при инициализации закачки: {ex}", TraceMessageKind.Error);
            }
        }

        #endregion Основные функции
    }
}
