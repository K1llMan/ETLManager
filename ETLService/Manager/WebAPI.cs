using System.Collections.Generic;

namespace ETLService.Manager
{
    /// <summary>
    /// Обработка запросов к WebAPI
    /// </summary>
    public class WebAPI
    {
        #region Перечисления

        private enum Statuses
        {
            OK,
            Error
        }

        #endregion Перечисления

        #region Вспомогательные функции

        private static object GetResponse(Statuses status, object data)
        {
            return new Dictionary<string, object>{
                { "status", status.ToString() },
                { "data", data }
            };
        }

        #endregion Вспомогательные функции

        #region Основные функции

        /// <summary>
        /// Успешное выполнение операции
        /// </summary>
        public static object OK(object data)
        {
            return GetResponse(Statuses.OK, data);
        }

        /// <summary>
        /// Выполнение с ошибкой
        /// </summary>
        public static object Error(object data)
        {
            return GetResponse(Statuses.Error, data);
        }

        #endregion Основные функции
    }
}
