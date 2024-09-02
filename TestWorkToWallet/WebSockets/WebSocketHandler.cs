using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace TestWorkToWallet.WebSockets
{
    public abstract class WebSocketHandler
    {
        private static readonly ConcurrentDictionary<WebSocket, Task> _sockets = new();

        protected static ConcurrentBag<WebSocket> ActiveSockets = new ConcurrentBag<WebSocket>();

        protected WebSocketHandler() { }


        public static async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine("Received: " + message);

                var responseMessage = "Server received: " + message;
                var responseBuffer = Encoding.UTF8.GetBytes(responseMessage);

                await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public virtual async Task OnConnected(WebSocket socket)
        {
            ActiveSockets.Add(socket);
        }

        public virtual async Task OnDisconnected(WebSocket socket)
        {
            ActiveSockets.TryTake(out socket);
        }

        public async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            var buffer = System.Text.Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket, CancellationToken cancellationToken)
        {
            _sockets[webSocket] = Task.CompletedTask;

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var buffer = new byte[1024 * 4];
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocket server", cancellationToken);
                        _sockets.TryRemove(webSocket, out _);
                    }
                    else
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await BroadcastMessageAsync(message, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
                _sockets.TryRemove(webSocket, out _);
            }
        }

        private async Task BroadcastMessageAsync(string message, CancellationToken cancellationToken)
        {
            foreach (var socket in _sockets.Keys.ToList())
            {
                if (socket.State == WebSocketState.Open)
                {
                    var buffer = Encoding.UTF8.GetBytes(message);
                    var segment = new ArraySegment<byte>(buffer);
                    await socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
                }
            }
        }
    }
}