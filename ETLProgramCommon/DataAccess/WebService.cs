using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace ETLProgramCommon.DataAccess
{
    /// <summary>
    /// Родительский класс для веб-сервисов
    /// </summary>
    public class WebService
    {
        #region Поля

        private Dictionary<string, string> serviceHeaders;
        private WebRequest request;

        #endregion Поля

        #region Свойства

        public string URL
        {
            get;
        }

        #endregion Свойства

        #region Основные функции

        /// <summary>
        /// Запрос к сервису
        /// </summary>
        protected string GetResponse(string route = "", string method = "GET", Dictionary<string, string> parameters = null, Dictionary<string, string> headers = null, 
            string content = "", string contentType = "plain/text")
        {
            if (headers == null)
                headers = new Dictionary<string, string>();

            // Подготовка заголовков запроса
            foreach (KeyValuePair<string, string> pair in serviceHeaders)
                if (!headers.ContainsKey(pair.Key))
                    headers.Add(pair.Key, pair.Value);

            // Добавление параметров к запросу
            string fullUrl = URL;
            if (route != null)
                fullUrl += $"/{route}";
            if (parameters != null)
                fullUrl += $"?{string.Join("&", parameters.Select(p => $"{p.Key}={p.Value}"))}";

            return request.GetResponse(fullUrl, method, headers, content, contentType);
        }

        /// <summary>
        /// Сохранение данных в файл
        /// </summary>
        protected void SaveToFile(string fileName, string data)
        {
            if (fileName == string.Empty)
                return;

            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            File.WriteAllText(fileName, data);
        }

        /// <summary>
        /// Установка заголовков запросов по умолчанию
        /// </summary>
        protected virtual void SetDefaultHeaders()
        {
            
        }

        /// <summary>
        /// Авторизация
        /// </summary>
        public virtual bool Login(string userName, string password)
        {
            return true;
        }

        public WebService(string url, IWebProxy webProxy = null)
        {
            URL = url.TrimEnd('/');

            request = new WebRequest(new WebRequestParams {
                Proxy = webProxy
            });
            serviceHeaders = new Dictionary<string, string>();
        }

        #endregion Основные функции
    }
}
