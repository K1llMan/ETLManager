using System.Net.WebSockets;
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
            if (context.Request.Path != "/api/broadcast")
                await nextDelegate(context);

            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await Program.Manager.Broadcast.Add(webSocket).Receive();
        }
    }
}
