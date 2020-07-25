using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketSharp
{
    public delegate void SocketMessageHandler(object sender, string message);
    public class WebSocketClient
    {
        private ClientWebSocket _socket;
        private CancellationToken listenCt;
        private bool listening = false;

        public event SocketMessageHandler Message;

        public WebSocketClient()
        {
            _socket = new ClientWebSocket();
        }

        public async Task<WebSocketState> ConnectAsync(Uri uri, CancellationToken ct)
        {
            if (_socket.State == WebSocketState.Open) return WebSocketState.Open;
            try
            {
                await _socket.ConnectAsync(uri, ct);
                return _socket.State;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DisconnectAsync()
        {
            StopListening();
            if (_socket.State != WebSocketState.Open) return;
            try
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", new CancellationTokenSource().Token);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void StartListening(CancellationToken ct, int buffSize)
        {
            _ = ListenAsync(ct, buffSize);
        }

        private async Task ListenAsync(CancellationToken ct, int buffSize)
        {
            if (listening) return;
            listening = true;
            while (!listenCt.IsCancellationRequested)
            {
                var buffer = new ArraySegment<byte>(new byte[buffSize]);
                await _socket.ReceiveAsync(buffer, listenCt);

                if (buffer.Count > 0)
                {
                    Message?.Invoke(this, Encoding.UTF8.GetString(buffer.Array));
                }
            }
        }

        public void StopListening()
        {
            if (!listening) return;
            var token = CancellationTokenSource.CreateLinkedTokenSource(new CancellationToken[] { listenCt });
            token.Cancel();
            listening = false;
        }

        public async Task Send(string message)
        {
            await _socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, new CancellationTokenSource().Token);
        }
    }
}
