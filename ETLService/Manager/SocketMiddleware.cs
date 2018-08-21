using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace ETLService.Manager
{
    /// <summary>
    /// Класс для обновления данных на всех подключённых клиентах
    /// </summary>
    public class SocketMiddleware
    {
        private readonly RequestDelegate nextDelegate;

        public SocketMiddleware(RequestDelegate next)
        {
            nextDelegate = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/api/broadcast")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    ETLSocket socket = Program.Manager.Broadcast.Add(webSocket);
                    await socket.Receive();
                }
                else
                    context.Response.StatusCode = 400;
            }
            else
                await nextDelegate(context);
        }
    }
}
