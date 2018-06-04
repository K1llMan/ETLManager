using System;
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

        public string ID { get; }

        public bool Enabled { get; }

        public string Name { get; }

        public StageFunc Method { get; }

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
                Method?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.WriteToTrace($"Произошла ошибка: {ex}.", TraceMessageKind.Error);
            }

        }

        public Stage(JProperty stage, ETLProgram program)
        {
            ID = stage.Name;

            JToken stageDesc = stage.Value;
            Enabled = Convert.ToBoolean(stageDesc["enabled"]);
            Name = stageDesc["name"].ToString();

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
