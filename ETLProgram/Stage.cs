using System;
using System.Collections.Generic;
using System.Reflection;

using ETLCommon;

using Newtonsoft.Json.Linq;

namespace ETLApp
{
    public delegate void StageFunc();

    public class Stage
    {
        #region Поля

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Идентификатор этапа
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// Доступность этапа
        /// </summary>
        public bool Enabled { get; }

        /// <summary>
        /// Текстовое описание
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Функция выполнения
        /// </summary>
        public StageFunc Method { get; }

        /// <summary>
        /// Индивидуальные параметры этапа
        /// </summary>
        public Dictionary<string, object> Parameters { get; }

        /// <summary>
        /// Родительская программа
        /// </summary>
        public ETLProgram RootProgram { get; }

        /// <summary>
        /// Результат выполнения
        /// </summary>
        public StageStatus Status { get; private set; }

        /// <summary>
        /// Статус выполнения
        /// </summary>
        public StageExecutionStatus ExecStatus { get; private set; }

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

        private void WriteTraceHandler(Logger.WriteEventArgs e)
        {
            switch (e.Kind)
            {
                case TraceMessageKind.Warning:
                    if (Status != StageStatus.Errors)
                        Status = StageStatus.Warnings;
                    break;
                case TraceMessageKind.Error:
                    Status = StageStatus.Errors;
                    break;
            }                
        }

        #endregion Вспомогательные функции

        #region Основные функции

        /// <summary>
        /// Выполнение этапа закачки
        /// </summary>
        public void Exec()
        {
            // Добавление контроллера записи в лог для установки статуса в зависимости от сообщений лога
            Logger.WriteEvent += WriteTraceHandler;

            try
            {
                Logger.WriteToTrace($"Запуск этапа закачки \"{Name}\".");
                ExecStatus = StageExecutionStatus.Execution;
                Method?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.WriteToTrace($"Произошла ошибка: {ex}.", TraceMessageKind.Error);
            }
            finally
            {
                ExecStatus = StageExecutionStatus.Finished;

                // Отключение контроллера
                Logger.WriteEvent -= WriteTraceHandler;
            }
        }

        public Stage(JProperty stage, ETLProgram program)
        {
            ID = stage.Name;

            JToken stageDesc = stage.Value;
            Enabled = Convert.ToBoolean(stageDesc["enabled"]);
            // Установка статусов по умолчанию
            Status = StageStatus.Successful;
            ExecStatus = Enabled ? StageExecutionStatus.InQueue : StageExecutionStatus.Skipped;

            Name = stageDesc["name"].ToString();

            // Формирование списка параметров этапа
            Parameters = FormParamsList(stageDesc["params"]);

            RootProgram = program;

            // Привязка метода объекта программы к этапу
            string functName = stageDesc["func"].ToString();
            if (string.IsNullOrEmpty(functName))
            {
                Logger.WriteToTrace("Не задана функция выполнения этапа.", TraceMessageKind.Warning);
                return;
            }

            MethodInfo method = program.GetType().GetMethod(functName);
            if (method == null)
            {
                Logger.WriteToTrace($"Не найдена функция выполнения \"{functName}\" этапа \"{ID}\".", TraceMessageKind.Warning);
                return;
            }

            Method = (StageFunc)method.CreateDelegate(typeof(StageFunc), program);
        }

        #endregion Основные функции
    }
}
