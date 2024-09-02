using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;

namespace TestWorkToWallet.WebSockets
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketHandlerImplementation _handler;

        public WebSocketMiddleware(RequestDelegate next, WebSocketHandlerImplementation handler)
        {
            _next = next;
            _handler = handler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws/messages")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    await _handler.OnConnected(socket);
                    await _handler.ListenAsync(socket);
                    return;
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }

            await _next(context);
        }
    }
}
