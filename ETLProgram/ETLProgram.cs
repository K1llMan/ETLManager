﻿using System;
using System.Collections.Generic;
using System.Linq;

using ETLCommon;

using Newtonsoft.Json.Linq;

namespace ETLApp
{
    public class ETLProgram
    {
        #region Поля

        private Stage curStage;

        private Dictionary<string, object> commonParams;

        #endregion Поля

        #region Свойства

        public string ID { get; private set; }

        public Dictionary<string, object> UserParams { get; private set; }

        public ETLSettings Settings { get; set; }

        public List<Stage> Stages { get; set; }

        #endregion Свойства

        #region Вспомогательные функции

        private Dictionary<string, object> FormParamsList(JToken list)
        {
            Dictionary<string, object> paramDict = new Dictionary<string, object>();

            foreach (JProperty param in list.Children())
                try
                {
                    if (paramDict.ContainsKey(param.Name))
                        continue;

                    paramDict.Add(param.Name, ((JValue)param.Value["value"]).Value);
                }
                catch (Exception ex)
                {
                    Logger.WriteToTrace($"Ошибка при формировании общих параметров: {ex}", TraceMessageKind.Error);
                }

            return paramDict;
        }

        #endregion Вспомогательные функции

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
                {
                    curStage = stage;
                    // Формирование набора доступных параметров для этапа
                    UserParams = commonParams
                        .Union(stage.Parameters.Where(p => !commonParams.ContainsKey(p.Key)))
                        .ToDictionary(p => p.Key, p => p.Value);
                    stage.Exec();
                }
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

                // Формирование общих параметров закачки
                commonParams = FormParamsList(data["commonParams"]);

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
