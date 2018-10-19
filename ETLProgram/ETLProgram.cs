using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ETLCommon;

using Newtonsoft.Json.Linq;

namespace ETLApp
{
    public class ETLProgram: IDisposable
    {
        #region Поля

        private Stage curStage;

        private Dictionary<string, object> commonParams;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Контекст
        /// </summary>
        public ETLContext Context { get; set; }

        /// <summary>
        /// Идентификатор программы закачки
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Номер сессии
        /// </summary>
        public decimal SessNo { get; set; }

        /// <summary>
        /// Пользовательские параметры
        /// </summary>
        public Dictionary<string, object> UserParams { get; private set; }

        /// <summary>
        /// Этапы
        /// </summary>
        public List<Stage> Stages { get; set; }

        /// <summary>
        /// Текущий статус
        /// </summary>
        public PumpStatus Status
        {
            get
            {
                List<StageStatus> stageStatuses = Stages.Select(s => s.Status).ToList();

                if (stageStatuses.Contains(StageStatus.Errors))
                    return PumpStatus.Errors;
                if (stageStatuses.Contains(StageStatus.Warnings))
                    return PumpStatus.Warnings;
                return PumpStatus.Successful;
            }
        }

        /// <summary>
        /// Корневой каталог входящих данных
        /// </summary>
        public DirectoryInfo RootInDir { get; private set; }

        /// <summary>
        /// Корневой каталог исходящих данных
        /// </summary>
        public DirectoryInfo RootOutDir { get; private set; }

        /// <summary>
        /// Версия программы закачки
        /// </summary>
        public Version Version { get; private set; }

        #endregion Свойства

        #region Вспомогательные функции

        private Dictionary<string, object> FormParamsList(JToken list)
        {
            Dictionary<string, object> paramDict = new Dictionary<string, object>();

            foreach (JObject paramList in list.Children())
                foreach (JProperty param in paramList.Properties())
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
            Logger.Indent();

            try
            {
                Logger.WriteToTrace($"Общие параметры закачки: \n\t{string.Join(";\n\t", commonParams.Select(p => $"{p.Key}: {p.Value}"))}.", 
                    TraceMessageKind.Information);

                // Обход всех этапов закачки
                foreach (Stage stage in Stages.Where(s => s.Enabled))
                {
                    Logger.WriteToTrace($"Запуск этапа закачки \"{stage.Name}\".");

                    Logger.Indent();

                    curStage = stage;

                    if (stage.Parameters.Count > 0)
                        Logger.WriteToTrace($"Параметры этапа \"{stage.Name}\": \n\t{string.Join(";\n\t", stage.Parameters.Select(p => $"{p.Key}: {p.Value}"))}.",
                            TraceMessageKind.Information);

                    // Формирование набора доступных параметров для этапа
                    UserParams = commonParams
                        .Union(curStage.Parameters.Where(p => !commonParams.ContainsKey(p.Key)))
                        .ToDictionary(p => p.Key, p => p.Value);
                    curStage.Exec();

                    Logger.Unindent();
                }

                Logger.WriteToTrace($"Результат выполнения этапов: \n\t{ string.Join("\n\t", Stages.Where(s => s.Enabled).Select(s => $"{s.Name}: {s.Status.GetDescription()}")) }", 
                    TraceMessageKind.Information);
            }
            catch (Exception ex)
            {
                Logger.WriteToTrace($"Ошибка при выполнении закачки: {ex}", TraceMessageKind.Error);
            }

            Logger.Unindent();
        }

        /// <summary>
        /// Инициализация программы
        /// </summary>
        public void Initialize(JObject data)
        {
            try
            {
                ID = data["id"].ToString();
                // Соединение с базой
                try
                {
                    Context.Initialize();
                }
                catch (Exception ex)
                {
                    Logger.WriteToTrace($"Ошибка при подключении к базе: {ex}", TraceMessageKind.Error);
                    return;
                }

                Version = new Version(data["version"]?.ToString() ?? "0.0.0");
                if (Version > Context.Version)
                {
                    Logger.WriteToTrace("Версия системы ниже необходимой для запуска.", TraceMessageKind.Error);
                    return;
                }

                if (data["rootInDir"].IsNullOrEmpty())
                {
                    Logger.WriteToTrace("Не задана директория входных данных.", TraceMessageKind.Error);
                    return;
                }

                // Основные рабочие каталоги с данными
                RootInDir = new DirectoryInfo(Path.Combine(Context.Settings.Registry.InputPath, data["rootInDir"].ToString()));
                if (!data["rootInDir"].IsNullOrEmpty())
                    RootOutDir = new DirectoryInfo(Path.Combine(Context.Settings.Registry.OutputPath, data["rootOutDir"].ToString()));

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

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}
