using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ETLService.Manager.Broadcast
{
    /// <summary>
    /// Действие рассылки
    /// </summary>
    public class ETLBroadcastAction
    {
        public string Action;

        public object Data;
    }

    /// <summary>
    /// Класс для обновления данных на всех подключённых клиентах
    /// </summary>
    public class ETLBroadcast
    {
        private ConcurrentDictionary<string, ETLSocket> sockets = new ConcurrentDictionary<string, ETLSocket>();

        #region Основные функции

        /// <summary>
        /// Рассылка данных всем клиентам
        /// </summary>
        public async Task Send(params ETLBroadcastAction[] actions)
        {
            foreach (ETLBroadcastAction action in actions)
            {
                string dataStr = JsonConvert.SerializeObject(action, Formatting.None, new JsonSerializerSettings{ ContractResolver = new CamelCasePropertyNamesContractResolver() });

                foreach (ETLSocket socket in sockets.Values)
                    await socket.Send(dataStr);
            }
        }

        /// <summary>
        /// Исполнение команды
        /// </summary>
        public async Task Invoke(string dataStr)
        {
            if (string.IsNullOrEmpty(dataStr))
                return;

            JObject data = (JObject)JsonConvert.DeserializeObject(dataStr);
            switch (data["Action"].ToString())
            {
                case "ololo":
                    await Send(new ETLBroadcastAction{
                        Action = "myFunc",
                        Data = new string[] { "1", "5", "5" }
                    });
                    break;
            }
        }

        /// <summary>
        /// Добавление нового подключения
        /// </summary>
        public ETLSocket Add(WebSocket socket)
        {
            ETLSocket etlSocket = new ETLSocket(socket);

            etlSocket.OnReceive += async (s, args) => await Invoke(args.Data);
            etlSocket.OnClose += (s, args) => {
                Remove(((ETLSocket)s).GUID);
            };

            sockets.TryAdd(etlSocket.GUID, etlSocket);
            return etlSocket;
        }

        /// <summary>
        /// Удаление подключения
        /// </summary>
        public void Remove(string guid)
        {
            ETLSocket socket = null;
            if (sockets.ContainsKey(guid))
                sockets.TryRemove(guid, out socket);

            socket?.Close();
        }

        /// <summary>
        /// Закрытие всех сокетов
        /// </summary>
        public async void Stop()
        {
            foreach (ETLSocket socket in sockets.Values)
                await socket.Close();
        }

        #endregion Основные функции
    }
}
