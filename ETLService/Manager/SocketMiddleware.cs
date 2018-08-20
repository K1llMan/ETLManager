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
        private static ConcurrentDictionary<string, ETLBroadcast> _activeConnections = new ConcurrentDictionary<string, ETLBroadcast>();
        private string _packet;

        private ManualResetEvent _send = new ManualResetEvent(false);
        private ManualResetEvent _exit = new ManualResetEvent(false);
        private readonly RequestDelegate _next;

        public SocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public void Send(string data)
        {
            _packet = data;
            _send.Set();
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
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
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
