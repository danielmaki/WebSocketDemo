using System.Net.WebSockets;

namespace WebSocketDemo.Factories
{
    public class ClientWebSocketFactory
    {
        public virtual ClientWebSocket Create()
        {
            return new ClientWebSocket();
        }
    }
}
