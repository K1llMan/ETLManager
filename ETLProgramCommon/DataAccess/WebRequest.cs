using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

using ETLCommon;

namespace ETLProgramCommon.DataAccess
{
    /// <summary>
    /// Структура для хранения параметров выполняемого запроса
    /// </summary>
    public struct WebRequestParams
    {
        public IWebProxy Proxy;
        public int RequestTimeout;
        public int Attempts;
        public int AttemptTimeout;
    }

    /// <summary>
    /// Веб-запрос
    /// </summary>
    public class WebRequest
    {
        #region Поля

        private CookieContainer cookies;

        private WebRequestParams parameters;

        #endregion Поля

        #region Основные функции

        public WebRequest(WebRequestParams requestParams)
        {
            parameters = requestParams;

            // Значения по умолчанию
            if (parameters.AttemptTimeout == 0)
                parameters.AttemptTimeout = 120000;
            if (parameters.Attempts == 0)
                parameters.Attempts = 5;
            if (parameters.RequestTimeout == 0)
                parameters.RequestTimeout = 600000;

            // Куки
            cookies = new CookieContainer();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        /// <summary>
        /// Получение ответа
        /// </summary>
        public string GetResponse(string url, string method = "GET", Dictionary<string, string> headers = null, string content = "",
            string contentType = "plain/text")
        {
            HttpWebResponse httpWebResponse = null;
            string errorResponse = string.Empty;
            for (int attempt = 1; attempt <= parameters.Attempts; attempt++)
                try
                {
                    HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                    httpWebRequest.Method = method;
                    httpWebRequest.ContentType = contentType;
                    httpWebRequest.Timeout = parameters.RequestTimeout;
                    httpWebRequest.CookieContainer = cookies;
                    // Некоторые сайты не отдают данные неизвестным агентам
                    httpWebRequest.UserAgent =
                        "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";

                    // Добавляем нужные заголовки
                    if (headers != null)
                        foreach (KeyValuePair<string, string> pair in headers)
                            httpWebRequest.Headers.Add(pair.Key, pair.Value);

                    // Если требуется работа через прокси-сервер
                    if (parameters.Proxy != null)
                        httpWebRequest.Proxy = parameters.Proxy;

                    // Добавляем содержимое к запросу
                    if (!string.IsNullOrEmpty(content))
                        try
                        {
                            Stream stream = httpWebRequest.GetRequestStream();
                            StreamWriter sr = new StreamWriter(stream);

                            sr.Write(content);

                            sr.Close();
                            stream.Close();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Ошибка записи содержимого запроса: {ex.Message}.");
                        }

                    httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    break;
                }
                catch (Exception ex)
                {
                    WebException webEx = ex as WebException;
                    if (webEx != null)
                        if (webEx.Response != null)
                        {
                            Stream stream = webEx.Response.GetResponseStream();
                            if (stream != null)
                            {
                                StreamReader sr = new StreamReader(stream);
                                errorResponse = sr.ReadToEnd().ReplaceRegex("<.*?>", " ");
                            }
                        }

                    if (attempt == parameters.Attempts)
                        throw new Exception($"Ошибка выполнения запроса: {ex.Message}).");

                    Thread.Sleep(parameters.AttemptTimeout);
                }

            if (httpWebResponse != null)
            {
                string encoding = "utf-8";
                if (!string.IsNullOrEmpty(httpWebResponse.CharacterSet))
                    encoding = httpWebResponse.CharacterSet;

                return new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding(encoding)).ReadToEnd();
            }

            throw new Exception(string.Format("На запрос не был получен ответ от \"{0}\". {1}", url,
                errorResponse != string.Empty ? " Содержание последнего ответа: \n" + errorResponse : string.Empty));
        }

        #endregion Основные функции
    }
}
