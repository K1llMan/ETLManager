using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ETLService.Manager
{
    /// <summary>
    /// Класс для обновления данных на всех подключённых клиентах
    /// </summary>
    public class ETLBroadcast
    {
        private static ConcurrentDictionary<string, ETLSocket> sockets = new ConcurrentDictionary<string, ETLSocket>();

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
            etlSocket.ReceiveEvent += async args => await Invoke(args.Data);

            sockets.TryAdd(Guid.NewGuid().ToString(), etlSocket);

            return etlSocket;
        }

        #endregion Основные функции
    }
}
