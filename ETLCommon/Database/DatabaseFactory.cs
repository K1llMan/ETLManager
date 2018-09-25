using System;
using System.Collections.Generic;
using System.Linq;

namespace ETLCommon
{
    public static class DatabaseFactory
    {
        private static DBType dbType;

        private static string GetConnectionString(string parameters)
        {
            string[] properties = parameters.Split(';', StringSplitOptions.RemoveEmptyEntries);

            Uri uri = new Uri(properties[0]);

            string typeStr = uri.Scheme;

            if (!Enum.TryParse(typeof(DBType), typeStr, true, out var type))
                return string.Empty;

            dbType = (DBType)type;

            Dictionary<string, string> paramDict = new Dictionary<string, string> {
                { "User ID", uri.UserInfo.Split(':')[0] },
                { "Password", uri.UserInfo.Split(':')[1] },
            };

            List<string> segments = uri.Segments.ToList();
            segments.Remove("/");

            // У Postgre другой набор параметров подключения
            if (dbType == DBType.PostgreSql)
            {
                paramDict.Add("Host", uri.Host);
                paramDict.Add("Port", uri.Port.ToString());

                if (segments.Count > 0 && !paramDict.ContainsKey("Database"))
                    paramDict.Add("Database", segments.Last());
            }
            else
                paramDict.Add("Data Source", dbType == DBType.Oracle
                    ? uri.Host
                    : uri.Host + "\\" + uri.Segments.Last());

            foreach (string property in properties.Skip(1))
            {
                string key = property.GetMatches(@".+(?=\=)").First().Trim();
                string value = property.GetMatches(@"(?<=\=).+").First().Trim();

                if (string.IsNullOrEmpty(key) || paramDict.ContainsKey(key))
                    continue;

                paramDict.Add(key, value);
            }

            return string.Join("; ", paramDict.Select(p => $"{p.Key}={p.Value}"));
        }

        /// <summary>
        /// Получение экземляра базы данных
        /// </summary>
        /// <param name="connection">
        /// Соединения с базой (формат [dbType]://[user]:[password]@[serverName[:portNumber][/instanceName]][;property=value[;property=value]])
        /// postgresql://sysdba:masterkey@localhost:5432/db
        /// </param>
        public static IDatabase GetDatabase(string connection)
        {
            string connectionString = GetConnectionString(connection);

            switch (dbType)
            {
                case DBType.PostgreSql:
                    return new PostgreSql(connectionString);

                case DBType.Oracle:
                    return new Oracle(connectionString);

                case DBType.SqlServer:
                    return new SqlServer(connectionString);
            }

            return null;
        }
    }
}
