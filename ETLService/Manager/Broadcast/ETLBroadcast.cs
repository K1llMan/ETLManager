using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ETLService.Manager
{
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
        public async Task Broadcast(object data)
        {
            string dataStr = JsonConvert.SerializeObject(data, Formatting.None);

            foreach (ETLSocket socket in sockets.Values)
                await socket.Send(dataStr);
        }

        /// <summary>
        /// Исполнение команды
        /// </summary>
        public async Task Invoke(string data)
        {
            object dataStr = JsonConvert.DeserializeObject(data);

            await Broadcast("invoked");
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

        #endregion Основные функции
    }
}
