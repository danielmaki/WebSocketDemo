using System.Net.WebSockets;

namespace WebSocketDemo.Services.Factories
{
    public class ClientWebSocketFactory
    {
        public virtual ClientWebSocket Create()
        {
            return new ClientWebSocket();
        }
    }
}
