using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ETLService.Manager
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
        public async Task Broadcast(ETLBroadcastAction action)
        {
            string dataStr = JsonConvert.SerializeObject(action, Formatting.None);

            foreach (ETLSocket socket in sockets.Values)
                await socket.Send(dataStr);
        }

        /// <summary>
        /// Исполнение команды
        /// </summary>
        public async Task Invoke(string dataStr)
        {
            if (string.IsNullOrEmpty(dataStr))
                return;

            JObject data = (JObject)JsonConvert.DeserializeObject(dataStr);
            switch (data["func"].ToString())
            {
                case "ololo":
                    await Broadcast(new ETLBroadcastAction{
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

            etlSocket.ReceiveEvent += async (s, args) => await Invoke(args.Data);
            etlSocket.CloseEvent += (s, args) => {
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

        public async void Stop()
        {
            foreach (ETLSocket socket in sockets.Values)
                await socket.Close();
        }

        #endregion Основные функции
    }
}
