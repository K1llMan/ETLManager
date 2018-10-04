using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

using ETLCommon;

using FluentFTP;
using FluentFTP.Proxy;

namespace ETLProgramCommon.DataAccess
{
    public delegate bool SkipFilesDelegate(string fileName);
    public delegate string DestinationDirectoryFilesDelegate(string fileName);

    /// <summary>
    /// Параметры соединения с ftp
    /// </summary>
    public class FtpParams
    {
        public string Host;
        public int Port;
        public NetworkCredential Credentials;
        public int Timeout;
        public int Attempts;
        public int AttemptTimeout;

        public ProxyInfo Proxy;

        #region Основные функции

        public FtpParams()
        {
            Port = 21;
            Timeout = 5000;
            Attempts = 5;
            AttemptTimeout = 15000;
        }

        #endregion Основные функции
    }

    /// <summary>
    /// Класс для работы с ftp
    /// </summary>
    public class FtpConnector
    {
        #region Поля

        private IFtpClient ftpClient;
        private int attempts;
        private int attemptsTimeout;

        #endregion Поля

        #region Свойства

        /// <summary>
        /// Рабочий каталог
        /// </summary>
        public string WorkDir
        {
            get; set;
        }

        #endregion Свойства

        #region Вспомогательные функции

        /// <summary>
        /// Формирование списка файлов на ftp
        /// </summary>
        private FtpListItem[] GetFtpItemsList(string topDirectory, FtpListOption options)
        {
            List<FtpListItem> files = new List<FtpListItem>();
            FtpListItem[] items = ftpClient.GetListing(topDirectory);

            foreach (FtpListItem item in items)
                if (item.Type != FtpFileSystemObjectType.Directory)
                    files.Add(item);
                else
                    if (options == FtpListOption.Recursive)
                        if (!item.Name.IsMatch(@"^\.+$"))
                            files.AddRange(GetFtpItemsList(topDirectory + Path.AltDirectorySeparatorChar + item.Name, options));

            return files.ToArray();
        }

        #endregion Вспомогательные функции

        #region Основные функции

        public FtpConnector(FtpParams parameters) : this(parameters, Encoding.UTF8)
        {
            
        }

        public FtpConnector(FtpParams parameters, Encoding encoding)
        {
            ftpClient = parameters.Proxy == null
                ? new FtpClient()
                : new FtpClientHttp11Proxy(parameters.Proxy);

            ftpClient.Encoding = encoding;

            ftpClient.Host = parameters.Host.Trim();
            ftpClient.Port = parameters.Port;
            ftpClient.Credentials = parameters.Credentials;
            ftpClient.ConnectTimeout = parameters.Timeout;
            attempts = parameters.Attempts < 1 ? 1 : parameters.Attempts;
            attemptsTimeout = parameters.AttemptTimeout < 1000 ? 1000 : parameters.AttemptTimeout;
        }

        /// <summary>
        /// Соединение
        /// </summary>
        public bool Connect()
        {
            if (string.IsNullOrEmpty(ftpClient.Host))
                throw new Exception("Не задан хост.");

            for (int attempt = 1; attempt <= attempts; attempt++)
                try
                {
                    ftpClient.Connect();
                    break;
                }
                catch (Exception ex)
                {
                    if (attempt == attempts)
                        throw new Exception("Ошибка соединения с ftp.", ex);
                    Thread.Sleep(attemptsTimeout);
                }

            return ftpClient.IsConnected;
        }

        /// <summary>
        ///  Отключение
        /// </summary>
        public void Disconnect()
        {
            if (ftpClient.IsConnected)
                ftpClient.Disconnect();
        }

        /// <summary>
        /// Загрузка файла
        /// </summary>
        public void GetFile(string sourceFileName, string destFileName)
        {
            if (ftpClient.IsConnected)
                ftpClient.DownloadFile(destFileName, sourceFileName);
        }

        /// <summary>
        /// Получить все файлы из директории
        /// </summary>
        public int GetFiles(string destDir)
        {
            return GetFiles("", destDir, FtpListOption.Recursive);
        }

        /// <summary>
        /// Получить файлы с сервера по маске - регулярному выражению
        /// </summary>
        public int GetFiles(string mask, string destDir, FtpListOption options)
        {
            SkipFilesDelegate skipFiles = name => !name.IsMatch(mask);

            return GetFiles(skipFiles, destDir, options);
        }

        /// <summary>
        /// Получение файлов с проверкой имени делегатом пропуска файлов
        /// </summary>
        public int GetFiles(SkipFilesDelegate skipFiles, string destDir, FtpListOption options)
        {
            int count = 0;
            if (ftpClient.IsConnected)
            {
                FtpListItem[] items = GetFtpItemsList(Path.AltDirectorySeparatorChar + WorkDir, options);

                foreach (FtpListItem itemData in items)
                    if (!skipFiles(itemData.Name))
                    {
                        string path = itemData.FullName.Replace(WorkDir, string.Empty).Trim('/').Replace('/', Path.DirectorySeparatorChar);
                        GetFile(itemData.FullName, Path.Combine(destDir, path));
                        count++;
                    }
            }

            return count;
        }

        /// <summary>
        /// Получение файлов с проверкой имени делегатом пропуска файлов и делегатом получения пути-назначения файла
        /// </summary>
        public int GetFiles(SkipFilesDelegate skipFiles, DestinationDirectoryFilesDelegate destDir, FtpListOption options)
        {
            int count = 0;
            if (ftpClient.IsConnected)
            {
                FtpListItem[] items = ftpClient.GetListing(Path.AltDirectorySeparatorChar + WorkDir, options);

                foreach (FtpListItem itemData in items)
                    if (!skipFiles(itemData.FullName))
                    {
                        string outDirectory = destDir(itemData.Name);
                        if (!Directory.Exists(outDirectory))
                            Directory.CreateDirectory(outDirectory);

                        GetFile(itemData.FullName, Path.Combine(outDirectory, itemData.Name));
                        count++;
                    }
            }
            return count;
        }

        #endregion Основные функции
    }
}
