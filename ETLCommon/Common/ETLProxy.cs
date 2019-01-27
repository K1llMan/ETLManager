using System;
using System.Net;

using FluentFTP.Proxy;

namespace ETLCommon
{
    public class ETLProxy
    {
        public bool Enabled;
        public string Host;
        public int Port;
        public NetworkCredential Credential;

        #region Вспомогательные функции

        private object Convert(Type t)
        {
            if (!Enabled)
                return null;

            switch (t.Name)
            {
                case "ProxyInfo":
                    return new ProxyInfo
                    {
                        Host = Host,
                        Port = Port,
                        Credentials = Credential
                    };

                case "IWebProxy":
                case "WebProxy":
                    return new WebProxy(Host, Port)
                    {
                        Credentials = Credential
                    };
            }

            return null;
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public T As<T>()
        {
            return (T)Convert(typeof(T));
        }

        #endregion Основные функции
    }
}
