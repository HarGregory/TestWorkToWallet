using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TestWorkToWallet.WebSockets
{
    public class WebSocketHandlerImplementation : WebSocketHandler
    {
        private readonly ILogger<WebSocketHandlerImplementation> _logger;

        public WebSocketHandlerImplementation(ILogger<WebSocketHandlerImplementation> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnected(WebSocket socket)
        {
            _logger.LogInformation("New WebSocket connection established.");
            await base.OnConnected(socket);
        }

        public override async Task OnDisconnected(WebSocket socket)
        {
            _logger.LogInformation("WebSocket connection closed.");
            await base.OnDisconnected(socket);
        }

        public async Task ReceiveMessageAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _logger.LogInformation($"Received message: {message}");

            // Send the received message to all connected clients
            await BroadcastMessageAsync(message, CancellationToken.None);
        }

        public async Task BroadcastMessageAsync(string message, CancellationToken cancellationToken)
        {
            foreach (var socket in ActiveSockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    var buffer = Encoding.UTF8.GetBytes(message);
                    var segment = new ArraySegment<byte>(buffer);
                    await socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
                }
            }
        }

        public async Task ListenAsync(WebSocket socket)
        {
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await ReceiveMessageAsync(socket, result, buffer);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await OnDisconnected(socket);
                }
            }
        }
    }
}
